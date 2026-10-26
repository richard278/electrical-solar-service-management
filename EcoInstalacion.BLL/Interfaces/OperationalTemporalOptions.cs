namespace EcoInstalacion.BLL.Interfaces;

/// <summary>
/// Configuración de la política temporal operativa y de umbrales del dashboard.
/// </summary>
public sealed class OperationalTemporalOptions
{
    public const string CanonicalTimeZoneId = "America/El_Salvador";
    public const string WindowsEquivalentTimeZoneId = "Central America Standard Time";
    public const int DefaultWarningHours = 24;
    public const int DefaultOverdueHours = 48;
    public const int DefaultPreviewLimit = 5;
    public const int DefaultFallbackRefreshMinutes = 15;

    public string OperationalTimeZoneId { get; init; } = CanonicalTimeZoneId;

    public int WarningHours { get; init; } = DefaultWarningHours;

    public int OverdueHours { get; init; } = DefaultOverdueHours;

    public int PreviewLimit { get; init; } = DefaultPreviewLimit;

    public int FallbackRefreshMinutes { get; init; } = DefaultFallbackRefreshMinutes;

    public TimeSpan WarningThreshold => TimeSpan.FromHours(WarningHours);

    public TimeSpan OverdueThreshold => TimeSpan.FromHours(OverdueHours);

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(OperationalTimeZoneId))
        {
            throw new ArgumentException(
                "La zona horaria operativa es obligatoria y debe configurarse como 'America/El_Salvador'.",
                nameof(OperationalTimeZoneId));
        }

        var normalizedZone = OperationalTimeZoneId.Trim();
        if (!string.Equals(normalizedZone, CanonicalTimeZoneId, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(normalizedZone, WindowsEquivalentTimeZoneId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"La zona horaria operativa '{OperationalTimeZoneId}' no es válida. Se requiere '{CanonicalTimeZoneId}'.",
                nameof(OperationalTimeZoneId));
        }

        if (WarningHours <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(WarningHours),
                WarningHours,
                "El umbral preventivo en horas (WarningHours) debe ser mayor que cero.");
        }

        if (OverdueHours <= WarningHours)
        {
            throw new ArgumentOutOfRangeException(
                nameof(OverdueHours),
                OverdueHours,
                "El umbral de atraso (OverdueHours) debe ser estrictamente mayor que el umbral preventivo (WarningHours).");
        }

        if (PreviewLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PreviewLimit),
                PreviewLimit,
                "El límite de vista previa (PreviewLimit) debe ser mayor que cero.");
        }

        if (FallbackRefreshMinutes <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(FallbackRefreshMinutes),
                FallbackRefreshMinutes,
                "El intervalo de refresco de respaldo (FallbackRefreshMinutes) debe ser mayor que cero.");
        }
    }
}
