# Translation Manifest: known-values → Go (knownvalues)

## Crate Summary

- Rust crate: `known-values`
- Rust version in source: `0.15.5`
- Package description: `Blockchain Commons Known Values.`
- Target package: `knownvalues`
- Target module: `github.com/nickel-blockchaincommons/knownvalues-go`

## Dependencies

### Internal BC Dependencies

- `bc-components`
  - Purpose: `Digest` type and `DigestProvider` behavior used by `KnownValue`
  - Go equivalent: `github.com/nickel-blockchaincommons/bccomponents-go`
- `dcbor`
  - Purpose: tagged CBOR encode/decode for `KnownValue`
  - Go equivalent: `github.com/nickel-blockchaincommons/dcbor-go`

### External Dependencies

- `paste`
  - Purpose: expands paired raw-value and `KnownValue` constants in the registry
  - Go equivalent: expand the registry constants directly in Go source
- `serde`
  - Purpose: JSON data binding for directory-loaded registries
  - Go equivalent: standard library `encoding/json`
- `serde_json`
  - Purpose: JSON parsing for registry files
  - Go equivalent: standard library `encoding/json`
- `dirs`
  - Purpose: discover the user home directory for `~/.known-values`
  - Go equivalent: standard library `os.UserHomeDir` and `path/filepath`
- `tempfile` (dev dependency)
  - Purpose: temporary directories for tests
  - Go equivalent: `testing.T.TempDir`

## Feature Flags

- `default = ["directory-loading"]`
  - Translate: yes
  - Notes: directory loading is part of the default Go package behavior.
- `directory-loading`
  - Gates: JSON registry file loading, configuration APIs, tolerant loading types, and related re-exports from `directory_loader.rs`
  - Translate: yes
- Non-default-only code
  - None beyond disabling `directory-loading`

## Public API Catalog

### Public Types

- `KnownValue`
  - Kind: struct
  - Fields: raw `u64` value and optional assigned name
  - Derives/behavior: `Clone`, `Debug`, `PartialEq`, `Eq`, `Hash`, `Display`
  - Protocols/traits: `DigestProvider`, `CBORTagged`, `CBORTaggedEncodable`, `CBORTaggedDecodable`, `From<u64>`, `From<i32>`, `From<usize>`, `TryFrom<CBOR>`, `Into<CBOR>`
- `KnownValuesStore`
  - Kind: struct
  - Fields: raw-value map and assigned-name map
  - Derives/behavior: `Clone`, `Debug`, `Default`
- `LazyKnownValues`
  - Kind: struct
  - Fields: `Once`-guarded lazy global store
  - Notes: Rust marks this `#[doc(hidden)]`, but it is public because the `KNOWN_VALUES` static exposes it
- `RegistryEntry`
  - Kind: struct
  - Fields: `codepoint`, `name`, `entry_type`, `uri`, `description`
- `OntologyInfo`
  - Kind: struct
  - Fields: `name`, `source_url`, `start_code_point`, `processing_strategy`
- `RegistryFile`
  - Kind: struct
  - Fields: `ontology`, `generated`, `entries`, `statistics`
- `GeneratedInfo`
  - Kind: struct
  - Fields: `tool`
- `LoadError`
  - Kind: enum/error type
  - Variants:
    - I/O error while reading files
    - JSON parse error with associated file path
- `LoadResult`
  - Kind: struct
  - Fields: loaded values keyed by codepoint, processed directory paths, accumulated non-fatal errors
- `DirectoryConfig`
  - Kind: struct
  - Fields: ordered search paths
  - Derives/behavior: `Debug`, `Clone`, `Default`
- `ConfigError`
  - Kind: enum/error type
  - Variants:
    - `AlreadyInitialized`

### Public Functions and Methods

#### `KnownValue`

- Constructor surface
  - `new` → Go `NewKnownValue`
  - `new_with_name` → Go `NewKnownValueWithName`
  - `new_with_static_name` → Go `NewKnownValueWithStaticName`
