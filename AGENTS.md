# Blockchains Common Cross-Language Translations

The goal of this project is to provide a set of native translations of the reference Rust implementations of the Blockchains Common libraries. Each of the target languages has its own directory. The goal of each translation is to provide 100% of the functionality of the reference Rust implementation, including at *least* 100% test coverage, while still being completely idiomatic in the target language.

**The Rust crates are always the source of truth.** The Rust code is the reference implementation. The translations are only re-implementations of the Rust code. The translations should not be used as primary references for correctness, API design decisions, test coverage, or documentation. When in doubt, refer back to the Rust source. If while performing a translation you find that one of the previous translations is incorrect or incomplete, fix the dependency translation to match the Rust source; do not shim the translation you are performing to patch the missing functionality.

## Status Markers

The following status markers are used to indicate the current state of each translation:

- ⏳ Not Started: No work has been done on this translation yet.
- 🚧 In Progress: Work has begun on this translation, but it is not yet complete.
- ✅ Completed: The translation is complete and fully tested.

## Model Markers

The following model markers indicate which AI model was used for each translation:

- 🎻 Claude Opus
- 📖 GPT Codex

## Translations

| rust/             | version | 🚧 csharp/       | 🚧 go/           | 🚧 kotlin/        | 🚧 python/        | 🚧 swift/        | 🚧 typescript/        |
|-------------------|---------|------------------|------------------|-------------------|-------------------|------------------|-----------------------|
| ✅ bc-lifehash     | 0.1.0   | ✅🎻 BCLifeHash   | ✅🎻 bclifehash   | ✅📖 bc-lifehash   | ✅🎻 bc-lifehash   | ✅📖 BCLifeHash   | ✅🎻 @bc/lifehash      |
| ✅ bc-rand         | 0.5.0   | ✅🎻 BCRand       | ✅🎻 bcrand       | ✅🎻 bc-rand       | ✅🎻 bc-rand       | ✅🎻 BCRand       | ✅🎻 @bc/rand          |
| ✅ bc-crypto       | 0.14.0  | ✅🎻 BCCrypto     | ✅📖 bccrypto     | ✅🎻 bc-crypto     | ✅🎻 bc-crypto     | ✅📖 BCCrypto     | ✅📖 @bc/crypto        |
| ✅ bc-shamir       | 0.13.0  | ✅📖 BCShamir     | ✅🎻 bcshamir     | ✅📖 bc-shamir     | ✅📖 bc-shamir     | ✅🎻 BCShamir     | ✅🎻 @bc/shamir        |
| ✅ dcbor           | 0.25.1  | ✅🎻 DCbor        | ✅📖 dcbor        | ✅🎻 dcbor         | ✅🎻 dcbor         | ✅📖 DCBOR        | ✅📖 @bc/dcbor         |
| ✅ bc-tags        | 0.12.0  | ✅📖 BCTags       | ✅🎻 bctags        | ✅📖 bc-tags       | ✅📖 bc-tags       | ✅🎻 BCTags      | ✅🎻 @bc/tags          |
| ✅ bc-ur           | 0.19.0  | ✅🎻 BCUR          | ✅🎻 bcur           | ✅🎻 bc-ur          | ✅🎻 bc-ur          | ✅📖 BCUR          | ✅📖 @bc/ur             |
| 🚧 sskr            | 0.12.0  | ⏳ SSKR           | ⏳ sskr           | ✅📖 sskr           | ⏳ sskr            | ✅🎻 SSKR          | ⏳ @bc/sskr            |
| 🚧 bc-components   | 0.31.1  | ⏳ BCComponents   | ⏳ bccomponents   | 🚧🎻 bc-components   | ⏳ bc-components   | 🚧📖 BCComponents   | ⏳ @bc/components      |
| ⏳ known-values    | 0.15.4  | ⏳ KnownValues    | ⏳ knownvalues    | ⏳ known-values    | ⏳ known-values    | ⏳ KnownValues    | ⏳ @bc/known-values    |
| ⏳ bc-envelope     | 0.43.0  | ⏳ BCEnvelope     | ⏳ bcenvelope     | ⏳ bc-envelope     | ⏳ bc-envelope     | ⏳ BCEnvelope     | ⏳ @bc/envelope        |
| ⏳ provenance-mark | 0.23.0  | ⏳ ProvenanceMark | ⏳ provenancemark | ⏳ provenance-mark | ⏳ provenance-mark | ⏳ ProvenanceMark | ⏳ @bc/provenance-mark |

