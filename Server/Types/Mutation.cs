using System.Globalization;
using nietras.SeparatedValues;
using Npgsql;
using Server.Common;

namespace Server.Types;

[MutationType]
public static class Mutation
{
    /// <summary>
    ///
    /// </summary>
    /// <returns>NpgsqlConnection that needs to be Disposed</returns>
    private static async Task<NpgsqlConnection> InitializeDatabase()
    {
        try
        {
            NpgsqlConnection db = new(DbCommon.CONNECTION_STRING);
            await db.OpenAsync();
            await using NpgsqlCommand command = new(DbCommon.INIT_STMT, db);
            await command.ExecuteNonQueryAsync();
            return db;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Db error: {ex.GetType()} {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="maybeDate"></param>
    /// <returns>Number of seconds since unix epoch or null if the string is empty</returns>
    private static long? ParseDate(ReadOnlySpan<char> maybeDate)
    {
        if (maybeDate.IsEmpty)
            return null;
        DateTime date = DateTime.ParseExact(
            maybeDate,
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None
        );
        return DateCommon.DateToSeconds(date);
    }

    private static async Task<bool> DateExists(NpgsqlConnection db, long dateSeconds)
    {
        string stmt = """
                SELECT EXISTS(
                    SELECT 1 FROM schedule WHERE date_s = @DateSeconds
                )
            """;
        await using NpgsqlCommand command = new(stmt, db);
        command.Parameters.AddWithValue("@DateSeconds", dateSeconds);
        await using NpgsqlDataReader query = await command.ExecuteReaderAsync();
        if (!await query.ReadAsync())
        {
            return false;
        }
        return query.GetBoolean(0);
    }

    private readonly record struct ParsedRow(
        int Id,
        int Level,
        string WbsCode,
        string Code,
        string Name,
        long? StartSeconds,
        long? EndSeconds,
        int DescendantEndIdx
    );

    /// <summary>Compute descendant end indices using the same algorithm as collectTree on the frontend.</summary>
    private static void ComputeDescendants(List<ParsedRow> rows)
    {
        // Compute the number of WBS segments to determine root depth
        int rootDepth = rows.Count > 0 ? rows[0].WbsCode.Split('.').Length : 0;

        // Stack of (index, depth)
        Stack<(int Index, int Depth)> open = new();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            int depth = row.WbsCode.Split('.').Length;

            // Close nodes that are at the same or higher depth
            while (open.Count > 0 && open.Peek().Depth >= depth)
            {
                var closed = open.Pop();
                rows[closed.Index] = rows[closed.Index] with { DescendantEndIdx = i };
            }

            open.Push((i, depth));
        }

        // Close remaining open nodes
        while (open.Count > 0)
        {
            var closed = open.Pop();
            rows[closed.Index] = rows[closed.Index] with { DescendantEndIdx = rows.Count };
        }
    }

    private static async Task InsertData(
        NpgsqlConnection db,
        NpgsqlTransaction tx,
        long dateSeconds,
        IFile file
    )
    {
        string stmt = """
            INSERT INTO schedule VALUES (
                @DateSeconds, 
                @Id, 
                @Level, 
                @WbsCode, 
                @Code, 
                @Name, 
                @Start, 
                @End, 
                @Index,
                @DescendantEndIdx
            )
            """;

        await using Stream stream = file.OpenReadStream();

        try
        {
            using SepReader reader = Sep.Reader().From(stream);

            // Buffer all rows to compute descendant_end_idx
            List<ParsedRow> rows = new(1024);

            int idx = -1;
            foreach (var csvRow in reader)
            {
                ++idx;
                int id = csvRow["Ид"].Parse<int>();
                int level = csvRow["Уровень"].Parse<int>();
                ReadOnlySpan<char> wbsCode = csvRow["Код WBS"].Span;
                ReadOnlySpan<char> code = csvRow["Код"].Span;
                ReadOnlySpan<char> name = csvRow["Название"].Span;
                long? startSeconds = ParseDate(csvRow["Начало"].Span);
                long? endSeconds = ParseDate(csvRow["Окончание"].Span);

                rows.Add(
                    new ParsedRow
                    {
                        Id = id,
                        Level = level,
                        WbsCode = wbsCode.ToString(),
                        Code = code.ToString(),
                        Name = name.ToString(),
                        StartSeconds = startSeconds,
                        EndSeconds = endSeconds,
                        DescendantEndIdx = 0, // placeholder, will be computed
                    }
                );
            }

            ComputeDescendants(rows);

            // Insert all rows with computed descendant_end_idx
            int r = -1;
            foreach (var row in rows)
            {
                ++r;
                using NpgsqlCommand insertCommand = new(stmt, db, tx);
                insertCommand.Parameters.AddWithValue("@DateSeconds", dateSeconds);
                insertCommand.Parameters.AddWithValue("@Id", row.Id);
                insertCommand.Parameters.AddWithValue("@Level", row.Level);
                insertCommand.Parameters.AddWithValue("@WbsCode", row.WbsCode);
                insertCommand.Parameters.AddWithValue("@Code", row.Code);
                insertCommand.Parameters.AddWithValue("@Name", row.Name);
                if (row.StartSeconds is not null)
                    insertCommand.Parameters.AddWithValue("@Start", row.StartSeconds);
                else
                    insertCommand.Parameters.AddWithValue("@Start", DBNull.Value);
                if (row.EndSeconds is not null)
                    insertCommand.Parameters.AddWithValue("@End", row.EndSeconds);
                else
                    insertCommand.Parameters.AddWithValue("@End", DBNull.Value);
                insertCommand.Parameters.AddWithValue("@Index", r);
                insertCommand.Parameters.AddWithValue("@DescendantEndIdx", row.DescendantEndIdx);
                await insertCommand.ExecuteNonQueryAsync();
            }
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine($"Parsing error: {ex.GetType()} {ex.Message}");
            IError error = ErrorBuilder.New().SetMessage("CSV файл плохо сформирован").Build();
            throw new GraphQLException(error);
        }
        catch (PostgresException ex)
        {
            Console.WriteLine($"Insert error: {ex.GetType()} {ex.Message}");
            // при нормальном использовании мы можем только ожидать primary key constraint error
            IError error = ErrorBuilder
                .New()
                .SetMessage("Файл содержит некорректные данные, например повторяющиеся ID")
                .Build();
            throw new GraphQLException(error);
        }
    }

    public static async Task<bool> CreateScheduleObjects(DateTime date, IFile file)
    {
        long dateSeconds = DateCommon.DateToSeconds(date);

        await using NpgsqlConnection db = await InitializeDatabase();

        if (await DateExists(db, dateSeconds))
        {
            IError error = ErrorBuilder
                .New()
                .SetMessage("Данные на данную дату уже существуют")
                .Build();
            throw new GraphQLException(error);
        }

        await using NpgsqlTransaction tx = db.BeginTransaction();
        await InsertData(db, tx, dateSeconds, file);
        await tx.CommitAsync();

        return true;
    }

    private static async Task DeleteObjectsForDate(
        NpgsqlConnection db,
        NpgsqlTransaction tx,
        long dateSeconds
    )
    {
        string stmt = """
                DELETE FROM schedule WHERE date_s = @DateSeconds
            """;
        await using NpgsqlCommand command = new(stmt, db, tx);
        command.Parameters.AddWithValue("@DateSeconds", dateSeconds);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        if (reader.RecordsAffected <= 0)
        {
            IError error = ErrorBuilder.New().SetMessage("Данных на дату не существует").Build();
            throw new GraphQLException(error);
        }
    }

    public static async Task<bool> EditScheduleObjects(DateTime date, IFile file)
    {
        long dateSeconds = DateCommon.DateToSeconds(date);

        await using NpgsqlConnection db = await InitializeDatabase();
        await using NpgsqlTransaction tx = await db.BeginTransactionAsync();

        await DeleteObjectsForDate(db, tx, dateSeconds);
        await InsertData(db, tx, dateSeconds, file);
        await tx.CommitAsync();

        return true;
    }

    public static async Task<bool> DeleteScheduleObjects(DateTime date)
    {
        long dateSeconds = DateCommon.DateToSeconds(date);

        await using NpgsqlConnection db = await InitializeDatabase();
        await using NpgsqlTransaction tx = await db.BeginTransactionAsync();
        await DeleteObjectsForDate(db, tx, dateSeconds);
        await tx.CommitAsync();

        return true;
    }
}
