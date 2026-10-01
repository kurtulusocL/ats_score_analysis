using ATS.Domain.Entities;
using ATS.Infrastructure.Persistence.Context.Mssql;
using ATS.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace ATS.Tests
{
    public class SkillTaxonomySeedTests
    {
        private static ApplicationDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        private static int GetMaximumLength<TEntity>(ApplicationDbContext context, string propertyName)
        {
            var maximumLength = context.Model.FindEntityType(typeof(TEntity))!.FindProperty(propertyName)!.GetMaxLength();
            Assert.NotNull(maximumLength);
            return maximumLength!.Value;
        }

        [Fact]
        public void Entries_HaveNamesAndAliasesThatAreUniqueAcrossTheWholeTaxonomy()
        {
            var terms = SkillTaxonomySeed.Entries.SelectMany(entry => entry.Aliases.Prepend(entry.Name)).ToList();

            Assert.Equal(terms.Count, terms.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        }

        [Fact]
        public void Entries_EachHaveANameACategoryAndADescription()
        {
            Assert.All(SkillTaxonomySeed.Entries, entry =>
            {
                Assert.False(string.IsNullOrWhiteSpace(entry.Name));
                Assert.False(string.IsNullOrWhiteSpace(entry.Category));
                Assert.False(string.IsNullOrWhiteSpace(entry.Description));
                Assert.All(entry.Aliases, alias => Assert.False(string.IsNullOrWhiteSpace(alias)));
            });
        }

        [Fact]
        public void Entries_FitTheColumnLimitsOfTheModel()
        {
            using var context = CreateContext();
            var nameLimit = GetMaximumLength<SkillTaxonomyEntry>(context, nameof(SkillTaxonomyEntry.Name));
            var categoryLimit = GetMaximumLength<SkillTaxonomyEntry>(context, nameof(SkillTaxonomyEntry.Category));
            var descriptionLimit = GetMaximumLength<SkillTaxonomyEntry>(context, nameof(SkillTaxonomyEntry.Description));
            var aliasLimit = GetMaximumLength<SkillTaxonomyAlias>(context, nameof(SkillTaxonomyAlias.Alias));

            Assert.All(SkillTaxonomySeed.Entries, entry =>
            {
                Assert.True(entry.Name.Length <= nameLimit);
                Assert.True(entry.Category.Length <= categoryLimit);
                Assert.True(entry.Description.Length <= descriptionLimit);
                Assert.All(entry.Aliases, alias => Assert.True(alias.Length <= aliasLimit));
            });
        }

        [Fact]
        public void TheModel_SeedsEveryEntryAndEveryAlias()
        {
            using var context = CreateContext();
            context.Database.EnsureCreated();

            Assert.Equal(SkillTaxonomySeed.Entries.Count, context.SkillTaxonomyEntries.Count());
            Assert.Equal(SkillTaxonomySeed.Entries.Sum(entry => entry.Aliases.Length), context.SkillTaxonomyAliases.Count());

            var sqlServerAliases = context.SkillTaxonomyEntries.Single(entry => entry.Name == "SQL Server")
                .SkillTaxonomyAliases.Select(alias => alias.Alias).ToList();
            Assert.Contains("MSSQL", sqlServerAliases);
        }
    }
}
