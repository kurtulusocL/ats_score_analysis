# 0003. Rule-based Job Match, a score cap and a 30/70 total

- Status: Accepted
- Date: 2026-10-01

## Context

The first Job Match compared word sets and a title match. A software developer's CV scored 81 against an accountant's posting, because a fixed CV-quality part dominated the total. Domain-specific keyword lists were rejected: the tool has to work for any profession.

## Decision

- The posting is split into requirement lines. Each line is looked up in the whole CV by its distinctive terms, weighted by how rare the term is across the posting's lines.
- A line is Met at 80% weighted coverage and Partial at 40%. A years requirement is compared with the experience computed from the CV's date ranges.
- Mandatory lines weigh twice as much as optional ones.
- Keyword matching cannot prove competence, so the rule-based Job Match score is capped at 70% of its maximum.
- With a posting the total is 30% CV quality and 70% job fit.

## Consequences

- Unrelated postings now give a low total; a perfect CV cannot hide a poor fit.
- Synonyms are invisible to the rule-based part (desktop UI against WPF). The optional semantic layer exists for that.
- Weights, thresholds and the cap live in two places (`JobMatchScoringRules`, `OverallScoreCalculator`) and are defaults, not values tuned on a labelled dataset.
