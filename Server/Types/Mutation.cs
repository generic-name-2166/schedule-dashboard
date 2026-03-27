using System.Globalization;
using System.Text;
using Microsoft.Data.Sqlite;
using nietras.SeparatedValues;
using Server.Common;

namespace Server.Types;

[MutationType]
public static class Mutation
{
    /// <summary>
    ///
    /// </summary>
    /// <returns>SqliteConnection that needs to be Disposed</returns>
    private static SqliteConnection InitializeDatabase()
    {
        try
        {
            SqliteConnection db = new(DbCommon.CONNECTION_STRING);
            db.Open();
            SqliteCommand command = new(DbCommon.INIT_STMT, db);
            command.ExecuteNonQuery();
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

    private static bool DateExists(SqliteConnection db, long dateSeconds)
    {
        string stmt = """
                SELECT EXISTS(
                    SELECT 1 FROM schedule WHERE date_s = @DateSeconds
                )
            """;
        SqliteCommand command = new(stmt, db);
        command.Parameters.AddWithValue("@DateSeconds", dateSeconds);
        SqliteDataReader query = command.ExecuteReader();
        if (!query.Read())
        {
            return false;
        }
        return query.GetBoolean(0);
    }

    private static async Task InsertData(
        SqliteConnection db,
        SqliteTransaction tx,
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
                @End 
            )
            """;

        await using Stream stream = file.OpenReadStream();

        try
        {
            using SepReader reader = Sep.Reader().From(stream);

            foreach (var row in reader)
            {
                int id = row["Ид"].Parse<int>();
                int level = row["Уровень"].Parse<int>();
                ReadOnlySpan<char> wbsCode = row["Код WBS"].Span;
                ReadOnlySpan<char> code = row["Код"].Span;
                ReadOnlySpan<char> name = row["Название"].Span;
                long? startSeconds = ParseDate(row["Начало"].Span);
                long? endSeconds = ParseDate(row["Окончание"].Span);

                SqliteCommand insertCommand = new(stmt, db, tx);
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
                insertCommand.ExecuteReader();
            }

            tx.Commit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Parsing error: {ex.GetType()} {ex.Message}");
            // TODO
            throw;
        }
    }

    public static async Task<bool> CreateScheduleObjects(DateTime date, IFile file)
    {
        long dateSeconds = DateCommon.DateToSeconds(date);

        using SqliteConnection db = InitializeDatabase();

        if (DateExists(db, dateSeconds))
        {
            throw new Exception("Данные на данную дату уже существуют");
        }

        using SqliteTransaction tx = db.BeginTransaction();

        await InsertData(db, tx, dateSeconds, file);

        return true;
    }

    public static async Task<bool> EditScheduleObjects(DateTime date, IFile file)
    {
        long dateSeconds = DateCommon.DateToSeconds(date);

        using SqliteConnection db = InitializeDatabase();
        using SqliteTransaction tx = db.BeginTransaction();

        string stmt = """
                DELETE FROM schedule WHERE date_s = @DateSeconds
            """;
        SqliteCommand command = new(stmt, db, tx);
        command.Parameters.AddWithValue("@DateSeconds", dateSeconds);
        SqliteDataReader reader = command.ExecuteReader();
        if (reader.RecordsAffected <= 0)
        {
            throw new Exception("Данных на дату не существует");
        }

        await InsertData(db, tx, dateSeconds, file);
        return true;
    }
}
