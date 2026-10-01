using ATS.Application.Analyzers.JobMatching;

namespace ATS.Tests
{
    public class JobMatchScoringRulesTests
    {
        [Fact]
        public void IsPassing_PassesAtExactlyHalfOfTheMaximum_AndFailsBelowIt()
        {
            Assert.True(JobMatchScoringRules.IsPassing(10, 20));
            Assert.False(JobMatchScoringRules.IsPassing(9, 20));
        }

        [Fact]
        public void IsPassing_FollowsTheMaximumInsteadOfAFixedScore()
        {
            Assert.True(JobMatchScoringRules.IsPassing(5, 10));
            Assert.False(JobMatchScoringRules.IsPassing(4, 10));
        }

        [Fact]
        public void IsPassing_FailsAZeroScore()
        {
            Assert.False(JobMatchScoringRules.IsPassing(0, 20));
        }
    }
}
