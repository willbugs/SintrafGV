using System.Globalization;

namespace SintrafGv.Application;

/// <summary>Conversão UTC → horário de Brasília (America/Sao_Paulo) para exibição em relatórios.</summary>
public static class BrasiliaTime
{
    private static readonly TimeZoneInfo Tz = ResolveTimeZone();

    private static readonly HashSet<string> DateOnlyPropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "DataNascimento",
        "DataAdmissao",
        "DataFiliacao",
        "DataDesligamento"
    };

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

    public static DateTime NormalizeUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    public static DateTime ToBrasilia(DateTime utc)
    {
        var normalized = NormalizeUtc(utc);
        return TimeZoneInfo.ConvertTimeFromUtc(normalized, Tz);
    }

    /// <summary>Data civil armazenada em UTC (meia-noite UTC) — exibe dia/mês/ano sem deslocar o dia.</summary>
    public static string FormatDateOnly(DateTime utc)
    {
        var normalized = NormalizeUtc(utc);
        return normalized.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }

    public static string FormatDate(DateTime utc) => ToBrasilia(utc).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    public static string FormatTime(DateTime utc) => ToBrasilia(utc).ToString("HH:mm:ss", CultureInfo.InvariantCulture);

    public static string FormatDateTime(DateTime utc) => ToBrasilia(utc).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

    public static string FormatDateTimeSeconds(DateTime utc) =>
        ToBrasilia(utc).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

    public static bool IsDateOnlyProperty(string? propertyName) =>
        !string.IsNullOrEmpty(propertyName) && DateOnlyPropertyNames.Contains(propertyName);

    public static string FormatForDisplay(DateTime utc, bool dateOnly = false) =>
        dateOnly ? FormatDateOnly(utc) : FormatDateTime(utc);

    public static string FormatForDisplay(DateTime utc, string? propertyName) =>
        FormatForDisplay(utc, IsDateOnlyProperty(propertyName));

    public static DateOnly ToBrasiliaDateOnly(DateTime utc) =>
        DateOnly.FromDateTime(ToBrasilia(utc));

    public static int ToBrasiliaHour(DateTime utc) => ToBrasilia(utc).Hour;
}
