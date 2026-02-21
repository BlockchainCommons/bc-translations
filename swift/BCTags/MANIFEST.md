# Translation Manifest: bc-tags → Swift (BCTags)

Source: `rust/bc-tags/` v0.12.0
Target: `swift/BCTags/` module `BCTags`

## Crate Metadata
- Crate: `bc-tags`
- Version: `0.12.0`
- Rust edition: `2024`
- Description: "Blockchain Commons CBOR Tags"

## Dependencies

### Internal BC dependencies
- `dcbor` (`^0.25.0`) — In the Swift ecosystem, BCTags is standalone (no dependencies).
  The Tag/TagsStore infrastructure that lives in Rust's dcbor is provided by BCTags in Swift.
  This matches the existing external BCSwiftTags package architecture that DCBOR already depends on.

### External dependencies
- Runtime: `paste` (macro helper) — no Swift equivalent needed; macro output expanded manually.
- Dev/test-only: none.

## Feature Flags
- No feature flags in `Cargo.toml`.
- Translation scope: full crate (default behavior only).

## Architecture Note

In Rust, `dcbor` defines `Tag`, `TagsStore`, and global tag infrastructure, while `bc-tags` only adds
tag constants and registration functions. In Swift, the external BCSwiftTags package bundles both:
Tag/TagsStore infrastructure AND tag constants. Our `swift/BCTags/` follows this Swift-ecosystem
pattern so it can serve as a drop-in replacement for the external BCSwiftTags package that DCBOR
already depends on.

## Public API Surface

### Type Catalog

#### Tag (struct, Sendable, Hashable, ExpressibleByIntegerLiteral, CustomStringConvertible)
- `value: UInt64` — the tag's numeric value
- `names: [String]` — the tag's known names (first is preferred)
- `name: String?` — computed, returns `names.first`
- `init(_ value: UInt64, _ names: [String])` — init with value and names array
- `init(_ value: UInt64, _ name: String?)` — convenience init with optional single name
- `init(integerLiteral:)` — ExpressibleByIntegerLiteral conformance
- `==` based on `value` only
- `hash(into:)` based on `value` only
- `description` returns `name ?? String(value)`

#### TagsStoreProtocol (protocol)
- `assignedName(for tag: Tag) -> String?`
- `name(for tag: Tag) -> String`
- `tag(for value: UInt64) -> Tag?`
- `tag(for name: String) -> Tag?`

#### TagsStore (class, final, TagsStoreProtocol, Sequence, @unchecked Sendable, ExpressibleByArrayLiteral)
- `tagsByValue: [UInt64: Tag]` — public read-only
- `tagsByName: [String: Tag]` — public read-only
- `init<T>(_ tags: T) where T: Sequence, T.Element == Tag`
- `init()` — convenience empty init
- `@MainActor insert(_ tag: Tag)`
- `insertAll(_ tags: [Tag])` — batch insert (for registration)
- `assignedName(for:)`, `name(for:)`, `tag(for value:)`, `tag(for name:)` — protocol conformance
- Sequence conformance via `TagsIterator`
- ExpressibleByArrayLiteral conformance

#### TagsIterator (struct, IteratorProtocol)
- Iterates tags in numeric order (sorted by value)

#### Free Functions
- `name(for tag: Tag, knownTags: TagsStoreProtocol?) -> String`

#### Globals
- `globalTags: TagsStore` — the shared global tag store

### Constant Catalog

75 tag constants as `static let` on `Tag`:

