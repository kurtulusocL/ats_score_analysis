using ATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATS.Infrastructure.Persistence.Seed
{
    public static class SkillTaxonomySeed
    {
        private static readonly DateTime SeedCreatedAt = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static IReadOnlyList<SkillTaxonomySeedEntry> Entries { get; } = new SkillTaxonomySeedEntry[]
        {
            new("SQL Server", "Database", "Microsoft relational database management system queried with T-SQL.", "MSSQL", "Microsoft SQL Server", "T-SQL", "Transact-SQL"),
            new("PostgreSQL", "Database", "Open-source relational database management system.", "Postgres"),
            new("MongoDB", "Database", "Document-oriented NoSQL database that stores JSON-like documents.", "Mongo", "NoSQL"),
            new("Redis", "Database", "In-memory key-value store used for caching and fast data access.", "Redis Cache", "distributed cache"),
            new("Elasticsearch", "Database", "Search and analytics engine for full-text search and log analysis.", "Elastic Search", "OpenSearch", "ELK"),
            new("Entity Framework Core", "Data access", "Object-relational mapper that lets .NET code work with relational databases through entities.", "EF Core", "Entity Framework", "ORM"),
            new("Dapper", "Data access", "Lightweight micro ORM for .NET that maps SQL query results to objects.", "micro ORM"),
            new("C#", "Language", "General-purpose object-oriented programming language of the .NET platform.", "C Sharp", "CSharp"),
            new(".NET", "Platform", "Microsoft application development platform for web, desktop, cloud and services.", "dotnet", ".NET Core", ".NET Framework"),
            new("ASP.NET Core", "Framework", "Web framework for building web applications and HTTP APIs on .NET.", "ASP.NET", "ASP.NET MVC", "ASP.NET Web API", "Web API"),
            new("REST API", "Architecture", "Architectural style for web services that expose resources over HTTP.", "RESTful", "RESTful API", "REST services"),
            new("gRPC", "Architecture", "High-performance remote procedure call framework that uses HTTP/2 and Protocol Buffers.", "Protocol Buffers", "protobuf"),
            new("GraphQL", "Architecture", "Query language for APIs that lets clients request exactly the data they need."),
            new("SignalR", "Framework", "ASP.NET library for real-time communication between server and clients.", "WebSockets", "real-time communication"),
            new("OAuth 2.0", "Security", "Authorization framework for delegated access, commonly combined with OpenID Connect for authentication.", "OAuth", "OAuth2", "OpenID Connect", "OIDC"),
            new("JWT", "Security", "Compact signed token format used to carry identity and claims between services.", "JSON Web Token", "bearer token"),
            new("Role-based access control", "Security", "Authorization model in which permissions are granted to roles that users hold.", "RBAC", "role-based authorization"),
            new("Unit testing", "Testing", "Automated tests that verify small units of code in isolation.", "unit tests", "xUnit", "NUnit", "MSTest"),
            new("Integration testing", "Testing", "Automated tests that verify how components work together, for example against a real database.", "integration tests"),
            new("Clean Architecture", "Architecture", "Layered design that keeps business logic independent of frameworks and infrastructure.", "Onion Architecture", "layered architecture", "hexagonal architecture"),
            new("Domain-driven design", "Architecture", "Software design approach that models the code around the business domain.", "DDD"),
            new("Microservices", "Architecture", "Architecture that splits a system into small independently deployable services.", "microservice architecture"),
            new("Event-driven architecture", "Architecture", "Architecture in which components communicate through asynchronous events and messages.", "event-driven", "publish-subscribe", "pub/sub", "asynchronous messaging"),
            new("RabbitMQ", "Messaging", "Message broker that routes messages between producers and consumers through queues.", "message broker", "AMQP"),
            new("MassTransit", "Messaging", ".NET library that provides a message bus abstraction over brokers such as RabbitMQ.", "message bus"),
            new("Apache Kafka", "Messaging", "Distributed event streaming platform for high-throughput data pipelines.", "Kafka", "event streaming"),
            new("Outbox pattern", "Architecture", "Pattern that stores outgoing events in the database transaction to publish them reliably.", "transactional outbox"),
            new("Docker", "DevOps", "Platform for packaging applications into containers.", "containers", "containerization", "Dockerfile"),
            new("Kubernetes", "DevOps", "Container orchestration platform for deploying and scaling containerized applications.", "K8s", "container orchestration", "Helm"),
            new("CI/CD", "DevOps", "Automated pipelines that build, test and deploy software.", "continuous integration", "continuous delivery", "continuous deployment", "GitHub Actions", "Jenkins", "GitLab CI", "Azure Pipelines"),
            new("Git", "DevOps", "Distributed version control system.", "version control", "GitHub", "GitLab", "Bitbucket"),
            new("Azure", "Cloud", "Microsoft cloud computing platform.", "Microsoft Azure"),
            new("AWS", "Cloud", "Amazon cloud computing platform.", "Amazon Web Services"),
            new("Terraform", "DevOps", "Infrastructure as code tool for provisioning cloud resources declaratively.", "infrastructure as code", "IaC"),
            new("Observability", "DevOps", "Logging, metrics and tracing used to monitor and troubleshoot running applications.", "logging", "monitoring", "distributed tracing", "OpenTelemetry", "Serilog"),
            new("TypeScript", "Language", "Typed superset of JavaScript that compiles to plain JavaScript.", "TS"),
            new("JavaScript", "Language", "Scripting language of web browsers and Node.js.", "JS", "ECMAScript"),
            new("React", "Frontend", "JavaScript library for building component-based user interfaces.", "React.js", "ReactJS"),
            new("Angular", "Frontend", "TypeScript-based framework for building single-page web applications."),
            new("Windows Forms", "Frontend", "Desktop UI framework for building Windows applications on .NET.", "WinForms"),
            new("WPF", "Frontend", "XAML-based desktop UI framework for rich Windows applications.", "Windows Presentation Foundation", "XAML"),
            new("Python", "Language", "General-purpose programming language widely used for scripting, data and AI."),
            new("Machine learning", "AI", "Techniques that let software learn patterns from data.", "ML", "ML.NET"),
            new("Large language models", "AI", "Neural language models that generate and understand text.", "LLM", "LLMs", "generative AI", "GenAI", "GPT"),
            new("Retrieval-augmented generation", "AI", "Technique that retrieves relevant knowledge and gives it to a language model as context.", "RAG", "retrieval augmented generation"),
            new("Vector search", "AI", "Search that compares embedding vectors to find semantically similar content.", "vector database", "embeddings", "semantic search", "similarity search"),
            new("AI agents", "AI", "Systems in which a language model plans steps and calls tools to complete tasks.", "agentic AI", "tool calling", "function calling"),
            new("Model Context Protocol", "AI", "Open protocol that lets language models connect to external tools and data sources.", "MCP"),
            new("LangChain", "AI", "Framework for building applications and agents around language models.", "LangGraph"),
            new("Local model inference", "AI", "Running language models on your own hardware with tools such as Ollama.", "Ollama", "vLLM", "llama.cpp", "LM Studio")
        };

        public static void Seed(ModelBuilder modelBuilder)
        {
            var skillTaxonomyEntries = new List<SkillTaxonomyEntry>();
            var skillTaxonomyAliases = new List<SkillTaxonomyAlias>();
            var nextAliasId = 1;

            for (var index = 0; index < Entries.Count; index++)
            {
                var seedEntry = Entries[index];
                var entryId = index + 1;

                skillTaxonomyEntries.Add(new SkillTaxonomyEntry
                {
                    Id = entryId,
                    Name = seedEntry.Name,
                    Category = seedEntry.Category,
                    Description = seedEntry.Description,
                    IsActive = true,
                    CreatedAt = SeedCreatedAt
                });

                foreach (var alias in seedEntry.Aliases)
                {
                    skillTaxonomyAliases.Add(new SkillTaxonomyAlias
                    {
                        Id = nextAliasId++,
                        Alias = alias,
                        SkillTaxonomyEntryId = entryId,
                        CreatedAt = SeedCreatedAt
                    });
                }
            }

            modelBuilder.Entity<SkillTaxonomyEntry>().HasData(skillTaxonomyEntries);
            modelBuilder.Entity<SkillTaxonomyAlias>().HasData(skillTaxonomyAliases);
        }
    }
}
