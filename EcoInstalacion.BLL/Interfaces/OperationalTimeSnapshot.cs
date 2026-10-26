namespace EcoInstalacion.BLL.Interfaces;

/// <summary>
/// Instantánea inmutable de tiempo operativo capturada en un único instante de referencia.
/// Permite evaluar múltiples solicitudes y visitas bajo el mismo corte temporal.
/// </summary>
public sealed record OperationalTimeSnapshot
{
    public OperationalTimeSnapshot(
        DateTimeOffset utcInstant,
        DateTimeOffset localInstant,
        DateOnly localDate,
        string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new ArgumentException(
                "El identificador de zona horaria del snapshot no puede estar vacío.",
                nameof(timeZoneId));
        }

        UtcInstant = utcInstant.ToUniversalTime();
        LocalInstant = localInstant;
        LocalDate = localDate;
        TimeZoneId = timeZoneId.Trim();
    }

    public DateTimeOffset UtcInstant { get; }

    public DateTimeOffset LocalInstant { get; }

    public DateOnly LocalDate { get; }

    public string TimeZoneId { get; }
}
