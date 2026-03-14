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

    public static async Task<bool> UploadFileAsync(DateTime date, IFile file)
    {
        string fileName = file.Name;
        long? fileSize = file.Length;

        Console.WriteLine($"{fileName} {fileSize}");

        long dateSeconds = DateCommon.DateToSeconds(date);

        using SqliteConnection db = InitializeDatabase();
        using SqliteTransaction tx = db.BeginTransaction();
        string query = """
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

                SqliteCommand insertCommand = new(query, db, tx);
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
            throw;
        }

        // We can now work with standard stream functionality of .NET
        // to handle the file.
        return true;
    }
}
