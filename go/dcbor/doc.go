// Package dcbor implements deterministic CBOR (dCBOR) encoding and decoding.
//
// The package provides:
//   - A core symbolic model: CBOR, ByteString, Map, Set, Simple, Date, Tag.
//   - Canonical encode/decode with deterministic map/set ordering.
//   - Diagnostic and annotated-hex formatting helpers.
//   - A tag registry with summarizer support and structured CBOR tree walking.
//   - Typed conversion and decode helpers spanning numeric widths, arrays,
//     maps, sets, tagged values, and date/bignum handling.
//
// API conventions:
//   - TryInto* methods return (value, error) for strict type extraction.
//   - Into* methods return (value, bool) for comma-ok style extraction.
//   - As* methods return (value, bool) for kind-checked access without conversion.
//   - Decode* free functions provide typed decoding via CBORDecodeFunc.
//   - WithTags provides locked access to the process-global tag registry.
//   - Equal methods on core types compare by deterministic CBOR encoding.
//
// Typical usage:
//   - Build values from Go data using FromAny/MustFromAny.
//   - Encode with ToCBORData or Hex and decode with TryFromData/TryFromHex.
//   - Use typed conversion helpers (TryInto*/Decode*) for strict extraction.
package dcbor
