using System;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ATS.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260930111934_init2")]
public class init2 : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		Type typeFromHandle = typeof(string);
		int? oldMaxLength = 1000;
		migrationBuilder.AlterColumn<string>("Feedback", "SectionScores", "nvarchar(max)", null, null, rowVersion: false, null, nullable: false, "", null, null, typeFromHandle, "nvarchar(1000)", null, oldMaxLength);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		int? maxLength = 1000;
		Type typeFromHandle = typeof(string);
		migrationBuilder.AlterColumn<string>("Feedback", "SectionScores", "nvarchar(1000)", null, maxLength, rowVersion: false, null, nullable: false, null, null, null, typeFromHandle, "nvarchar(max)", null, null, oldRowVersion: false, oldNullable: false, "");
	}

	protected override void BuildTargetModel(ModelBuilder modelBuilder)
	{
		modelBuilder.HasAnnotation("ProductVersion", "9.0.14").HasAnnotation("Proxies:ChangeTracking", false).HasAnnotation("Proxies:CheckEquality", false)
			.HasAnnotation("Proxies:LazyLoading", true)
			.HasAnnotation("Relational:MaxIdentifierLength", 128);
		modelBuilder.UseIdentityColumns(1L);
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
		modelBuilder.Entity("ATS.Domain.Entities.JobPosting", (EntityTypeBuilder b) =>
		{
			b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
			b.Property<int>("Id").UseIdentityColumn(1L);
			b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
			b.Property<int>("MatchScore").ValueGeneratedOnAdd().HasColumnType("int")
				.HasDefaultValue(0);
			b.Property<string>("RawText").IsRequired().HasColumnType("nvarchar(max)");
			b.Property<string>("Title").IsRequired().HasMaxLength(300)
				.HasColumnType("nvarchar(300)");
			b.HasKey("Id");
			b.ToTable("JobPostings", (string?)null);
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
		modelBuilder.Entity("ATS.Domain.Entities.CvScan", (EntityTypeBuilder b) =>
		{
			b.HasOne("ATS.Domain.Entities.JobPosting", "JobPosting").WithMany("CvScans").HasForeignKey("JobPostingId")
				.OnDelete(DeleteBehavior.SetNull);
			b.Navigation("JobPosting");
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
		modelBuilder.Entity("ATS.Domain.Entities.CvScan", (EntityTypeBuilder b) =>
		{
			b.Navigation("ScoreReports");
			b.Navigation("SectionScores");
		});
		modelBuilder.Entity("ATS.Domain.Entities.JobPosting", (EntityTypeBuilder b) =>
		{
			b.Navigation("CvScans");
		});
	}
}
