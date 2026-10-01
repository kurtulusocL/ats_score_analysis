using ATS.Application.Knowledge;

namespace ATS.Tests
{
    public class SkillKnowledgeTextBuilderTests
    {
        [Fact]
        public void Build_JoinsTheNameTheSortedAliasesAndTheDescription()
        {
            var entry = new SkillKnowledgeEntry("SQL Server", "Database", "Microsoft database.", new[] { "T-SQL", "MSSQL" });

            Assert.Equal("SQL Server. Also known as: MSSQL, T-SQL. Microsoft database.", SkillKnowledgeTextBuilder.Build(entry));
        }

        [Fact]
        public void Build_ProducesTheSameTextWhateverTheAliasOrder()
        {
            var first = new SkillKnowledgeEntry("Docker", "DevOps", "Containers.", new[] { "b", "a", "c" });
            var second = new SkillKnowledgeEntry("Docker", "DevOps", "Containers.", new[] { "c", "b", "a" });

            Assert.Equal(SkillKnowledgeTextBuilder.Build(first), SkillKnowledgeTextBuilder.Build(second));
        }

        [Fact]
        public void Build_LeavesOutTheAliasPartWhenThereAreNoAliases()
        {
            var entry = new SkillKnowledgeEntry("Docker", "DevOps", "Container platform.", Array.Empty<string>());

            Assert.Equal("Docker. Container platform.", SkillKnowledgeTextBuilder.Build(entry));
        }

        [Fact]
        public void Build_LeavesOutTheDescriptionWhenItIsBlank()
        {
            var entry = new SkillKnowledgeEntry("Docker", "DevOps", "   ", new[] { "containers" });

            Assert.Equal("Docker. Also known as: containers.", SkillKnowledgeTextBuilder.Build(entry));
        }

        [Fact]
        public void Build_TrimsTextAndIgnoresBlankAliases()
        {
            var entry = new SkillKnowledgeEntry(" SQL Server ", "Database", " Microsoft database. ", new[] { " MSSQL ", "  " });

            Assert.Equal("SQL Server. Also known as: MSSQL. Microsoft database.", SkillKnowledgeTextBuilder.Build(entry));
        }
    }
}
