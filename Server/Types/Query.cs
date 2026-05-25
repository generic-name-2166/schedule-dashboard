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
    /// Reads one row from a NpgsqlDataReader into a ScheduleObject.
    /// Column order must be: id(0), level(1), wbs_code(2), code(3), name(4), start_s(5), end_s(6), idx(7), descendant_end_idx(8)
    /// </summary>
    private static ScheduleObject ReadScheduleObject(NpgsqlDataReader reader)
    {
        DateTime? start = reader.IsDBNull(5)
            ? null
            : DateCommon.SecondsToDate(reader.GetInt32(5));
        DateTime? end = reader.IsDBNull(6)
            ? null
            : DateCommon.SecondsToDate(reader.GetInt32(6));
        return new(
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            start,
            end,
            reader.GetInt32(7),
            reader.GetInt32(8)
        );
    }

    public static async Task<List<ScheduleObject>> GetScheduleObjects(DateTime date)
    {
        await using NpgsqlConnection db = await InitializeDatabase();
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
                idx,
                descendant_end_idx
            FROM schedule
            WHERE date_s = @DateSeconds
            """;
        await using NpgsqlCommand selectCommand = new(stmt, db);
        selectCommand.Parameters.AddWithValue("@DateSeconds", DateCommon.DateToSeconds(date));

        try
        {
            await using NpgsqlDataReader query = await selectCommand.ExecuteReaderAsync();
            while (await query.ReadAsync())
            {
                objects.Add(ReadScheduleObject(query));
            }
            return objects;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Querying error: {ex.GetType()} {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get multiple subtrees (each root + all its descendants) by their codes, using a single self-join query.
    /// Returns subtrees in the same order as the input codes; missing codes produce an empty inner list.
    /// </summary>
    public static async Task<List<List<ScheduleObject>>> GetScheduleSubtrees(
        DateTime date,
        List<string> codes
    )
    {
        await using NpgsqlConnection db = await InitializeDatabase();

        // Single query: self-join each root's range (idx..descendant_end_idx) against all rows.
        // Note: code is NOT unique per date — some objects have a child with the same code.
        // We use DISTINCT ON to pick the first occurrence (by idx) for each requested code.
        string stmt = """
                SELECT t.id, t.level, t.wbs_code, t.code, t.name,
                       t.start_s, t.end_s, t.idx, t.descendant_end_idx,
                       r.code AS root_code
                FROM schedule t
                INNER JOIN (
                    SELECT DISTINCT ON (code) code,
                           idx AS start_idx,
                           descendant_end_idx AS end_idx
                    FROM schedule
                    WHERE date_s = @DateSeconds AND code = ANY(@Codes)
                    ORDER BY code, idx
                ) r
                    ON t.idx >= r.start_idx AND t.idx < r.end_idx
                WHERE t.date_s = @DateSeconds
                ORDER BY r.start_idx, t.idx
            """;
        await using NpgsqlCommand command = new(stmt, db);
        command.Parameters.AddWithValue("@DateSeconds", DateCommon.DateToSeconds(date));
        command.Parameters.AddWithValue("@Codes", codes);

        try
        {
            // Group by root_code preserving input order
            Dictionary<string, List<ScheduleObject>> grouped = [];
            foreach (string code in codes)
            {
                grouped[code] = []; // preserve input order via insertion order
            }

            await using NpgsqlDataReader query = await command.ExecuteReaderAsync();
            while (await query.ReadAsync())
            {
                string rootCode = query.GetString(9);
                ScheduleObject node = ReadScheduleObject(query);
                grouped[rootCode].Add(node);
            }

            // Map back to input order
            List<List<ScheduleObject>> result = [];
            foreach (string code in codes)
            {
                result.Add(grouped[code]);
            }
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Querying error: {ex.GetType()} {ex.Message}");
            throw;
        }
    }

    public static async Task<List<DateTime>> GetAvailableDates()
    {
        await using NpgsqlConnection db = await InitializeDatabase();
        List<DateTime> dates = [];
        string stmt = """
                SELECT DISTINCT date_s FROM schedule ORDER BY date_s ASC
            """;
        await using NpgsqlCommand selectCommand = new(stmt, db);

        await using NpgsqlDataReader query = await selectCommand.ExecuteReaderAsync();
        while (await query.ReadAsync())
        {
            DateTime date = DateCommon.SecondsToDate(query.GetInt64(0));
            dates.Add(date);
        }
        return dates;
    }

    public static async Task<List<ScheduleDiff>> GetDateDiff(DateTime oldDate, DateTime newDate)
    {
        long newSeconds;
        long oldSeconds;
        if (newDate > oldDate)
        {
            newSeconds = DateCommon.DateToSeconds(newDate);
            oldSeconds = DateCommon.DateToSeconds(oldDate);
        }
        else
        {
            newSeconds = DateCommon.DateToSeconds(oldDate);
            oldSeconds = DateCommon.DateToSeconds(newDate);
        }
        await using NpgsqlConnection db = await InitializeDatabase();
        string stmt = """
            WITH OldSchedule AS (
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
                WHERE date_s = @OldSeconds
            ),
            NewSchedule AS (
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
                WHERE date_s = @NewSeconds
            )
            SELECT 
                n.id,
                n.wbs_code,
                n.name,
                o.start_s AS old_start,
                n.start_s AS new_start,
                o.end_s AS old_end,
                n.end_s AS new_end
            FROM NewSchedule AS n
            INNER JOIN OldSchedule AS o
                ON n.id = o.id AND (
                    n.start_s <> o.start_s
                    OR n.end_s <> o.end_s
                )
            ORDER BY n.idx
            """;
        await using NpgsqlCommand selectCommand = new(stmt, db);
        selectCommand.Parameters.AddWithValue("@OldSeconds", oldSeconds);
        selectCommand.Parameters.AddWithValue("@NewSeconds", newSeconds);

        List<ScheduleDiff> diffs = [];
        await using NpgsqlDataReader query = await selectCommand.ExecuteReaderAsync();
        while (await query.ReadAsync())
        {
            DateTime oldStart = DateCommon.SecondsToDate(query.GetInt64(3));
            DateTime newStart = DateCommon.SecondsToDate(query.GetInt64(4));
            DateTime oldEnd = DateCommon.SecondsToDate(query.GetInt64(5));
            DateTime newEnd = DateCommon.SecondsToDate(query.GetInt64(6));
            ScheduleDiff diff = new(
                query.GetInt32(0),
                query.GetString(1),
                query.GetString(2),
                oldStart,
                newStart,
                oldEnd,
                newEnd
            );
            diffs.Add(diff);
        }
        return diffs;
    }
}
