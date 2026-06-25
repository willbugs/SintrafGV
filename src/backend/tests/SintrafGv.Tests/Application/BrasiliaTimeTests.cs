using FluentAssertions;
using SintrafGv.Application;

namespace SintrafGv.Tests.Application;

public class BrasiliaTimeTests
{
    [Fact]
    public void FormatDateTime_ConverteUtcParaBrasilia()
    {
        var utc = new DateTime(2026, 6, 23, 19, 31, 37, DateTimeKind.Utc);
        BrasiliaTime.FormatDateTime(utc).Should().Be("23/06/2026 16:31");
    }

    [Fact]
    public void FormatDateOnly_UsaComponentesUtc_SemDeslocarDia()
    {
        var utc = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc);
        BrasiliaTime.FormatDateOnly(utc).Should().Be("15/05/1990");
    }

    [Fact]
    public void FormatForDisplay_UltimaVotacao_UsaHorarioBrasilia()
    {
        var utc = new DateTime(2026, 6, 23, 19, 31, 37, DateTimeKind.Utc);
        BrasiliaTime.FormatForDisplay(utc, "UltimaVotacao").Should().Be("23/06/2026 16:31");
    }

    [Fact]
    public void FormatForDisplay_DataNascimento_UsaDataCivil()
    {
        var utc = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc);
        BrasiliaTime.FormatForDisplay(utc, "DataNascimento").Should().Be("15/05/1990");
    }

    [Fact]
    public void ToBrasiliaHour_RetornaHoraLocal()
    {
        var utc = new DateTime(2026, 6, 23, 19, 31, 37, DateTimeKind.Utc);
        BrasiliaTime.ToBrasiliaHour(utc).Should().Be(16);
    }

    [Fact]
    public void ToBrasiliaDateOnly_RetornaDataLocal()
    {
        var utc = new DateTime(2026, 6, 23, 2, 30, 0, DateTimeKind.Utc);
        BrasiliaTime.ToBrasiliaDateOnly(utc).Should().Be(new DateOnly(2026, 6, 22));
    }
}
