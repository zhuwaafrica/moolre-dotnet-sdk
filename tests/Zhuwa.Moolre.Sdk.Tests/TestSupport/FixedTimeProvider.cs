namespace Zhuwa.Moolre;

/// <summary>A clock stopped at one instant, so tests that compare timestamps with "now" give the same answer on every run.</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
