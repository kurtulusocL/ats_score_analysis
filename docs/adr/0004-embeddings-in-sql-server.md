# 0004. Store embeddings in SQL Server and compare them in memory

- Status: Accepted
- Date: 2026-10-01

## Context

The project targets SQL Server 2022, which has no native vector type (SQL Server 2025 does), and EF Core 9 has no vector support. The vector sets involved are small: a CV's chunks, a posting's requirements and a taxonomy of some dozens of entries.

## Decision

Vectors are stored as `varbinary` in `EmbeddingCacheEntries`, unique on the text hash and the provider-qualified model name (for example `Ollama:nomic-embed-text`). Cosine similarity is computed in memory. Everything goes through `IEmbeddingService`, which looks up the cache first and sends only missing texts to the model.

## Consequences

- No extra infrastructure; repeated texts are never embedded twice.
- Vectors from different providers or models are never compared.
- This is not a vector database. It fits hundreds of vectors per analysis, not millions. If the data grows, replace `IEmbeddingService` and the retriever behind their interfaces.
