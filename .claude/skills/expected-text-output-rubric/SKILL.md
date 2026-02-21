---
name: expected-text-output-rubric
description: >-
  Use full expected-text output assertions for complex rendered structures,
  instead of many brittle field-level assertions.
user-invocable: false
context: fork
---

# Expected Text Output Rubric

When a test validates complex rendered output (diagnostic notation, pretty dumps,
tree output, formatted documents, CLI output), prefer one whole-text comparison
over many small assertions on fragments.

## Use This Rubric When

- Output is multiline or deeply nested.
- Output ordering, spacing, escaping, and punctuation are behaviorally important.
- Existing tests use many assert calls on individual fields of a rendered form.
- A failure should show the whole mismatch clearly.

## Do Not Use This Rubric When

- The behavior is a simple scalar check.
- The test is validating binary vectors/bytes where exact byte checks are clearer.
- The output is intentionally nondeterministic.

## Workflow

1. Generate actual text output from the translated code path.
2. Run the test once to collect a stable expected output.
3. Paste the expected text as a literal block in the test.
4. Compare `actual` vs `expected` with one assertion.
5. Ensure mismatch output prints both actual and expected text.

Use this marker in translated tests where applied:

```text
// expected-text-output-rubric:
```

## Assertion Helper Pattern

Keep a single helper per package/test module that reports full diffs clearly.

Pseudo-pattern:

```text
if actual != expected {
  print("Actual:\n" + actual)
  print("Expected:\n" + expected)
  fail()
}
```

## Manifest Note Template

When this rubric applies, add this section to the target `MANIFEST.md`:

```text
EXPECTED TEXT OUTPUT RUBRIC:
- Applicable: yes
- Source signals: [e.g., `expected-text-output-rubric` comments, complex formatting tests]
- Target tests to apply: [list test groups]
- Required pattern: one full-text assertion with actual/expected mismatch output
```

If it does not apply, record:

```text
EXPECTED TEXT OUTPUT RUBRIC:
- Applicable: no
- Reason: [short reason]
```
