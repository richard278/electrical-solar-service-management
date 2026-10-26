using EcoInstalacion.BLL.Implementacion;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.BLL.Tests.Fakes;
using EcoInstalacion.Entity;
using EcoInstalacion.IOC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EcoInstalacion.BLL.Tests;

public sealed class OperationalClockAndConfigurationTests
{
    [Fact]
    public void Validate_DefaultOptions_PassesWithCanonicalValues()
    {
        // Arrange
        var options = new OperationalTemporalOptions();

        // Act
        var exception = Record.Exception(() => options.Validate());

        // Assert
        Assert.Null(exception);
        Assert.Equal("America/El_Salvador", options.OperationalTimeZoneId);
        Assert.Equal(24, options.WarningHours);
        Assert.Equal(48, options.OverdueHours);
        Assert.Equal(5, options.PreviewLimit);
        Assert.Equal(15, options.FallbackRefreshMinutes);
        Assert.Equal(TimeSpan.FromHours(24), options.WarningThreshold);
        Assert.Equal(TimeSpan.FromHours(48), options.OverdueThreshold);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("UTC")]
    [InlineData("America/New_York")]
    [InlineData("Europe/Madrid")]
    public void Validate_InvalidTimeZoneId_ThrowsArgumentException(string? invalidZoneId)
    {
        // Arrange
        var options = new OperationalTemporalOptions
        {
            OperationalTimeZoneId = invalidZoneId!
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => options.Validate());
    }

    [Theory]
    [InlineData(0, 48)]
    [InlineData(-5, 48)]
    [InlineData(24, 24)]
    [InlineData(24, 12)]
    public void Validate_InvalidWarningOrOverdueHours_ThrowsArgumentOutOfRangeException(
        int warningHours,
        int overdueHours)
    {
        // Arrange
        var options = new OperationalTemporalOptions
        {
            WarningHours = warningHours,
            OverdueHours = overdueHours
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate());
    }

    [Theory]
    [InlineData(0, 15)]
    [InlineData(-1, 15)]
    [InlineData(5, 0)]
    [InlineData(5, -10)]
    public void Validate_InvalidPreviewLimitOrRefreshMinutes_ThrowsArgumentOutOfRangeException(
        int previewLimit,
        int fallbackRefreshMinutes)
    {
        // Arrange
        var options = new OperationalTemporalOptions
        {
            PreviewLimit = previewLimit,
            FallbackRefreshMinutes = fallbackRefreshMinutes
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate());
    }

    [Fact]
    public void ToLocal_ConvertsUtcInstantToElSalvadorOffset_AcrossMidnightBoundary()
    {
        // Arrange
        var clock = new ElSalvadorOperationalClock();
        var utcBeforeLocalMidnight = new DateTimeOffset(2026, 10, 21, 5, 59, 59, 999, TimeSpan.Zero);
        var utcAtLocalMidnight = new DateTimeOffset(2026, 10, 21, 6, 0, 0, 0, TimeSpan.Zero);

        // Act
        var localBeforeMidnight = clock.ToLocal(utcBeforeLocalMidnight);
        var localAtMidnight = clock.ToLocal(utcAtLocalMidnight);
        var dateBeforeMidnight = clock.ToLocalDate(utcBeforeLocalMidnight);
        var dateAtMidnight = clock.ToLocalDate(utcAtLocalMidnight);

        // Assert
        Assert.Equal(TimeSpan.FromHours(-6), localBeforeMidnight.Offset);
        Assert.Equal(new DateTime(2026, 10, 20, 23, 59, 59, 999), localBeforeMidnight.DateTime);
        Assert.Equal(new DateOnly(2026, 10, 20), dateBeforeMidnight);

        Assert.Equal(TimeSpan.FromHours(-6), localAtMidnight.Offset);
        Assert.Equal(new DateTime(2026, 10, 21, 0, 0, 0, 0), localAtMidnight.DateTime);
        Assert.Equal(new DateOnly(2026, 10, 21), dateAtMidnight);
    }

