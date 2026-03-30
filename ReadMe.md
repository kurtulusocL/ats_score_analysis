ATS - AI-Enhanced Resume Analysis & Scoring System
This project is a .NET 9 based desktop (Windows Forms) application designed to automate recruitment workflows using Symbolic AI and Fuzzy Logic algorithms. It parses complex resume files (PDF, Word, TXT) to perform intelligent scoring and detailed reporting based on predefined professional criteria.

🚀 Purpose and Working Logic
In modern HR, filtering thousands of resumes manually is inefficient. This application bridges the gap between raw data and decision-making by:

Data Extraction: Scans and converts resumes into structured text data using PdfPig and OpenXML.

Smart Segmentation: Identifies and extracts critical sections like "Education", "Experience", and "Skills" using pattern-matching heuristics.

Symbolic AI & Fuzzy Logic: Unlike rigid string matching, the system employs Fuzzy Matching to simulate human-like understanding by tolerating typos, abbreviations, and naming variations. This ensures a high-accuracy Eligibility Score.

Automated Reporting: Generates professional, data-driven PDF reports via QuestPDF for stakeholders.

Persistence: Maintains an audit trail and historical analysis data on SQL Server.

🛠 Tech Stack
The project is built on Clean (Onion) Architecture to ensure high maintainability and clear separation of concerns.

Framework: .NET 9.0 (Windows Forms)

Architecture: Clean Architecture (Domain, Application, Infrastructure, Core)

Intelligence Layer: Symbolic AI (Fuzzy Logic & Rule-Based Heuristics)

Database & ORM: SQL Server & Entity Framework Core (Code-First)

OCR & File Processing: PdfPig (PDF), DocumentFormat.OpenXml (Word), TXT

Reporting: QuestPDF (Modern, code-based PDF generation)

Logging: Serilog (Structured logging with Environment enrichment)

Dependency Injection: Microsoft.Extensions.DependencyInjection

External API: RapidApi / Google Search API (For contextual verification)

📂 Project Structure
ATS.Domain: Core business entities and domain constants.

ATS.Core: Infrastructure-wide helpers, extensions, and logging.

ATS.Application: Business logic, service interfaces, and the AI Scoring Engine.

ATS.Infrastructure: Data persistence, external service implementations, and PDF generation.

ATS.FromUI: Presentation layer and application entry point.

⚙️ Setup and Configuration
Configure the ConnectionStrings in appsettings.json.

Provide your RapidApi credentials in the configuration file.

Execute Update-Database via Package Manager Console to initialize the schema.

Build and run the solution.

🔮 Roadmap and Future Updates
While the current version utilizes high-performance Symbolic AI, the next major release will pivot towards Connectionist AI:

Integration of a local Machine Learning (ML.NET) library for predictive talent acquisition.

Semantic Analysis to understand the deeper context of professional achievements beyond keywords.

Career path forecasting using historical candidate data.