- Methods
  - `value(&self) -> u64` → Go `Value() uint64`
  - `assigned_name(&self) -> Option<&str>` → Go `AssignedName() (string, bool)`
  - `name(&self) -> String` → Go `Name() string`
  - `digest(&self) -> Digest` → Go `Digest() bccomponents.Digest`
  - `cbor_tags() -> Vec<Tag>` → Go `KnownValueCBORTags() []dcbor.Tag` plus `CBORTags() []dcbor.Tag`
  - `untagged_cbor(&self) -> CBOR` → Go `UntaggedCBOR() dcbor.CBOR`
  - `from_untagged_cbor(cbor: CBOR) -> dcbor::Result<Self>` → Go `DecodeKnownValue(dcbor.CBOR) (KnownValue, error)`
  - `from_tagged_cbor(cbor: CBOR)` via trait helpers → Go `DecodeTaggedKnownValue(dcbor.CBOR) (KnownValue, error)`
- Conversion equivalents
  - `From<u64>`
  - `From<i32>`
  - `From<usize>`
  - `TryFrom<CBOR>`
  - `Into<CBOR>`

#### `KnownValuesStore`

- `new<T: IntoIterator<Item = KnownValue>>(known_values: T) -> Self` → Go `NewKnownValuesStore(...KnownValue) *KnownValuesStore`
- `insert(&mut self, known_value: KnownValue)` → Go `Insert(KnownValue)`
- `assigned_name(&self, known_value: &KnownValue) -> Option<&str>` → Go `AssignedName(KnownValue) (string, bool)`
- `name(&self, known_value: KnownValue) -> String` → Go `Name(KnownValue) string`
- `known_value_named(&self, assigned_name: &str) -> Option<&KnownValue>` → Go `KnownValueNamed(string) (KnownValue, bool)`
- `known_value_for_raw_value(raw_value: u64, known_values: Option<&Self>) -> KnownValue`
- `known_value_for_name(name: &str, known_values: Option<&Self>) -> Option<KnownValue>`
- `name_for_known_value(known_value: KnownValue, known_values: Option<&Self>) -> String`
- `load_from_directory(&mut self, path: &Path) -> Result<usize, LoadError>` → Go `LoadFromDirectory(string) (int, error)`
- `load_from_config(&mut self, config: &DirectoryConfig) -> LoadResult` → Go `LoadFromConfig(DirectoryConfig) LoadResult`

#### `LazyKnownValues`

- `get(&self) -> MutexGuard<'_, Option<KnownValuesStore>>` → Go `Get() *KnownValuesStore`

#### `LoadResult`

- `values_count(&self) -> usize` → Go `ValuesCount() int`
- `values_iter(&self) -> impl Iterator<Item = &KnownValue>` → Go slice-returning helper
- `into_values(self) -> impl Iterator<Item = KnownValue>` → Go slice-returning helper
- `has_errors(&self) -> bool` → Go `HasErrors() bool`

#### `DirectoryConfig`

- `new() -> Self` → Go `NewDirectoryConfig() DirectoryConfig`
- `default_only() -> Self` → Go `DefaultOnlyDirectoryConfig() DirectoryConfig`
- `with_paths(paths: Vec<PathBuf>) -> Self` → Go `DirectoryConfigWithPaths([]string) DirectoryConfig`
- `with_paths_and_default(paths: Vec<PathBuf>) -> Self` → Go `DirectoryConfigWithPathsAndDefault([]string) DirectoryConfig`
- `default_directory() -> PathBuf` → Go `DefaultDirectory() string`
- `paths(&self) -> &[PathBuf]` → Go `Paths() []string`
- `add_path(&mut self, path: PathBuf)` → Go `AddPath(string)`

#### Free Functions Re-exported by `lib.rs`

- `load_from_directory(path: &Path) -> Result<Vec<KnownValue>, LoadError>` → Go `LoadFromDirectory(string) ([]KnownValue, error)`
- `load_from_config(config: &DirectoryConfig) -> LoadResult` → Go `LoadFromConfig(DirectoryConfig) LoadResult`
- `set_directory_config(config: DirectoryConfig) -> Result<(), ConfigError>` → Go `SetDirectoryConfig(DirectoryConfig) error`
- `add_search_paths(paths: Vec<PathBuf>) -> Result<(), ConfigError>` → Go `AddSearchPaths([]string) error`

