// Package knownvalues defines Blockchain Commons known values.
//
// Known values provide compact, deterministic identifiers for common
// ontological concepts. Each known value is represented by a 64-bit unsigned
// integer with an optional assigned display name and supports tagged dCBOR
// encoding. By default, the package can also load additional registry entries
// from JSON files in configured directories such as `~/.known-values`.
package knownvalues
