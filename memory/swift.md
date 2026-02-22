# Swift Translation Memory

- When a translated API intentionally uses a type name that collides with Swift stdlib (`Set`), qualify references in tests and docs (`DCBOR.Set`) to avoid ambiguity.
- In Swift Testing assertions against optional enums, compare with explicit enum cases (`EdgeType.none`) instead of bare `.none` so `-warnings-as-errors` builds stay clean.
- After importing an upstream Swift package baseline, run `swift test` immediately before parity edits to separate baseline integration issues from new translation regressions.
- When Swift tests import both `BCTags` and `Testing`, qualify tag references as `BCTags.Tag` to avoid collisions with `Testing.Tag`, especially under `-warnings-as-errors`.
- For UR multipart decoding, normalize incoming part strings to lowercase and keep an explicit uppercase-QR regression test to protect scanner interoperability.
- In this repo, `swift/DCBOR` currently resolves `BCTags` from `BCSwiftTags` (0.2.x); adding a direct local `../BCTags` dependency in a downstream package can cause duplicate-target conflicts and mismatched tag APIs. Prefer the transitive `Tag` type from `DCBOR` and define missing newer tags explicitly with numeric values in the consumer package.
- When changing a protocol method to a computed property (e.g., `func hex() -> String` to `var hex: String`), you must update both the protocol definition AND all conforming types simultaneously, or protocol conformance errors will cascade across the codebase.
- When removing a protocol requirement that provided a default implementation for another requirement (e.g., `fromDataRef` providing default `fromHex`), you must add the removed requirement's functionality as an `init` protocol requirement or provide a new default implementation, otherwise conforming types that relied on the default will fail to compile.
- For Swift fluency reviews: Rust's `from_data_ref` maps idiomatically to a throwing `init(_ data: Data)` in Swift, not a separate static factory method. Make this `init` a protocol requirement to support default `fromHex` implementations.
