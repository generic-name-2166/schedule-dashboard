namespace Server.Common;

public static class DateCommon
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="date"></param>
    /// <returns>Amount of seconds since UNIX epoch</returns>
    public static long DateToSeconds(DateTime date)
    {
        TimeSpan diff = date.ToUniversalTime() - DateTime.UnixEpoch;
        return (long)Math.Floor(diff.TotalSeconds);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="seconds">Amount of seconds since UNIX epoch</param>
    /// <returns></returns>
    public static DateTime SecondsToDate(long seconds) => DateTime.UnixEpoch.AddSeconds(seconds);
}
