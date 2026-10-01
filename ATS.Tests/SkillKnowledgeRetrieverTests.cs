using ATS.Application.Knowledge;
using ATS.Application.Options;
using ATS.Application.Results;
using ATS.Tests.TestSupport;

namespace ATS.Tests
{
    public class SkillKnowledgeRetrieverTests
    {
        private static readonly SkillKnowledgeEntry SqlServer = new("SQL Server", "Database", "Microsoft database.", new[] { "MSSQL" });
        private static readonly SkillKnowledgeEntry PostgreSql = new("PostgreSQL", "Database", "Open-source database.", Array.Empty<string>());
        private static readonly SkillKnowledgeEntry Kubernetes = new("Kubernetes", "DevOps", "Container orchestration.", Array.Empty<string>());
        private static readonly ExtractedRequirement DatabaseRequirement = new("MSSQL experience", true, "Database");
        private static readonly ExtractedRequirement ContainerRequirement = new("Container orchestration skills", false, "Tool");
        private static readonly ExtractedRequirement UnrelatedRequirement = new("Baking bread", true, "Other");

        private static readonly SkillKnowledgeEntry[] AllEntries = { SqlServer, PostgreSql, Kubernetes };

        private static Dictionary<string, float[]> CreateVectors() => new()
        {
            [SkillKnowledgeTextBuilder.Build(SqlServer)] = new[] { 1f, 0f, 0f },
            [SkillKnowledgeTextBuilder.Build(PostgreSql)] = new[] { 0.6f, 0.8f, 0f },
            [SkillKnowledgeTextBuilder.Build(Kubernetes)] = new[] { 0f, 0f, 1f },
            [DatabaseRequirement.Name] = new[] { 1f, 0f, 0f },
            [ContainerRequirement.Name] = new[] { 0f, 0f, 1f },
            [UnrelatedRequirement.Name] = new[] { 0f, -1f, 0f }
        };

        private static FakeEmbeddingService CreateEmbeddingService() =>
            new(texts => texts.Select(text => CreateVectors()[text]).ToList());

        private static SkillKnowledgeRetriever CreateRetriever(
            FakeEmbeddingService embeddingService, IReadOnlyList<SkillKnowledgeEntry>? entries = null, int hitsPerRequirement = 3) =>
            new(new FakeSkillKnowledgeSource(entries ?? AllEntries), embeddingService,
                new SemanticOptions { KnowledgeHitsPerRequirement = hitsPerRequirement, KnowledgeSimilarityThreshold = 0.5 });

        [Fact]
        public async Task RetrieveAsync_ReturnsTheBestEntriesForEachRequirementInInputOrder()
        {
            var retriever = CreateRetriever(CreateEmbeddingService());

            var results = await retriever.RetrieveAsync(new[] { DatabaseRequirement, ContainerRequirement });

            Assert.Equal(2, results.Count);
            Assert.Same(DatabaseRequirement, results[0].Requirement);
            Assert.Equal(new[] { "SQL Server", "PostgreSQL" }, results[0].Hits.Select(hit => hit.Entry.Name));
            Assert.Equal(1.0, results[0].Hits[0].Similarity, 6);
            Assert.Equal(0.6, results[0].Hits[1].Similarity, 4);
            Assert.Same(ContainerRequirement, results[1].Requirement);
            Assert.Equal("Kubernetes", Assert.Single(results[1].Hits).Entry.Name);
        }

        [Fact]
        public async Task RetrieveAsync_EmbedsTheEntriesFirstAndThenTheRequirementNames_InASingleCall()
        {
            var embeddingService = CreateEmbeddingService();

            await CreateRetriever(embeddingService).RetrieveAsync(new[] { DatabaseRequirement, ContainerRequirement });

            var expectedTexts = AllEntries.Select(SkillKnowledgeTextBuilder.Build)
                .Concat(new[] { DatabaseRequirement.Name, ContainerRequirement.Name });
            Assert.Equal(expectedTexts, Assert.Single(embeddingService.ReceivedTextLists));
        }

        [Fact]
        public async Task RetrieveAsync_KeepsOnlyTheConfiguredNumberOfHits()
        {
            var retriever = CreateRetriever(CreateEmbeddingService(), hitsPerRequirement: 1);

            var results = await retriever.RetrieveAsync(new[] { DatabaseRequirement });

            Assert.Equal("SQL Server", Assert.Single(results[0].Hits).Entry.Name);
        }

        [Fact]
        public async Task RetrieveAsync_ReturnsTheRequirementWithoutHits_WhenNoEntryReachesTheThreshold()
        {
            var results = await CreateRetriever(CreateEmbeddingService()).RetrieveAsync(new[] { UnrelatedRequirement });

            Assert.Same(UnrelatedRequirement, Assert.Single(results).Requirement);
            Assert.Empty(results[0].Hits);
        }

        [Fact]
        public async Task RetrieveAsync_ReturnsRequirementsWithoutHits_AndSkipsEmbedding_WhenThereAreNoEntries()
        {
            var embeddingService = new FakeEmbeddingService(_ => throw new InvalidOperationException("must not be called"));
            var retriever = CreateRetriever(embeddingService, entries: Array.Empty<SkillKnowledgeEntry>());

            var results = await retriever.RetrieveAsync(new[] { DatabaseRequirement });

            Assert.Empty(Assert.Single(results).Hits);
            Assert.Empty(embeddingService.ReceivedTextLists);
        }

        [Fact]
        public async Task RetrieveAsync_ReturnsNothing_WithoutTouchingTheSourceOrTheEmbeddingService_WhenThereAreNoRequirements()
        {
            var source = new FakeSkillKnowledgeSource(AllEntries);
            var embeddingService = CreateEmbeddingService();
            var retriever = new SkillKnowledgeRetriever(source, embeddingService, new SemanticOptions());

            var results = await retriever.RetrieveAsync(Array.Empty<ExtractedRequirement>());

            Assert.Empty(results);
            Assert.Equal(0, source.CallCount);
            Assert.Empty(embeddingService.ReceivedTextLists);
        }

        [Fact]
        public async Task RetrieveAsync_Throws_WhenTheEmbeddingServiceReturnsTheWrongNumberOfVectors()
        {
            var embeddingService = new FakeEmbeddingService(_ => new List<float[]> { new[] { 1f } });
            var retriever = CreateRetriever(embeddingService);

            await Assert.ThrowsAsync<InvalidOperationException>(() => retriever.RetrieveAsync(new[] { DatabaseRequirement }));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_Throws_WhenTheNumberOfHitsIsNotPositive(int hitsPerRequirement)
        {
            Assert.Throws<InvalidOperationException>(() =>
                CreateRetriever(CreateEmbeddingService(), hitsPerRequirement: hitsPerRequirement));
        }
    }
}
