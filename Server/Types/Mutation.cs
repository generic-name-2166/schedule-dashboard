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
                @Index 
            )
            """;

        await using Stream stream = file.OpenReadStream();

        try
        {
            using SepReader reader = Sep.Reader().From(stream);

            int idx = -1;
            foreach (var row in reader)
            {
                ++idx;
                int id = row["Ид"].Parse<int>();
                int level = row["Уровень"].Parse<int>();
                ReadOnlySpan<char> wbsCode = row["Код WBS"].Span;
                ReadOnlySpan<char> code = row["Код"].Span;
                ReadOnlySpan<char> name = row["Название"].Span;
                long? startSeconds = ParseDate(row["Начало"].Span);
                long? endSeconds = ParseDate(row["Окончание"].Span);

                using NpgsqlCommand insertCommand = new(stmt, db, tx);
                insertCommand.Parameters.AddWithValue("@DateSeconds", dateSeconds);
                insertCommand.Parameters.AddWithValue("@Id", id);
                insertCommand.Parameters.AddWithValue("@Level", level);
                insertCommand.Parameters.AddWithValue("@WbsCode", wbsCode.ToString());
                insertCommand.Parameters.AddWithValue("@Code", code.ToString());
                insertCommand.Parameters.AddWithValue("@Name", name.ToString());
                if (startSeconds is not null)
                    insertCommand.Parameters.AddWithValue("@Start", startSeconds);
                else
                    insertCommand.Parameters.AddWithValue("@Start", DBNull.Value);
                if (endSeconds is not null)
                    insertCommand.Parameters.AddWithValue("@End", endSeconds);
                else
                    insertCommand.Parameters.AddWithValue("@End", DBNull.Value);
                insertCommand.Parameters.AddWithValue("@Index", idx);
                await insertCommand.ExecuteNonQueryAsync();
            }
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine($"Parsing error: {ex.GetType()} {ex.Message}");
            IError error = ErrorBuilder.New().SetMessage("CSV файл плохо сформирован").Build();
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
