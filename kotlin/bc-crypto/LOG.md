# Translation Log: bc-crypto → Kotlin (bc-crypto)

Model: Claude Opus 4.6

## 2026-02-20 — Stage 1: Plan
COMPLETED
- Reused MANIFEST.md from Python bc-crypto translation
- Identified dependency mapping: Bouncy Castle (HKDF, X25519, Ed25519, PBKDF2, scrypt, argon2id), secp256k1-kmp (ECDSA), pure BIP-340 (Schnorr)

## 2026-02-20 — Stage 2: Code
COMPLETED
- Translated 11 source files and 9 test files
- 42 tests all passing
- Key decisions: BIP-340 Schnorr implemented with Bouncy Castle EC math (secp256k1-kmp only supports 32-byte messages), secp256k1-kmp retained for ECDSA operations
- Fixed Ed25519 RFC 8032 test vector 4 hex transcription error (missing `ec` byte at position 543)

## 2026-02-20 — Stage 3: Check
COMPLETED
- API coverage: 57/57 items (100%) — 18 constants, 37 functions, 1 type, 1 exception
- Test coverage: 42/42 tests (100%) — all 30 Rust test functions translated with matching vectors
- Signature compatibility: 0 mismatches
- Verdict: COMPLETE

## 2026-02-20 — Stage 4: Critique
COMPLETED
- 7 issues found (3 MUST FIX, 4 SHOULD FIX), all fixed
- Renamed `memzeroVecVecU8` → `memzeroAll` (Rust naming leak)
- Merged `crc32DataOpt` into `crc32Data` with default parameter
- Merged `scryptOpt` into `scrypt` with default parameters
- Merged `aeadChaCha20Poly1305EncryptWithAad`/`DecryptWithAad` into base functions with default `aad`
- Replaced `java.util.Arrays.fill()` with Kotlin `ByteArray.fill()`
- Fixed `var` → `val` for immutable `kPrime` in SchnorrSigning
- Added missing `assertTrue()` assertion in `testVerifyTweaked`
- 42/42 tests passing after fixes
- Verdict: IDIOMATIC
