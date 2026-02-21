# Completeness: bc-tags → TypeScript (@bc/tags)

## Source Files
- [x] src/tags-registry.ts — 150 constants (75 value + 75 name), bcTags array, registerTagsIn, registerTags
- [x] src/index.ts — barrel exports for all public API

## Constants (75 tag pairs)
- [x] Standard IANA tags (URI, UUID)
- [x] Core Envelope tags (ENCODED_CBOR, ENVELOPE, LEAF, JSON)
- [x] Envelope extension tags (KNOWN_VALUE, DIGEST, ENCRYPTED, COMPRESSED)
- [x] Distributed Function Call tags (REQUEST, RESPONSE, FUNCTION, PARAMETER, PLACEHOLDER, REPLACEMENT)
- [x] Cryptographic key and identity tags (X25519_PRIVATE_KEY through ENCRYPTED_KEY)
- [x] Post-Quantum Cryptography tags (MLKEM_*, MLDSA_*)
- [x] Key and descriptor tags (SEED through ACCOUNT_DESCRIPTOR)
- [x] SSH tags (SSH_TEXT_PRIVATE_KEY through SSH_TEXT_CERTIFICATE)
- [x] Provenance tag (PROVENANCE_MARK)
- [x] Deprecated V1 tags (SEED_V1 through ACCOUNT_V1)
- [x] Output descriptor sub-tags (OUTPUT_SCRIPT_HASH through OUTPUT_COSIGNER)

## Functions
- [x] registerTagsIn(tagsStore) — calls dcbor registerTagsIn first, then inserts 75 bc-tags
- [x] registerTags() — registers in global store

## Registration Order
- [x] bcTags array matches Rust registration order exactly (75 entries)

## Tests
- [x] Constant value spot-checks (12 representative value/name pairs)
- [x] bcTags array has 75 entries
- [x] bcTags matches expectedTags in order
- [x] registerTagsIn registers dcbor base tags (date, positive-bignum, negative-bignum)
- [x] Forward lookup (value → tag) for all 75 tags
- [x] Reverse lookup (name → tag) for all 75 tags
- [x] Idempotent registration
- [x] Global store registration
- [x] Unique tag values
- [x] Unique tag names
- [x] nameForValue spot-checks (crypto-key, provenance)

## Build & Config
- [x] .gitignore
- [x] package.json
- [x] tsconfig.json
- [x] vitest.config.ts
