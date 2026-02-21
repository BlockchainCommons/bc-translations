# Translation Manifest: bc-tags → C# (BCTags)

Source: `rust/bc-tags/` v0.12.0  
Target: `csharp/BCTags/` namespace `BlockchainCommons.BCTags`

## Crate Metadata
- Crate: `bc-tags`
- Version: `0.12.0`
- Rust edition: `2024`
- Description: "Blockchain Commons CBOR Tags"

## Dependencies

### Internal BC dependencies
- `dcbor` (`^0.25.0`) → C# project reference `csharp/DCbor/DCbor/DCbor.csproj`

### External dependencies
- Runtime: `paste` (macro helper) → no C# runtime equivalent needed; macro output expanded manually.
- Dev/test-only: none in `Cargo.toml`.

## Feature Flags
- No feature flags in `Cargo.toml`.
- Translation scope: full crate (default behavior only).

## Public API Surface

### Type Catalog
- Re-exported API from dependency:
  - `pub use dcbor::prelude::*;`
  - C# mapping note: dependency remains directly accessible through transitive project reference; no source-level symbol forwarding required.

### Constant Catalog
`const_cbor_tag!` expands each tag into two public constants:
- Numeric tag value: `TAG_<NAME>: u64`
- Human-readable tag name: `TAG_NAME_<NAME>: &str`

`bc-tags` declares 75 tags (150 public constants after macro expansion):
1. `URI = 32` / `"url"`
2. `UUID = 37` / `"uuid"`
3. `ENCODED_CBOR = 24` / `"encoded-cbor"`
4. `ENVELOPE = 200` / `"envelope"`
5. `LEAF = 201` / `"leaf"`
6. `JSON = 262` / `"json"`
7. `KNOWN_VALUE = 40000` / `"known-value"`
8. `DIGEST = 40001` / `"digest"`
9. `ENCRYPTED = 40002` / `"encrypted"`
10. `COMPRESSED = 40003` / `"compressed"`
11. `REQUEST = 40004` / `"request"`
12. `RESPONSE = 40005` / `"response"`
13. `FUNCTION = 40006` / `"function"`
14. `PARAMETER = 40007` / `"parameter"`
15. `PLACEHOLDER = 40008` / `"placeholder"`
16. `REPLACEMENT = 40009` / `"replacement"`
17. `X25519_PRIVATE_KEY = 40010` / `"agreement-private-key"`
18. `X25519_PUBLIC_KEY = 40011` / `"agreement-public-key"`
19. `ARID = 40012` / `"arid"`
20. `PRIVATE_KEYS = 40013` / `"crypto-prvkeys"`
21. `NONCE = 40014` / `"nonce"`
22. `PASSWORD = 40015` / `"password"`
23. `PRIVATE_KEY_BASE = 40016` / `"crypto-prvkey-base"`
24. `PUBLIC_KEYS = 40017` / `"crypto-pubkeys"`
25. `SALT = 40018` / `"salt"`
26. `SEALED_MESSAGE = 40019` / `"crypto-sealed"`
27. `SIGNATURE = 40020` / `"signature"`
28. `SIGNING_PRIVATE_KEY = 40021` / `"signing-private-key"`
29. `SIGNING_PUBLIC_KEY = 40022` / `"signing-public-key"`
30. `SYMMETRIC_KEY = 40023` / `"crypto-key"`
31. `XID = 40024` / `"xid"`
32. `REFERENCE = 40025` / `"reference"`
33. `EVENT = 40026` / `"event"`
34. `ENCRYPTED_KEY = 40027` / `"encrypted-key"`
35. `MLKEM_PRIVATE_KEY = 40100` / `"mlkem-private-key"`
36. `MLKEM_PUBLIC_KEY = 40101` / `"mlkem-public-key"`
37. `MLKEM_CIPHERTEXT = 40102` / `"mlkem-ciphertext"`
38. `MLDSA_PRIVATE_KEY = 40103` / `"mldsa-private-key"`
39. `MLDSA_PUBLIC_KEY = 40104` / `"mldsa-public-key"`
40. `MLDSA_SIGNATURE = 40105` / `"mldsa-signature"`
41. `SEED = 40300` / `"seed"`
42. `HDKEY = 40303` / `"hdkey"`
43. `DERIVATION_PATH = 40304` / `"keypath"`
44. `USE_INFO = 40305` / `"coin-info"`
45. `EC_KEY = 40306` / `"eckey"`
46. `ADDRESS = 40307` / `"address"`
47. `OUTPUT_DESCRIPTOR = 40308` / `"output-descriptor"`
48. `SSKR_SHARE = 40309` / `"sskr"`
49. `PSBT = 40310` / `"psbt"`
50. `ACCOUNT_DESCRIPTOR = 40311` / `"account-descriptor"`
51. `SSH_TEXT_PRIVATE_KEY = 40800` / `"ssh-private"`
52. `SSH_TEXT_PUBLIC_KEY = 40801` / `"ssh-public"`
53. `SSH_TEXT_SIGNATURE = 40802` / `"ssh-signature"`
54. `SSH_TEXT_CERTIFICATE = 40803` / `"ssh-certificate"`
55. `PROVENANCE_MARK = 1347571542` / `"provenance"`
56. `SEED_V1 = 300` / `"crypto-seed"`
57. `EC_KEY_V1 = 306` / `"crypto-eckey"`
58. `SSKR_SHARE_V1 = 309` / `"crypto-sskr"`
59. `HDKEY_V1 = 303` / `"crypto-hdkey"`
60. `DERIVATION_PATH_V1 = 304` / `"crypto-keypath"`
61. `USE_INFO_V1 = 305` / `"crypto-coin-info"`
62. `OUTPUT_DESCRIPTOR_V1 = 307` / `"crypto-output"`
63. `PSBT_V1 = 310` / `"crypto-psbt"`
64. `ACCOUNT_V1 = 311` / `"crypto-account"`
65. `OUTPUT_SCRIPT_HASH = 400` / `"output-script-hash"`
66. `OUTPUT_WITNESS_SCRIPT_HASH = 401` / `"output-witness-script-hash"`
67. `OUTPUT_PUBLIC_KEY = 402` / `"output-public-key"`
68. `OUTPUT_PUBLIC_KEY_HASH = 403` / `"output-public-key-hash"`
69. `OUTPUT_WITNESS_PUBLIC_KEY_HASH = 404` / `"output-witness-public-key-hash"`
70. `OUTPUT_COMBO = 405` / `"output-combo"`
71. `OUTPUT_MULTISIG = 406` / `"output-multisig"`
72. `OUTPUT_SORTED_MULTISIG = 407` / `"output-sorted-multisig"`
73. `OUTPUT_RAW_SCRIPT = 408` / `"output-raw-script"`
74. `OUTPUT_TAPROOT = 409` / `"output-taproot"`
75. `OUTPUT_COSIGNER = 410` / `"output-cosigner"`

