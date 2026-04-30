namespace Server.Common;

public static class DbCommon
{
    // public const string CONNECTION_STRING = "Filename=./db.sqlite";
    public const string CONNECTION_STRING = "Host=localhost;Username=postgres;Password=postgres;Database=schedule";
    public const string INIT_STMT = """
            CREATE TABLE IF NOT EXISTS schedule (
                date_s INTEGER NOT NULL, 
                id INTEGER NOT NULL, 
                level INTEGER NOT NULL, 
                wbs_code TEXT NOT NULL, 
                code TEXT NOT NULL, 
                name TEXT NOT NULL, 
                start INTEGER, 
                end INTEGER, 
                idx INTEGER NOT NULL, 
                PRIMARY KEY (date_s, id)
            ) STRICT
        """;
}
