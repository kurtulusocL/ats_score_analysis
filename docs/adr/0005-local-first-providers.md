# 0005. Local-first providers with fallback, no key required

- Status: Accepted
- Date: 2026-10-01

## Context

CVs are personal data. The tool must be usable without paying for an API, without an API key, and without a CV leaving the machine.

## Decision

- `Ai:ProviderPriority` lists providers in the order they are tried (`Ollama`, then `OpenAiCompatible` by default). The first one that is reachable and has its models is used.
- One provider serves a whole analysis, because vectors of different providers are not comparable. The choice is made before the first call, not in the middle.
- An OpenAI-compatible server does not need a key (some local servers want none). If the server answers 401 or 403, the analysis continues with a warning.
- An API key is never stored in `appsettings.json`. It comes from user secrets or the environment variable `Ai__OpenAiCompatible__ApiKey` and is read on every request.

## Consequences

- With the local provider no CV text leaves the machine. With a hosted provider it does; the README says so.
- A key added while the application runs is picked up without a restart.
- There is no settings screen yet; configuration is file based.
