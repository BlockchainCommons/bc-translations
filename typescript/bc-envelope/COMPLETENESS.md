# Completeness: bc-envelope -> TypeScript (@bc/envelope)

## Source Files
- [x] `src/error.ts`
- [x] `src/assertion.ts`
- [x] `src/envelope-case.ts`
- [x] `src/envelope-encodable.ts`
- [x] `src/envelope.ts`
- [x] `src/edge-type.ts`
- [x] `src/obscure-action.ts`
- [x] `src/obscure-type.ts`
- [x] `src/format-context.ts`
- [x] `src/envelope-notation.ts`
- [x] `src/envelope-tree.ts`
- [x] `src/envelope-summary.ts`
- [x] `src/envelope-diagnostic.ts`
- [x] `src/envelope-hex.ts`
- [x] `src/envelope-mermaid.ts`
- [x] `src/function.ts`
- [x] `src/functions-store.ts`
- [x] `src/parameter.ts`
- [x] `src/parameters-store.ts`
- [x] `src/expression.ts`
- [x] `src/request.ts`
- [x] `src/response.ts`
- [x] `src/event.ts`
- [x] `src/envelope-signature.ts`
- [x] `src/envelope-recipient.ts`
- [x] `src/envelope-secret.ts`
- [x] `src/envelope-sskr.ts`
- [x] `src/envelope-proof.ts`
- [x] `src/envelope-types.ts`
- [x] `src/envelope-attachment.ts`
- [x] `src/envelope-edge.ts`
- [x] `src/envelope-seal.ts`
- [x] `src/string-utils.ts`
- [x] `src/index.ts`

## Tests

Test coverage is significantly below the Rust source of truth. Rust has 139 tests across 22 files; TypeScript has 54 tests across 23 files. Many Rust test files have been consolidated into single tests or have substantial coverage gaps:

- [x] `tests/core.test.ts` -- 6 of 17 Rust tests (missing: negative int, cbor encodable, two assertions, double wrapped, assertion with assertions, digest leaf, unknown leaf, true, false, unit, position)
- [x] `tests/core-nesting.test.ts` -- 3 of 6 Rust tests
- [x] `tests/core-encoding.test.ts` -- 3 of 4 Rust tests
- [x] `tests/format.test.ts` -- 4 of 12 Rust tests (missing: top level assertion, elided object, signed subject, wrap then signed, encrypt to recipients, assertion positions, complex metadata, credential, redacted credential)
- [x] `tests/elision.test.ts` -- 2 of 16 Rust tests (missing: single/double assertion remove/reveal, digests, target reveal/remove, walkReplace suite of 8 tests)
- [x] `tests/edge.test.ts` -- 1 of 44 Rust tests (substantial gap)
- [x] `tests/crypto.test.ts` -- 2 of 10 Rust tests (missing: encrypt_decrypt, sign_then_encrypt, encrypt_then_sign, multi_recipient, visible/hidden signature multi-recipient, secret tests)
- [x] `tests/obscuring.test.ts` -- 2 of 6 Rust tests
- [x] `tests/proof.test.ts` -- 2 of 3 Rust tests
- [x] `tests/non-correlation.test.ts` -- 2 of 3 Rust tests
- [x] `tests/type.test.ts` -- 2 of 4 Rust tests
- [x] `tests/signature.test.ts` -- 2 of 3 Rust tests
- [x] `tests/compression.test.ts` -- 2 of 2 Rust tests
- [x] `tests/keypair-signing.test.ts` -- 1 of 2 Rust tests
- [x] `tests/attachment.test.ts` -- 2 of 1 Rust tests (more than Rust)
- [x] `tests/ed25519.test.ts` -- 1 of 1 Rust tests
- [x] `tests/encapsulation.test.ts` -- 1 of 1 Rust tests
- [x] `tests/encrypted.test.ts` -- 2 of 1 Rust tests (more than Rust, but both fail due to Node.js 18 argon2 limitation)
- [x] `tests/multi-permit.test.ts` -- 1 of 1 Rust tests
- [x] `tests/sskr.test.ts` -- 1 of 1 Rust tests
- [x] `tests/expression.test.ts` -- 9 tests (inline module tests, no direct Rust file equivalent)
- [x] `tests/seal.test.ts` -- 2 tests (inline module tests, no direct Rust file equivalent)
- [x] `tests/ssh.test.ts` -- 2 stub tests (SSH not fully supported in TS)

## Build & Config
- [x] `.gitignore`
- [x] `package.json`
- [x] `tsconfig.json`
- [x] `vitest.config.ts`

## Cross-Check Notes (2026-03-05, Claude Opus 4.6)

### Test Coverage Gap Summary

Total Rust tests: 139, Total TypeScript tests: 54. Coverage: ~39%.

Largest gaps:
- edge_tests.rs: 44 tests vs 1 test (43 missing)
- elision_tests.rs: 16 tests vs 2 tests (14 missing)
- core_tests.rs: 17 tests vs 6 tests (11 missing)
- crypto_tests.rs: 10 tests vs 2 tests (8 missing)
- format_tests.rs: 12 tests vs 4 tests (8 missing)

### Environment Blockers
- 2 tests (lock/unlock with password) fail due to argon2 requiring Node.js 22+; current env is Node.js 18.14.2.

### API Surface
Source API surface is complete. All public types, methods, and extension families from the Rust source are translated and exported.