### Public Constants and Statics

- Public macro surface
  - `const_known_value!`
  - Go translation note: no exported macro equivalent; define the generated raw constants and `KnownValue` vars directly.
- Public static
  - `KNOWN_VALUES` → Go `KnownValues`

### Registry Inventory

Each Rust `const_known_value!` declaration expands to two public items:

- `<NAME>_RAW: u64`
- `<NAME>: KnownValue`

The full public registry inventory is:

| Rust Raw | Rust Value | Go Raw | Go Value | Numeric Value | Display Name |
| --- | --- | --- | --- | ---: | --- |
| `UNIT_RAW` | `UNIT` | `UnitRaw` | `Unit` | 0 | `""` |
| `IS_A_RAW` | `IS_A` | `IsARaw` | `IsA` | 1 | `"isA"` |
| `ID_RAW` | `ID` | `IDRaw` | `ID` | 2 | `"id"` |
| `SIGNED_RAW` | `SIGNED` | `SignedRaw` | `Signed` | 3 | `"signed"` |
| `NOTE_RAW` | `NOTE` | `NoteRaw` | `Note` | 4 | `"note"` |
| `HAS_RECIPIENT_RAW` | `HAS_RECIPIENT` | `HasRecipientRaw` | `HasRecipient` | 5 | `"hasRecipient"` |
| `SSKR_SHARE_RAW` | `SSKR_SHARE` | `SSKRShareRaw` | `SSKRShare` | 6 | `"sskrShare"` |
| `CONTROLLER_RAW` | `CONTROLLER` | `ControllerRaw` | `Controller` | 7 | `"controller"` |
| `KEY_RAW` | `KEY` | `KeyRaw` | `Key` | 8 | `"key"` |
| `DEREFERENCE_VIA_RAW` | `DEREFERENCE_VIA` | `DereferenceViaRaw` | `DereferenceVia` | 9 | `"dereferenceVia"` |
| `ENTITY_RAW` | `ENTITY` | `EntityRaw` | `Entity` | 10 | `"entity"` |
| `NAME_RAW` | `NAME` | `NameRaw` | `Name` | 11 | `"name"` |
| `LANGUAGE_RAW` | `LANGUAGE` | `LanguageRaw` | `Language` | 12 | `"language"` |
| `ISSUER_RAW` | `ISSUER` | `IssuerRaw` | `Issuer` | 13 | `"issuer"` |
| `HOLDER_RAW` | `HOLDER` | `HolderRaw` | `Holder` | 14 | `"holder"` |
| `SALT_RAW` | `SALT` | `SaltRaw` | `Salt` | 15 | `"salt"` |
| `DATE_RAW` | `DATE` | `DateRaw` | `Date` | 16 | `"date"` |
| `UNKNOWN_VALUE_RAW` | `UNKNOWN_VALUE` | `UnknownValueRaw` | `UnknownValue` | 17 | `"Unknown"` |
| `VERSION_VALUE_RAW` | `VERSION_VALUE` | `VersionValueRaw` | `VersionValue` | 18 | `"version"` |
| `HAS_SECRET_RAW` | `HAS_SECRET` | `HasSecretRaw` | `HasSecret` | 19 | `"hasSecret"` |
| `DIFF_EDITS_RAW` | `DIFF_EDITS` | `DiffEditsRaw` | `DiffEdits` | 20 | `"edits"` |
| `VALID_FROM_RAW` | `VALID_FROM` | `ValidFromRaw` | `ValidFrom` | 21 | `"validFrom"` |
| `VALID_UNTIL_RAW` | `VALID_UNTIL` | `ValidUntilRaw` | `ValidUntil` | 22 | `"validUntil"` |
| `POSITION_RAW` | `POSITION` | `PositionRaw` | `Position` | 23 | `"position"` |
| `NICKNAME_RAW` | `NICKNAME` | `NicknameRaw` | `Nickname` | 24 | `"nickname"` |
| `VALUE_RAW` | `VALUE` | `ValueRaw` | `Value` | 25 | `"value"` |
| `ATTESTATION_RAW` | `ATTESTATION` | `AttestationRaw` | `Attestation` | 26 | `"attestation"` |
| `VERIFIABLE_AT_RAW` | `VERIFIABLE_AT` | `VerifiableAtRaw` | `VerifiableAt` | 27 | `"verifiableAt"` |
| `ATTACHMENT_RAW` | `ATTACHMENT` | `AttachmentRaw` | `Attachment` | 50 | `"attachment"` |
| `VENDOR_RAW` | `VENDOR` | `VendorRaw` | `Vendor` | 51 | `"vendor"` |
| `CONFORMS_TO_RAW` | `CONFORMS_TO` | `ConformsToRaw` | `ConformsTo` | 52 | `"conformsTo"` |
| `ALLOW_RAW` | `ALLOW` | `AllowRaw` | `Allow` | 60 | `"allow"` |
| `DENY_RAW` | `DENY` | `DenyRaw` | `Deny` | 61 | `"deny"` |
| `ENDPOINT_RAW` | `ENDPOINT` | `EndpointRaw` | `Endpoint` | 62 | `"endpoint"` |
| `DELEGATE_RAW` | `DELEGATE` | `DelegateRaw` | `Delegate` | 63 | `"delegate"` |
| `PROVENANCE_RAW` | `PROVENANCE` | `ProvenanceRaw` | `Provenance` | 64 | `"provenance"` |
| `PRIVATE_KEY_RAW` | `PRIVATE_KEY` | `PrivateKeyRaw` | `PrivateKey` | 65 | `"privateKey"` |
| `SERVICE_RAW` | `SERVICE` | `ServiceRaw` | `Service` | 66 | `"service"` |
| `CAPABILITY_RAW` | `CAPABILITY` | `CapabilityRaw` | `Capability` | 67 | `"capability"` |
| `PROVENANCE_GENERATOR_RAW` | `PROVENANCE_GENERATOR` | `ProvenanceGeneratorRaw` | `ProvenanceGenerator` | 68 | `"provenanceGenerator"` |
| `PRIVILEGE_ALL_RAW` | `PRIVILEGE_ALL` | `PrivilegeAllRaw` | `PrivilegeAll` | 70 | `"All"` |
| `PRIVILEGE_AUTH_RAW` | `PRIVILEGE_AUTH` | `PrivilegeAuthRaw` | `PrivilegeAuth` | 71 | `"Authorize"` |
| `PRIVILEGE_SIGN_RAW` | `PRIVILEGE_SIGN` | `PrivilegeSignRaw` | `PrivilegeSign` | 72 | `"Sign"` |
| `PRIVILEGE_ENCRYPT_RAW` | `PRIVILEGE_ENCRYPT` | `PrivilegeEncryptRaw` | `PrivilegeEncrypt` | 73 | `"Encrypt"` |
| `PRIVILEGE_ELIDE_RAW` | `PRIVILEGE_ELIDE` | `PrivilegeElideRaw` | `PrivilegeElide` | 74 | `"Elide"` |
| `PRIVILEGE_ISSUE_RAW` | `PRIVILEGE_ISSUE` | `PrivilegeIssueRaw` | `PrivilegeIssue` | 75 | `"Issue"` |
| `PRIVILEGE_ACCESS_RAW` | `PRIVILEGE_ACCESS` | `PrivilegeAccessRaw` | `PrivilegeAccess` | 76 | `"Access"` |
| `PRIVILEGE_DELEGATE_RAW` | `PRIVILEGE_DELEGATE` | `PrivilegeDelegateRaw` | `PrivilegeDelegate` | 80 | `"Delegate"` |
| `PRIVILEGE_VERIFY_RAW` | `PRIVILEGE_VERIFY` | `PrivilegeVerifyRaw` | `PrivilegeVerify` | 81 | `"Verify"` |
| `PRIVILEGE_UPDATE_RAW` | `PRIVILEGE_UPDATE` | `PrivilegeUpdateRaw` | `PrivilegeUpdate` | 82 | `"Update"` |
| `PRIVILEGE_TRANSFER_RAW` | `PRIVILEGE_TRANSFER` | `PrivilegeTransferRaw` | `PrivilegeTransfer` | 83 | `"Transfer"` |
| `PRIVILEGE_ELECT_RAW` | `PRIVILEGE_ELECT` | `PrivilegeElectRaw` | `PrivilegeElect` | 84 | `"Elect"` |
| `PRIVILEGE_BURN_RAW` | `PRIVILEGE_BURN` | `PrivilegeBurnRaw` | `PrivilegeBurn` | 85 | `"Burn"` |
| `PRIVILEGE_REVOKE_RAW` | `PRIVILEGE_REVOKE` | `PrivilegeRevokeRaw` | `PrivilegeRevoke` | 86 | `"Revoke"` |
| `BODY_RAW` | `BODY` | `BodyRaw` | `Body` | 100 | `"body"` |
| `RESULT_RAW` | `RESULT` | `ResultRaw` | `Result` | 101 | `"result"` |
| `ERROR_RAW` | `ERROR` | `ErrorRaw` | `Error` | 102 | `"error"` |
| `OK_VALUE_RAW` | `OK_VALUE` | `OKValueRaw` | `OKValue` | 103 | `"OK"` |
| `PROCESSING_VALUE_RAW` | `PROCESSING_VALUE` | `ProcessingValueRaw` | `ProcessingValue` | 104 | `"Processing"` |
| `SENDER_RAW` | `SENDER` | `SenderRaw` | `Sender` | 105 | `"sender"` |
| `SENDER_CONTINUATION_RAW` | `SENDER_CONTINUATION` | `SenderContinuationRaw` | `SenderContinuation` | 106 | `"senderContinuation"` |
| `RECIPIENT_CONTINUATION_RAW` | `RECIPIENT_CONTINUATION` | `RecipientContinuationRaw` | `RecipientContinuation` | 107 | `"recipientContinuation"` |
| `CONTENT_RAW` | `CONTENT` | `ContentRaw` | `Content` | 108 | `"content"` |
| `SEED_TYPE_RAW` | `SEED_TYPE` | `SeedTypeRaw` | `SeedType` | 200 | `"Seed"` |
| `PRIVATE_KEY_TYPE_RAW` | `PRIVATE_KEY_TYPE` | `PrivateKeyTypeRaw` | `PrivateKeyType` | 201 | `"PrivateKey"` |
| `PUBLIC_KEY_TYPE_RAW` | `PUBLIC_KEY_TYPE` | `PublicKeyTypeRaw` | `PublicKeyType` | 202 | `"PublicKey"` |
| `MASTER_KEY_TYPE_RAW` | `MASTER_KEY_TYPE` | `MasterKeyTypeRaw` | `MasterKeyType` | 203 | `"MasterKey"` |
| `ASSET_RAW` | `ASSET` | `AssetRaw` | `Asset` | 300 | `"asset"` |
| `BITCOIN_VALUE_RAW` | `BITCOIN_VALUE` | `BitcoinValueRaw` | `BitcoinValue` | 301 | `"Bitcoin"` |
| `ETHEREUM_VALUE_RAW` | `ETHEREUM_VALUE` | `EthereumValueRaw` | `EthereumValue` | 302 | `"Ethereum"` |
| `TEZOS_VALUE_RAW` | `TEZOS_VALUE` | `TezosValueRaw` | `TezosValue` | 303 | `"Tezos"` |
| `NETWORK_RAW` | `NETWORK` | `NetworkRaw` | `Network` | 400 | `"network"` |
| `MAIN_NET_VALUE_RAW` | `MAIN_NET_VALUE` | `MainNetValueRaw` | `MainNetValue` | 401 | `"MainNet"` |
| `TEST_NET_VALUE_RAW` | `TEST_NET_VALUE` | `TestNetValueRaw` | `TestNetValue` | 402 | `"TestNet"` |
| `BIP32_KEY_TYPE_RAW` | `BIP32_KEY_TYPE` | `BIP32KeyTypeRaw` | `BIP32KeyType` | 500 | `"BIP32Key"` |
| `CHAIN_CODE_RAW` | `CHAIN_CODE` | `ChainCodeRaw` | `ChainCode` | 501 | `"chainCode"` |
| `DERIVATION_PATH_TYPE_RAW` | `DERIVATION_PATH_TYPE` | `DerivationPathTypeRaw` | `DerivationPathType` | 502 | `"DerivationPath"` |
| `PARENT_PATH_RAW` | `PARENT_PATH` | `ParentPathRaw` | `ParentPath` | 503 | `"parentPath"` |
| `CHILDREN_PATH_RAW` | `CHILDREN_PATH` | `ChildrenPathRaw` | `ChildrenPath` | 504 | `"childrenPath"` |
| `PARENT_FINGERPRINT_RAW` | `PARENT_FINGERPRINT` | `ParentFingerprintRaw` | `ParentFingerprint` | 505 | `"parentFingerprint"` |
| `PSBT_TYPE_RAW` | `PSBT_TYPE` | `PSBTTypeRaw` | `PSBTType` | 506 | `"PSBT"` |
| `OUTPUT_DESCRIPTOR_TYPE_RAW` | `OUTPUT_DESCRIPTOR_TYPE` | `OutputDescriptorTypeRaw` | `OutputDescriptorType` | 507 | `"OutputDescriptor"` |
| `OUTPUT_DESCRIPTOR_RAW` | `OUTPUT_DESCRIPTOR` | `OutputDescriptorRaw` | `OutputDescriptor` | 508 | `"outputDescriptor"` |
| `GRAPH_RAW` | `GRAPH` | `GraphRaw` | `Graph` | 600 | `"Graph"` |
| `SOURCE_TARGET_GRAPH_RAW` | `SOURCE_TARGET_GRAPH` | `SourceTargetGraphRaw` | `SourceTargetGraph` | 601 | `"SourceTargetGraph"` |
| `PARENT_CHILD_GRAPH_RAW` | `PARENT_CHILD_GRAPH` | `ParentChildGraphRaw` | `ParentChildGraph` | 602 | `"ParentChildGraph"` |
| `DIGRAPH_RAW` | `DIGRAPH` | `DigraphRaw` | `Digraph` | 603 | `"Digraph"` |
| `ACYCLIC_GRAPH_RAW` | `ACYCLIC_GRAPH` | `AcyclicGraphRaw` | `AcyclicGraph` | 604 | `"AcyclicGraph"` |
| `MULTIGRAPH_RAW` | `MULTIGRAPH` | `MultigraphRaw` | `Multigraph` | 605 | `"Multigraph"` |
| `PSEUDOGRAPH_RAW` | `PSEUDOGRAPH` | `PseudographRaw` | `Pseudograph` | 606 | `"Pseudograph"` |
| `GRAPH_FRAGMENT_RAW` | `GRAPH_FRAGMENT` | `GraphFragmentRaw` | `GraphFragment` | 607 | `"GraphFragment"` |
| `DAG_RAW` | `DAG` | `DAGRaw` | `DAG` | 608 | `"DAG"` |
| `TREE_RAW` | `TREE` | `TreeRaw` | `Tree` | 609 | `"Tree"` |
| `FOREST_RAW` | `FOREST` | `ForestRaw` | `Forest` | 610 | `"Forest"` |
| `COMPOUND_GRAPH_RAW` | `COMPOUND_GRAPH` | `CompoundGraphRaw` | `CompoundGraph` | 611 | `"CompoundGraph"` |
| `HYPERGRAPH_RAW` | `HYPERGRAPH` | `HypergraphRaw` | `Hypergraph` | 612 | `"Hypergraph"` |
| `DIHYPERGRAPH_RAW` | `DIHYPERGRAPH` | `DihypergraphRaw` | `Dihypergraph` | 613 | `"Dihypergraph"` |
| `NODE_RAW` | `NODE` | `NodeRaw` | `Node` | 700 | `"node"` |
| `EDGE_RAW` | `EDGE` | `EdgeRaw` | `Edge` | 701 | `"edge"` |
| `SOURCE_RAW` | `SOURCE` | `SourceRaw` | `Source` | 702 | `"source"` |
| `TARGET_RAW` | `TARGET` | `TargetRaw` | `Target` | 703 | `"target"` |
| `PARENT_RAW` | `PARENT` | `ParentRaw` | `Parent` | 704 | `"parent"` |
| `CHILD_RAW` | `CHILD` | `ChildRaw` | `Child` | 705 | `"child"` |
| `SELF_RAW` | `SELF` | `SelfRaw` | `Self` | 706 | `"Self"` |

