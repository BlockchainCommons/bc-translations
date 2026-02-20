---
name: fluency-critic
description: >-
  Reviews translated code for target-language idiomaticness. Checks naming
  conventions, error handling patterns, API design, and ecosystem norms.
  Use after the completeness checker confirms a translation is complete.
user-invocable: false
---

# Fluency Critic

Review translated code for idiomaticness in the target language. This is a separate pass from the coder — the code is already correct and complete. The goal is to make it read like a native developer wrote it.

## Inputs

- **Translated source code** in `<lang>/<package>/`
- The relevant `rust-to-<lang>` skill (auto-loaded) for conventions

Do NOT read the Rust source during this review. Judge the code solely as target-language code.

## Checklist

### Naming

- [ ] Types/classes follow target naming convention
- [ ] Functions/methods follow target naming convention
- [ ] Variables/parameters follow target naming convention
- [ ] Constants follow target naming convention
- [ ] Package/module name follows target ecosystem convention
- [ ] No Rust naming leaking through (snake_case in PascalCase language, etc.)
- [ ] Acronyms handled per target convention (e.g., `URL` not `Url` in Go)

### Error Handling

- [ ] Uses the target language's standard error idiom, not a Rust-shaped wrapper
- [ ] Error types are idiomatic (exceptions vs error values vs Result types)
- [ ] Error messages are clear and follow target conventions
- [ ] No unnecessary error wrapping or unwrapping

### Types and Data

- [ ] Enums/ADTs use the target's natural representation
- [ ] Nullable/optional types use the target's natural syntax
- [ ] Collections use the target's standard types
- [ ] Binary data uses the target's standard byte type
- [ ] No unnecessary boxing/indirection carried over from Rust

### API Design

- [ ] Constructors follow target convention (init, New, factory, etc.)
- [ ] Getters/setters follow target convention (properties vs methods)
- [ ] Method signatures feel natural in the target language
- [ ] Overloading/default arguments used where idiomatic
- [ ] Builder patterns translated to target-idiomatic equivalent

### Resource Management

- [ ] Cleanup patterns use target idiom (using/defer/with/try-with-resources/Disposable)
- [ ] No Rust-style explicit drop/free calls in a GC'd language
- [ ] Crypto key zeroing uses appropriate target pattern

### Structure and Organization

- [ ] File organization follows target ecosystem norms
- [ ] Visibility modifiers are correct and idiomatic
- [ ] Module/package structure is idiomatic
- [ ] Imports are organized per target convention

### Tests

- [ ] Test naming follows target framework conventions
- [ ] Test organization follows target ecosystem norms
- [ ] Assertions use the target test framework's assertion style
- [ ] Test setup/teardown uses target patterns

### Documentation

- [ ] Public API has doc comments in target format (/// vs /** vs docstring vs #)
- [ ] Doc comments describe what, not how it was translated from Rust

## Output

Produce a review with specific findings:

```
FLUENCY REVIEW for <package> (<lang>)

ISSUES:
  1. [naming] `split_secret` should be `SplitSecret` in Go (exported function)
  2. [error] Using custom Result type instead of Go's (value, error) convention
  3. [api] Constructor `ShamirShare::new()` should be `NewShamirShare()` in Go
  4. [structure] Tests in separate directory; Go convention is *_test.go alongside source

SUGGESTED FIXES:
  [specific code changes for each issue]

VERDICT: IDIOMATIC | NEEDS REVISION (N issues)
```

After producing the review, apply the fixes directly. Then re-run the tests to verify the fixes don't break anything.