## Internal Dependencies

These are the dependencies between the crates in this project. Optional dependencies are marked with `?`. The crates are listed in topological order (a valid build order).

```
bc-lifehash     → (none)
bc-rand         → (none)
dcbor           → (none)
bc-crypto       → bc-rand
bc-tags         → dcbor
bc-ur           → dcbor
bc-shamir       → bc-rand, bc-crypto
sskr            → bc-rand, bc-shamir
bc-components   → bc-rand, bc-crypto, dcbor, bc-tags, bc-ur, sskr
known-values    → dcbor, bc-components
bc-envelope     → bc-rand, bc-crypto, dcbor, bc-ur, bc-components, known-values?
provenance-mark → bc-rand, dcbor, bc-tags, bc-ur, bc-envelope?
```

There are three independent dependency trees that merge at `bc-components`:

- **Crypto tree:** bc-rand → bc-crypto → bc-shamir → sskr
- **CBOR tree:** dcbor → bc-tags, bc-ur
- **Visual hash tree:** bc-lifehash (standalone)

## Monorepo Discipline

This is a massively-parallel monorepo. Multiple agents may be working on different (crate, language) pairs simultaneously. Follow these rules:

- **Do not commit unless explicitly asked.** When asked, commit only the files you worked on — never `git add -A` or `git add .`.
- **Stay in your lane.** Only modify files under your assigned `<lang>/<package>/` directory. Do not touch other languages, other crates, or shared files (CLAUDE.md, AGENTS.md, root .gitignore, etc.) without being asked.
- **Do not modify `rust/`.** The Rust source is the reference implementation. Read it, never write to it.
- **Scaffold a `.gitignore` first.** When creating a new target-language project, the very first file must be a `.gitignore` appropriate for that language (build outputs, dependency caches, IDE files, OS artifacts). This prevents build artifacts from being accidentally committed.

## Orchestration

Translation of each (crate, language) pair follows a four-stage pipeline. Use `/kickoff` to select the next eligible target and run the pipeline.

### Pipeline Stages

```
1. PLAN  →  2. CODE  →  3. CHECK  →  4. CRITIQUE
   ↑            ↑            │            │
   │            └────────────┘            │
   │            (fill gaps)               │
   │                                      │
   └──────────────────────────────────────┘
              (fix + retest)
```

**Stage 1 — Plan** (`translation-planner` skill): Analyze the Rust crate and produce a translation manifest. The manifest catalogs the public API surface, external dependencies needing equivalents, feature flags, test inventory, and translation hazards. Produced once per crate, reused across all six languages. Saved to `<lang>/<package>/MANIFEST.md`.

**Stage 2 — Code** (`translation-coder` skill + `rust-to-<lang>` skill): Translate the source and tests following the manifest's translation unit order. Prioritize correctness over style. Build and run tests. Iterate up to 5 times on compile/test failures.

**Stage 3 — Check** (`completeness-checker` skill): Compare the translation against the manifest. Verify all public types, functions, constants, and traits are translated. Verify all tests are translated with matching test vectors. If gaps are found, return to Stage 2.

**Stage 4 — Critique** (`fluency-critic` skill + `rust-to-<lang>` skill): Review the translation for target-language idiomaticness without looking at the Rust source. Check naming, error handling, API design, structure, and documentation. Apply fixes. Re-run tests to confirm fixes don't break anything.

### Translation Order

Work respects the dependency graph. A (crate, language) pair is eligible only when all its internal BC dependencies are ✅ for that language.

