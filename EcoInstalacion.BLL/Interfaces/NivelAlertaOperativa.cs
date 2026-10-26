namespace EcoInstalacion.BLL.Interfaces;

/// <summary>
/// Niveles canónicos de severidad para alertas operativas de solicitudes y visitas técnicas.
/// </summary>
public enum NivelAlertaOperativa
{
    Normal = 0,
    Preventiva = 1,
    Atrasada = 2
}

/// <summary>
/// Resultado determinista de la evaluación de antigüedad de una solicitud de instalación.
/// </summary>
public sealed record EvaluacionAlertaSolicitud(
    NivelAlertaOperativa Nivel,
    string EstadoNormalizado,
    bool EsElegible,
    TimeSpan Antiguedad,
    bool EsFechaFutura);

/// <summary>
/// Resultado determinista de la evaluación por fecha calendario local de una visita técnica.
/// </summary>
public sealed record EvaluacionAlertaVisita(
    NivelAlertaOperativa Nivel,
    string EstadoSolicitudNormalizado,
    bool EsElegible,
    DateOnly FechaProgramadaLocal,
    DateOnly FechaReferenciaLocal,
    int DiasDiferenciaCalendario);
