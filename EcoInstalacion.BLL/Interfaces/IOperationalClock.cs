namespace EcoInstalacion.BLL.Interfaces;

/// <summary>
/// Fuente de tiempo inyectable para cálculos operativos en la zona horaria America/El_Salvador.
/// </summary>
public interface IOperationalClock
{
    string OperationalTimeZoneId { get; }

    TimeZoneInfo OperationalTimeZone { get; }

    DateTimeOffset UtcNow { get; }

    DateTimeOffset LocalNow { get; }

    DateOnly LocalToday { get; }

    DateTimeOffset ToLocal(DateTimeOffset instant);

    DateOnly ToLocalDate(DateTimeOffset instant);

    OperationalTimeSnapshot CaptureSnapshot();

    OperationalTimeSnapshot CreateSnapshot(DateTimeOffset referenceInstant);
}
