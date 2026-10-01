# 0001. Use Microsoft.Extensions.AI as the AI abstraction

- Status: Accepted
- Date: 2026-10-01

## Context

The AI layer has to talk to a local model server and to hosted APIs without tying the application code to one vendor SDK. Semantic Kernel would add an orchestration framework that this project does not need: it has a fixed pipeline of a few calls.

## Decision

Application code depends on `IChatClient` and `IEmbeddingGenerator` from Microsoft.Extensions.AI. Ollama is reached through OllamaSharp (the `Microsoft.Extensions.AI.Ollama` package is deprecated), OpenAI-compatible servers through `Microsoft.Extensions.AI.OpenAI`. Azure OpenAI is out of scope. Provider-specific code lives in `ATS.Infrastructure` behind `IAiProviderClientFactory`.

## Consequences

- Adding a provider means one client factory and one availability checker.
- Every call goes through one logging decorator that records duration and token counts.
- Vendor features that the abstraction does not expose are not available.