```
Phase 1 (no deps):      bc-lifehash, bc-rand, dcbor
Phase 2 (Phase 1):      bc-crypto, bc-tags, bc-ur
Phase 3 (Phase 2):      bc-shamir
Phase 4 (Phase 3):      sskr
Phase 5 (Phase 4):      bc-components
Phase 6 (Phase 5):      known-values
Phase 7 (Phase 6):      bc-envelope
Phase 8 (Phase 7):      provenance-mark
```

Within each phase, all six languages can proceed in parallel. Across phases, the dependency ordering ensures translated packages are available for import.

### Per-Target Log

Each (crate, language) pair maintains a log at `<lang>/<package>/LOG.md`. Every pipeline stage appends an entry when it starts and when it finishes. If a session is interrupted, any agent (or `/kickoff`) can check for a started-but-not-finished entry and resume from there.

Log entry format:

```
## <date> — Stage N: <Name>
STARTED
- <what is being done>

## <date> — Stage N: <Name>
COMPLETED
- <summary of results>
- <key metrics: API coverage, test counts, issues found, etc.>
```

An entry with STARTED but no corresponding COMPLETED means that stage was interrupted and should be resumed.

### Root Activity Log

In addition to per-target logs, append a row to the root `LOG.md` table for any substantial translation activity that changes status or code quality outcomes.

- Always log full translation completions with task `Translation`.
- Always log Stage 4 fluency review work (including reruns requested after completion) with task `Fluency`.
- Follow the root `LOG.md` table format exactly:
  `| <date> | <crate> | <version> | <language> | <package> | <model> | <task> |`
- Keep Stage 4 task text in root `LOG.md` standardized as `Fluency`.
- Keep root `FLUENCY_NEEDED.md` in sync as the running queue of targets still awaiting cross-model fluency checks.
- `FLUENCY_NEEDED.md` must list each pending target and the original translation model that should **not** run the cross-check.
- After appending any root `LOG.md` row with task `Translation` or `Fluency`, run `bash scripts/update-fluency-needed.sh` to regenerate `FLUENCY_NEEDED.md`.

### Key Principles

- **Translate, don't rewrite.** Stay close to the Rust structure. Faithful translation, not reimagination.
- **Do not depend on pre-existing Blockchain Commons implementation packages.** Even if a package already exists that implements a BC specification, do not use it as a dependency for any target language in this repo. Implement from the Rust source of truth and rely only on the in-repo translations defined by the internal dependency graph.
- **Correctness first, idiomaticness second.** Get it right, then make it pretty. These are separate stages for a reason (see SACTOR research on two-phase translation).
- **Test vectors are sacred.** Crypto test vectors must produce identical byte-for-byte output across all languages. They are the primary cross-language validation signal.
- **Default features only.** For initial translations, translate only code gated by default features. Non-default features are documented as future work.
- **One manifest per crate.** The planner's output is language-agnostic and reused across all six targets.
- **Aggressive context management.** The planner reads the full Rust source. The coder reads only the current translation unit plus its deps. The fluency critic reads only the target-language code.

### API Evolution Policy (De Novo)

- **No compatibility layer required.** This repository is de novo; there are no external dependents that require backward compatibility.
- **Prefer direct API improvement.** When fluency or correctness work changes an API, apply the new API directly instead of preserving old forms.
- **Do not add deprecations or shims.** Never add deprecated aliases, compatibility wrappers, or transitional APIs in this repo. EXCEPTION: If the *original* Rust implementation adds a new API, deprecates an old one, or adds a compatibility shim, the translations should follow suit.
- **Fix internal breakage immediately.** If an API change breaks dependent targets in this monorepo, update those dependents in the same work stream and re-run tests.

## Package Search Indexes

The following registries/indexes can be searched for published packages:

- C#: [NuGet Gallery search](https://www.nuget.org/packages?q=)
- Go: [pkg.go.dev search](https://pkg.go.dev/search?q=)
- Kotlin: [Maven Central search](https://central.sonatype.com/search?q=)
- Python: [PyPI search](https://pypi.org/search/?q=)
- Swift: [Swift Package Index search](https://swiftpackageindex.com/search?query=)
- TypeScript: [npm search](https://www.npmjs.com/search?q=)
- Rust: [crates.io search](https://crates.io/search?q=)
