---
name: completeness-checker
description: >-
  Verifies a translation is complete by comparing it against the planner manifest.
  Checks API surface coverage, test coverage, and signature compatibility.
  Use after the coder finishes translating a crate to verify nothing was missed.
user-invocable: false
context: fork
---

# Completeness Checker

## API Evolution Policy (De Novo)

- This repository is de novo; there are no external consumers requiring backward compatibility.
- When correctness or fluency work changes an API, apply the new API directly.
- Never add deprecated aliases, compatibility wrappers, or transitional shims.
- If an API change breaks dependent targets in this monorepo, update those dependents in the same work stream and re-run tests.

Verify that a translation covers the full public API and test suite of the Rust source.

## Inputs

- **Translation manifest** (`<lang>/<package>/MANIFEST.md`)
- **Translated source code** in `<lang>/<package>/`
- **Rust source code** in `rust/<crate>/` (for reference)

## Procedure

### Pass 1: API Surface Coverage

For every item in the manifest's catalogs (types, functions, constants, traits), verify it exists in the translated code.

Produce a report:

```
API COVERAGE REPORT for <package> (<lang>)

TYPES:
  ✅ ShamirShare — translated as ShamirShare class
  ✅ ShamirError — translated as ShamirError sealed class
  ❌ MISSING: GroupSpec — not found in translation

FUNCTIONS:
  ✅ split_secret — translated as splitSecret()
  ✅ recover_secret — translated as recoverSecret()
  ❌ MISSING: validate_shares — not found

CONSTANTS:
  ✅ MAX_SHARES — translated
  ✅ MIN_THRESHOLD — translated

TRAITS:
  ✅ CBOREncodable — translated as CborEncodable interface
```

### Pass 2: Signature Compatibility

For each translated function/method, verify the signature is semantically equivalent:
- Parameter types map correctly (per the `rust-to-<lang>` type mappings)
- Return types map correctly
- Error handling maps correctly (Result → throws/error tuple/etc.)
- Generic constraints are preserved

Flag mismatches:

```
SIGNATURE MISMATCHES:
  ⚠️ split_secret: Rust takes &mut impl Rng, translation missing RNG parameter
  ⚠️ recover_secret: Rust returns Result<Vec<u8>>, translation returns Vec<u8> (no error path)
```

### Pass 3: Test Coverage

For every test in the manifest's test catalog, verify a corresponding test exists:

```
TEST COVERAGE:
  ✅ test_split_and_recover — translated as testSplitAndRecover
  ✅ test_threshold_validation — translated as testThresholdValidation
  ❌ MISSING: test_invalid_share_count — no corresponding test
  ⚠️ test_deterministic_output — translated but test vectors differ from Rust
```

Pay special attention to test vectors. Compare hardcoded expected byte values between Rust tests and translated tests — they must match exactly.

### Pass 4: Derive/Protocol Coverage

For types that derive traits in Rust, verify the equivalent protocol conformances exist:
- `Clone` → copy semantics or clone method
- `PartialEq`/`Eq` → equality implementation
- `Hash` → hash implementation
- `Debug`/`Display` → string representation
- `CBOREncodable`/`CBORDecodable` → CBOR serialization

### Pass 5: Documentation Coverage

For every public item in the manifest's doc catalog that has a doc comment in Rust, verify the translated item also has a doc comment:

```
DOC COVERAGE:
  ✅ ShamirShare — has doc comment
  ✅ split_secret — has doc comment
  ❌ MISSING DOC: recover_secret — Rust has doc comment, translation does not
  ⬚ validate_shares — no doc in Rust, no doc in translation (OK)
```

Also verify:
- Package metadata has a description

### Output

Produce a summary:

```
COMPLETENESS SUMMARY for <package> (<lang>)

API Coverage:  N/M items (X%)
Test Coverage: N/M tests (X%)
Signatures:    N mismatches
Derives:       N missing conformances
Docs:          N/M items

VERDICT: COMPLETE | INCOMPLETE (list gaps)
```

If incomplete, list specific items the coder needs to add or fix.

### Update COMPLETENESS.md

After producing the summary, update `<lang>/<package>/COMPLETENESS.md` to reflect the checker's findings:

- **Check off** items that are confirmed present and correct.
- **Add unchecked items** for any newly discovered gaps (missing API items, missing tests, signature mismatches, missing docs).
- **Use nested checkboxes** for subtasks within a category (e.g., individual test vectors under a test file).
- **Preserve existing structure** — don't reorganize categories the coder already set up. Add new sections only for gap categories not yet tracked.

This ensures `COMPLETENESS.md` remains the single source of truth for what remains to be done. When the verdict is INCOMPLETE and control returns to the coder (Step 2), the coder should work from `COMPLETENESS.md` rather than re-reading the full checker output.

### Log

Append entries to `<lang>/<package>/LOG.md` when starting and completing this stage. Include the summary metrics (API coverage %, test coverage %, mismatches, verdict). See the Orchestration section of CLAUDE.md for the log format.
