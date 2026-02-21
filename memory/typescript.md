# TypeScript Translation Lessons

- When using `@noble/curves` v2 point parsing, convert `Uint8Array` keys to hex strings for `Point.fromHex(...)`; passing raw bytes causes runtime/type failures.
- For Node `crypto.scryptSync` with Rust-like recommended params (`N=32768`, `r=8`, `p=1`), set `maxmem` explicitly (at least 64 MB) to avoid memory-limit errors.
- For `chacha20-poly1305` in Node types, call `setAAD(aad, { plaintextLength })` to satisfy both typing and algorithm expectations.
- If a runtime crypto API is present but missing in `@types/node` (for example `argon2Sync`), use a narrow typed wrapper around `node:crypto` instead of suppressing type checks.
- When using `collections/sorted-map` in standalone TypeScript packages, keep a local declaration file (for example `src/collections.d.ts`); `@types/collections` alone may not satisfy `tsc` for that submodule path.
- Vitest v3 CLI differs from newer docs: avoid `--include`; run the default suite or pass file globs positionally/configure includes in `vitest.config.ts`.
