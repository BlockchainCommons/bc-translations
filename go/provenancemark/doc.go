// Package provenancemark provides a cryptographically secured system for
// establishing and verifying provenance marks.
//
// Provenance marks combine cryptography, pseudorandom number generation, and
// linguistic identifiers to produce unique, sequential marks that commit to the
// content of preceding and subsequent works. The package exposes the core mark
// model, sequence generator, validation report types, and deterministic
// byteword / UR helpers needed for cross-language parity with the Rust source.
package provenancemark
