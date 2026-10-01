# 0006. Hidden text is excluded from scoring but stays visible

- Status: Accepted (supersedes an earlier decision to flag it only)
- Date: 2026-10-01

## Context

A CV can contain text that a human never sees but a parser reads: white text, text below 3 pt, text outside the page, Word's hidden formatting. First it was only flagged, which left a hole: keywords stacked in white text could still raise the Job Match score.

## Decision

Hidden text found by the PDF and DOCX scanners is removed from the text that is scored, translated and sent to the model. The raw text and the findings are stored unchanged and the full hidden text is listed in the feedback and in the PDF report under its own heading, so a reader knows the CV contains it. Instruction-like text in visible places is only reported, not removed.

## Consequences

- Stacking keywords in hidden text no longer helps.
- Legitimate white text on a dark sidebar cannot be told from hidden text, because a PDF page's background colour is not available. That text is excluded from scoring as well. This is accepted and documented under limitations.
- If the extracted text does not contain a hidden segment (for example it was split across lines in an unexpected way), the analysis carries a warning that it may not have been excluded.
