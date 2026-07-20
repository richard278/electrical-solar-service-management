namespace EcoInstalacion.Entity;

/// <summary>
/// Catálogo canónico de estados de una solicitud de instalación.
/// El valor de <see cref="EnProceso"/> debe permanecer alineado con el trigger
/// dbo.tr_ActualizarEstadoSolicitud.
/// </summary>
public static class EstadosSolicitudSistema
{
    public const string Pendiente = "Pendiente";
    public const string Programada = "Programada";
    public const string EnProceso = "En proceso";
    public const string Cerrada = "Cerrada";
    public const string Rechazada = "Rechazada";

    public static readonly IReadOnlyList<string> Todos =
        new[] { Pendiente, Programada, EnProceso, Cerrada, Rechazada };

    public static bool TryNormalizar(string? valor, out string estadoNormalizado)
    {
        var estado = valor?.Trim();

        foreach (var estadoCanonico in Todos)
        {
            if (string.Equals(estado, estadoCanonico, StringComparison.OrdinalIgnoreCase))
            {
                estadoNormalizado = estadoCanonico;
                return true;
            }
        }

        estadoNormalizado = string.Empty;
        return false;
    }

    public static string NormalizarRequerido(string? valor)
    {
        if (TryNormalizar(valor, out var estadoNormalizado))
            return estadoNormalizado;

        throw new ArgumentException("El estado de la solicitud no es válido.");
    }

    public static bool EsTerminal(string? valor) =>
        TryNormalizar(valor, out var estadoNormalizado) &&
        estadoNormalizado is Cerrada or Rechazada;
}
