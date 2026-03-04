using Microsoft.Data.Sqlite;

namespace Server.Types;

[QueryType]
public static class Query
{
    public static Book GetBook() => new Book("C# in depth.", new Author("Jon Skeet"));

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
            string stmt = """
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
            SqliteCommand command = new(stmt, db);
            command.ExecuteNonQuery();
            return db;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Db error: {ex.GetType()} {ex.Message}");
            throw;
        }
    }

    private static DateTime ConvertSecondsToDateTime(int seconds) =>
        DateTime.UnixEpoch.AddSeconds(seconds);

    public static List<ScheduleObject> GetScheduleObjects()
    {
        using SqliteConnection db = InitializeDatabase();
        List<ScheduleObject> objects = [];
        string stmt = """
            SELECT 
                id,
                level,
                wbs_code,
                code,
                name,
                start,
                end
            FROM schedule
            """;
        SqliteCommand selectCommand = new(stmt, db);

        try
        {
            SqliteDataReader query = selectCommand.ExecuteReader();

            while (query.Read())
            {
                DateTime? start = query.IsDBNull(5)
                    ? null
                    : ConvertSecondsToDateTime(query.GetInt32(5));
                DateTime? end = query.IsDBNull(6)
                    ? null
                    : ConvertSecondsToDateTime(query.GetInt32(6));
                ScheduleObject node = new(
                    query.GetInt32(0),
                    query.GetInt32(1),
                    query.GetString(2),
                    query.GetString(3),
                    query.GetString(4),
                    start,
                    end
                );
                objects.Add(node);
            }
            return objects;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Querying error: {ex.GetType()} {ex.Message}");
            throw;
        }
    }
}
