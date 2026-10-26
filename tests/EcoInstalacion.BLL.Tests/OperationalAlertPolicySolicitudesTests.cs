using EcoInstalacion.BLL.Implementacion;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.BLL.Tests.Fakes;
using EcoInstalacion.Entity;
using Xunit;

namespace EcoInstalacion.BLL.Tests;

public sealed class OperationalAlertPolicySolicitudesTests
{
    private static readonly DateTimeOffset ReferenceNow =
        new(2026, 10, 21, 14, 0, 0, TimeSpan.FromHours(-6));

    [Fact]
    public void EvaluarSolicitud_InstantBefore24Hours_ReturnsNormal()
    {
        // Arrange
        var clock = new FakeOperationalClock(ReferenceNow);
        var policy = new OperationalAlertPolicy(clock);
        var fechaSolicitud = ReferenceNow.AddHours(-24).AddTicks(1);

        // Act
        var resultado = policy.EvaluarSolicitud(
            EstadosSolicitudSistema.Pendiente,
            fechaSolicitud,
            ReferenceNow);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Normal, resultado.Nivel);
        Assert.True(resultado.EsElegible);
        Assert.False(resultado.EsFechaFutura);
        Assert.Equal(TimeSpan.FromHours(24) - TimeSpan.FromTicks(1), resultado.Antiguedad);
    }

    [Fact]
    public void EvaluarSolicitud_Exactly24Hours_ReturnsPreventiva()
    {
        // Arrange
        var clock = new FakeOperationalClock(ReferenceNow);
        var policy = new OperationalAlertPolicy(clock);
        var fechaSolicitud = ReferenceNow.AddHours(-24);

        // Act
        var resultado = policy.EvaluarSolicitud(
            EstadosSolicitudSistema.Pendiente,
            fechaSolicitud,
            ReferenceNow);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Preventiva, resultado.Nivel);
        Assert.True(resultado.EsElegible);
        Assert.False(resultado.EsFechaFutura);
        Assert.Equal(TimeSpan.FromHours(24), resultado.Antiguedad);
    }

    [Fact]
    public void EvaluarSolicitud_InstantBefore48Hours_ReturnsPreventiva()
    {
        // Arrange
        var clock = new FakeOperationalClock(ReferenceNow);
        var policy = new OperationalAlertPolicy(clock);
        var fechaSolicitud = ReferenceNow.AddHours(-48).AddTicks(1);

        // Act
        var resultado = policy.EvaluarSolicitud(
            EstadosSolicitudSistema.Pendiente,
            fechaSolicitud,
            ReferenceNow);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Preventiva, resultado.Nivel);
        Assert.True(resultado.EsElegible);
        Assert.False(resultado.EsFechaFutura);
        Assert.Equal(TimeSpan.FromHours(48) - TimeSpan.FromTicks(1), resultado.Antiguedad);
    }

    [Fact]
    public void EvaluarSolicitud_Exactly48Hours_ReturnsAtrasada()
    {
        // Arrange
        var clock = new FakeOperationalClock(ReferenceNow);
        var policy = new OperationalAlertPolicy(clock);
        var fechaSolicitud = ReferenceNow.AddHours(-48);

        // Act
        var resultado = policy.EvaluarSolicitud(
            EstadosSolicitudSistema.Pendiente,
            fechaSolicitud,
            ReferenceNow);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Atrasada, resultado.Nivel);
        Assert.True(resultado.EsElegible);
        Assert.False(resultado.EsFechaFutura);
        Assert.Equal(TimeSpan.FromHours(48), resultado.Antiguedad);
    }

    [Theory]
    [InlineData(48.001)]
    [InlineData(72)]
    [InlineData(168)]
    public void EvaluarSolicitud_MoreThan48Hours_ReturnsAtrasada(double elapsedHours)
    {
        // Arrange
        var clock = new FakeOperationalClock(ReferenceNow);
        var policy = new OperationalAlertPolicy(clock);
        var fechaSolicitud = ReferenceNow - TimeSpan.FromHours(elapsedHours);

        // Act
        var nivel = policy.ClasificarSolicitud(
            EstadosSolicitudSistema.Pendiente,
            fechaSolicitud,
            ReferenceNow);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Atrasada, nivel);
    }

    [Theory]
    [InlineData(EstadosSolicitudSistema.Programada)]
    [InlineData(EstadosSolicitudSistema.EnProceso)]
    [InlineData(EstadosSolicitudSistema.Cerrada)]
    [InlineData(EstadosSolicitudSistema.Rechazada)]
    [InlineData(" programada ")]
    [InlineData("EN PROCESO")]
    public void EvaluarSolicitud_NonEligibleStates_NeverGenerateAlertEvenAfter72Hours(string estado)
    {
        // Arrange
        var clock = new FakeOperationalClock(ReferenceNow);
        var policy = new OperationalAlertPolicy(clock);
        var fechaSolicitud = ReferenceNow.AddHours(-72);

        // Act
        var resultado = policy.EvaluarSolicitud(estado, fechaSolicitud, ReferenceNow);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Normal, resultado.Nivel);
        Assert.False(resultado.EsElegible);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("En revisión")]
    public void EvaluarSolicitud_InvalidState_ThrowsArgumentException(string? invalidState)
    {
        // Arrange
        var clock = new FakeOperationalClock(ReferenceNow);
        var policy = new OperationalAlertPolicy(clock);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            policy.EvaluarSolicitud(invalidState, ReferenceNow, ReferenceNow));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3600)]
    [InlineData(86400)]
    public void EvaluarSolicitud_ExplicitFutureTimestamps_ReturnNormalWithFutureFlag(int secondsInFuture)
    {
        // Arrange
        var clock = new FakeOperationalClock(ReferenceNow);
        var policy = new OperationalAlertPolicy(clock);
        var fechaFutura = ReferenceNow.AddSeconds(secondsInFuture);

        // Act
        var resultado = policy.EvaluarSolicitud(
            EstadosSolicitudSistema.Pendiente,
            fechaFutura,
            ReferenceNow);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Normal, resultado.Nivel);
        Assert.True(resultado.EsElegible);
        Assert.True(resultado.EsFechaFutura);
        Assert.Equal(TimeSpan.Zero, resultado.Antiguedad);
    }

    [Fact]
    public void FromSpecifiedDateTime_RejectsUnspecifiedAndLocalDateTime_AndConvertsUtcExplicitly()
    {
        // Arrange
        var clock = new FakeOperationalClock(ReferenceNow);
        var policy = new OperationalAlertPolicy(clock);
        var unspecified = new DateTime(2026, 10, 21, 10, 0, 0, DateTimeKind.Unspecified);
        var local = new DateTime(2026, 10, 21, 10, 0, 0, DateTimeKind.Local);
        var utc = new DateTime(2026, 10, 21, 16, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => policy.FromSpecifiedDateTime(unspecified));
        Assert.Throws<ArgumentException>(() => policy.FromSpecifiedDateTime(local));

        var convertedFromUtc = policy.FromSpecifiedDateTime(utc);
        Assert.Equal(TimeSpan.FromHours(-6), convertedFromUtc.Offset);
        Assert.Equal(new DateTime(2026, 10, 21, 10, 0, 0), convertedFromUtc.DateTime);
    }
}
