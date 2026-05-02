using Npgsql;
using Server.Common;

namespace Server.Types;

[QueryType]
public static class Query
{
    /// <summary>
    ///
    /// </summary>
    /// <returns>NpgsqlConnection that needs to be Disposed</returns>
    private static NpgsqlConnection InitializeDatabase()
    {
        try
        {
            NpgsqlConnection db = new(DbCommon.CONNECTION_STRING);
            db.Open();
            NpgsqlCommand command = new(DbCommon.INIT_STMT, db);
            command.ExecuteNonQuery();
            return db;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Db error: {ex.GetType()} {ex.Message}");
            throw;
        }
    }

    public static List<ScheduleObject> GetScheduleObjects(DateTime date)
    {
        using NpgsqlConnection db = InitializeDatabase();
        List<ScheduleObject> objects = [];
        string stmt = """
            SELECT 
                id,
                level,
                wbs_code,
                code,
                name,
                start_s,
                end_s, 
                idx
            FROM schedule
            WHERE date_s = @DateSeconds
            """;
        using NpgsqlCommand selectCommand = new(stmt, db);
        selectCommand.Parameters.AddWithValue("@DateSeconds", DateCommon.DateToSeconds(date));

        try
        {
            using NpgsqlDataReader query = selectCommand.ExecuteReader();

            while (query.Read())
            {
                DateTime? start = query.IsDBNull(5)
                    ? null
                    : DateCommon.SecondsToDate(query.GetInt32(5));
                DateTime? end = query.IsDBNull(6)
                    ? null
                    : DateCommon.SecondsToDate(query.GetInt32(6));
                ScheduleObject node = new(
                    query.GetInt32(0),
                    query.GetInt32(1),
                    query.GetString(2),
                    query.GetString(3),
                    query.GetString(4),
                    start,
                    end,
                    query.GetInt32(7)
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

    public static List<DateTime> GetAvailableDates()
    {
        using NpgsqlConnection db = InitializeDatabase();
        List<DateTime> dates = [];
        string stmt = """
                SELECT DISTINCT date_s FROM schedule ORDER BY date_s ASC
            """;
        using NpgsqlCommand selectCommand = new(stmt, db);

        using NpgsqlDataReader query = selectCommand.ExecuteReader();
        while (query.Read())
        {
            DateTime date = DateCommon.SecondsToDate(query.GetInt64(0));
            dates.Add(date);
        }
        return dates;
    }
}
