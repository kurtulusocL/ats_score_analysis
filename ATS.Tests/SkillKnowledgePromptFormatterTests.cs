using ATS.Application.Knowledge;
using ATS.Application.Results;

namespace ATS.Tests
{
    public class SkillKnowledgePromptFormatterTests
    {
        private static readonly SkillKnowledgeEntry SqlServer = new("SQL Server", "Database", "Microsoft database.", new[] { "MSSQL" });
        private static readonly SkillKnowledgeEntry PostgreSql = new("PostgreSQL", "Database", "Open-source database.", Array.Empty<string>());

        private static RequirementKnowledge Knowledge(string requirementName, params SkillKnowledgeEntry[] entries) =>
            new(new ExtractedRequirement(requirementName, true, "Skill"),
                entries.Select(entry => new SkillKnowledgeHit(entry, 0.9)).ToList());

        [Fact]
        public void Format_ListsEachRequirementWithItsEntriesInTheRankedOrder()
        {
            var text = SkillKnowledgePromptFormatter.Format(new[] { Knowledge("MSSQL experience", SqlServer, PostgreSql) });

            Assert.Equal(
                "Requirement: MSSQL experience\n"
                + "- " + SkillKnowledgeTextBuilder.Build(SqlServer) + "\n"
                + "- " + SkillKnowledgeTextBuilder.Build(PostgreSql),
                text);
        }

        [Fact]
        public void Format_SeparatesTheRequirementsWithABlankLine()
        {
            var text = SkillKnowledgePromptFormatter.Format(new[] { Knowledge("First", SqlServer), Knowledge("Second", PostgreSql) });

            Assert.Contains("\n\nRequirement: Second", text);
        }

        [Fact]
        public void Format_LeavesOutRequirementsWithoutHits()
        {
            var text = SkillKnowledgePromptFormatter.Format(new[] { Knowledge("No context"), Knowledge("With context", SqlServer) });

            Assert.DoesNotContain("No context", text);
            Assert.Contains("Requirement: With context", text);
        }

        [Fact]
        public void Format_ReturnsNull_WhenNoRequirementHasAnyHit()
        {
            Assert.Null(SkillKnowledgePromptFormatter.Format(new[] { Knowledge("No context") }));
            Assert.Null(SkillKnowledgePromptFormatter.Format(Array.Empty<RequirementKnowledge>()));
        }
    }
}
