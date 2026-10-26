using EcoInstalacion.BLL.Interfaces;

namespace EcoInstalacion.BLL.Implementacion;

/// <summary>
/// Implementación del reloj operativo para la zona horaria America/El_Salvador (UTC-06:00 fijo, sin DST),
/// compatible con entornos Windows (Central America Standard Time) y Linux/IANA (America/El_Salvador).
/// </summary>
public sealed class ElSalvadorOperationalClock : IOperationalClock
{
    public static readonly TimeSpan ElSalvadorUtcOffset = TimeSpan.FromHours(-6);

    private readonly TimeZoneInfo _operationalTimeZone;

    public ElSalvadorOperationalClock()
        : this(new OperationalTemporalOptions())
    {
    }

    public ElSalvadorOperationalClock(OperationalTemporalOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        _operationalTimeZone = ResolveTimeZone(options.OperationalTimeZoneId);
        OperationalTimeZoneId = OperationalTemporalOptions.CanonicalTimeZoneId;
    }

    public string OperationalTimeZoneId { get; }

    public TimeZoneInfo OperationalTimeZone => _operationalTimeZone;

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateTimeOffset LocalNow => ToLocal(DateTimeOffset.UtcNow);

    public DateOnly LocalToday => ToLocalDate(DateTimeOffset.UtcNow);

    public DateTimeOffset ToLocal(DateTimeOffset instant)
    {
        var utcInstant = instant.ToUniversalTime();
        return TimeZoneInfo.ConvertTime(utcInstant, _operationalTimeZone);
    }

    public DateOnly ToLocalDate(DateTimeOffset instant)
    {
        var localInstant = ToLocal(instant);
        return DateOnly.FromDateTime(localInstant.DateTime);
    }

    public OperationalTimeSnapshot CaptureSnapshot() =>
        CreateSnapshot(DateTimeOffset.UtcNow);

    public OperationalTimeSnapshot CreateSnapshot(DateTimeOffset referenceInstant)
    {
        var utcInstant = referenceInstant.ToUniversalTime();
        var localInstant = ToLocal(utcInstant);
        var localDate = DateOnly.FromDateTime(localInstant.DateTime);

        return new OperationalTimeSnapshot(
            utcInstant,
            localInstant,
            localDate,
            OperationalTimeZoneId);
    }

    /// <summary>
    /// Resuelve de forma determinista la zona horaria de El Salvador (UTC-06:00 sin horario de verano),
    /// soportando identificadores IANA y Windows con respaldo fijo si el SO carece de catálogos tzdata.
    /// </summary>
    public static TimeZoneInfo ResolveTimeZone(
        string? configuredTimeZoneId = OperationalTemporalOptions.CanonicalTimeZoneId,
        Func<string, TimeZoneInfo?>? systemTimeZoneResolver = null)
    {
        var options = new OperationalTemporalOptions
        {
            OperationalTimeZoneId = configuredTimeZoneId ?? string.Empty
        };
        options.Validate();

        var resolver = systemTimeZoneResolver ?? TryFindSystemTimeZone;
        var primaryId = options.OperationalTimeZoneId.Trim();
        var secondaryId = string.Equals(
            primaryId,
            OperationalTemporalOptions.CanonicalTimeZoneId,
            StringComparison.OrdinalIgnoreCase)
            ? OperationalTemporalOptions.WindowsEquivalentTimeZoneId
            : OperationalTemporalOptions.CanonicalTimeZoneId;

        var candidate = resolver(primaryId) ?? resolver(secondaryId);
        if (candidate is not null &&
            candidate.BaseUtcOffset == ElSalvadorUtcOffset &&
            !candidate.SupportsDaylightSavingTime)
        {
            return candidate;
        }

        return TimeZoneInfo.CreateCustomTimeZone(
            id: OperationalTemporalOptions.CanonicalTimeZoneId,
            baseUtcOffset: ElSalvadorUtcOffset,
            displayName: "(UTC-06:00) America/El_Salvador",
            standardDisplayName: OperationalTemporalOptions.CanonicalTimeZoneId);
    }

    private static TimeZoneInfo? TryFindSystemTimeZone(string timeZoneId) =>
        TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var tz) ? tz : null;
}
