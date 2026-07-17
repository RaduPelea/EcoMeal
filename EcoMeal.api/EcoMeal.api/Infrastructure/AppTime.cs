namespace EcoMeal.api.Infrastructure;

// Romanian wall-clock time regardless of the server's own time zone
// (the production host runs on UTC+2, one hour behind Romania).
public static class AppTime
{
    private static readonly TimeZoneInfo Tz = GetTz();

    private static TimeZoneInfo GetTz()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Bucharest"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time"); }
    }

    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Tz);
}
