using System.Globalization;

namespace ATS.Application.Results
{
    public static class OverallScoreExplanation
    {
        public static string Build()
        {
            var jobFitPercentage = OverallScoreCalculator.JobMatchWeight * 100;
            var cvQualityPercentage = 100 - jobFitPercentage;

            return string.Format(
                CultureInfo.InvariantCulture,
                "Total score = {0:0}% CV quality + {1:0}% job fit. The section scores are shown as each analyzer calculated them; they do not add up to the total.",
                cvQualityPercentage, jobFitPercentage);
        }
    }
}
