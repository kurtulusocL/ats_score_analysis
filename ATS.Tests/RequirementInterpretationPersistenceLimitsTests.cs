using ATS.Core.Constants;
using ATS.Domain.Entities;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;

namespace ATS.Tests
{
    public class RequirementInterpretationPersistenceLimitsTests
    {
        [Fact]
        public void PersistenceLimits_MatchTheRequirementInterpretationColumnMaximumLengths()
        {
            using var context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var entityType = context.Model.FindEntityType(typeof(RequirementInterpretation))!;

            Assert.Equal((int?)PersistenceLimits.RequirementInterpretationEvidenceQuoteMaximumLength,
                entityType.FindProperty(nameof(RequirementInterpretation.EvidenceQuote))!.GetMaxLength());
            Assert.Equal((int?)PersistenceLimits.RequirementInterpretationExplanationMaximumLength,
                entityType.FindProperty(nameof(RequirementInterpretation.Explanation))!.GetMaxLength());
            Assert.Equal((int?)PersistenceLimits.RequirementInterpretationSuggestionMaximumLength,
                entityType.FindProperty(nameof(RequirementInterpretation.Suggestion))!.GetMaxLength());
            Assert.Equal((int?)PersistenceLimits.RequirementInterpretationModelIdentityMaximumLength,
                entityType.FindProperty(nameof(RequirementInterpretation.ModelIdentity))!.GetMaxLength());
        }
    }
}
