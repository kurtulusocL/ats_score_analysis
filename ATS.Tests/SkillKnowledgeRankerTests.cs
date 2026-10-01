using ATS.Application.Knowledge;
using ATS.Core.Helpers;

namespace ATS.Tests
{
    public class SkillKnowledgeRankerTests
    {
        private static SkillKnowledgeEntry Entry(string name) => new(name, "Category", "Description.", Array.Empty<string>());

        private static readonly float[] Query = { 1f, 0f };

        [Fact]
        public void Rank_OrdersTheHitsBySimilarityDescending()
        {
            var entries = new[] { Entry("Weak"), Entry("Strong"), Entry("Medium") };
            var vectors = new[] { new[] { 0.6f, 0.8f }, new[] { 1f, 0f }, new[] { 0.8f, 0.6f } };

            var hits = SkillKnowledgeRanker.Rank(Query, entries, vectors, 3, 0.0);

            Assert.Equal(new[] { "Strong", "Medium", "Weak" }, hits.Select(hit => hit.Entry.Name));
        }

        [Fact]
        public void Rank_KeepsOnlyTheRequestedNumberOfBestHits()
        {
            var entries = new[] { Entry("Weak"), Entry("Strong"), Entry("Medium") };
            var vectors = new[] { new[] { 0.6f, 0.8f }, new[] { 1f, 0f }, new[] { 0.8f, 0.6f } };

            var hits = SkillKnowledgeRanker.Rank(Query, entries, vectors, 2, 0.0);

            Assert.Equal(new[] { "Strong", "Medium" }, hits.Select(hit => hit.Entry.Name));
        }

        [Fact]
        public void Rank_DropsHitsBelowTheThreshold_AndKeepsOneExactlyAtIt()
        {
            var entries = new[] { Entry("Exact"), Entry("Below") };
            var vectors = new[] { new[] { 1f, 0f }, new[] { 0f, 1f } };

            var hits = SkillKnowledgeRanker.Rank(Query, entries, vectors, 3, 1.0);

            Assert.Equal("Exact", Assert.Single(hits).Entry.Name);
        }

        [Fact]
        public void Rank_BreaksTiesByName()
        {
            var entries = new[] { Entry("B"), Entry("A") };
            var vectors = new[] { new[] { 1f, 0f }, new[] { 1f, 0f } };

            var hits = SkillKnowledgeRanker.Rank(Query, entries, vectors, 3, 0.0);

            Assert.Equal(new[] { "A", "B" }, hits.Select(hit => hit.Entry.Name));
        }

        [Fact]
        public void Rank_ReturnsNothing_WhenNoEntryReachesTheThreshold()
        {
            var hits = SkillKnowledgeRanker.Rank(Query, new[] { Entry("Far") }, new[] { new[] { 0f, 1f } }, 3, 0.5);

            Assert.Empty(hits);
        }

        [Fact]
        public void Rank_Throws_WhenTheNumberOfVectorsDoesNotMatchTheEntries()
        {
            Assert.Throws<ArgumentException>(() =>
                SkillKnowledgeRanker.Rank(Query, new[] { Entry("A"), Entry("B") }, new[] { new[] { 1f, 0f } }, 3, 0.0));
        }
    }
}
