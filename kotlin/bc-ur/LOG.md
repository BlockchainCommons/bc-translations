# Translation Log: bc-ur → Kotlin (bc-ur)

Model: Claude Opus 4.6

## 2026-02-21 — Stage 1: Plan
STARTED
- Using existing language-agnostic manifest from csharp/BCUR/MANIFEST.md
- Adapting for Kotlin naming conventions

## 2026-02-21 — Stage 1: Plan
COMPLETED
- Manifest adapted for Kotlin at kotlin/bc-ur/MANIFEST.md
- 16 translation units identified
- 30 tests inventoried
- Key hazard: external ur crate must be reimplemented from scratch

## 2026-02-21 — Stage 2: Code
STARTED
- Translating all 16 units in build order
- Using dcbor Kotlin dependency for CBOR operations

## 2026-02-21 — Stage 2: Code
COMPLETED
- 17 source files translated (16 main + BytewordsStyle enum)
- 8 test files with 30 tests, all passing
- External `ur` crate reimplemented from scratch (fountain codes, Xoshiro256**, weighted sampling, bytewords, CRC32)
- Gradle build with dcbor includeBuild dependency

## 2026-02-21 — Stage 3: Check
STARTED
- Comparing translation against manifest

## 2026-02-21 — Stage 3: Check
COMPLETED
- 16/16 translation units complete
- 30/30 tests implemented and passing
- All public API surface covered
- All internal types reimplemented
- No gaps found

## 2026-02-21 — Stage 4: Critique
STARTED
- Running fluency review for Kotlin idiomaticness

## 2026-02-21 — Stage 4: Critique
COMPLETED
- 2 MUST FIX: CborError cause chaining, === reference equality → startsWith
- 9 SHOULD FIX: URDecoder→DecoderError rename, URType/UR→data class, UInt.toBytesBigEndian() extraction, Array→List for constants, case validation consistency, FountainPart copy→deepCopy + properties, MultipartDecoder lowercase
- 13 NICE TO HAVE: Global ExperimentalStdlibApi opt-in, @throws KDoc, require messages, ArrayDeque for queue, partsCount→partCount, test helper rename
- All 24 findings addressed, all 30 tests pass
