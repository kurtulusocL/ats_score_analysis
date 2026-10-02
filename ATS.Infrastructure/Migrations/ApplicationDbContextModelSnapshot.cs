using System;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATS.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
internal class ApplicationDbContextModelSnapshot : ModelSnapshot
{
	protected override void BuildModel(ModelBuilder modelBuilder)
	{
		modelBuilder.HasAnnotation("ProductVersion", "9.0.14").HasAnnotation("Proxies:ChangeTracking", false).HasAnnotation("Proxies:CheckEquality", false)
			.HasAnnotation("Proxies:LazyLoading", true)
			.HasAnnotation("Relational:MaxIdentifierLength", 128);
		modelBuilder.UseIdentityColumns(1L);
		modelBuilder.Entity("ATS.Domain.Entities.AnalysisAudit", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<string>("AiProviderName").HasMaxLength(50).HasColumnType("nvarchar(50)");
			b.Property<string>("AiStatus").IsRequired().HasMaxLength(30)
				.HasColumnType("nvarchar(30)");
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<int>("CvScanId").HasColumnType("int");
			b.Property<int?>("DeterministicJobMatchScore").HasColumnType("int");
			b.Property<long?>("DurationMilliseconds").HasColumnType("bigint");
			b.Property<int?>("EmbeddingCacheHitCount").HasColumnType("int");
			b.Property<int?>("EmbeddingGeneratedCount").HasColumnType("int");
			b.Property<int?>("HybridJobMatchScore").HasColumnType("int");
			b.Property<int?>("InputTokenCount").HasColumnType("int");
			b.Property<int?>("OutputTokenCount").HasColumnType("int");
			b.Property<double?>("SemanticJobMatchScore").HasColumnType("float");
			b.HasKey("Id");
			b.HasIndex("CvScanId").IsUnique();
			b.ToTable("AnalysisAudits", (string?)null);
		});
		modelBuilder.Entity("ATS.Domain.Entities.AnalysisWarning", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<int>("CvScanId").HasColumnType("int");
			b.Property<string>("Message").IsRequired().HasMaxLength(1000)
				.HasColumnType("nvarchar(1000)");
			b.HasKey("Id");
			b.HasIndex("CvScanId");
			b.ToTable("AnalysisWarnings", (string?)null);
		});
		modelBuilder.Entity("ATS.Domain.Entities.AnalyzerKeyword", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<string>("Category").IsRequired().HasMaxLength(50)
				.HasColumnType("nvarchar(50)");
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<bool>("IsActive").ValueGeneratedOnAdd().HasColumnType("bit")
				.HasDefaultValue(true);
			b.Property<string>("SubCategory").IsRequired().ValueGeneratedOnAdd()
				.HasMaxLength(50)
				.HasColumnType("nvarchar(50)")
				.HasDefaultValue("");
			b.Property<string>("Word").IsRequired().HasMaxLength(100)
				.HasColumnType("nvarchar(100)");
			b.HasKey("Id");
			b.HasIndex("Category");
			b.HasIndex("Word", "Category").IsUnique();
			b.ToTable("AnalyzerKeywords", (string?)null);
			b.HasData(new
			{
				Id = 1,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "developed"
			}, new
			{
				Id = 2,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "designed"
			}, new
			{
				Id = 3,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "implemented"
			}, new
			{
				Id = 4,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "built"
			}, new
			{
				Id = 5,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "created"
			}, new
			{
				Id = 6,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "launched"
			}, new
			{
				Id = 7,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "delivered"
			}, new
			{
				Id = 8,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "improved"
			}, new
			{
				Id = 9,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "increased"
			}, new
			{
				Id = 10,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "decreased"
			}, new
			{
				Id = 11,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "reduced"
			}, new
			{
				Id = 12,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "optimized"
			}, new
			{
				Id = 13,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "automated"
			}, new
			{
				Id = 14,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "managed"
			}, new
			{
				Id = 15,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "led"
			}, new
			{
				Id = 16,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "coordinated"
			}, new
			{
				Id = 17,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "architected"
			}, new
			{
				Id = 18,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "migrated"
			}, new
			{
				Id = 19,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "integrated"
			}, new
			{
				Id = 20,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "deployed"
			}, new
			{
				Id = 21,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "refactored"
			}, new
			{
				Id = 22,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "established"
			}, new
			{
				Id = 23,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "streamlined"
			}, new
			{
				Id = 24,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "accelerated"
			}, new
			{
				Id = 25,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "transformed"
			}, new
			{
				Id = 26,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "spearheaded"
			}, new
			{
				Id = 27,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "achieved"
			}, new
			{
				Id = 28,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "engineered"
			}, new
			{
				Id = 29,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "resolved"
			}, new
			{
				Id = 30,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "maintained"
			}, new
			{
				Id = 31,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "monitored"
			}, new
			{
				Id = 32,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "configured"
			}, new
			{
				Id = 33,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "scaled"
			}, new
			{
				Id = 34,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "secured"
			}, new
			{
				Id = 35,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "trained"
			}, new
			{
				Id = 36,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "mentored"
			}, new
			{
				Id = 37,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "collaborated"
			}, new
			{
				Id = 38,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "planned"
			}, new
			{
				Id = 39,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "executed"
			}, new
			{
				Id = 40,
				Category = "ImpactVerb",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "",
				Word = "analyzed"
			}, new
			{
				Id = 41,
				Category = "PassiveIndicator",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Passive",
				Word = "responsible for"
			}, new
			{
				Id = 42,
				Category = "PassiveIndicator",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Passive",
				Word = "worked on"
			}, new
			{
				Id = 43,
				Category = "PassiveIndicator",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Passive",
				Word = "helped with"
			}, new
			{
				Id = 44,
				Category = "PassiveIndicator",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Passive",
				Word = "assisted in"
			}, new
			{
				Id = 45,
				Category = "PassiveIndicator",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Passive",
				Word = "involved in"
			}, new
			{
				Id = 46,
				Category = "PassiveIndicator",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Passive",
				Word = "participated in"
			}, new
			{
				Id = 47,
				Category = "PassiveIndicator",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Passive",
				Word = "part of"
			}, new
			{
				Id = 48,
				Category = "PassiveIndicator",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Passive",
				Word = "contributed to"
			}, new
			{
				Id = 49,
				Category = "PassiveIndicator",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Passive",
				Word = "exposure to"
			}, new
			{
				Id = 50,
				Category = "PassiveIndicator",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Passive",
				Word = "familiar with"
			}, new
			{
				Id = 51,
				Category = "PassiveIndicator",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Passive",
				Word = "knowledge of"
			}, new
			{
				Id = 52,
				Category = "PassiveIndicator",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Passive",
				Word = "experience in"
			}, new
			{
				Id = 53,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "ContactInfo",
				Word = "phone"
			}, new
			{
				Id = 54,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "ContactInfo",
				Word = "email"
			}, new
			{
				Id = 55,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "ContactInfo",
				Word = "tel"
			}, new
			{
				Id = 56,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "ContactInfo",
				Word = "mobile"
			}, new
			{
				Id = 57,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "ContactInfo",
				Word = "linkedin"
			}, new
			{
				Id = 58,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "ContactInfo",
				Word = "github"
			}, new
			{
				Id = 59,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "ContactInfo",
				Word = "contact"
			}, new
			{
				Id = 60,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "ContactInfo",
				Word = "address"
			}, new
			{
				Id = 61,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "ContactInfo",
				Word = "location"
			}, new
			{
				Id = 62,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Summary",
				Word = "summary"
			}, new
			{
				Id = 63,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Summary",
				Word = "profile"
			}, new
			{
				Id = 64,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Summary",
				Word = "about me"
			}, new
			{
				Id = 65,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Summary",
				Word = "career summary"
			}, new
			{
				Id = 66,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Summary",
				Word = "professional summary"
			}, new
			{
				Id = 67,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Summary",
				Word = "objective"
			}, new
			{
				Id = 68,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Summary",
				Word = "about"
			}, new
			{
				Id = 69,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Summary",
				Word = "personal profile"
			}, new
			{
				Id = 70,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Experience",
				Word = "experience"
			}, new
			{
				Id = 71,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Experience",
				Word = "work experience"
			}, new
			{
				Id = 72,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Experience",
				Word = "employment history"
			}, new
			{
				Id = 73,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Experience",
				Word = "professional experience"
			}, new
			{
				Id = 74,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Experience",
				Word = "work history"
			}, new
			{
				Id = 75,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Experience",
				Word = "career history"
			}, new
			{
				Id = 76,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Experience",
				Word = "professional background"
			}, new
			{
				Id = 77,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Experience",
				Word = "relevant experience"
			}, new
			{
				Id = 78,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Education",
				Word = "education"
			}, new
			{
				Id = 79,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Education",
				Word = "academic background"
			}, new
			{
				Id = 80,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Education",
				Word = "university"
			}, new
			{
				Id = 81,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Education",
				Word = "college"
			}, new
			{
				Id = 82,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Education",
				Word = "degree"
			}, new
			{
				Id = 83,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Education",
				Word = "bachelor"
			}, new
			{
				Id = 84,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Education",
				Word = "master"
			}, new
			{
				Id = 85,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Education",
				Word = "graduate"
			}, new
			{
				Id = 86,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Skills",
				Word = "skills"
			}, new
			{
				Id = 87,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Skills",
				Word = "technical skills"
			}, new
			{
				Id = 88,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Skills",
				Word = "technologies"
			}, new
			{
				Id = 89,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Skills",
				Word = "tech stack"
			}, new
			{
				Id = 90,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Skills",
				Word = "tools"
			}, new
			{
				Id = 91,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Skills",
				Word = "competencies"
			}, new
			{
				Id = 92,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Skills",
				Word = "expertise"
			}, new
			{
				Id = 93,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Skills",
				Word = "stack"
			}, new
			{
				Id = 94,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Skills",
				Word = "core skills"
			}, new
			{
				Id = 95,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Skills",
				Word = "core technical skills"
			}, new
			{
				Id = 96,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Skills",
				Word = "professional skills"
			}, new
			{
				Id = 97,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Skills",
				Word = "critical keywords"
			}, new
			{
				Id = 98,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Languages",
				Word = "languages"
			}, new
			{
				Id = 99,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Languages",
				Word = "language skills"
			}, new
			{
				Id = 100,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Languages",
				Word = "fluent"
			}, new
			{
				Id = 101,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Languages",
				Word = "native"
			}, new
			{
				Id = 102,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Languages",
				Word = "proficiency"
			}, new
			{
				Id = 103,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Certifications",
				Word = "certifications"
			}, new
			{
				Id = 104,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Certifications",
				Word = "certificate"
			}, new
			{
				Id = 105,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Certifications",
				Word = "certified"
			}, new
			{
				Id = 106,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Certifications",
				Word = "credential"
			}, new
			{
				Id = 107,
				Category = "SectionHeader",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				IsActive = true,
				SubCategory = "Certifications",
				Word = "license"
			});
		});
		modelBuilder.Entity("ATS.Domain.Entities.CvScan", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<string>("CandidateName").IsRequired().HasMaxLength(200)
				.HasColumnType("nvarchar(200)");
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<string>("FilePath").IsRequired().HasMaxLength(500)
				.HasColumnType("nvarchar(500)");
			b.Property<string>("FileType").IsRequired().HasMaxLength(10)
				.HasColumnType("nvarchar(10)");
			b.Property<bool>("IsJobMatched").ValueGeneratedOnAdd().HasColumnType("bit")
				.HasDefaultValue(false);
			b.Property<int?>("JobPostingId").HasColumnType("int");
			b.Property<int>("OverallScore").ValueGeneratedOnAdd().HasColumnType("int")
				.HasDefaultValue(0);
			b.Property<string>("RawText").IsRequired().HasColumnType("nvarchar(max)");
			b.HasKey("Id");
			b.HasIndex("JobPostingId");
			b.ToTable("CvScans", (string?)null);
		});
		modelBuilder.Entity("ATS.Domain.Entities.EmbeddingCacheEntry", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<int>("Dimensions").HasColumnType("int");
			b.Property<string>("Model").IsRequired().HasMaxLength(100)
				.HasColumnType("nvarchar(100)");
			b.Property<string>("TextHash").IsRequired().HasMaxLength(64)
				.HasColumnType("nchar(64)")
				.IsFixedLength();
			b.Property<byte[]>("Vector").IsRequired().HasColumnType("varbinary(max)");
			b.HasKey("Id");
			b.HasIndex("TextHash", "Model").IsUnique();
			b.ToTable("EmbeddingCacheEntries", (string?)null);
		});
		modelBuilder.Entity("ATS.Domain.Entities.JobPosting", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<string>("RawText").IsRequired().HasColumnType("nvarchar(max)");
			b.Property<string>("Title").IsRequired().HasMaxLength(300)
				.HasColumnType("nvarchar(300)");
			b.HasKey("Id");
			b.ToTable("JobPostings", (string?)null);
		});
		modelBuilder.Entity("ATS.Domain.Entities.JobRequirement", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<string>("Category").IsRequired().ValueGeneratedOnAdd()
				.HasMaxLength(50)
				.HasColumnType("nvarchar(50)")
				.HasDefaultValue("");
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<bool>("IsMandatory").ValueGeneratedOnAdd().HasColumnType("bit")
				.HasDefaultValue(true);
			b.Property<int>("JobPostingId").HasColumnType("int");
			b.Property<string>("Name").IsRequired().HasMaxLength(200)
				.HasColumnType("nvarchar(200)");
			b.HasKey("Id");
			b.HasIndex("JobPostingId");
			b.ToTable("JobRequirements", (string?)null);
		});
		modelBuilder.Entity("ATS.Domain.Entities.RequirementInterpretation", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<string>("EvidenceQuote").HasMaxLength(2000).HasColumnType("nvarchar(2000)");
			b.Property<string>("Explanation").IsRequired().HasMaxLength(1000)
				.HasColumnType("nvarchar(1000)");
			b.Property<string>("ModelIdentity").IsRequired().HasMaxLength(100)
				.HasColumnType("nvarchar(100)");
			b.Property<int>("RequirementMatchId").HasColumnType("int");
			b.Property<string>("Status").IsRequired().HasMaxLength(20)
				.HasColumnType("nvarchar(20)");
			b.Property<string>("Suggestion").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
			b.HasKey("Id");
			b.HasIndex("RequirementMatchId").IsUnique();
			b.ToTable("RequirementInterpretations", (string?)null);
		});
		modelBuilder.Entity("ATS.Domain.Entities.RequirementMatch", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<int>("CvScanId").HasColumnType("int");
			b.Property<string>("Evidence").HasMaxLength(2000).HasColumnType("nvarchar(2000)");
			b.Property<int>("JobRequirementId").HasColumnType("int");
			b.Property<double>("Similarity").HasColumnType("float");
			b.Property<string>("Status").IsRequired().HasMaxLength(20)
				.HasColumnType("nvarchar(20)");
			b.HasKey("Id");
			b.HasIndex("JobRequirementId");
			b.HasIndex("CvScanId", "JobRequirementId").IsUnique();
			b.ToTable("RequirementMatches", (string?)null);
		});
		modelBuilder.Entity("ATS.Domain.Entities.ScoreReport", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<int>("CvScanId").HasColumnType("int");
			b.Property<bool>("IsGenerated").ValueGeneratedOnAdd().HasColumnType("bit")
				.HasDefaultValue(false);
			b.Property<string>("ReportPath").IsRequired().HasMaxLength(500)
				.HasColumnType("nvarchar(500)");
			b.Property<string>("ReportType").IsRequired().HasMaxLength(20)
				.HasColumnType("nvarchar(20)");
			b.HasKey("Id");
			b.HasIndex("CvScanId");
			b.ToTable("ScoreReports", (string?)null);
		});
		modelBuilder.Entity("ATS.Domain.Entities.SectionScore", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<int>("CvScanId").HasColumnType("int");
			b.Property<string>("Feedback").IsRequired().ValueGeneratedOnAdd()
				.HasColumnType("nvarchar(max)")
				.HasDefaultValue("");
			b.Property<bool>("IsPassed").ValueGeneratedOnAdd().HasColumnType("bit")
				.HasDefaultValue(false);
			b.Property<int>("MaxScore").ValueGeneratedOnAdd().HasColumnType("int")
				.HasDefaultValue(0);
			b.Property<int>("Score").ValueGeneratedOnAdd().HasColumnType("int")
				.HasDefaultValue(0);
			b.Property<string>("SectionName").IsRequired().HasMaxLength(100)
				.HasColumnType("nvarchar(100)");
			b.HasKey("Id");
			b.HasIndex("CvScanId");
			b.ToTable("SectionScores", (string?)null);
		});
		modelBuilder.Entity("ATS.Domain.Entities.SecurityFinding", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<int>("CvScanId").HasColumnType("int");
			b.Property<string>("Description").IsRequired().HasMaxLength(500)
				.HasColumnType("nvarchar(500)");
			b.Property<string>("Severity").IsRequired().HasColumnType("nvarchar(max)");
			b.Property<string>("Snippet").IsRequired().HasColumnType("nvarchar(max)");
			b.Property<string>("Type").IsRequired().HasColumnType("nvarchar(max)");
			b.HasKey("Id");
			b.HasIndex("CvScanId");
			b.ToTable("SecurityFindings", (string?)null);
		});
		modelBuilder.Entity("ATS.Domain.Entities.SkillTaxonomyAlias", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<string>("Alias").IsRequired().HasMaxLength(100)
				.HasColumnType("nvarchar(100)");
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<int>("SkillTaxonomyEntryId").HasColumnType("int");
			b.HasKey("Id");
			b.HasIndex("SkillTaxonomyEntryId", "Alias").IsUnique();
			b.ToTable("SkillTaxonomyAliases", (string?)null);
			b.HasData(new
			{
				Id = 1,
				Alias = "MSSQL",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 1
			}, new
			{
				Id = 2,
				Alias = "Microsoft SQL Server",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 1
			}, new
			{
				Id = 3,
				Alias = "T-SQL",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 1
			}, new
			{
				Id = 4,
				Alias = "Transact-SQL",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 1
			}, new
			{
				Id = 5,
				Alias = "Postgres",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 2
			}, new
			{
				Id = 6,
				Alias = "Mongo",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 3
			}, new
			{
				Id = 7,
				Alias = "NoSQL",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 3
			}, new
			{
				Id = 8,
				Alias = "Redis Cache",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 4
			}, new
			{
				Id = 9,
				Alias = "distributed cache",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 4
			}, new
			{
				Id = 10,
				Alias = "Elastic Search",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 5
			}, new
			{
				Id = 11,
				Alias = "OpenSearch",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 5
			}, new
			{
				Id = 12,
				Alias = "ELK",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 5
			}, new
			{
				Id = 13,
				Alias = "EF Core",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 6
			}, new
			{
				Id = 14,
				Alias = "Entity Framework",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 6
			}, new
			{
				Id = 15,
				Alias = "ORM",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 6
			}, new
			{
				Id = 16,
				Alias = "micro ORM",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 7
			}, new
			{
				Id = 17,
				Alias = "C Sharp",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 8
			}, new
			{
				Id = 18,
				Alias = "CSharp",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 8
			}, new
			{
				Id = 19,
				Alias = "dotnet",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 9
			}, new
			{
				Id = 20,
				Alias = ".NET Core",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 9
			}, new
			{
				Id = 21,
				Alias = ".NET Framework",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 9
			}, new
			{
				Id = 22,
				Alias = "ASP.NET",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 10
			}, new
			{
				Id = 23,
				Alias = "ASP.NET MVC",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 10
			}, new
			{
				Id = 24,
				Alias = "ASP.NET Web API",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 10
			}, new
			{
				Id = 25,
				Alias = "Web API",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 10
			}, new
			{
				Id = 26,
				Alias = "RESTful",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 11
			}, new
			{
				Id = 27,
				Alias = "RESTful API",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 11
			}, new
			{
				Id = 28,
				Alias = "REST services",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 11
			}, new
			{
				Id = 29,
				Alias = "Protocol Buffers",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 12
			}, new
			{
				Id = 30,
				Alias = "protobuf",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 12
			}, new
			{
				Id = 31,
				Alias = "WebSockets",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 14
			}, new
			{
				Id = 32,
				Alias = "real-time communication",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 14
			}, new
			{
				Id = 33,
				Alias = "OAuth",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 15
			}, new
			{
				Id = 34,
				Alias = "OAuth2",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 15
			}, new
			{
				Id = 35,
				Alias = "OpenID Connect",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 15
			}, new
			{
				Id = 36,
				Alias = "OIDC",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 15
			}, new
			{
				Id = 37,
				Alias = "JSON Web Token",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 16
			}, new
			{
				Id = 38,
				Alias = "bearer token",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 16
			}, new
			{
				Id = 39,
				Alias = "RBAC",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 17
			}, new
			{
				Id = 40,
				Alias = "role-based authorization",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 17
			}, new
			{
				Id = 41,
				Alias = "unit tests",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 18
			}, new
			{
				Id = 42,
				Alias = "xUnit",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 18
			}, new
			{
				Id = 43,
				Alias = "NUnit",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 18
			}, new
			{
				Id = 44,
				Alias = "MSTest",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 18
			}, new
			{
				Id = 45,
				Alias = "integration tests",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 19
			}, new
			{
				Id = 46,
				Alias = "Onion Architecture",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 20
			}, new
			{
				Id = 47,
				Alias = "layered architecture",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 20
			}, new
			{
				Id = 48,
				Alias = "hexagonal architecture",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 20
			}, new
			{
				Id = 49,
				Alias = "DDD",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 21
			}, new
			{
				Id = 50,
				Alias = "microservice architecture",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 22
			}, new
			{
				Id = 51,
				Alias = "event-driven",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 23
			}, new
			{
				Id = 52,
				Alias = "publish-subscribe",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 23
			}, new
			{
				Id = 53,
				Alias = "pub/sub",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 23
			}, new
			{
				Id = 54,
				Alias = "asynchronous messaging",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 23
			}, new
			{
				Id = 55,
				Alias = "message broker",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 24
			}, new
			{
				Id = 56,
				Alias = "AMQP",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 24
			}, new
			{
				Id = 57,
				Alias = "message bus",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 25
			}, new
			{
				Id = 58,
				Alias = "Kafka",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 26
			}, new
			{
				Id = 59,
				Alias = "event streaming",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 26
			}, new
			{
				Id = 60,
				Alias = "transactional outbox",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 27
			}, new
			{
				Id = 61,
				Alias = "containers",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 28
			}, new
			{
				Id = 62,
				Alias = "containerization",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 28
			}, new
			{
				Id = 63,
				Alias = "Dockerfile",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 28
			}, new
			{
				Id = 64,
				Alias = "K8s",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 29
			}, new
			{
				Id = 65,
				Alias = "container orchestration",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 29
			}, new
			{
				Id = 66,
				Alias = "Helm",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 29
			}, new
			{
				Id = 67,
				Alias = "continuous integration",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 30
			}, new
			{
				Id = 68,
				Alias = "continuous delivery",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 30
			}, new
			{
				Id = 69,
				Alias = "continuous deployment",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 30
			}, new
			{
				Id = 70,
				Alias = "GitHub Actions",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 30
			}, new
			{
				Id = 71,
				Alias = "Jenkins",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 30
			}, new
			{
				Id = 72,
				Alias = "GitLab CI",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 30
			}, new
			{
				Id = 73,
				Alias = "Azure Pipelines",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 30
			}, new
			{
				Id = 74,
				Alias = "version control",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 31
			}, new
			{
				Id = 75,
				Alias = "GitHub",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 31
			}, new
			{
				Id = 76,
				Alias = "GitLab",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 31
			}, new
			{
				Id = 77,
				Alias = "Bitbucket",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 31
			}, new
			{
				Id = 78,
				Alias = "Microsoft Azure",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 32
			}, new
			{
				Id = 79,
				Alias = "Amazon Web Services",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 33
			}, new
			{
				Id = 80,
				Alias = "infrastructure as code",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 34
			}, new
			{
				Id = 81,
				Alias = "IaC",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 34
			}, new
			{
				Id = 82,
				Alias = "logging",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 35
			}, new
			{
				Id = 83,
				Alias = "monitoring",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 35
			}, new
			{
				Id = 84,
				Alias = "distributed tracing",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 35
			}, new
			{
				Id = 85,
				Alias = "OpenTelemetry",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 35
			}, new
			{
				Id = 86,
				Alias = "Serilog",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 35
			}, new
			{
				Id = 87,
				Alias = "TS",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 36
			}, new
			{
				Id = 88,
				Alias = "JS",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 37
			}, new
			{
				Id = 89,
				Alias = "ECMAScript",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 37
			}, new
			{
				Id = 90,
				Alias = "React.js",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 38
			}, new
			{
				Id = 91,
				Alias = "ReactJS",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 38
			}, new
			{
				Id = 92,
				Alias = "WinForms",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 40
			}, new
			{
				Id = 93,
				Alias = "Windows Presentation Foundation",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 41
			}, new
			{
				Id = 94,
				Alias = "XAML",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 41
			}, new
			{
				Id = 95,
				Alias = "ML",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 43
			}, new
			{
				Id = 96,
				Alias = "ML.NET",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 43
			}, new
			{
				Id = 97,
				Alias = "LLM",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 44
			}, new
			{
				Id = 98,
				Alias = "LLMs",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 44
			}, new
			{
				Id = 99,
				Alias = "generative AI",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 44
			}, new
			{
				Id = 100,
				Alias = "GenAI",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 44
			}, new
			{
				Id = 101,
				Alias = "GPT",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 44
			}, new
			{
				Id = 102,
				Alias = "RAG",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 45
			}, new
			{
				Id = 103,
				Alias = "retrieval augmented generation",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 45
			}, new
			{
				Id = 104,
				Alias = "vector database",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 46
			}, new
			{
				Id = 105,
				Alias = "embeddings",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 46
			}, new
			{
				Id = 106,
				Alias = "semantic search",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 46
			}, new
			{
				Id = 107,
				Alias = "similarity search",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 46
			}, new
			{
				Id = 108,
				Alias = "agentic AI",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 47
			}, new
			{
				Id = 109,
				Alias = "tool calling",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 47
			}, new
			{
				Id = 110,
				Alias = "function calling",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 47
			}, new
			{
				Id = 111,
				Alias = "MCP",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 48
			}, new
			{
				Id = 112,
				Alias = "LangGraph",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 49
			}, new
			{
				Id = 113,
				Alias = "Ollama",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 50
			}, new
			{
				Id = 114,
				Alias = "vLLM",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 50
			}, new
			{
				Id = 115,
				Alias = "llama.cpp",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 50
			}, new
			{
				Id = 116,
				Alias = "LM Studio",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				SkillTaxonomyEntryId = 50
			});
		});
		modelBuilder.Entity("ATS.Domain.Entities.SkillTaxonomyEntry", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<string>("Category").IsRequired().ValueGeneratedOnAdd()
				.HasMaxLength(50)
				.HasColumnType("nvarchar(50)")
				.HasDefaultValue("");
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<string>("Description").IsRequired().ValueGeneratedOnAdd()
				.HasMaxLength(1000)
				.HasColumnType("nvarchar(1000)")
				.HasDefaultValue("");
			b.Property<bool>("IsActive").ValueGeneratedOnAdd().HasColumnType("bit")
				.HasDefaultValue(true);
			b.Property<string>("Name").IsRequired().HasMaxLength(200)
				.HasColumnType("nvarchar(200)");
			b.HasKey("Id");
			b.HasIndex("Category");
			b.HasIndex("Name").IsUnique();
			b.ToTable("SkillTaxonomyEntries", (string?)null);
			b.HasData(new
			{
				Id = 1,
				Category = "Database",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Microsoft relational database management system queried with T-SQL.",
				IsActive = true,
				Name = "SQL Server"
			}, new
			{
				Id = 2,
				Category = "Database",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Open-source relational database management system.",
				IsActive = true,
				Name = "PostgreSQL"
			}, new
			{
				Id = 3,
				Category = "Database",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Document-oriented NoSQL database that stores JSON-like documents.",
				IsActive = true,
				Name = "MongoDB"
			}, new
			{
				Id = 4,
				Category = "Database",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "In-memory key-value store used for caching and fast data access.",
				IsActive = true,
				Name = "Redis"
			}, new
			{
				Id = 5,
				Category = "Database",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Search and analytics engine for full-text search and log analysis.",
				IsActive = true,
				Name = "Elasticsearch"
			}, new
			{
				Id = 6,
				Category = "Data access",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Object-relational mapper that lets .NET code work with relational databases through entities.",
				IsActive = true,
				Name = "Entity Framework Core"
			}, new
			{
				Id = 7,
				Category = "Data access",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Lightweight micro ORM for .NET that maps SQL query results to objects.",
				IsActive = true,
				Name = "Dapper"
			}, new
			{
				Id = 8,
				Category = "Language",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "General-purpose object-oriented programming language of the .NET platform.",
				IsActive = true,
				Name = "C#"
			}, new
			{
				Id = 9,
				Category = "Platform",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Microsoft application development platform for web, desktop, cloud and services.",
				IsActive = true,
				Name = ".NET"
			}, new
			{
				Id = 10,
				Category = "Framework",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Web framework for building web applications and HTTP APIs on .NET.",
				IsActive = true,
				Name = "ASP.NET Core"
			}, new
			{
				Id = 11,
				Category = "Architecture",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Architectural style for web services that expose resources over HTTP.",
				IsActive = true,
				Name = "REST API"
			}, new
			{
				Id = 12,
				Category = "Architecture",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "High-performance remote procedure call framework that uses HTTP/2 and Protocol Buffers.",
				IsActive = true,
				Name = "gRPC"
			}, new
			{
				Id = 13,
				Category = "Architecture",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Query language for APIs that lets clients request exactly the data they need.",
				IsActive = true,
				Name = "GraphQL"
			}, new
			{
				Id = 14,
				Category = "Framework",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "ASP.NET library for real-time communication between server and clients.",
				IsActive = true,
				Name = "SignalR"
			}, new
			{
				Id = 15,
				Category = "Security",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Authorization framework for delegated access, commonly combined with OpenID Connect for authentication.",
				IsActive = true,
				Name = "OAuth 2.0"
			}, new
			{
				Id = 16,
				Category = "Security",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Compact signed token format used to carry identity and claims between services.",
				IsActive = true,
				Name = "JWT"
			}, new
			{
				Id = 17,
				Category = "Security",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Authorization model in which permissions are granted to roles that users hold.",
				IsActive = true,
				Name = "Role-based access control"
			}, new
			{
				Id = 18,
				Category = "Testing",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Automated tests that verify small units of code in isolation.",
				IsActive = true,
				Name = "Unit testing"
			}, new
			{
				Id = 19,
				Category = "Testing",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Automated tests that verify how components work together, for example against a real database.",
				IsActive = true,
				Name = "Integration testing"
			}, new
			{
				Id = 20,
				Category = "Architecture",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Layered design that keeps business logic independent of frameworks and infrastructure.",
				IsActive = true,
				Name = "Clean Architecture"
			}, new
			{
				Id = 21,
				Category = "Architecture",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Software design approach that models the code around the business domain.",
				IsActive = true,
				Name = "Domain-driven design"
			}, new
			{
				Id = 22,
				Category = "Architecture",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Architecture that splits a system into small independently deployable services.",
				IsActive = true,
				Name = "Microservices"
			}, new
			{
				Id = 23,
				Category = "Architecture",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Architecture in which components communicate through asynchronous events and messages.",
				IsActive = true,
				Name = "Event-driven architecture"
			}, new
			{
				Id = 24,
				Category = "Messaging",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Message broker that routes messages between producers and consumers through queues.",
				IsActive = true,
				Name = "RabbitMQ"
			}, new
			{
				Id = 25,
				Category = "Messaging",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = ".NET library that provides a message bus abstraction over brokers such as RabbitMQ.",
				IsActive = true,
				Name = "MassTransit"
			}, new
			{
				Id = 26,
				Category = "Messaging",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Distributed event streaming platform for high-throughput data pipelines.",
				IsActive = true,
				Name = "Apache Kafka"
			}, new
			{
				Id = 27,
				Category = "Architecture",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Pattern that stores outgoing events in the database transaction to publish them reliably.",
				IsActive = true,
				Name = "Outbox pattern"
			}, new
			{
				Id = 28,
				Category = "DevOps",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Platform for packaging applications into containers.",
				IsActive = true,
				Name = "Docker"
			}, new
			{
				Id = 29,
				Category = "DevOps",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Container orchestration platform for deploying and scaling containerized applications.",
				IsActive = true,
				Name = "Kubernetes"
			}, new
			{
				Id = 30,
				Category = "DevOps",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Automated pipelines that build, test and deploy software.",
				IsActive = true,
				Name = "CI/CD"
			}, new
			{
				Id = 31,
				Category = "DevOps",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Distributed version control system.",
				IsActive = true,
				Name = "Git"
			}, new
			{
				Id = 32,
				Category = "Cloud",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Microsoft cloud computing platform.",
				IsActive = true,
				Name = "Azure"
			}, new
			{
				Id = 33,
				Category = "Cloud",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Amazon cloud computing platform.",
				IsActive = true,
				Name = "AWS"
			}, new
			{
				Id = 34,
				Category = "DevOps",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Infrastructure as code tool for provisioning cloud resources declaratively.",
				IsActive = true,
				Name = "Terraform"
			}, new
			{
				Id = 35,
				Category = "DevOps",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Logging, metrics and tracing used to monitor and troubleshoot running applications.",
				IsActive = true,
				Name = "Observability"
			}, new
			{
				Id = 36,
				Category = "Language",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Typed superset of JavaScript that compiles to plain JavaScript.",
				IsActive = true,
				Name = "TypeScript"
			}, new
			{
				Id = 37,
				Category = "Language",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Scripting language of web browsers and Node.js.",
				IsActive = true,
				Name = "JavaScript"
			}, new
			{
				Id = 38,
				Category = "Frontend",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "JavaScript library for building component-based user interfaces.",
				IsActive = true,
				Name = "React"
			}, new
			{
				Id = 39,
				Category = "Frontend",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "TypeScript-based framework for building single-page web applications.",
				IsActive = true,
				Name = "Angular"
			}, new
			{
				Id = 40,
				Category = "Frontend",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Desktop UI framework for building Windows applications on .NET.",
				IsActive = true,
				Name = "Windows Forms"
			}, new
			{
				Id = 41,
				Category = "Frontend",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "XAML-based desktop UI framework for rich Windows applications.",
				IsActive = true,
				Name = "WPF"
			}, new
			{
				Id = 42,
				Category = "Language",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "General-purpose programming language widely used for scripting, data and AI.",
				IsActive = true,
				Name = "Python"
			}, new
			{
				Id = 43,
				Category = "AI",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Techniques that let software learn patterns from data.",
				IsActive = true,
				Name = "Machine learning"
			}, new
			{
				Id = 44,
				Category = "AI",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Neural language models that generate and understand text.",
				IsActive = true,
				Name = "Large language models"
			}, new
			{
				Id = 45,
				Category = "AI",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Technique that retrieves relevant knowledge and gives it to a language model as context.",
				IsActive = true,
				Name = "Retrieval-augmented generation"
			}, new
			{
				Id = 46,
				Category = "AI",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Search that compares embedding vectors to find semantically similar content.",
				IsActive = true,
				Name = "Vector search"
			}, new
			{
				Id = 47,
				Category = "AI",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Systems in which a language model plans steps and calls tools to complete tasks.",
				IsActive = true,
				Name = "AI agents"
			}, new
			{
				Id = 48,
				Category = "AI",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Open protocol that lets language models connect to external tools and data sources.",
				IsActive = true,
				Name = "Model Context Protocol"
			}, new
			{
				Id = 49,
				Category = "AI",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Framework for building applications and agents around language models.",
				IsActive = true,
				Name = "LangChain"
			}, new
			{
				Id = 50,
				Category = "AI",
				CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				Description = "Running language models on your own hardware with tools such as Ollama.",
				IsActive = true,
				Name = "Local model inference"
			});
		});
		modelBuilder.Entity("ATS.Domain.Entities.AnalysisAudit", (EntityTypeBuilder b) =>
		{
			b.HasOne("ATS.Domain.Entities.CvScan", "CvScan").WithOne("AnalysisAudit").HasForeignKey("ATS.Domain.Entities.AnalysisAudit", "CvScanId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("CvScan");
		});
		modelBuilder.Entity("ATS.Domain.Entities.AnalysisWarning", (EntityTypeBuilder b) =>
		{
			b.HasOne("ATS.Domain.Entities.CvScan", "CvScan").WithMany("AnalysisWarnings").HasForeignKey("CvScanId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("CvScan");
		});
		modelBuilder.Entity("ATS.Domain.Entities.CvScan", (EntityTypeBuilder b) =>
		{
			b.HasOne("ATS.Domain.Entities.JobPosting", "JobPosting").WithMany("CvScans").HasForeignKey("JobPostingId")
				.OnDelete(DeleteBehavior.SetNull);
			b.Navigation("JobPosting");
		});
		modelBuilder.Entity("ATS.Domain.Entities.JobRequirement", (EntityTypeBuilder b) =>
		{
			b.HasOne("ATS.Domain.Entities.JobPosting", "JobPosting").WithMany("JobRequirements").HasForeignKey("JobPostingId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("JobPosting");
		});
		modelBuilder.Entity("ATS.Domain.Entities.RequirementInterpretation", (EntityTypeBuilder b) =>
		{
			b.HasOne("ATS.Domain.Entities.RequirementMatch", "RequirementMatch").WithOne("RequirementInterpretation").HasForeignKey("ATS.Domain.Entities.RequirementInterpretation", "RequirementMatchId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("RequirementMatch");
		});
		modelBuilder.Entity("ATS.Domain.Entities.RequirementMatch", (EntityTypeBuilder b) =>
		{
			b.HasOne("ATS.Domain.Entities.CvScan", "CvScan").WithMany("RequirementMatches").HasForeignKey("CvScanId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.HasOne("ATS.Domain.Entities.JobRequirement", "JobRequirement").WithMany("RequirementMatches").HasForeignKey("JobRequirementId")
				.OnDelete(DeleteBehavior.Restrict)
				.IsRequired();
			b.Navigation("CvScan");
			b.Navigation("JobRequirement");
		});
		modelBuilder.Entity("ATS.Domain.Entities.ScoreReport", (EntityTypeBuilder b) =>
		{
			b.HasOne("ATS.Domain.Entities.CvScan", "CvScan").WithMany("ScoreReports").HasForeignKey("CvScanId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("CvScan");
		});
		modelBuilder.Entity("ATS.Domain.Entities.SectionScore", (EntityTypeBuilder b) =>
		{
			b.HasOne("ATS.Domain.Entities.CvScan", "CvScan").WithMany("SectionScores").HasForeignKey("CvScanId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("CvScan");
		});
		modelBuilder.Entity("ATS.Domain.Entities.SecurityFinding", (EntityTypeBuilder b) =>
		{
			b.HasOne("ATS.Domain.Entities.CvScan", "CvScan").WithMany("SecurityFindings").HasForeignKey("CvScanId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("CvScan");
		});
		modelBuilder.Entity("ATS.Domain.Entities.SkillTaxonomyAlias", (EntityTypeBuilder b) =>
		{
			b.HasOne("ATS.Domain.Entities.SkillTaxonomyEntry", "SkillTaxonomyEntry").WithMany("SkillTaxonomyAliases").HasForeignKey("SkillTaxonomyEntryId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("SkillTaxonomyEntry");
		});
		modelBuilder.Entity("ATS.Domain.Entities.CvScan", (EntityTypeBuilder b) =>
		{
			b.Navigation("AnalysisAudit");
			b.Navigation("AnalysisWarnings");
			b.Navigation("RequirementMatches");
			b.Navigation("ScoreReports");
			b.Navigation("SectionScores");
			b.Navigation("SecurityFindings");
		});
		modelBuilder.Entity("ATS.Domain.Entities.JobPosting", (EntityTypeBuilder b) =>
		{
			b.Navigation("CvScans");
			b.Navigation("JobRequirements");
		});
		modelBuilder.Entity("ATS.Domain.Entities.JobRequirement", (EntityTypeBuilder b) =>
		{
			b.Navigation("RequirementMatches");
		});
		modelBuilder.Entity("ATS.Domain.Entities.RequirementMatch", (EntityTypeBuilder b) =>
		{
			b.Navigation("RequirementInterpretation");
		});
		modelBuilder.Entity("ATS.Domain.Entities.SkillTaxonomyEntry", (EntityTypeBuilder b) =>
		{
			b.Navigation("SkillTaxonomyAliases");
		});
	}
}
