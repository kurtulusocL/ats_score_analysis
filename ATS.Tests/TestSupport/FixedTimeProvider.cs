

namespace ATS.Tests.TestSupport
{
    public sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTime utcNow)
        {
            _utcNow = new DateTimeOffset(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