1. `uri` = Tag(32, "url")
2. `uuid` = Tag(37, "uuid")
3. `encodedCBOR` = Tag(24, "encoded-cbor")
4. `envelope` = Tag(200, "envelope")
5. `leaf` = Tag(201, "leaf")
6. `json` = Tag(262, "json")
7. `knownValue` = Tag(40000, "known-value")
8. `digest` = Tag(40001, "digest")
9. `encrypted` = Tag(40002, "encrypted")
10. `compressed` = Tag(40003, "compressed")
11. `request` = Tag(40004, "request")
12. `response` = Tag(40005, "response")
13. `function` = Tag(40006, "function")
14. `parameter` = Tag(40007, "parameter")
15. `placeholder` = Tag(40008, "placeholder")
16. `replacement` = Tag(40009, "replacement")
17. `x25519PrivateKey` = Tag(40010, "agreement-private-key")
18. `x25519PublicKey` = Tag(40011, "agreement-public-key")
19. `arid` = Tag(40012, "arid")
20. `privateKeys` = Tag(40013, "crypto-prvkeys")
21. `nonce` = Tag(40014, "nonce")
22. `password` = Tag(40015, "password")
23. `privateKeyBase` = Tag(40016, "crypto-prvkey-base")
24. `publicKeys` = Tag(40017, "crypto-pubkeys")
25. `salt` = Tag(40018, "salt")
26. `sealedMessage` = Tag(40019, "crypto-sealed")
27. `signature` = Tag(40020, "signature")
28. `signingPrivateKey` = Tag(40021, "signing-private-key")
29. `signingPublicKey` = Tag(40022, "signing-public-key")
30. `symmetricKey` = Tag(40023, "crypto-key")
31. `xid` = Tag(40024, "xid")
32. `reference` = Tag(40025, "reference")
33. `event` = Tag(40026, "event")
34. `encryptedKey` = Tag(40027, "encrypted-key")
35. `mlkemPrivateKey` = Tag(40100, "mlkem-private-key")
36. `mlkemPublicKey` = Tag(40101, "mlkem-public-key")
37. `mlkemCiphertext` = Tag(40102, "mlkem-ciphertext")
38. `mldsaPrivateKey` = Tag(40103, "mldsa-private-key")
39. `mldsaPublicKey` = Tag(40104, "mldsa-public-key")
40. `mldsaSignature` = Tag(40105, "mldsa-signature")
41. `seed` = Tag(40300, "seed")
42. `hdKey` = Tag(40303, "hdkey")
43. `derivationPath` = Tag(40304, "keypath")
44. `useInfo` = Tag(40305, "coin-info")
45. `ecKey` = Tag(40306, "eckey")
46. `address` = Tag(40307, "address")
47. `outputDescriptor` = Tag(40308, "output-descriptor")
48. `sskrShare` = Tag(40309, "sskr")
49. `psbt` = Tag(40310, "psbt")
50. `accountDescriptor` = Tag(40311, "account-descriptor")
51. `sshTextPrivateKey` = Tag(40800, "ssh-private")
52. `sshTextPublicKey` = Tag(40801, "ssh-public")
53. `sshTextSignature` = Tag(40802, "ssh-signature")
54. `sshTextCertificate` = Tag(40803, "ssh-certificate")
55. `provenanceMark` = Tag(1347571542, "provenance")
56. `seedV1` = Tag(300, "crypto-seed")
57. `ecKeyV1` = Tag(306, "crypto-eckey")
58. `sskrShareV1` = Tag(309, "crypto-sskr")
59. `hdKeyV1` = Tag(303, "crypto-hdkey")
60. `derivationPathV1` = Tag(304, "crypto-keypath")
61. `useInfoV1` = Tag(305, "crypto-coin-info")
62. `outputDescriptorV1` = Tag(307, "crypto-output")
63. `psbtV1` = Tag(310, "crypto-psbt")
64. `accountV1` = Tag(311, "crypto-account")
65. `outputScriptHash` = Tag(400, "output-script-hash")
66. `outputWitnessScriptHash` = Tag(401, "output-witness-script-hash")
67. `outputPublicKey` = Tag(402, "output-public-key")
68. `outputPublicKeyHash` = Tag(403, "output-public-key-hash")
69. `outputWitnessPublicKeyHash` = Tag(404, "output-witness-public-key-hash")
70. `outputCombo` = Tag(405, "output-combo")
71. `outputMultisig` = Tag(406, "output-multisig")
72. `outputSortedMultisig` = Tag(407, "output-sorted-multisig")
73. `outputRawScript` = Tag(408, "output-raw-script")
74. `outputTaproot` = Tag(409, "output-taproot")
75. `outputCosigner` = Tag(410, "output-cosigner")

### Function Catalog

- `registerTagsIn(_ tagsStore: TagsStore)` — inserts dcbor base tags (date) then all 75 bc-tags
- `registerTags()` — calls `registerTagsIn` on globalTags via @MainActor

### dcbor base tags (registered by `registerTagsIn` before bc-tags)
- `date` = Tag(1, "date")

## Documentation Catalog
- Module-level doc: narrative comments from `src/tags_registry.rs` about IANA allocation context
- Public items: tag constants are self-documenting via names; registration functions get doc comments

## Test Inventory
- Rust unit/integration tests: none (no `#[test]` items in source)
- Translation test plan (parity safety net):
  1. Validate all 75 tag value/name pairs
  2. Validate `registerTagsIn` inserts all tags and preserves dcbor base registrations
  3. Validate `registerTags` mutates the shared global store
  4. Validate Tag equality, hashing, and description

## EXPECTED TEXT OUTPUT RUBRIC
- Applicable: no
- Reason: crate has no text-rendering tests and no complex formatted output assertions.

## Translation Hazards

1. **Swift-specific Tag infrastructure**
   - Tag, TagsStore, globalTags must match the external BCSwiftTags API that DCBOR already imports
   - `@MainActor` on insert and `@unchecked Sendable` on TagsStore for thread safety

2. **Large constant surface area**
   - 75 constants increase typo risk; define in logical groups matching Rust source comments

3. **Registration ordering**
   - `registerTagsIn` must register dcbor base tags (date) before bc-tags constants

4. **V1 deprecated tags**
   - 9 deprecated tags must be included (not omitted) to match Rust source faithfully

## Translation Unit Order
1. Project scaffolding (.gitignore, Package.swift)
2. Tag.swift — Tag struct
3. TagsStore.swift — TagsStoreProtocol, TagsStore, TagsIterator, globalTags
4. Tags.swift — 75 tag constants + registration functions
5. TagsTests.swift — test all constants, registration, Tag behavior
6. Build/test loop and checklist reconciliation

## Planned Swift File Mapping
- External BCSwiftTags `Tag.swift` + Rust dcbor `Tag` → `Sources/BCTags/Tag.swift`
- External BCSwiftTags `TagsStore.swift` + Rust dcbor `TagsStore` → `Sources/BCTags/TagsStore.swift`
- Rust `src/tags_registry.rs` → `Sources/BCTags/Tags.swift`
- (No Rust tests) parity coverage → `Tests/BCTagsTests/TagsTests.swift`

## Project Structure
```
swift/BCTags/
├── .gitignore
├── Package.swift
├── LOG.md
├── MANIFEST.md
├── COMPLETENESS.md
├── Sources/BCTags/
│   ├── Tag.swift
│   ├── TagsStore.swift
│   └── Tags.swift
└── Tests/BCTagsTests/
    └── TagsTests.swift
```
