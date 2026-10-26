using EcoInstalacion.BLL.Interfaces;
using EcoInstalacion.Entity;

namespace EcoInstalacion.BLL.Implementacion;

/// <summary>
/// Política pura y determinista de clasificación de alertas operativas para Solicitudes y Visitas Técnicas.
/// </summary>
public sealed class OperationalAlertPolicy : IOperationalAlertPolicy
{
    private readonly IOperationalClock _clock;
    private readonly OperationalTemporalOptions _options;

    public OperationalAlertPolicy(IOperationalClock clock)
        : this(clock, new OperationalTemporalOptions())
    {
    }

    public OperationalAlertPolicy(IOperationalClock clock, OperationalTemporalOptions options)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        _clock = clock;
        _options = options;
    }

    public OperationalTemporalOptions Options => _options;

    public EvaluacionAlertaSolicitud EvaluarSolicitud(
        string? estadoSolicitud,
        DateTimeOffset fechaSolicitud,
        OperationalTimeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return EvaluarSolicitud(estadoSolicitud, fechaSolicitud, snapshot.LocalInstant);
    }

    public EvaluacionAlertaSolicitud EvaluarSolicitud(
        string? estadoSolicitud,
        DateTimeOffset fechaSolicitud,
        DateTimeOffset instanteReferencia)
    {
        var estadoNormalizado = EstadosSolicitudSistema.NormalizarRequerido(estadoSolicitud);

        var fechaSolicitudLocal = _clock.ToLocal(fechaSolicitud);
        var referenciaLocal = _clock.ToLocal(instanteReferencia);
        var transcurrido = referenciaLocal - fechaSolicitudLocal;
        var esFechaFutura = transcurrido < TimeSpan.Zero;
        var antiguedadEfectiva = esFechaFutura ? TimeSpan.Zero : transcurrido;

        if (!string.Equals(
                estadoNormalizado,
                EstadosSolicitudSistema.Pendiente,
                StringComparison.Ordinal))
        {
            return new EvaluacionAlertaSolicitud(
                Nivel: NivelAlertaOperativa.Normal,
                EstadoNormalizado: estadoNormalizado,
                EsElegible: false,
                Antiguedad: antiguedadEfectiva,
                EsFechaFutura: esFechaFutura);
        }

        if (esFechaFutura || antiguedadEfectiva < _options.WarningThreshold)
        {
            return new EvaluacionAlertaSolicitud(
                Nivel: NivelAlertaOperativa.Normal,
                EstadoNormalizado: estadoNormalizado,
                EsElegible: true,
                Antiguedad: antiguedadEfectiva,
                EsFechaFutura: esFechaFutura);
        }

        if (antiguedadEfectiva < _options.OverdueThreshold)
        {
            return new EvaluacionAlertaSolicitud(
                Nivel: NivelAlertaOperativa.Preventiva,
                EstadoNormalizado: estadoNormalizado,
                EsElegible: true,
                Antiguedad: antiguedadEfectiva,
                EsFechaFutura: false);
        }

        return new EvaluacionAlertaSolicitud(
            Nivel: NivelAlertaOperativa.Atrasada,
            EstadoNormalizado: estadoNormalizado,
            EsElegible: true,
            Antiguedad: antiguedadEfectiva,
            EsFechaFutura: false);
    }

    public NivelAlertaOperativa ClasificarSolicitud(
        string? estadoSolicitud,
        DateTimeOffset fechaSolicitud,
        DateTimeOffset instanteReferencia) =>
        EvaluarSolicitud(estadoSolicitud, fechaSolicitud, instanteReferencia).Nivel;

    public EvaluacionAlertaVisita EvaluarVisita(
        DateOnly fechaProgramadaLocal,
        bool? aprobado,
        string? estadoSolicitud,
        OperationalTimeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return EvaluarVisitaPorFechaLocal(
            fechaProgramadaLocal,
            aprobado,
            estadoSolicitud,
            snapshot.LocalDate);
    }

    public EvaluacionAlertaVisita EvaluarVisita(
        DateOnly fechaProgramadaLocal,
        bool? aprobado,
        string? estadoSolicitud,
        DateTimeOffset instanteReferencia)
    {
        var hoyLocal = _clock.ToLocalDate(instanteReferencia);
        return EvaluarVisitaPorFechaLocal(
            fechaProgramadaLocal,
            aprobado,
            estadoSolicitud,
            hoyLocal);
    }

    public EvaluacionAlertaVisita EvaluarVisita(
        DateTimeOffset instanteVisitaProgramada,
        bool? aprobado,
        string? estadoSolicitud,
        DateTimeOffset instanteReferencia)
    {
        var fechaProgramadaLocal = _clock.ToLocalDate(instanteVisitaProgramada);
        var hoyLocal = _clock.ToLocalDate(instanteReferencia);
        return EvaluarVisitaPorFechaLocal(
            fechaProgramadaLocal,
            aprobado,
            estadoSolicitud,
            hoyLocal);
    }

    public NivelAlertaOperativa ClasificarVisita(
        DateOnly fechaProgramadaLocal,
        bool? aprobado,
        string? estadoSolicitud,
        DateTimeOffset instanteReferencia) =>
        EvaluarVisita(fechaProgramadaLocal, aprobado, estadoSolicitud, instanteReferencia).Nivel;

    public NivelAlertaOperativa ClasificarVisita(
        DateTimeOffset instanteVisitaProgramada,
        bool? aprobado,
        string? estadoSolicitud,
        DateTimeOffset instanteReferencia) =>
        EvaluarVisita(instanteVisitaProgramada, aprobado, estadoSolicitud, instanteReferencia).Nivel;

    public DateTimeOffset FromSpecifiedDateTime(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException(
                "No se permite convertir implícitamente valores DateTime con DateTimeKind.Unspecified. Especifique explícitamente el origen temporal antes de evaluar la política.",
                nameof(dateTime));
        }

        if (dateTime.Kind == DateTimeKind.Local)
        {
            throw new ArgumentException(
                "No se permite convertir valores DateTime con DateTimeKind.Local porque dependen del huso horario del host. Suministre un DateTimeOffset con su offset explícito o un DateTime en UTC.",
                nameof(dateTime));
        }

        return _clock.ToLocal(new DateTimeOffset(dateTime, TimeSpan.Zero));
    }

    private static EvaluacionAlertaVisita EvaluarVisitaPorFechaLocal(
        DateOnly fechaProgramadaLocal,
        bool? aprobado,
        string? estadoSolicitud,
        DateOnly hoyLocal)
    {
        var estadoNormalizado = EstadosSolicitudSistema.NormalizarRequerido(estadoSolicitud);
        var diasDiferencia = hoyLocal.DayNumber - fechaProgramadaLocal.DayNumber;
        var estaAprobada = aprobado == true;

        if (estaAprobada || EstadosSolicitudSistema.EsTerminal(estadoNormalizado))
        {
            return new EvaluacionAlertaVisita(
                Nivel: NivelAlertaOperativa.Normal,
                EstadoSolicitudNormalizado: estadoNormalizado,
                EsElegible: false,
                FechaProgramadaLocal: fechaProgramadaLocal,
                FechaReferenciaLocal: hoyLocal,
                DiasDiferenciaCalendario: diasDiferencia);
        }

        if (fechaProgramadaLocal > hoyLocal)
        {
            return new EvaluacionAlertaVisita(
                Nivel: NivelAlertaOperativa.Normal,
                EstadoSolicitudNormalizado: estadoNormalizado,
                EsElegible: true,
                FechaProgramadaLocal: fechaProgramadaLocal,
                FechaReferenciaLocal: hoyLocal,
                DiasDiferenciaCalendario: diasDiferencia);
        }

        if (fechaProgramadaLocal == hoyLocal)
        {
            return new EvaluacionAlertaVisita(
                Nivel: NivelAlertaOperativa.Preventiva,
                EstadoSolicitudNormalizado: estadoNormalizado,
                EsElegible: true,
                FechaProgramadaLocal: fechaProgramadaLocal,
                FechaReferenciaLocal: hoyLocal,
                DiasDiferenciaCalendario: 0);
        }

        return new EvaluacionAlertaVisita(
            Nivel: NivelAlertaOperativa.Atrasada,
            EstadoSolicitudNormalizado: estadoNormalizado,
            EsElegible: true,
            FechaProgramadaLocal: fechaProgramadaLocal,
            FechaReferenciaLocal: hoyLocal,
            DiasDiferenciaCalendario: diasDiferencia);
    }
}
