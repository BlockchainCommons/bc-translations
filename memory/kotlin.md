# Kotlin Translation Memory

- For Kotlin exception hierarchies that mirror Rust error enums, avoid `object` singleton throwables; use classes so each throw gets a fresh exception instance and stack trace.
- Before final shared-status edits in this monorepo, re-read the target row in `AGENTS.md` to avoid overwriting concurrent agent updates.
- For Rust crates with macro-generated public constants, extract symbols and registration order from source with a script, then verify translated constant counts before sign-off.
- For Kotlin wrappers over `ByteArray` that must behave like Rust `Eq`/`PartialEq`, always implement `equals`/`hashCode` using `contentEquals`/`contentHashCode`; default array equality is by reference.
- For Rust crates with test-local fake RNGs, mirror that exact fake RNG behavior in Kotlin tests instead of reusing shared helper RNGs from dependencies.
- For Rust `u64` fields parsed from JSON in Kotlin, decode into `BigInteger` and enforce `0..2^64-1` bounds before converting to `ULong`; avoid `Long` DTO fields that truncate valid unsigned values.
- Do not assume transitive `implementation` dependencies from sibling Kotlin modules are importable; if a symbol is needed, add a direct dependency or use a crate-local constant/value.