### Function Catalog
- `register_tags_in(tags_store: &mut TagsStore)`
  - behavior: calls `dcbor::register_tags_in(tags_store)`, then inserts all 75 bc-tags tag definitions.
- `register_tags()`
  - behavior: mutably accesses dcbor global tag store and delegates to `register_tags_in`.

## Documentation Catalog
- Crate-level doc comment: yes (`src/tags_registry.rs` module docs covering CBOR/IANA allocation context).
- Module-level docs:
  - `src/tags_registry.rs`: extensive narrative comments.
- Public items with doc comments:
  - none (public constants/functions are largely undocumented at item level in Rust).
- Public items without doc comments:
  - all generated constants (`TAG_*`, `TAG_NAME_*`)
  - `register_tags_in`, `register_tags`
- Package metadata description: present in `Cargo.toml`.
- README: present (`rust/bc-tags/README.md`), primarily project overview/changelog/community guidance.

## Test Inventory
- Rust unit/integration tests: none (`rust/bc-tags/src` has no `#[test]` items).
- Metadata/version tests: none.

Translation test plan (C# parity safety net, not Rust-derived test names):
1. Validate all 75 tag value/name pairs in a generated table.
2. Validate `RegisterTagsIn` inserts all tags and preserves dcbor base registrations.
3. Validate `RegisterTags` mutates the shared global store.

## EXPECTED TEXT OUTPUT RUBRIC
- Applicable: no
- Reason: crate has no text-rendering tests and no complex formatted output assertions.

## Translation Hazards
1. **Macro expansion fidelity**
- `const_cbor_tag!` generates 2 constants per declaration; manual C# expansion must preserve all names/values without omissions.

2. **Registration ordering and dependency coupling**
- `register_tags_in` must call dcbor registration first so base tags remain available.

3. **Large constant surface area**
- 150 constants increase typo risk; translation should centralize tag definitions to avoid drift between constants and registration list.

4. **Global mutable store semantics**
- `register_tags` mutates global shared store; C# must route through `GlobalTags.WithTagsMut(...)`.

## Translation Unit Order
1. Project scaffolding (`.gitignore`, solution, csproj files).
2. Tag constants + tag definition table + registration API (`Tags.cs`).
3. Tests for constant parity and store registration behavior (`TagsTests.cs`).
4. Build/test loop and checklist reconciliation.

## Planned C# File Mapping
- `src/lib.rs` + `src/tags_registry.rs` → `BCTags/Tags.cs`
- (No Rust tests) parity coverage → `BCTags.Tests/TagsTests.cs`

## Project Structure
```
csharp/BCTags/
├── .gitignore
├── LOG.md
├── MANIFEST.md
├── COMPLETENESS.md
├── BCTags.slnx
├── BCTags/
│   ├── BCTags.csproj
│   └── Tags.cs
└── BCTags.Tests/
    ├── BCTags.Tests.csproj
    └── TagsTests.cs
```
