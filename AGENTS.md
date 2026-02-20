# Blockchains Common Cross-Language Translations

The goal of this project is to provide a set of native translations of the reference Rust implementations of the Blockchains Common libraries. Each of the target languages has its own directory. The goal of each translation is to be as close as possible to the reference Rust implementation, including at *least* as much test coverage, while still being idiomatic in the target language.

| rust/           | version | csharp/        | go/            | kotlin/         | python/         | swift/          | typescript/          |
|-----------------|---------|----------------|----------------|-----------------|-----------------|-----------------|----------------------|
| bc-rand         | 0.5.0   | BCRand         | bcrand         | bc-rand         | bc-rand         | BCRand          | @bcts/rand           |
| bc-crypto       | 0.14.0  | BCCrypto       | bccrypto       | bc-crypto       | bc-crypto       | BCCrypto        | @bcts/crypto         |
| bc-shamir       | 0.13.0  | BCShamir       | bcshamir       | bc-shamir       | bc-shamir       | BCShamir        | @bcts/shamir         |
| dcbor           | 0.25.1  | DCbor          | dcbor          | dcbor           | dcbor           | DCBOR           | @bcts/dcbor          |
| bc-tags         | 0.12.0  | BCTags         | bctags         | bc-tags         | bc-tags         | BCTags          | @bcts/tags           |
| bc-ur           | 0.19.0  | BCUR           | bcur           | bc-ur           | bc-ur           | BCUR            | @bcts/ur             |
| sskr            | 0.12.0  | SSKR           | sskr           | sskr            | sskr            | SSKR            | @bcts/sskr           |
| bc-components   | 0.31.1  | BCComponents   | bccomponents   | bc-components   | bc-components   | BCComponents    | @bcts/components     |
| known-values    | 0.15.4  | KnownValues    | knownvalues    | known-values    | known-values    | KnownValues     | @bcts/known-values   |
| bc-envelope     | 0.43.0  | BCEnvelope     | bcenvelope     | bc-envelope     | bc-envelope     | BCEnvelope      | @bcts/envelope       |
| provenance-mark | 0.23.0  | ProvenanceMark | provenancemark | provenance-mark | provenance-mark | ProvenanceMark  | @bcts/provenance-mark|

## Internal Dependencies

These are the dependencies between the crates in this project. Optional dependencies are marked with `?`. The crates are listed in topological order (a valid build order).

```
bc-rand         → (none)
dcbor           → (none)
bc-crypto       → bc-rand
bc-tags         → dcbor
bc-ur           → dcbor
bc-shamir       → bc-rand, bc-crypto
sskr            → bc-rand, bc-shamir
bc-components   → bc-rand, bc-crypto, dcbor, bc-tags, bc-ur, sskr
known-values    → dcbor, bc-components
bc-envelope     → bc-rand, bc-crypto, dcbor, bc-ur, bc-components, known-values?
provenance-mark → bc-rand, dcbor, bc-tags, bc-ur, bc-envelope?
```

There are two independent dependency trees that merge at `bc-components`:

- **Crypto tree:** bc-rand → bc-crypto → bc-shamir → sskr
- **CBOR tree:** dcbor → bc-tags, bc-ur

## Package Search Indexes

The following registries/indexes can be searched for published packages:

- C#: [NuGet Gallery search](https://www.nuget.org/packages?q=)
- Go: [pkg.go.dev search](https://pkg.go.dev/search?q=)
- Kotlin: [Maven Central search](https://central.sonatype.com/search?q=)
- Python: [PyPI search](https://pypi.org/search/?q=)
- Swift: [Swift Package Index search](https://swiftpackageindex.com/search?query=)
- TypeScript: [npm search](https://www.npmjs.com/search?q=)
- Rust: [crates.io search](https://crates.io/search?q=)
