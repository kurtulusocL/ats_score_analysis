using ATS.Core.Constants;
using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATS.Infrastructure.Persistence.Seed
{
    public static class AnalyzerKeywordSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            var keywords = new List<AnalyzerKeyword>();
            var id = 1;

            var impactVerbs = new[]
            {
                "developed", "designed", "implemented", "built", "created", "launched",
                "delivered", "improved", "increased", "decreased", "reduced", "optimized",
                "automated", "managed", "led", "coordinated", "architected", "migrated",
                "integrated", "deployed", "refactored", "established", "streamlined",
                "accelerated", "transformed", "spearheaded", "achieved", "engineered",
                "resolved", "maintained", "monitored", "configured", "scaled", "secured",
                "trained", "mentored", "collaborated", "planned", "executed", "analyzed"
            };

            foreach (var verb in impactVerbs)
                keywords.Add(new AnalyzerKeyword
                {
                    Id = id++,
                    Word = verb,
                    Category = KeywordCategories.ImpactVerb,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });

            var passiveIndicators = new[]
            {
                "responsible for", "worked on", "helped with", "assisted in",
                "involved in", "participated in", "part of", "contributed to",
                "exposure to", "familiar with", "knowledge of", "experience in"
            };

            foreach (var passive in passiveIndicators)
                keywords.Add(new AnalyzerKeyword
                {
                    Id = id++,
                    Word = passive,
                    Category = KeywordCategories.PassiveIndicator,
                    SubCategory = "Passive",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });

            var contactHeaders = new[]
            {
                "phone", "email", "tel", "mobile", "linkedin", "github", "contact", "address", "location"
            };

            foreach (var header in contactHeaders)
                keywords.Add(new AnalyzerKeyword
                {
                    Id = id++,
                    Word = header,
                    Category = KeywordCategories.SectionHeader,
                    SubCategory = "ContactInfo",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });

            var summaryHeaders = new[]
            {
                "summary", "profile", "about me", "career summary",
                "professional summary", "objective", "about", "personal profile"
            };

            foreach (var header in summaryHeaders)
                keywords.Add(new AnalyzerKeyword
                {
                    Id = id++,
                    Word = header,
                    Category = KeywordCategories.SectionHeader,
                    SubCategory = "Summary",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });

            var experienceHeaders = new[]
            {
                "experience", "work experience", "employment history",
                "professional experience", "work history", "career history",
                "professional background", "relevant experience"
            };

            foreach (var header in experienceHeaders)
                keywords.Add(new AnalyzerKeyword
                {
                    Id = id++,
                    Word = header,
                    Category = KeywordCategories.SectionHeader,
                    SubCategory = "Experience",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });

            var educationHeaders = new[]
            {
                "education", "academic background", "university", "college",
                "degree", "bachelor", "master", "graduate"
            };

            foreach (var header in educationHeaders)
                keywords.Add(new AnalyzerKeyword
                {
                    Id = id++,
                    Word = header,
                    Category = KeywordCategories.SectionHeader,
                    SubCategory = "Education",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });

            var skillsHeaders = new[]
            {
              "skills", "technical skills", "technologies", "tech stack",
              "tools", "competencies", "expertise", "stack", "core skills",
              "core technical skills", "professional skills", "critical keywords"
            };

            foreach (var header in skillsHeaders)
                keywords.Add(new AnalyzerKeyword
                {
                    Id = id++,
                    Word = header,
                    Category = KeywordCategories.SectionHeader,
                    SubCategory = "Skills",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });

            var languageHeaders = new[]
            {
                "languages", "language skills", "fluent", "native", "proficiency"
            };

            foreach (var header in languageHeaders)
                keywords.Add(new AnalyzerKeyword
                {
                    Id = id++,
                    Word = header,
                    Category = KeywordCategories.SectionHeader,
                    SubCategory = "Languages",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });

            var certHeaders = new[]
            {
                "certifications", "certificate", "certified", "credential", "license"
            };

            foreach (var header in certHeaders)
                keywords.Add(new AnalyzerKeyword
                {
                    Id = id++,
                    Word = header,
                    Category = KeywordCategories.SectionHeader,
                    SubCategory = "Certifications",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });

            modelBuilder.Entity<AnalyzerKeyword>().HasData(keywords);
        }
    }
}
