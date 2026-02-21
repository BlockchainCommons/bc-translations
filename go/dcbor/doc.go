// Package dcbor implements deterministic CBOR with Rust dcbor parity semantics
// adapted to idiomatic Go APIs.
//
// Parity scope (default-feature Rust behavior):
//   - Core symbolic model: CBOR, ByteString, Map, Set, Simple, Date, Tag.
//   - Canonical encode/decode behavior with deterministic map/set ordering.
//   - Diagnostic and annotated-hex formatting helpers.
//   - Tag registry/summarizer support and structured CBOR tree walking.
//   - Typed conversion/decode helpers spanning numeric widths, arrays/maps/sets,
//     tagged values, and date/bignum handling.
//
// Rust-to-Go API mapping:
//   - Rust trait matrices (TryFrom/Into, CBORDecodable, tagged decode traits)
//     are represented as explicit Go helpers such as TryInto*, Into*, Decode*,
//     TryFromCBOR*, and DecodeTagged*.
//   - Rust macro helpers (with_tags!/with_tags_mut!) map to WithTags/WithTagsMut.
//   - Deterministic equality expectations are provided via Equal methods on core
//     value/collection wrappers.
//
// Typical usage:
//   - Build values from Go data using FromAny/MustFromAny.
//   - Encode with ToCBORData or Hex and decode with TryFromData/TryFromHex.
//   - Use typed conversion helpers (TryInto*/Decode*) for strict extraction.
//
// This package focuses on semantic parity and deterministic behavior first,
// while using Go-native API shapes where Rust trait/macro constructs do not map
// one-to-one.
package dcbor
