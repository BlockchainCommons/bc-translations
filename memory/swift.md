# Swift Translation Memory

- When a translated API intentionally uses a type name that collides with Swift stdlib (`Set`), qualify references in tests and docs (`DCBOR.Set`) to avoid ambiguity.
- In Swift Testing assertions against optional enums, compare with explicit enum cases (`EdgeType.none`) instead of bare `.none` so `-warnings-as-errors` builds stay clean.
- After importing an upstream Swift package baseline, run `swift test` immediately before parity edits to separate baseline integration issues from new translation regressions.
- When Swift tests import both `BCTags` and `Testing`, qualify tag references as `BCTags.Tag` to avoid collisions with `Testing.Tag`, especially under `-warnings-as-errors`.
- For UR multipart decoding, normalize incoming part strings to lowercase and keep an explicit uppercase-QR regression test to protect scanner interoperability.
