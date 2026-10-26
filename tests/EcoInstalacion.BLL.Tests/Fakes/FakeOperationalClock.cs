using EcoInstalacion.BLL.Implementacion;
using EcoInstalacion.BLL.Interfaces;

namespace EcoInstalacion.BLL.Tests.Fakes;

/// <summary>
/// Reloj operativo falso y determinista para pruebas unitarias.
/// No consulta el reloj del sistema operativo.
/// </summary>
public sealed class FakeOperationalClock : IOperationalClock
{
    private readonly TimeZoneInfo _operationalTimeZone;
    private DateTimeOffset _currentUtcInstant;

    public FakeOperationalClock(DateTimeOffset initialInstant)
    {
        _operationalTimeZone = ElSalvadorOperationalClock.ResolveTimeZone(
            OperationalTemporalOptions.CanonicalTimeZoneId);
        _currentUtcInstant = initialInstant.ToUniversalTime();
    }

    public string OperationalTimeZoneId => OperationalTemporalOptions.CanonicalTimeZoneId;

    public TimeZoneInfo OperationalTimeZone => _operationalTimeZone;

    public DateTimeOffset UtcNow => _currentUtcInstant;

    public DateTimeOffset LocalNow => ToLocal(_currentUtcInstant);

    public DateOnly LocalToday => ToLocalDate(_currentUtcInstant);

    public void SetInstant(DateTimeOffset instant)
    {
        _currentUtcInstant = instant.ToUniversalTime();
    }

    public void Advance(TimeSpan delta)
    {
        _currentUtcInstant = _currentUtcInstant.Add(delta);
    }

    public DateTimeOffset ToLocal(DateTimeOffset instant)
    {
        return TimeZoneInfo.ConvertTime(instant.ToUniversalTime(), _operationalTimeZone);
    }

    public DateOnly ToLocalDate(DateTimeOffset instant)
    {
        var local = ToLocal(instant);
        return DateOnly.FromDateTime(local.DateTime);
    }

    public OperationalTimeSnapshot CaptureSnapshot() =>
        CreateSnapshot(_currentUtcInstant);

    public OperationalTimeSnapshot CreateSnapshot(DateTimeOffset referenceInstant)
    {
        var utc = referenceInstant.ToUniversalTime();
        var local = ToLocal(utc);
        var localDate = DateOnly.FromDateTime(local.DateTime);

        return new OperationalTimeSnapshot(
            utc,
            local,
            localDate,
            OperationalTimeZoneId);
    }
}
