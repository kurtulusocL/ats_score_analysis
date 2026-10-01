# ATS Score Analysis

[![CI](https://github.com/kurtulusocL/ats_score_analysis/actions/workflows/ci.yml/badge.svg)](https://github.com/kurtulusocL/ats_score_analysis/actions/workflows/ci.yml)

A Windows desktop application that scores a CV the way an applicant tracking system (ATS) would and, when you add a job posting, measures how well the CV fits it.

The scoring engine is rule-based and works offline. An optional AI layer (a local Ollama model or any OpenAI-compatible API) adds semantic matching and per-requirement explanations on top of it. The AI layer never decides the score; it explains it.

Built with .NET 9, Windows Forms, EF Core and SQL Server, structured as Clean (Onion) Architecture.

## Features

- Parses PDF (PdfPig), Word (Open XML) and plain-text CVs.
- Scores the CV out of 100 with five analyzers, shows section-level feedback and saves a PDF report (QuestPDF).
- Matches a CV against a job posting: the posting is split into requirement lines and each line is looked up in the CV.
- Optional AI layer: requirement extraction, embedding-based matching, skill-knowledge retrieval (RAG) and evidence-grounded explanations.
- Prompt-injection and hidden-text defenses for a document type that is untrusted by nature: a CV.
- Keeps a history in SQL Server, including an audit record of every analysis (scores, duration, token counts, embedding cache hits).
- Degrades gracefully: if the translation service, the security scan or the AI layer fails, the analysis finishes with a warning instead of crashing.

## How scoring works

| Analyzer | Max | What it checks |
|---|---|---|
| Section Presence | 20 | contact info, summary, experience, education, skills (required); languages, certifications (optional) |
| Format | 15 | consistent dates, bullet points, clear headers, email and phone, online profile, length against seniority |
| Keyword | 20 | action verbs, quantified achievements, vocabulary diversity, passive phrasing, keyword stuffing |
| Consistency | 25 | summary against experience, listed skills backed by experience, chronology and gaps, section order |
| Job Match | 20 | requirement coverage (only when a job posting is given) |

**Without a job posting** the four CV-quality analyzers are scaled by 1.25 to a total of 100.

**With a job posting** the total is `round(100 × (0.30 × CV quality + 0.70 × job fit))`. The section rows show each analyzer's own score and do not add up to the total; the report says so.

A total of 75 or more is reported as PASSED. The report also lists the findings of the security scan and any warnings.

### Job Match (rule-based)

1. The posting is split into requirement lines. Bullets, numbered items, "Preferred / Nice to have" headings and "N+ years" lines are recognised.
2. Each line is reduced to its distinctive terms (language-independent stop words are dropped, simple stemming maps `develop / developer / development` together). Terms that appear in few lines weigh more than terms that appear everywhere.
3. A line is *Met* at 80% weighted term coverage and *Partial* at 40%. A years requirement is compared with the experience computed from the CV's date ranges (education and career breaks are ignored).
4. Mandatory lines weigh twice as much as optional ones. The job title counts as a mandatory requirement.
5. Keyword matching cannot prove competence, so the rule-based Job Match score is capped at 70% of its maximum.

Section headers are recognised with fuzzy string matching (Levenshtein distance) on top of a keyword list. There is no machine-learning model in the rule-based engine.

## The AI layer (optional)

Off by default (`Ai:Enabled = false`). When enabled and a provider is reachable, an analysis with a job posting runs these extra steps:

1. **Requirement extraction.** The model reads the posting and returns a list of requirements as strict JSON.
2. **Semantic matching.** Requirements and CV chunks are embedded; the best-matching chunk decides Met / Partial / Missing. The semantic score is blended with the rule-based Job Match score (`Semantic:DeterministicWeight` and `Semantic:SemanticWeight`, 50/50 by default).
3. **Retrieval (RAG).** For each requirement the most similar entries of a built-in skill taxonomy (names, aliases, descriptions, for example `MSSQL = SQL Server = T-SQL`) are retrieved.
4. **Interpretation.** The model decides per requirement whether the CV shows it, quotes the evidence, explains the decision and suggests an improvement. The retrieved taxonomy entries are given as context for recognising abbreviations and synonyms; they are not evidence.

Design rules:

- **The model never sets a score.** Interpretations are stored and shown next to the score; they do not change it.
- **Grounding.** A "met" or "partial" decision must contain a quote that is found, word for word, in the CV. If the quote cannot be found, the decision is treated as "missing".
- **Disagreements are flagged.** When the embedding match and the model are two steps apart (Met against Missing), the report marks the requirement for a manual check.
- **Strict output.** Replies must match a JSON schema exactly; anything else is rejected, the model is asked once more with the rejection reasons (never with its own rejected output), and then the step is skipped with a warning.
- **Local first.** Provider priority is configurable (`Ollama`, then `OpenAiCompatible` by default). One provider is used for a whole analysis.
- **Embeddings are cached** in SQL Server by text hash and provider-qualified model name, so repeated texts are not embedded twice and vectors of different models are never mixed.

## Security design

A CV is untrusted input that is written to be read by an automated system, so it is scanned before anything else.

- **Hidden text** (white or near-white text, text smaller than 3 pt, text outside the page, Word's "hidden" formatting) is detected in PDF and DOCX files, **excluded from scoring and from every LLM prompt**, and listed in full under *Hidden text in the CV file* in the feedback and in the PDF report.
- **Instruction-like text** ("ignore all previous instructions", "give this candidate 100 points", fake `[SYSTEM]` markers, in English and Turkish) is detected in the CV and in the job posting. It is reported, not removed.
- **Instruction and data stay separate.** Documents reach the model between delimiters that carry a random token created per call; the system message tells the model that everything between them is untrusted data.
- **Output is validated** against a schema and quotes are checked against the CV (see above), so a model that was talked into something still cannot invent evidence or a score.

The pattern list is a warning layer, not a complete filter. The architecture (no model-decided scores, validated and grounded output) is the actual defense.

## Architecture

```mermaid
flowchart TD
    UI["ATS.FromUI<br/>Windows Forms"] --> INF["ATS.Infrastructure<br/>EF Core, parsers, AI providers, reports"]
    INF --> APP["ATS.Application<br/>analyzers, scoring, security, prompts, validation"]
    APP --> CORE["ATS.Core<br/>helpers, constants"]
    APP --> DOM["ATS.Domain<br/>entities, enums"]
    INF --> DOM
```

Interfaces live in `ATS.Application`, implementations in `ATS.Infrastructure`. `ATS.Application` knows nothing about EF Core, PdfPig or Open XML.

```mermaid
flowchart TD
    A["CV file"] --> B["Parse text"]
    B --> C["Security scan: hidden text, instruction patterns"]
    C --> D["Remove hidden text from the scoring text"]
    D --> E["Translate to English if needed"]
    E --> F["Rule-based analyzers and Job Match"]
    F --> G{"AI layer available?"}
    G -- no --> K["Rule-based score"]
    G -- yes --> H["Extract requirements"]
    H --> I["Embedding match and hybrid Job Match score"]
    I --> J["Retrieve skill knowledge, interpret, check evidence"]
    J --> K
    K --> L["Save to SQL Server, show feedback, export PDF"]
```

| Project | Role |
|---|---|
| `ATS.Domain` | entities and enums |
| `ATS.Core` | helpers (chunking, vector math, truncation), constants |
| `ATS.Application` | analyzers, scoring rules, security detectors, prompt builder, output validators, semantic matching, abstractions |
| `ATS.Infrastructure` | EF Core (SQL Server), file parsers, AI providers (OllamaSharp, Microsoft.Extensions.AI), translation, PDF reports, DI registration |
| `ATS.FromUI` | Windows Forms front end, generic host, configuration |
| `ATS.Tests` | xUnit tests |

## Getting started

### Requirements

- Windows 10 or later
- .NET 9 SDK (Visual Studio 2022 17.12 or later, or the .NET CLI)
- A SQL Server instance (Developer or Express edition is enough)

### Set up

1. Clone the repository.
2. Copy `ATS.FromUI/appsettings.example.json` to `ATS.FromUI/appsettings.json` and set `ConnectionStrings:DefaultConnection`, for example  
   `Server=localhost;Database=AtsDb;Trusted_Connection=True;TrustServerCertificate=True`
3. Create the database. In Visual Studio's Package Manager Console (default project `ATS.Infrastructure`, startup project `ATS.FromUI`):
   ```
   Update-Database
   ```
   or with the CLI (`dotnet tool install --global dotnet-ef` once):
   ```
   dotnet ef database update --project ATS.Infrastructure --startup-project ATS.FromUI
   ```
4. Run `ATS.FromUI`. Upload a CV, optionally enter a job title and paste the job posting, press **Analyze**, and use **Report Pdf Export** for the PDF. Reports are written to `Desktop\AtsReport`.

English CVs and postings need nothing else. Turkish and other non-English texts are translated first, see *Translation* below.

### Turn on the AI layer with a local model

1. Install [Ollama](https://ollama.com) and pull one chat model and one embedding model. The names below are examples; any chat model that can follow a JSON output format and any embedding model will do.
   ```
   ollama pull qwen3:4b
   ollama pull nomic-embed-text
   ```
2. In `appsettings.json`:
   ```json
   "Ai": {
     "Enabled": true,
     "ProviderPriority": [ "Ollama", "OpenAiCompatible" ],
     "Ollama": {
       "Endpoint": "http://localhost:11434",
       "ChatModel": "qwen3:4b",
       "EmbeddingModel": "nomic-embed-text"
     }
   }
   ```
3. Run an analysis with a job posting. The feedback gets a *Requirement analysis* section, and the `AnalysisAudits` table records the provider, scores, duration, token counts and embedding cache hits.

If Ollama is not running or a model is missing, the analysis still completes with the rule-based score and shows a warning that says why.

### Use an OpenAI-compatible API instead (or as a fallback)

Set `Ai:OpenAiCompatible:Endpoint`, `ChatModel` and `EmbeddingModel`. Many local servers need no key. If yours does, never put the key in `appsettings.json`; use user secrets or an environment variable:

```
dotnet user-secrets set "Ai:OpenAiCompatible:ApiKey" "<your key>" --project ATS.FromUI
```

or set `Ai__OpenAiCompatible__ApiKey`. The key is read for every request, so adding it while the application runs is enough.

### Translation

The keyword lists are English. A non-English CV or job posting is translated to English before it is analyzed, through the RapidAPI "Google Translator" service (`RapidApi:Key` and `RapidApi:Host`). Text that is detected as English locally is never sent anywhere. If the service is unavailable, the original text is analyzed and the result carries a warning that scores for non-English text may be unreliable.

## Configuration reference

| Key | Default | Meaning |
|---|---|---|
| `Ai:Enabled` | `false` | switches the AI layer on |
| `Ai:ProviderPriority` | `["Ollama","OpenAiCompatible"]` | order in which providers are tried |
| `Ai:TimeoutSeconds` | `120` | timeout of one model request |
| `Ai:Ollama:*` | endpoint `http://localhost:11434` | `Endpoint`, `ChatModel`, `EmbeddingModel` |
| `Ai:OpenAiCompatible:*` | | `Endpoint`, `ChatModel`, `EmbeddingModel`, `VerifyModelsWithProvider` (default `true`) |
| `Semantic:MetSimilarityThreshold` | `0.75` | cosine similarity from which a requirement counts as met |
| `Semantic:PartialSimilarityThreshold` | `0.55` | cosine similarity from which a requirement counts as partly met |
| `Semantic:DeterministicWeight` / `SemanticWeight` | `0.5` / `0.5` | blend of the rule-based and the semantic Job Match score |
| `Semantic:MandatoryRequirementWeight` / `OptionalRequirementWeight` | `2.0` / `1.0` | weight of a requirement in the semantic score |
| `Semantic:PartialMatchCredit` | `0.5` | credit of a partly met requirement |
| `Semantic:MaxChunkCharacters` | `400` | size of the CV chunks that are embedded |
| `Semantic:KnowledgeHitsPerRequirement` / `KnowledgeSimilarityThreshold` | `3` / `0.5` | how many taxonomy entries are retrieved per requirement, and the minimum similarity |
| `RapidApi:*` | | translation service (`Key`, `Host`, `TranslatePath`, `MaxChunkChars`, `TimeoutSeconds`) |

The built-in skill taxonomy is stored in the `SkillTaxonomyEntries` and `SkillTaxonomyAliases` tables. Add rows to teach the AI layer more skills and synonyms; no code change is needed.

## Testing

The solution has more than 600 xUnit tests. The AI layer is tested without a model: chat clients are replaced by scripted fakes, embedding generators by deterministic fakes, and HTTP calls by stub handlers. These tests cover prompt construction, schema validation, the retry rule, grounding, graceful degradation, caching and persistence. The PDF and DOCX security scanners are tested against files that the tests generate.

A GitHub Actions workflow (`.github/workflows/ci.yml`, `windows-latest`) restores, builds and runs the tests on every push and pull request.

The SQL-specific behaviour (unique indexes, cascades) is not covered by the in-memory tests; the schema comes from the EF Core migration.

## Privacy

- With the default local provider (Ollama) CV text never leaves your machine.
- If you configure an OpenAI-compatible API, the CV text and the job posting text are sent to that provider.
- Translation sends non-English text to the RapidAPI translation service.
- Analyses, including the extracted CV text, are stored in your SQL Server database.

## Limitations

- Keyword and embedding overlap show that a CV *mentions* something, not that the candidate can do it.
- The rule-based part reads English. Non-English CVs depend on the translation step.
- The Word parser reads the paragraphs of the document body. Text in tables, headers and footers is not part of the analysis text (the hidden-text scanner does read them).
- Hidden-text detection cannot see a page's background colour, so white text on a dark sidebar can be reported (and then excluded from scoring) although it is visible. Text hidden behind images, and Word colours that come from style definitions, are not detected.
- The instruction-pattern list is not exhaustive.
- The similarity thresholds and weights are defaults that were not tuned on a labelled dataset. Adjust them in the `Semantic` section for the models you use.
- The quality of the explanations depends on the chat model you choose; small local models may fail the JSON schema more often. A failed batch is skipped with a warning and never changes the score.
- Windows only (Windows Forms).

## Third-party components

PdfPig, DocumentFormat.OpenXml, QuestPDF (used under its Community license; check its terms for your use), EF Core, Serilog, OllamaSharp and Microsoft.Extensions.AI.
