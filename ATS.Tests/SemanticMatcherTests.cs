using ATS.Application.Abstract.AI;
using ATS.Application.Options;
using ATS.Application.Results;
using ATS.Application.Semantic;
using ATS.Domain.Enums;

namespace ATS.Tests
{
    public class SemanticMatcherTests
    {
        private sealed class FakeEmbeddingService(Func<IReadOnlyList<string>, IReadOnlyList<float[]>> respond) : IEmbeddingService
        {
            public List<IReadOnlyList<string>> ReceivedTextLists { get; } = new();

            public Task<IReadOnlyList<float[]>> GetEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
            {
                ReceivedTextLists.Add(texts);
                return Task.FromResult(respond(texts));
            }
        }

        private static FakeEmbeddingService CreateFromDictionary(Dictionary<string, float[]> vectorsByText) =>
            new(texts => texts.Select(text => vectorsByText[text]).ToList());

        private static SemanticOptions CreateOptions() => new()
        {
            MetSimilarityThreshold = 0.9,
            PartialSimilarityThreshold = 0.5,
            MaxChunkCharacters = 12
        };

        private static Dictionary<string, float[]> CreateVectors() => new()
        {
            ["sql"] = new[] { 1f, 0f, 0f },
            ["java"] = new[] { 0.6f, 0.8f, 0f },
            ["cobol"] = new[] { 0f, 0f, 1f },
            ["strong sql"] = new[] { 1f, 0f, 0f },
            ["some java"] = new[] { 0f, 1f, 0f },
            ["no match"] = new[] { 0.1f, -0.1f, 0f }
        };

        private static readonly ExtractedRequirement[] ExtractedRequirements =
        {
        new("sql", true, "Database"),
        new("java", true, "Language"),
        new("cobol", false, "Language")
    };

        private const string CvText = "strong sql\nsome java\nno match";

        [Fact]
        public async Task MatchAsync_AssignsStatusByThresholds_AndUsesBestChunkAsEvidence()
        {
            var matcher = new SemanticMatcher(CreateFromDictionary(CreateVectors()), CreateOptions());

            var results = await matcher.MatchAsync(ExtractedRequirements, CvText);

            Assert.Equal(3, results.Count);

            Assert.Equal(MatchStatus.Met, results[0].Status);
            Assert.Equal(1.0, results[0].Similarity, 6);
            Assert.Equal("strong sql", results[0].Evidence);

            Assert.Equal(MatchStatus.Partial, results[1].Status);
            Assert.Equal(0.8, results[1].Similarity, 4);
            Assert.Equal("some java", results[1].Evidence);

            Assert.Equal(MatchStatus.Missing, results[2].Status);
            Assert.Equal(0.0, results[2].Similarity, 6);
            Assert.Null(results[2].Evidence);
        }

        [Fact]
        public async Task MatchAsync_CallsEmbeddingServiceOnce_WithRequirementsBeforeChunks()
        {
            var embeddingService = CreateFromDictionary(CreateVectors());
            var matcher = new SemanticMatcher(embeddingService, CreateOptions());

            await matcher.MatchAsync(ExtractedRequirements, CvText);

            Assert.Single(embeddingService.ReceivedTextLists);
            Assert.Equal(
                new[] { "sql", "java", "cobol", "strong sql", "some java", "no match" },
                embeddingService.ReceivedTextLists[0]);
        }

        [Fact]
        public async Task MatchAsync_ReturnsMissingForEveryRequirement_WhenCvTextIsBlank_WithoutCallingEmbeddingService()
        {
            var embeddingService = CreateFromDictionary(CreateVectors());
            var matcher = new SemanticMatcher(embeddingService, CreateOptions());

            var results = await matcher.MatchAsync(ExtractedRequirements, "   \n  ");

            Assert.Equal(3, results.Count);
            Assert.All(results, result =>
            {
                Assert.Equal(MatchStatus.Missing, result.Status);
                Assert.Null(result.Evidence);
            });
            Assert.Empty(embeddingService.ReceivedTextLists);
        }

        [Fact]
        public async Task MatchAsync_ReturnsEmpty_WhenThereAreNoRequirements()
        {
            var embeddingService = CreateFromDictionary(CreateVectors());
            var matcher = new SemanticMatcher(embeddingService, CreateOptions());

            var results = await matcher.MatchAsync(Array.Empty<ExtractedRequirement>(), CvText);

            Assert.Empty(results);
            Assert.Empty(embeddingService.ReceivedTextLists);
        }

        [Fact]
        public async Task MatchAsync_TruncatesEvidenceToTwoThousandCharacters()
        {
            var longChunk = string.Join(" ", Enumerable.Repeat("word", 600));
            var embeddingService = CreateFromDictionary(new Dictionary<string, float[]>
            {
                ["req"] = new[] { 1f, 0f },
                [longChunk] = new[] { 1f, 0f }
            });
            var options = new SemanticOptions { MaxChunkCharacters = 5000 };
            var matcher = new SemanticMatcher(embeddingService, options);

            var results = await matcher.MatchAsync(new[] { new ExtractedRequirement("req", true, "General") }, longChunk);

            Assert.Equal(MatchStatus.Met, results[0].Status);
            Assert.Equal(2000, results[0].Evidence!.Length);
        }

        [Fact]
        public async Task MatchAsync_Throws_WhenEmbeddingServiceReturnsWrongNumberOfVectors()
        {
            var embeddingService = new FakeEmbeddingService(_ => new List<float[]> { new[] { 1f } });
            var matcher = new SemanticMatcher(embeddingService, CreateOptions());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                matcher.MatchAsync(new[] { new ExtractedRequirement("req", true, "General") }, "some text"));
        }

        [Fact]
        public void Constructor_Throws_WhenPartialThresholdIsGreaterThanMetThreshold()
        {
            var embeddingService = CreateFromDictionary(CreateVectors());
            var options = new SemanticOptions { MetSimilarityThreshold = 0.5, PartialSimilarityThreshold = 0.8 };

            Assert.Throws<InvalidOperationException>(() => new SemanticMatcher(embeddingService, options));
        }
    }
}
