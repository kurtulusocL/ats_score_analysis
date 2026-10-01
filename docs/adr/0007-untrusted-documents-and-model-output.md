# 0007. Treat documents and model output as untrusted

- Status: Accepted
- Date: 2026-10-01

## Context

A CV and a job posting are written by third parties and are put in front of a model. Both can contain instructions aimed at the model.

## Decision

- **Delimiters.** Each call creates a random 32-character token. Documents are placed between `BEGIN` and `END` markers that carry the token; the system message states that everything between them is data and must not be obeyed. A document that happens to contain the token gets a new one.
- **Strict schemas.** Replies must be exactly the agreed JSON (one outer code fence at most). Extra, missing or duplicated properties, wrong types, over-long or control-character text are rejected. Error messages never repeat model text.
- **One retry.** The model is asked once more with the rejection reasons, never with its own rejected output. After a second failure the step is skipped with a warning.
- **Grounding.** A "met" or "partial" decision needs a quote that appears in the CV (compared without regard to case and whitespace). Otherwise the decision becomes "missing".
- **Detection as a warning layer.** Known injection phrases (English and Turkish) in the CV and in the posting are reported.

## Consequences

- A model that is talked into something still cannot invent evidence or a score.
- A model that paraphrases instead of quoting is downgraded. That is deliberately strict.
- The phrase list is not complete; the structure above is the real defense.
