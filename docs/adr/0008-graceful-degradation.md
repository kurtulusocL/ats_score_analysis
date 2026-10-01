# 0008. Degrade gracefully instead of failing the analysis

- Status: Accepted
- Date: 2026-10-01

## Context

The application depends on things it does not control: a translation web service, a model server, a model that may be missing, files that may not scan. A CV analysis should not be lost because one of them fails.

## Decision

Every optional step has a fallback that finishes the analysis and adds a warning that is saved with it:

| Step | If it fails |
|---|---|
| AI disabled, not configured, unreachable, model missing | rule-based score |
| Requirement extraction or embedding match | rule-based Job Match score |
| Hybrid score | rule-based Job Match score |
| Interpretation | no explanations, score unchanged |
| Skill knowledge retrieval | interpretation without that context |
| Translation | original text, with a warning that scores may be unreliable |
| Security scan | no findings, with a warning |

Cancellation by the user is never swallowed. The AI components are resolved lazily, so a broken AI configuration cannot break the rule-based path.

## Consequences

- The user always gets a result and sees why part of it is missing.
- Warnings are stored, so they also show when an old analysis is reopened.
