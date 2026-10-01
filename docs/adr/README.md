# Architecture decision records

Short records of the decisions that shape this project.

| # | Decision |
|---|---|
| [0001](0001-use-microsoft-extensions-ai.md) | Use Microsoft.Extensions.AI as the AI abstraction |
| [0002](0002-the-model-never-decides-the-score.md) | The language model never decides the score |
| [0003](0003-rule-based-job-match.md) | Rule-based Job Match, a score cap and a 30/70 total |
| [0004](0004-embeddings-in-sql-server.md) | Store embeddings in SQL Server and compare them in memory |
| [0005](0005-local-first-providers.md) | Local-first providers with fallback, no key required |
| [0006](0006-hidden-text-is-excluded-but-stays-visible.md) | Hidden text is excluded from scoring but stays visible |
| [0007](0007-untrusted-documents-and-model-output.md) | Treat documents and model output as untrusted |
| [0008](0008-graceful-degradation.md) | Degrade gracefully instead of failing the analysis |
| [0009](0009-skill-taxonomy-as-data.md) | A skill taxonomy as data, used for retrieval and not as evidence |