    [Fact]
    public void ResolveTimeZone_HandlesWindowsAndIanaAndFallbackDeterministically()
    {
        // Arrange
        var customElSalvadorZone = TimeZoneInfo.CreateCustomTimeZone(
            "America/El_Salvador",
            TimeSpan.FromHours(-6),
            "El Salvador",
            "El Salvador");
        var customWindowsZone = TimeZoneInfo.CreateCustomTimeZone(
            "Central America Standard Time",
            TimeSpan.FromHours(-6),
            "Central America",
            "Central America");

        // Act: Simular Linux (solo resuelve IANA)
        var resolvedOnLinux = ElSalvadorOperationalClock.ResolveTimeZone(
            "America/El_Salvador",
            id => id == "America/El_Salvador" ? customElSalvadorZone : null);

        // Act: Simular Windows (solo resuelve Windows ID)
        var resolvedOnWindows = ElSalvadorOperationalClock.ResolveTimeZone(
            "America/El_Salvador",
            id => id == "Central America Standard Time" ? customWindowsZone : null);

        // Act: Simular contenedor mínimo sin tzdata (usa fallback fijo UTC-06:00)
        var resolvedFallback = ElSalvadorOperationalClock.ResolveTimeZone(
            "Central America Standard Time",
            _ => null);

        // Assert
        Assert.Equal(TimeSpan.FromHours(-6), resolvedOnLinux.BaseUtcOffset);
        Assert.False(resolvedOnLinux.SupportsDaylightSavingTime);

        Assert.Equal(TimeSpan.FromHours(-6), resolvedOnWindows.BaseUtcOffset);
        Assert.False(resolvedOnWindows.SupportsDaylightSavingTime);

        Assert.Equal(TimeSpan.FromHours(-6), resolvedFallback.BaseUtcOffset);
        Assert.False(resolvedFallback.SupportsDaylightSavingTime);
    }

    [Fact]
    public void Snapshot_ProducesIdenticalResults_EvenWhenClockAdvancesLater()
    {
        // Arrange
        var initialReference = new DateTimeOffset(2026, 10, 21, 12, 0, 0, TimeSpan.FromHours(-6));
        var fakeClock = new FakeOperationalClock(initialReference);
        var policy = new OperationalAlertPolicy(fakeClock);

        var solicitudDate = initialReference.AddHours(-24);
        var visitaDate = new DateOnly(2026, 10, 21);

        var capturedSnapshot = fakeClock.CaptureSnapshot();

        // Act: Evaluar antes y después de avanzar el reloj 30 horas
        var evalSolicitudBefore = policy.EvaluarSolicitud(
            EstadosSolicitudSistema.Pendiente,
            solicitudDate,
            capturedSnapshot);
        var evalVisitaBefore = policy.EvaluarVisita(
            visitaDate,
            aprobado: false,
            EstadosSolicitudSistema.Programada,
            capturedSnapshot);

        fakeClock.Advance(TimeSpan.FromHours(30));

        var evalSolicitudAfter = policy.EvaluarSolicitud(
            EstadosSolicitudSistema.Pendiente,
            solicitudDate,
            capturedSnapshot);
        var evalVisitaAfter = policy.EvaluarVisita(
            visitaDate,
            aprobado: false,
            EstadosSolicitudSistema.Programada,
            capturedSnapshot);

        // Assert
        Assert.Equal(evalSolicitudBefore, evalSolicitudAfter);
        Assert.Equal(NivelAlertaOperativa.Preventiva, evalSolicitudAfter.Nivel);
        Assert.Equal(evalVisitaBefore, evalVisitaAfter);
        Assert.Equal(NivelAlertaOperativa.Preventiva, evalVisitaAfter.Nivel);
    }

    [Fact]
    public void OperationalClock_RealInstance_ExposesConsistentSnapshotsAndProperties()
    {
        // Arrange
        var clock = new ElSalvadorOperationalClock();

        // Act
        var utcNow = clock.UtcNow;
        var localNow = clock.LocalNow;
        var localToday = clock.LocalToday;
        var snapshot = clock.CaptureSnapshot();

        // Assert
        Assert.Equal("America/El_Salvador", clock.OperationalTimeZoneId);
        Assert.Equal(TimeSpan.FromHours(-6), clock.OperationalTimeZone.BaseUtcOffset);
        Assert.Equal(TimeSpan.Zero, utcNow.Offset);
        Assert.Equal(TimeSpan.FromHours(-6), localNow.Offset);
        Assert.Equal(DateOnly.FromDateTime(localNow.DateTime), localToday);
        Assert.Equal(TimeSpan.FromHours(-6), snapshot.LocalInstant.Offset);
        Assert.Equal("America/El_Salvador", snapshot.TimeZoneId);
        Assert.Throws<ArgumentException>(() =>
            new OperationalTimeSnapshot(utcNow, localNow, localToday, " "));
    }

    [Fact]
    public void DependencyInjection_RegistersClockOptionsAndPolicyAsSingletons()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.InyectarDependencia(configuration);
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<OperationalTemporalOptions>();
        var clock = provider.GetRequiredService<IOperationalClock>();
        var policy = provider.GetRequiredService<IOperationalAlertPolicy>();

        // Assert
        Assert.Equal("America/El_Salvador", options.OperationalTimeZoneId);
        Assert.IsType<ElSalvadorOperationalClock>(clock);
        Assert.IsType<OperationalAlertPolicy>(policy);
        Assert.Same(options, policy.Options);
    }
}
