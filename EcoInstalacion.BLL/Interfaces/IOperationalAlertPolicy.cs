namespace EcoInstalacion.BLL.Interfaces;

/// <summary>
/// Contrato de la política compartida de clasificación de alertas operativas.
/// Opera de forma pura y determinista sin acoplarse a persistencia ni al reloj global del sistema.
/// </summary>
public interface IOperationalAlertPolicy
{
    OperationalTemporalOptions Options { get; }

    EvaluacionAlertaSolicitud EvaluarSolicitud(
        string? estadoSolicitud,
        DateTimeOffset fechaSolicitud,
        OperationalTimeSnapshot snapshot);

    EvaluacionAlertaSolicitud EvaluarSolicitud(
        string? estadoSolicitud,
        DateTimeOffset fechaSolicitud,
        DateTimeOffset instanteReferencia);

    NivelAlertaOperativa ClasificarSolicitud(
        string? estadoSolicitud,
        DateTimeOffset fechaSolicitud,
        DateTimeOffset instanteReferencia);

    EvaluacionAlertaVisita EvaluarVisita(
        DateOnly fechaProgramadaLocal,
        bool? aprobado,
        string? estadoSolicitud,
        OperationalTimeSnapshot snapshot);

    EvaluacionAlertaVisita EvaluarVisita(
        DateOnly fechaProgramadaLocal,
        bool? aprobado,
        string? estadoSolicitud,
        DateTimeOffset instanteReferencia);

    EvaluacionAlertaVisita EvaluarVisita(
        DateTimeOffset instanteVisitaProgramada,
        bool? aprobado,
        string? estadoSolicitud,
        DateTimeOffset instanteReferencia);

    NivelAlertaOperativa ClasificarVisita(
        DateOnly fechaProgramadaLocal,
        bool? aprobado,
        string? estadoSolicitud,
        DateTimeOffset instanteReferencia);

    NivelAlertaOperativa ClasificarVisita(
        DateTimeOffset instanteVisitaProgramada,
        bool? aprobado,
        string? estadoSolicitud,
        DateTimeOffset instanteReferencia);

    DateTimeOffset FromSpecifiedDateTime(DateTime dateTime);
}
