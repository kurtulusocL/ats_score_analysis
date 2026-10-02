using System;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace ATS.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260930101335_init1")]
public class init1 : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable("AnalyzerKeywords", (ColumnsBuilder table) =>
		{
			OperationBuilder<AddColumnOperation> id = table.Column<int>("int").Annotation("SqlServer:Identity", "1, 1");
			int? maxLength = 100;
			OperationBuilder<AddColumnOperation> word = table.Column<string>("nvarchar(100)", null, maxLength);
			maxLength = 50;
			OperationBuilder<AddColumnOperation> category = table.Column<string>("nvarchar(50)", null, maxLength);
			maxLength = 50;
			return new
			{
				Id = id,
				Word = word,
				Category = category,
				SubCategory = table.Column<string>("nvarchar(50)", null, maxLength, rowVersion: false, null, nullable: false, ""),
				IsActive = table.Column<bool>("bit", null, null, rowVersion: false, null, nullable: false, true),
				CreatedAt = table.Column<DateTime>("datetime2")
			};
		}, null, table =>
		{
			table.PrimaryKey("PK_AnalyzerKeywords", x => x.Id);
		});
		migrationBuilder.CreateTable("JobPostings", (ColumnsBuilder table) =>
		{
			OperationBuilder<AddColumnOperation> id = table.Column<int>("int").Annotation("SqlServer:Identity", "1, 1");
			int? maxLength = 300;
			return new
			{
				Id = id,
				Title = table.Column<string>("nvarchar(300)", null, maxLength),
				RawText = table.Column<string>("nvarchar(max)"),
				MatchScore = table.Column<int>("int", null, null, rowVersion: false, null, nullable: false, 0),
				CreatedAt = table.Column<DateTime>("datetime2")
			};
		}, null, table =>
		{
			table.PrimaryKey("PK_JobPostings", x => x.Id);
		});
		migrationBuilder.CreateTable("CvScans", (ColumnsBuilder table) =>
		{
			OperationBuilder<AddColumnOperation> id = table.Column<int>("int").Annotation("SqlServer:Identity", "1, 1");
			int? maxLength = 200;
			OperationBuilder<AddColumnOperation> candidateName = table.Column<string>("nvarchar(200)", null, maxLength);
			OperationBuilder<AddColumnOperation> rawText = table.Column<string>("nvarchar(max)");
			maxLength = 10;
			OperationBuilder<AddColumnOperation> fileType = table.Column<string>("nvarchar(10)", null, maxLength);
			maxLength = 500;
			return new
			{
				Id = id,
				CandidateName = candidateName,
				RawText = rawText,
				FileType = fileType,
				FilePath = table.Column<string>("nvarchar(500)", null, maxLength),
				OverallScore = table.Column<int>("int", null, null, rowVersion: false, null, nullable: false, 0),
				IsJobMatched = table.Column<bool>("bit", null, null, rowVersion: false, null, nullable: false, false),
				JobPostingId = table.Column<int>("int", null, null, rowVersion: false, null, nullable: true),
				CreatedAt = table.Column<DateTime>("datetime2")
			};
		}, null, table =>
		{
			table.PrimaryKey("PK_CvScans", x => x.Id);
			table.ForeignKey("FK_CvScans_JobPostings_JobPostingId", x => x.JobPostingId, "JobPostings", "Id", null, ReferentialAction.NoAction, ReferentialAction.SetNull);
		});
		migrationBuilder.CreateTable("ScoreReports", (ColumnsBuilder table) =>
		{
			OperationBuilder<AddColumnOperation> id = table.Column<int>("int").Annotation("SqlServer:Identity", "1, 1");
			int? maxLength = 500;
			OperationBuilder<AddColumnOperation> reportPath = table.Column<string>("nvarchar(500)", null, maxLength);
			maxLength = 20;
			return new
			{
				Id = id,
				ReportPath = reportPath,
				ReportType = table.Column<string>("nvarchar(20)", null, maxLength),
				IsGenerated = table.Column<bool>("bit", null, null, rowVersion: false, null, nullable: false, false),
				CvScanId = table.Column<int>("int"),
				CreatedAt = table.Column<DateTime>("datetime2")
			};
		}, null, table =>
		{
			table.PrimaryKey("PK_ScoreReports", x => x.Id);
			table.ForeignKey("FK_ScoreReports_CvScans_CvScanId", x => x.CvScanId, "CvScans", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
		});
		migrationBuilder.CreateTable("SectionScores", (ColumnsBuilder table) =>
		{
			OperationBuilder<AddColumnOperation> id = table.Column<int>("int").Annotation("SqlServer:Identity", "1, 1");
			int? maxLength = 100;
			OperationBuilder<AddColumnOperation> sectionName = table.Column<string>("nvarchar(100)", null, maxLength);
			OperationBuilder<AddColumnOperation> score = table.Column<int>("int", null, null, rowVersion: false, null, nullable: false, 0);
			OperationBuilder<AddColumnOperation> maxScore = table.Column<int>("int", null, null, rowVersion: false, null, nullable: false, 0);
			maxLength = 1000;
			return new
			{
				Id = id,
				SectionName = sectionName,
				Score = score,
				MaxScore = maxScore,
				Feedback = table.Column<string>("nvarchar(1000)", null, maxLength),
				IsPassed = table.Column<bool>("bit", null, null, rowVersion: false, null, nullable: false, false),
				CvScanId = table.Column<int>("int"),
				CreatedAt = table.Column<DateTime>("datetime2")
			};
		}, null, table =>
		{
			table.PrimaryKey("PK_SectionScores", x => x.Id);
			table.ForeignKey("FK_SectionScores_CvScans_CvScanId", x => x.CvScanId, "CvScans", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
		});
		migrationBuilder.InsertData("AnalyzerKeywords", new string[6] { "Id", "Category", "CreatedAt", "IsActive", "SubCategory", "Word" }, new object[107, 6]
		{
			{
				1,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"developed"
			},
			{
				2,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"designed"
			},
			{
				3,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"implemented"
			},
			{
				4,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"built"
			},
			{
				5,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"created"
			},
			{
				6,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"launched"
			},
			{
				7,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"delivered"
			},
			{
				8,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"improved"
			},
			{
				9,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"increased"
			},
			{
				10,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"decreased"
			},
			{
				11,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"reduced"
			},
			{
				12,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"optimized"
			},
			{
				13,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"automated"
			},
			{
				14,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"managed"
			},
			{
				15,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"led"
			},
			{
				16,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"coordinated"
			},
			{
				17,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"architected"
			},
			{
				18,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"migrated"
			},
			{
				19,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"integrated"
			},
			{
				20,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"deployed"
			},
			{
				21,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"refactored"
			},
			{
				22,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"established"
			},
			{
				23,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"streamlined"
			},
			{
				24,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"accelerated"
			},
			{
				25,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"transformed"
			},
			{
				26,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"spearheaded"
			},
			{
				27,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"achieved"
			},
			{
				28,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"engineered"
			},
			{
				29,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"resolved"
			},
			{
				30,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"maintained"
			},
			{
				31,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"monitored"
			},
			{
				32,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"configured"
			},
			{
				33,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"scaled"
			},
			{
				34,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"secured"
			},
			{
				35,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"trained"
			},
			{
				36,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"mentored"
			},
			{
				37,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"collaborated"
			},
			{
				38,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"planned"
			},
			{
				39,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"executed"
			},
			{
				40,
				"ImpactVerb",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"",
				"analyzed"
			},
			{
				41,
				"PassiveIndicator",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Passive",
				"responsible for"
			},
			{
				42,
				"PassiveIndicator",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Passive",
				"worked on"
			},
			{
				43,
				"PassiveIndicator",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Passive",
				"helped with"
			},
			{
				44,
				"PassiveIndicator",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Passive",
				"assisted in"
			},
			{
				45,
				"PassiveIndicator",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Passive",
				"involved in"
			},
			{
				46,
				"PassiveIndicator",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Passive",
				"participated in"
			},
			{
				47,
				"PassiveIndicator",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Passive",
				"part of"
			},
			{
				48,
				"PassiveIndicator",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Passive",
				"contributed to"
			},
			{
				49,
				"PassiveIndicator",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Passive",
				"exposure to"
			},
			{
				50,
				"PassiveIndicator",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Passive",
				"familiar with"
			},
			{
				51,
				"PassiveIndicator",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Passive",
				"knowledge of"
			},
			{
				52,
				"PassiveIndicator",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Passive",
				"experience in"
			},
			{
				53,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"ContactInfo",
				"phone"
			},
			{
				54,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"ContactInfo",
				"email"
			},
			{
				55,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"ContactInfo",
				"tel"
			},
			{
				56,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"ContactInfo",
				"mobile"
			},
			{
				57,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"ContactInfo",
				"linkedin"
			},
			{
				58,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"ContactInfo",
				"github"
			},
			{
				59,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"ContactInfo",
				"contact"
			},
			{
				60,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"ContactInfo",
				"address"
			},
			{
				61,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"ContactInfo",
				"location"
			},
			{
				62,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Summary",
				"summary"
			},
			{
				63,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Summary",
				"profile"
			},
			{
				64,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Summary",
				"about me"
			},
			{
				65,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Summary",
				"career summary"
			},
			{
				66,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Summary",
				"professional summary"
			},
			{
				67,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Summary",
				"objective"
			},
			{
				68,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Summary",
				"about"
			},
			{
				69,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Summary",
				"personal profile"
			},
			{
				70,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Experience",
				"experience"
			},
			{
				71,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Experience",
				"work experience"
			},
			{
				72,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Experience",
				"employment history"
			},
			{
				73,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Experience",
				"professional experience"
			},
			{
				74,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Experience",
				"work history"
			},
			{
				75,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Experience",
				"career history"
			},
			{
				76,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Experience",
				"professional background"
			},
			{
				77,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Experience",
				"relevant experience"
			},
			{
				78,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Education",
				"education"
			},
			{
				79,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Education",
				"academic background"
			},
			{
				80,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Education",
				"university"
			},
			{
				81,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Education",
				"college"
			},
			{
				82,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Education",
				"degree"
			},
			{
				83,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Education",
				"bachelor"
			},
			{
				84,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Education",
				"master"
			},
			{
				85,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Education",
				"graduate"
			},
			{
				86,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Skills",
				"skills"
			},
			{
				87,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Skills",
				"technical skills"
			},
			{
				88,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Skills",
				"technologies"
			},
			{
				89,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Skills",
				"tech stack"
			},
			{
				90,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Skills",
				"tools"
			},
			{
				91,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Skills",
				"competencies"
			},
			{
				92,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Skills",
				"expertise"
			},
			{
				93,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Skills",
				"stack"
			},
			{
				94,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Skills",
				"core skills"
			},
			{
				95,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Skills",
				"core technical skills"
			},
			{
				96,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Skills",
				"professional skills"
			},
			{
				97,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Skills",
				"critical keywords"
			},
			{
				98,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Languages",
				"languages"
			},
			{
				99,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Languages",
				"language skills"
			},
			{
				100,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Languages",
				"fluent"
			},
			{
				101,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Languages",
				"native"
			},
			{
				102,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Languages",
				"proficiency"
			},
			{
				103,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Certifications",
				"certifications"
			},
			{
				104,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Certifications",
				"certificate"
			},
			{
				105,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Certifications",
				"certified"
			},
			{
				106,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Certifications",
				"credential"
			},
			{
				107,
				"SectionHeader",
				new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
				true,
				"Certifications",
				"license"
			}
		});
		migrationBuilder.CreateIndex("IX_AnalyzerKeywords_Category", "AnalyzerKeywords", "Category");
		migrationBuilder.CreateIndex("IX_AnalyzerKeywords_Word_Category", "AnalyzerKeywords", new string[2] { "Word", "Category" }, null, unique: true);
		migrationBuilder.CreateIndex("IX_CvScans_JobPostingId", "CvScans", "JobPostingId");
		migrationBuilder.CreateIndex("IX_ScoreReports_CvScanId", "ScoreReports", "CvScanId");
		migrationBuilder.CreateIndex("IX_SectionScores_CvScanId", "SectionScores", "CvScanId");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable("AnalyzerKeywords");
		migrationBuilder.DropTable("ScoreReports");
		migrationBuilder.DropTable("SectionScores");
		migrationBuilder.DropTable("CvScans");
		migrationBuilder.DropTable("JobPostings");
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
			b.Property<string>("Feedback").IsRequired().HasMaxLength(1000)
				.HasColumnType("nvarchar(1000)");
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