### Global Store Initialization Inventory

- The Rust lazy singleton initializes the store with 102 of the 104 public `KnownValue` constants.
- `VALUE` / `Value` and `SELF` / `Self` are public constants, but they are intentionally omitted from the `KNOWN_VALUES` / `KnownValues` initialization list in the current Rust source.
- The Go translation must preserve this current source-of-truth behavior and test for it explicitly.

## Documentation Catalog

- Crate-level doc comment in `src/lib.rs`: present
  - Summary: overview, basic usage, default directory-loading behavior, JSON file format, custom configuration, and disabling default features
- Module-level docs
  - `directory_loader.rs`: present
  - `known_value.rs`: present
  - `known_value_store.rs`: present
  - `known_values_registry.rs`: present
- Public items with Rust doc comments
  - `KnownValue`, `KnownValuesStore`, `LazyKnownValues`, `RegistryEntry`, `OntologyInfo`, `RegistryFile`, `GeneratedInfo`, `LoadError`, `LoadResult`, `DirectoryConfig`, `ConfigError`
  - All public methods and free functions listed above
- Public items without Rust doc comments
  - Individual registry constants generated by `const_known_value!`
- Package metadata description
  - Present in `Cargo.toml`
- README
  - Present; includes getting started, version history, and review status

## Test Inventory

