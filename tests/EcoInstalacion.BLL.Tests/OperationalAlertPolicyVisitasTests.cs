using EcoInstalacion.BLL.Implementacion;
using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.BLL.Tests.Fakes;
using EcoInstalacion.Entity;
using Xunit;

namespace EcoInstalacion.BLL.Tests;

public sealed class OperationalAlertPolicyVisitasTests
{
    private static readonly DateOnly ScheduledDate = new(2026, 10, 21);

    [Fact]
    public void EvaluarVisita_DayBeforeScheduledDate_ReturnsNormal()
    {
        // Arrange: 2026-10-20 23:59:59.9999999 en America/El_Salvador (05:59:59.9999999Z del 2026-10-21)
        var referenceBeforeMidnight = new DateTimeOffset(2026, 10, 20, 23, 59, 59, TimeSpan.FromHours(-6))
            .AddTicks(9_999_999);
        var clock = new FakeOperationalClock(referenceBeforeMidnight);
        var policy = new OperationalAlertPolicy(clock);

        // Act
        var resultado = policy.EvaluarVisita(
            ScheduledDate,
            aprobado: false,
            EstadosSolicitudSistema.Programada,
            referenceBeforeMidnight);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Normal, resultado.Nivel);
        Assert.True(resultado.EsElegible);
        Assert.Equal(new DateOnly(2026, 10, 20), resultado.FechaReferenciaLocal);
        Assert.Equal(-1, resultado.DiasDiferenciaCalendario);
    }

    [Fact]
    public void EvaluarVisita_ExactStartOfScheduledDay_ReturnsPreventiva()
    {
        // Arrange: 2026-10-21 00:00:00.0000000 en America/El_Salvador (06:00:00.0000000Z del 2026-10-21)
        var referenceStartOfDay = new DateTimeOffset(2026, 10, 21, 0, 0, 0, TimeSpan.FromHours(-6));
        var clock = new FakeOperationalClock(referenceStartOfDay);
        var policy = new OperationalAlertPolicy(clock);

        // Act
        var resultado = policy.EvaluarVisita(
            ScheduledDate,
            aprobado: false,
            EstadosSolicitudSistema.Programada,
            referenceStartOfDay);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Preventiva, resultado.Nivel);
        Assert.True(resultado.EsElegible);
        Assert.Equal(ScheduledDate, resultado.FechaReferenciaLocal);
        Assert.Equal(0, resultado.DiasDiferenciaCalendario);
    }

    [Fact]
    public void EvaluarVisita_LastInstantOfScheduledDay_ReturnsPreventivaRegardlessOfScheduledHour()
    {
        // Arrange: 2026-10-21 23:59:59.9999999 en America/El_Salvador (05:59:59.9999999Z del 2026-10-22)
        // Aunque la visita estuviera agendada a las 08:00 AM de ese día, sigue siendo Preventiva durante todo el día local.
        var scheduledMorningInstant = new DateTimeOffset(2026, 10, 21, 8, 0, 0, TimeSpan.FromHours(-6));
        var referenceLastInstantOfDay = new DateTimeOffset(2026, 10, 21, 23, 59, 59, TimeSpan.FromHours(-6))
            .AddTicks(9_999_999);
        var clock = new FakeOperationalClock(referenceLastInstantOfDay);
        var policy = new OperationalAlertPolicy(clock);

        // Act
        var resultado = policy.EvaluarVisita(
            scheduledMorningInstant,
            aprobado: false,
            EstadosSolicitudSistema.Programada,
            referenceLastInstantOfDay);
        var nivelDirecto = policy.ClasificarVisita(
            scheduledMorningInstant,
            aprobado: false,
            EstadosSolicitudSistema.Programada,
            referenceLastInstantOfDay);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Preventiva, resultado.Nivel);
        Assert.Equal(NivelAlertaOperativa.Preventiva, nivelDirecto);
        Assert.True(resultado.EsElegible);
        Assert.Equal(0, resultado.DiasDiferenciaCalendario);
    }

    [Fact]
    public void EvaluarVisita_ExactStartOfNextDay_ReturnsAtrasada()
    {
        // Arrange: 2026-10-22 00:00:00.0000000 en America/El_Salvador (06:00:00.0000000Z del 2026-10-22)
        var referenceStartOfNextDay = new DateTimeOffset(2026, 10, 22, 0, 0, 0, TimeSpan.FromHours(-6));
        var clock = new FakeOperationalClock(referenceStartOfNextDay);
        var policy = new OperationalAlertPolicy(clock);

        // Act
        var resultado = policy.EvaluarVisita(
            ScheduledDate,
            aprobado: false,
            EstadosSolicitudSistema.Programada,
            referenceStartOfNextDay);
        var nivelDirecto = policy.ClasificarVisita(
            ScheduledDate,
            aprobado: false,
            EstadosSolicitudSistema.Programada,
            referenceStartOfNextDay);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Atrasada, resultado.Nivel);
        Assert.Equal(NivelAlertaOperativa.Atrasada, nivelDirecto);
        Assert.True(resultado.EsElegible);
        Assert.Equal(1, resultado.DiasDiferenciaCalendario);
    }

    [Fact]
    public void EvaluarVisita_ApprovedVisit_ReturnsNormalAndNotEligible()
    {
        // Arrange
        var referencePastDue = new DateTimeOffset(2026, 10, 25, 10, 0, 0, TimeSpan.FromHours(-6));
        var clock = new FakeOperationalClock(referencePastDue);
        var policy = new OperationalAlertPolicy(clock);

        // Act
        var resultado = policy.EvaluarVisita(
            ScheduledDate,
            aprobado: true,
            EstadosSolicitudSistema.EnProceso,
            referencePastDue);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Normal, resultado.Nivel);
        Assert.False(resultado.EsElegible);
    }

    [Theory]
    [InlineData(0, NivelAlertaOperativa.Preventiva)]
    [InlineData(1, NivelAlertaOperativa.Atrasada)]
    public void EvaluarVisita_NullAprobado_IsTreatedAsUnapproved(
        int daysAfterScheduled,
        NivelAlertaOperativa expectedLevel)
    {
        // Arrange
        var referenceDate = ScheduledDate.AddDays(daysAfterScheduled);
        var referenceInstant = new DateTimeOffset(
            referenceDate.ToDateTime(new TimeOnly(9, 30)),
            TimeSpan.FromHours(-6));
        var clock = new FakeOperationalClock(referenceInstant);
        var policy = new OperationalAlertPolicy(clock);

        // Act
        var resultado = policy.EvaluarVisita(
            ScheduledDate,
            aprobado: null,
            EstadosSolicitudSistema.Pendiente,
            referenceInstant);

        // Assert
        Assert.Equal(expectedLevel, resultado.Nivel);
        Assert.True(resultado.EsElegible);
    }

    [Fact]
    public void EvaluarVisita_ClosedSolicitud_ReturnsNormalAndNotEligible()
    {
        // Arrange
        var referencePastDue = new DateTimeOffset(2026, 10, 25, 10, 0, 0, TimeSpan.FromHours(-6));
        var clock = new FakeOperationalClock(referencePastDue);
        var policy = new OperationalAlertPolicy(clock);

        // Act
        var resultado = policy.EvaluarVisita(
            ScheduledDate,
            aprobado: false,
            EstadosSolicitudSistema.Cerrada,
            referencePastDue);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Normal, resultado.Nivel);
        Assert.False(resultado.EsElegible);
        Assert.Equal(EstadosSolicitudSistema.Cerrada, resultado.EstadoSolicitudNormalizado);
    }

    [Fact]
    public void EvaluarVisita_RejectedSolicitud_ReturnsNormalAndNotEligible()
    {
        // Arrange
        var referencePastDue = new DateTimeOffset(2026, 10, 25, 10, 0, 0, TimeSpan.FromHours(-6));
        var clock = new FakeOperationalClock(referencePastDue);
        var policy = new OperationalAlertPolicy(clock);

        // Act
        var resultado = policy.EvaluarVisita(
            ScheduledDate,
            aprobado: null,
            EstadosSolicitudSistema.Rechazada,
            referencePastDue);

        // Assert
        Assert.Equal(NivelAlertaOperativa.Normal, resultado.Nivel);
        Assert.False(resultado.EsElegible);
        Assert.Equal(EstadosSolicitudSistema.Rechazada, resultado.EstadoSolicitudNormalizado);
    }
}
