using System.Globalization;
using System.Text;
using Microsoft.Data.Sqlite;
using nietras.SeparatedValues;

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
            SqliteConnection db = new("Filename=./db.sqlite");
            db.Open();
            string query = """
                CREATE TABLE IF NOT EXISTS schedule (
                    id INTEGER PRIMARY KEY, 
                    level INTEGER NOT NULL, 
                    wbs_code TEXT NOT NULL, 
                    code TEXT NOT NULL, 
                    name TEXT NOT NULL, 
                    start INTEGER, 
                    end INTEGER 
                ) STRICT
                """;
            SqliteCommand command = new(query, db);
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
    private static int? ParseDate(ReadOnlySpan<char> maybeDate)
    {
        if (maybeDate.IsEmpty)
            return null;
        DateTime date = DateTime.ParseExact(
            maybeDate,
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None
        );
        TimeSpan diff = date.ToUniversalTime() - DateTime.UnixEpoch;
        return (int)Math.Floor(diff.TotalSeconds);
    }

    public static async Task<bool> UploadFileAsync(IFile file)
    {
        var fileName = file.Name;
        var fileSize = file.Length;

        Console.WriteLine($"{fileName} {fileSize}");

        using SqliteConnection db = InitializeDatabase();
        using SqliteTransaction tx = db.BeginTransaction();
        string query = """
            INSERT INTO schedule VALUES (
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
                int? startSeconds = ParseDate(row["Начало"].Span);
                int? endSeconds = ParseDate(row["Окончание"].Span);

                SqliteCommand insertCommand = new(query, db, tx);
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
            throw;
        }

        // We can now work with standard stream functionality of .NET
        // to handle the file.
        return true;
    }
}
