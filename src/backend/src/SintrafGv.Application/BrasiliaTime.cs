namespace SintrafGv.Application;

/// <summary>Conversão UTC → horário de Brasília (America/Sao_Paulo) para exibição em relatórios.</summary>
public static class BrasiliaTime
{
    private static readonly TimeZoneInfo Tz = ResolveTimeZone();

    private static TimeZoneInfo ResolveTimeZone()
    {
        foreach (var id in new[] { "E. South America Standard Time", "America/Sao_Paulo" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }

        return TimeZoneInfo.Utc;
    }

    public static DateTime ToBrasilia(DateTime utc)
    {
        var normalized = utc.Kind switch
        {
            DateTimeKind.Utc => utc,
            DateTimeKind.Local => utc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(utc, DateTimeKind.Utc)
        };
        return TimeZoneInfo.ConvertTimeFromUtc(normalized, Tz);
    }

    public static string FormatDate(DateTime utc) => ToBrasilia(utc).ToString("dd/MM/yyyy");

    public static string FormatTime(DateTime utc) => ToBrasilia(utc).ToString("HH:mm:ss");

    public static string FormatDateTime(DateTime utc) => ToBrasilia(utc).ToString("dd/MM/yyyy HH:mm");
}
