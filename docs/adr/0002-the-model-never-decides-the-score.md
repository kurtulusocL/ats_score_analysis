# 0002. The language model never decides the score

- Status: Accepted
- Date: 2026-10-01

## Context

Model output varies from run to run, costs time, and can be steered by text inside a CV. A score that an applicant can influence by writing the right sentence is worse than no score.

## Decision

The score comes from the rule-based analyzers and, when the AI layer is on, from an embedding-based semantic score blended with the rule-based Job Match score (`Semantic:DeterministicWeight` and `SemanticWeight`). The model extracts requirements and writes explanations. Its decisions are stored next to the score and do not change it.

## Consequences

- Without a model the same CV always gets the same score.
- A model that has been talked into something cannot raise a score directly.
- The list of requirements that the model extracts does feed the semantic part, so a badly extracted list can shift that part. The rule-based part is unaffected.
- Explanations can disagree with the embedding match. When they are two steps apart (Met against Missing) the report flags the requirement for a manual check.
