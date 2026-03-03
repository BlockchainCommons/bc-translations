# Completeness: sskr → TypeScript (@bc/sskr)

## Source Files
- [x] constants.ts — public constants (MIN_SECRET_LEN, MAX_SECRET_LEN, etc.)
- [x] error.ts — SskrError class with static factory methods
- [x] secret.ts — Secret class with validation
- [x] group-spec.ts — GroupSpec class with create/parse/default/toString
- [x] spec.ts — Spec class with create and accessors
- [x] share.ts — SSKRShare internal class
- [x] encoding.ts — sskrGenerate, sskrGenerateUsing, sskrCombine
- [x] index.ts — public exports

## Public API
- [x] SskrError (14 static factory methods + shamirError wrapper)
- [x] Secret.create, length, isEmpty, data, equals
- [x] GroupSpec.create, default, parse, memberThreshold, memberCount, toString
- [x] Spec.create, groupThreshold, groups, groupCount, shareCount
- [x] sskrGenerate
- [x] sskrGenerateUsing
- [x] sskrCombine
- [x] MIN_SECRET_LEN, MAX_SECRET_LEN, MAX_SHARE_COUNT, MAX_GROUPS_COUNT
- [x] METADATA_SIZE_BYTES, MIN_SERIALIZE_SIZE_BYTES

## Tests
- [x] split 3/5 — deterministic 3-of-5 single-group split/recover
- [x] split 2/7 — deterministic 2-of-7 single-group split/recover
- [x] split 2-3/2-3 — deterministic two-group split/recover
- [x] shuffle — Fisher-Yates with bc-rand fake RNG, exact output vector
- [x] fuzz test — 100 randomized round-trips with bc-rand fake RNG
- [x] example encode — two-group example with secure RNG
- [x] example encode 3 — regression for 1-of-3 group
- [x] example encode 4 — regression for extra unrecoverable group

## Build & Config
- [x] package.json
- [x] tsconfig.json
- [x] vitest.config.ts
- [x] .gitignore
