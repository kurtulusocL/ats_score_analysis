using ATS.Application.Knowledge;

namespace ATS.Core.Helpers
{
    public static class SkillKnowledgeRanker
    {
        public static IReadOnlyList<SkillKnowledgeHit> Rank(
            float[] requirementVector,
            IReadOnlyList<SkillKnowledgeEntry> entries,
            IReadOnlyList<float[]> entryVectors,
            int maximumHits,
            double minimumSimilarity)
        {
            ArgumentNullException.ThrowIfNull(requirementVector);
            ArgumentNullException.ThrowIfNull(entries);
            ArgumentNullException.ThrowIfNull(entryVectors);

            if (entries.Count != entryVectors.Count)
                throw new ArgumentException("There must be exactly one vector for each entry.", nameof(entryVectors));

            return entries
                .Select((entry, index) => new SkillKnowledgeHit(entry, VectorMathHelper.CosineSimilarity(requirementVector, entryVectors[index])))
                .Where(hit => hit.Similarity >= minimumSimilarity)
                .OrderByDescending(hit => hit.Similarity)
                .ThenBy(hit => hit.Entry.Name, StringComparer.Ordinal)
                .Take(maximumHits)
                .ToList();
        }
    }
}
