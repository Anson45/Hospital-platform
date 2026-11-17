namespace PharmacyBilling.Tests.TestDoubles;

/// <summary>
/// A clock that always returns the same instant, so stock and expiry rules are deterministic.
/// </summary>
public sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FixedTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;
}
