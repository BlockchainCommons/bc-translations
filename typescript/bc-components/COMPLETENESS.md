# Completeness: bc-components → TypeScript (@bc/components)

## Source Files
- [x] src/error.ts — error types and helpers
- [x] src/digest.ts — digest wrapper and conversions
- [x] src/digest-provider.ts — digest provider interface
- [x] src/id/* — ARID, UUID, URI, XID, provider
- [x] src/compressed.ts — compressed data wrapper
- [x] src/nonce.ts — nonce wrapper and RNG helpers
- [x] src/symmetric/* — symmetric key, encrypted message, auth tag
- [x] src/encrypted-key/* — encrypted key and derivation types
- [x] src/salt.ts — salt wrapper
- [x] src/json.ts — JSON wrapper and codec
- [x] src/x25519/* — X25519 keys
- [x] src/seed.ts — seed wrapper and derivation
- [x] src/signing/* — signatures, schemes, key wrappers
- [x] src/encrypter.ts — encrypter/decrypter interfaces
- [x] src/ec-key/* — secp256k1 key wrappers
- [x] src/reference.ts — references and constants
- [x] src/tags-registry.ts — tag registry bindings
- [x] src/private-key-data-provider.ts — private key data provider
- [x] src/private-key-base.ts — shared private key base
- [x] src/private-keys.ts — private key collection
- [x] src/public-keys.ts — public key collection
- [x] src/mldsa/* — ML-DSA wrappers
- [x] src/mlkem/* — ML-KEM wrappers
- [x] src/encapsulation/* — encapsulation abstractions and sealed message
- [x] src/sskr-mod.ts — SSKR glue APIs
- [x] src/hkdf-rng.ts — HKDF-backed RNG
- [x] src/keypair.ts — keypair constructors
- [x] src/index.ts — package exports

## Tests
- [x] tests/lib.test.ts — crate-level API tests from `lib.rs`
- [x] tests/digest.test.ts — digest tests
- [x] tests/compressed.test.ts — compressed tests
- [x] tests/nonce.test.ts — nonce tests
- [x] tests/json.test.ts — JSON tests
- [x] tests/hkdf-rng.test.ts — HKDF RNG tests
- [x] tests/signing.test.ts — signing tests
- [x] tests/symmetric.test.ts — symmetric tests
- [x] tests/private-keys.test.ts — private key collection tests
- [x] tests/public-keys.test.ts — public key collection tests
- [x] tests/encrypted-key.test.ts — encrypted key tests
- [x] tests/encapsulation.test.ts — encapsulation tests
- [x] tests/id-xid.test.ts — XID tests
- [x] tests/mldsa.test.ts — ML-DSA tests
- [x] tests/mlkem.test.ts — ML-KEM tests

## Build & Config
- [x] .gitignore
- [x] package.json
- [x] tsconfig.json
- [x] vitest.config.ts
- [x] npm install / lockfile
- [x] `npm run test` passes
