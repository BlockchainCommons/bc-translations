// Package bctags defines Blockchain Commons CBOR semantic tag constants
// and registration functions built on top of dcbor.
//
// The package provides 75 named tags covering the Gordian Envelope ecosystem,
// distributed function calls, cryptographic key types, wallet descriptors,
// SSH artifacts, and provenance marks. Legacy (V1) tags are included for
// backward compatibility.
//
// Usage:
//
//	import "github.com/nickel-blockchaincommons/bctags-go"
//
//	// Register all bc-tags (plus dcbor base tags) in the global store.
//	bctags.RegisterTags()
//
//	// Or register into a custom store:
//	store := dcbor.NewTagsStore(nil)
//	bctags.RegisterTagsIn(store)
package bctags
