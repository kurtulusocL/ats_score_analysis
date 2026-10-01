using ATS.Application.Results;

namespace ATS.Tests
{
    public class OverallScoreExplanationTests
    {
        [Fact]
        public void Build_StatesTheCvQualityAndTheJobFitShares()
        {
            var text = OverallScoreExplanation.Build();

            Assert.Contains("30% CV quality", text);
            Assert.Contains("70% job fit", text);
        }

        [Fact]
        public void Build_TellsTheReaderThatTheSectionScoresDoNotAddUpToTheTotal()
        {
            Assert.Contains("do not add up to the total", OverallScoreExplanation.Build());
        }
    }
}
