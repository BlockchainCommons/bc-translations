# Completeness: bc-shamir -> TypeScript (@bc/shamir)

## Source Files
- [x] src/error.ts -- ShamirError class
- [x] src/hazmat.ts -- GF(2^8) bitsliced arithmetic
- [x] src/interpolate.ts -- Lagrange interpolation
- [x] src/shamir.ts -- split/recover core logic
- [x] src/index.ts -- public API exports

## API Surface

### Constants
- [x] MIN_SECRET_LEN (16)
- [x] MAX_SECRET_LEN (32)
- [x] MAX_SHARE_COUNT (16)
- [x] SECRET_INDEX (255) -- internal
- [x] DIGEST_INDEX (254) -- internal

### Error Type
- [x] ShamirError class extends Error
- [x] ShamirError.secretTooLong()
- [x] ShamirError.tooManyShares()
- [x] ShamirError.interpolationFailure()
- [x] ShamirError.checksumFailure()
- [x] ShamirError.secretTooShort()
- [x] ShamirError.secretNotEvenLen()
- [x] ShamirError.invalidThreshold()
- [x] ShamirError.sharesUnequalLength()

### Public Functions
- [x] splitSecret(threshold, shareCount, secret, randomGenerator)
- [x] recoverSecret(indexes, shares)

### Internal (hazmat) Functions
- [x] bitslice
- [x] unbitslice
- [x] bitsliceSetall
- [x] gf256Add
- [x] gf256Mul
- [x] gf256Square
- [x] gf256Inv

### Internal (interpolate) Functions
- [x] hazmatLagrangeBasis (private)
- [x] interpolate (exported from module)

### Internal (shamir) Functions
- [x] createDigest (private)
- [x] validateParameters (private)

## Signature Compatibility
- [x] splitSecret: throws ShamirError (Rust returns Result) -- correct mapping
- [x] recoverSecret: throws ShamirError (Rust returns Result) -- correct mapping
- [x] interpolate: returns Uint8Array (Rust returns Result<Vec<u8>>) -- acceptable, never errors
- [x] recoverSecret: uses `&&` for checksum validation -- consistent with Go/Kotlin translations

## Tests
- [x] src/index.test.ts -- split/recover with test vectors
  - [x] test split secret 3/5 (threshold=3, count=5, 16-byte)
    - [x] FakeRandomNumberGenerator (sequential bytes, step 17)
    - [x] All 5 share hex values match Rust test vectors
    - [x] Recovery from shares [1,2,4] matches original secret
  - [x] test split secret 2/7 (threshold=2, count=7, 32-byte)
    - [x] All 7 share hex values match Rust test vectors
    - [x] Recovery from shares [3,4] matches original secret
- [x] src/shamir.test.ts -- example split/recover
  - [x] example split (SecureRNG, count check)
  - [x] example recover (hardcoded shares match Rust doc example)

## Derive/Protocol Coverage
- [x] Debug (Rust) -> ShamirError.name = 'ShamirError' (adequate for JS)
- [x] Error (Rust thiserror) -> extends Error with matching messages

## Documentation
- [x] index.ts -- package-level JSDoc with @packageDocumentation
- [x] MIN_SECRET_LEN -- JSDoc comment
- [x] MAX_SECRET_LEN -- JSDoc comment
- [x] MAX_SHARE_COUNT -- JSDoc comment
- [x] ShamirError -- JSDoc comment
- [x] splitSecret -- JSDoc with @param and @throws
- [x] recoverSecret -- JSDoc with @param and @throws
- [x] bitslice -- JSDoc comment
- [x] unbitslice -- JSDoc comment
- [x] bitsliceSetall -- JSDoc comment
- [x] gf256Add -- JSDoc comment
- [x] gf256Mul -- JSDoc comment
- [x] gf256Square -- JSDoc comment
- [x] gf256Inv -- JSDoc comment
- [x] hazmatLagrangeBasis -- JSDoc comment
- [x] interpolate -- JSDoc comment
- [x] createDigest -- JSDoc comment
- [x] validateParameters -- JSDoc comment
- [x] package.json -- has description field

## Build & Config
- [x] .gitignore
- [x] package.json (version 0.13.0 matches Rust)
- [x] tsconfig.json (ES2022, strict, ESM)
