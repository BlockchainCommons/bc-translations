# Completeness: bc-envelope → Swift (BCEnvelope)

## Source Files
- [x] Rust API surface mapped in `MANIFEST.md`

## Public API
- [x] Public exports translated (base, format, extension modules)
- [x] Envelope core type and assertion model translated
- [x] Error model and result equivalents translated
- [x] Envelope encoding/decoding traits and CBOR mappings translated
- [x] Formatting APIs translated (notation, tree, diagnostic, hex, mermaid)
- [x] Extension APIs translated (attachment, edge, compress, encrypt, expression, proof, recipient, salt, secret, signature, sskr, types)

## Tests
- [x] Rust unit and integration tests inventoried in `MANIFEST.md`
- [x] Core tests translated
- [x] Format/output tests translated
- [x] Crypto/signature/encryption tests translated
- [x] Elision/proof/obscuring tests translated
- [x] Edge/attachment/expression/type tests translated

## Build & Config
- [x] .gitignore
- [x] Package.swift
- [x] Swift package builds successfully
- [x] All Swift tests pass
