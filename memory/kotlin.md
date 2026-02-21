# Kotlin Translation Memory

- For Kotlin exception hierarchies that mirror Rust error enums, avoid `object` singleton throwables; use classes so each throw gets a fresh exception instance and stack trace.
- Before final shared-status edits in this monorepo, re-read the target row in `AGENTS.md` to avoid overwriting concurrent agent updates.
- For Rust crates with macro-generated public constants, extract symbols and registration order from source with a script, then verify translated constant counts before sign-off.
