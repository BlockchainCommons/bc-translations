# Swift Translation Memory

- When a translated API intentionally uses a type name that collides with Swift stdlib (`Set`), qualify references in tests and docs (`DCBOR.Set`) to avoid ambiguity.
- In Swift Testing assertions against optional enums, compare with explicit enum cases (`EdgeType.none`) instead of bare `.none` so `-warnings-as-errors` builds stay clean.
- After importing an upstream Swift package baseline, run `swift test` immediately before parity edits to separate baseline integration issues from new translation regressions.
