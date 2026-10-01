using ATS.Domain.Entities;
using ATS.Infrastructure.Concrete.AI;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;

namespace ATS.Tests
{
    public class SkillKnowledgeSourceTests
    {
        private static ApplicationDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        private static SkillTaxonomyEntry CreateEntry(string name, bool isActive = true, params string[] aliases)
        {
            var entry = new SkillTaxonomyEntry
            {
                Name = name,
                Category = "Test category",
                Description = name + " description.",
                IsActive = isActive
            };

            foreach (var alias in aliases)
                entry.SkillTaxonomyAliases.Add(new SkillTaxonomyAlias { Alias = alias });

            return entry;
        }

        private static async Task<List<Application.Knowledge.SkillKnowledgeEntry>> GetTestEntriesAsync(ApplicationDbContext context)
        {
            var entries = await new SkillKnowledgeSource(context).GetActiveEntriesAsync();
            return entries.Where(entry => entry.Name.StartsWith("Test ", StringComparison.Ordinal)).ToList();
        }

        [Fact]
        public async Task GetActiveEntriesAsync_ReturnsTheEntriesOrderedByName_WithTheirFieldsAndSortedAliases()
        {
            using var context = CreateContext();
            context.SkillTaxonomyEntries.AddRange(CreateEntry("Test B skill", true, "tb2", "tb1"), CreateEntry("Test A skill"));
            await context.SaveChangesAsync();

            var entries = await GetTestEntriesAsync(context);

            Assert.Equal(new[] { "Test A skill", "Test B skill" }, entries.Select(entry => entry.Name));
            Assert.Equal("Test category", entries[1].Category);
            Assert.Equal("Test B skill description.", entries[1].Description);
            Assert.Equal(new[] { "tb1", "tb2" }, entries[1].Aliases);
            Assert.Empty(entries[0].Aliases);
        }

        [Fact]
        public async Task GetActiveEntriesAsync_LeavesOutInactiveEntries()
        {
            using var context = CreateContext();
            context.SkillTaxonomyEntries.AddRange(CreateEntry("Test active skill"), CreateEntry("Test inactive skill", false));
            await context.SaveChangesAsync();

            var entries = await GetTestEntriesAsync(context);

            Assert.Equal("Test active skill", Assert.Single(entries).Name);
        }
    }
}
