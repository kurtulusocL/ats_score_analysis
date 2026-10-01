# 0009. A skill taxonomy as data, used for retrieval and not as evidence

- Status: Accepted
- Date: 2026-10-01

## Context

A model reading "MSSQL" in a CV and "SQL Server" in a posting should recognise them as the same thing. A fixed list inside prompts does not scale, and a list inside code needs a release for every new synonym.

## Decision

Skills, aliases and short descriptions live in the `SkillTaxonomyEntries` and `SkillTaxonomyAliases` tables (seeded with a small .NET-oriented set). For each requirement the retriever embeds the requirement name, compares it with the embedded taxonomy entries, and returns the best few above a similarity threshold. They are handed to the interpretation prompt as a separate document and the instruction says to use them only to recognise equivalent wording. They are never evidence.

There is no separate ingestion service: the embedding cache already stores the vectors of the taxonomy texts after their first use.

## Consequences

- Adding a skill or a synonym is a row, not a code change.
- Requirements outside the taxonomy get no extra context; the interpretation still works without it.
- The first analysis after a taxonomy change embeds the new entries once.
