using ATS.Application.Reporting;
using ATS.Domain.Entities;
using ATS.Domain.Enums;

namespace ATS.Tests
{
    public class RequirementReportBuilderTests
    {
        private static RequirementMatch CreateMatch(
            string name, bool isMandatory, MatchStatus status, double similarity = 0.9, RequirementInterpretation? interpretation = null, int id = 0) =>
            new()
            {
                Id = id,
                Similarity = similarity,
                Status = status,
                JobRequirement = new JobRequirement { Name = name, IsMandatory = isMandatory, Category = "Skill" },
                RequirementInterpretation = interpretation
            };

        private static RequirementInterpretation CreateInterpretation(MatchStatus status, string modelIdentity = "Ollama:test-model") => new()
        {
            Status = status,
            EvidenceQuote = status == MatchStatus.Missing ? null : "quote",
            Explanation = "Because.",
            Suggestion = "Do more.",
            ModelIdentity = modelIdentity
        };

        private static CvScan CreateScan(params RequirementMatch[] requirementMatches)
        {
            var cvScan = new CvScan();
            foreach (var requirementMatch in requirementMatches)
                cvScan.RequirementMatches.Add(requirementMatch);

            return cvScan;
        }

        private static CvScan CreateScanWithScores(
            int? deterministicScore, double? semanticScore, int? hybridScore, bool withJobMatchSection = true, bool withAudit = true)
        {
            var cvScan = CreateScan(CreateMatch("SQL Server", true, MatchStatus.Met));

            if (withJobMatchSection)
                cvScan.SectionScores.Add(new SectionScore { SectionName = "Job Match", Score = 10, MaxScore = 20 });

            if (withAudit)
            {
                cvScan.AnalysisAudit = new AnalysisAudit
                {
                    DeterministicJobMatchScore = deterministicScore,
                    SemanticJobMatchScore = semanticScore,
                    HybridJobMatchScore = hybridScore
                };
            }

            return cvScan;
        }

        [Fact]
        public void Build_ReturnsAnEmptyReport_WhenThereAreNoRequirementMatches()
        {
            var report = RequirementReportBuilder.Build(new CvScan());

            Assert.False(report.HasRows);
            Assert.Empty(report.Rows);
            Assert.Null(report.ScoreComponentsText);
            Assert.Null(report.ModelIdentity);
        }

        [Fact]
        public void Build_OrdersMandatoryRequirementsFirst_AndKeepsTheStoredOrderWithinAGroup()
        {
            var cvScan = CreateScan(
                CreateMatch("optional-2", false, MatchStatus.Met, id: 4),
                CreateMatch("mandatory-2", true, MatchStatus.Met, id: 3),
                CreateMatch("optional-1", false, MatchStatus.Met, id: 2),
                CreateMatch("mandatory-1", true, MatchStatus.Met, id: 1));

            var report = RequirementReportBuilder.Build(cvScan);

            Assert.Equal(
                new[] { "mandatory-1", "mandatory-2", "optional-1", "optional-2" },
                report.Rows.Select(row => row.RequirementName));
        }

        [Fact]
        public void Build_CarriesTheSemanticMatchAndTheInterpretationOfEachRequirement()
        {
            var cvScan = CreateScan(CreateMatch("SQL Server", true, MatchStatus.Met, 0.87, CreateInterpretation(MatchStatus.Partial)));

            var row = Assert.Single(RequirementReportBuilder.Build(cvScan).Rows);

            Assert.Equal("SQL Server", row.RequirementName);
            Assert.True(row.IsMandatory);
            Assert.Equal(MatchStatus.Met, row.SemanticStatus);
            Assert.Equal(0.87, row.Similarity);
            Assert.Equal(MatchStatus.Partial, row.ModelStatus);
            Assert.Equal("quote", row.EvidenceQuote);
            Assert.Equal("Because.", row.Explanation);
            Assert.Equal("Do more.", row.Suggestion);
        }

        [Fact]
        public void Build_LeavesTheModelFieldsEmpty_WhenThereIsNoInterpretation()
        {
            var report = RequirementReportBuilder.Build(CreateScan(CreateMatch("SQL Server", true, MatchStatus.Met)));

            var row = Assert.Single(report.Rows);
            Assert.Null(row.ModelStatus);
            Assert.Null(row.EvidenceQuote);
            Assert.Null(row.Explanation);
            Assert.Null(row.Suggestion);
            Assert.False(row.HasDisagreement);
            Assert.False(report.HasInterpretations);
        }

        [Theory]
        [InlineData(MatchStatus.Met, MatchStatus.Missing, true)]
        [InlineData(MatchStatus.Missing, MatchStatus.Met, true)]
        [InlineData(MatchStatus.Met, MatchStatus.Partial, false)]
        [InlineData(MatchStatus.Partial, MatchStatus.Missing, false)]
        [InlineData(MatchStatus.Met, MatchStatus.Met, false)]
        public void Build_FlagsADisagreement_OnlyWhenTheStatusesAreTwoStepsApart(MatchStatus semanticStatus, MatchStatus modelStatus, bool expected)
        {
            var cvScan = CreateScan(CreateMatch("SQL Server", true, semanticStatus, interpretation: CreateInterpretation(modelStatus)));

            Assert.Equal(expected, Assert.Single(RequirementReportBuilder.Build(cvScan).Rows).HasDisagreement);
        }

        [Fact]
        public void Build_ReportsTheModelIdentityOfTheFirstInterpretation()
        {
            var cvScan = CreateScan(
                CreateMatch("First", true, MatchStatus.Met, id: 1),
                CreateMatch("Second", true, MatchStatus.Met, interpretation: CreateInterpretation(MatchStatus.Met, "Ollama:qwen"), id: 2));

            var report = RequirementReportBuilder.Build(cvScan);

            Assert.Equal("Ollama:qwen", report.ModelIdentity);
            Assert.True(report.HasInterpretations);
        }

        [Fact]
        public void Build_HasNoModelIdentity_WhenNothingWasInterpreted()
        {
            Assert.Null(RequirementReportBuilder.Build(CreateScan(CreateMatch("SQL Server", true, MatchStatus.Met))).ModelIdentity);
        }

        [Fact]
        public void Build_WritesTheThreeScoreComponents_WhenTheHybridScoreWasApplied()
        {
            var report = RequirementReportBuilder.Build(CreateScanWithScores(10, 14.5, 12));

            Assert.Equal("Keyword score 10/20 | Semantic score 14.5/20 | Hybrid score 12/20", report.ScoreComponentsText);
        }

        [Fact]
        public void Build_LeavesOutTheSemanticPart_WhenTheAuditHasNoSemanticScore()
        {
            var report = RequirementReportBuilder.Build(CreateScanWithScores(10, null, 12));

            Assert.Equal("Keyword score 10/20 | Hybrid score 12/20", report.ScoreComponentsText);
        }

        [Fact]
        public void Build_HasNoScoreComponents_WhenNoHybridScoreWasApplied()
        {
            Assert.Null(RequirementReportBuilder.Build(CreateScanWithScores(10, null, null)).ScoreComponentsText);
        }

        [Fact]
        public void Build_HasNoScoreComponents_WhenThereIsNoAudit()
        {
            Assert.Null(RequirementReportBuilder.Build(CreateScanWithScores(10, 14.5, 12, withAudit: false)).ScoreComponentsText);
        }

        [Fact]
        public void Build_HasNoScoreComponents_WhenThereIsNoJobMatchSection()
        {
            Assert.Null(RequirementReportBuilder.Build(CreateScanWithScores(10, 14.5, 12, withJobMatchSection: false)).ScoreComponentsText);
        }
    }
}