### Rust unit tests

- `src/known_values_registry.rs`
  - `test_1`
- `src/directory_loader.rs`
  - `test_parse_registry_json`
  - `test_parse_minimal_registry`
  - `test_parse_full_entry`
  - `test_directory_config_default`
  - `test_directory_config_custom_paths`
  - `test_directory_config_with_default`
  - `test_load_from_nonexistent_directory`
  - `test_load_result_methods`

### Rust integration tests

- `tests/directory_loading.rs`
  - `test_global_registry_still_works`
  - `test_load_from_temp_directory`
  - `test_override_hardcoded_value`
  - `test_multiple_files_in_directory`
  - `test_directory_config_custom_paths`
  - `test_later_directory_overrides_earlier`
  - `test_nonexistent_directory_is_ok`
  - `test_invalid_json_is_error`
  - `test_tolerant_loading_continues_on_error`
  - `test_full_registry_format`
  - `test_load_result_methods`
  - `test_empty_entries_array`
  - `test_non_json_files_ignored`

### Translation coverage notes

- Rust relies heavily on doctests for `KnownValue` and `KnownValuesStore`.
- The Go translation should add direct API coverage tests for:
  - `KnownValue` naming, equality, string form, digest, and CBOR helpers
  - `KnownValuesStore` clone behavior and helper functions
  - `SetDirectoryConfig` / `AddSearchPaths` locking behavior around the lazy global registry
  - Full registry inventory parity and the intentional omission of `Value` and `Self` from the global store

## EXPECTED TEXT OUTPUT RUBRIC

- Applicable: no
- Reason: the Rust test suite validates numeric values, registry lookups, JSON parsing, and filesystem-backed loading; it does not contain complex rendered-text or multiline formatting assertions.

## Translation Unit Order

1. Package docs and module scaffolding
2. `KnownValue`
3. `KnownValuesStore`
4. Registry constants and lazy global store
5. Directory loader models and configuration
6. Rust unit tests translated into Go unit tests
7. Additional Go API coverage tests for doctest-only Rust surface

## Translation Hazards

- The registry surface is macro-generated in Rust. The Go translation should generate or verify the constant table mechanically to avoid transcription drift.
- The lazy global store intentionally omits `Value` and `Self`; this is easy to “fix” accidentally when building the Go registry slice.
- The default feature is filesystem-backed and global. Tests must isolate the lazy singleton and configuration state so local user directories cannot leak values into deterministic test runs.
- Rust exposes both strict directory loading (`load_from_directory`) and tolerant multi-directory loading (`load_from_config`). The Go translation must preserve the difference in error-handling behavior.
- `KnownValue` equality and hashing are based only on the raw numeric value, not the assigned name.
- CBOR digest parity depends on encoding the tagged form, not the untagged integer alone.
