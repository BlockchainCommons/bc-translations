import { hmacSha256, memzero, memzeroAll } from '@bc/crypto';
import type { RandomNumberGenerator } from '@bc/rand';

import {
    MAX_SECRET_LEN,
    MAX_SHARE_COUNT,
    MIN_SECRET_LEN,
} from './index.js';
import { ShamirError } from './error.js';
import { interpolate } from './interpolate.js';

const SECRET_INDEX = 255;
const DIGEST_INDEX = 254;

/** Compute the first 32 bytes of HMAC-SHA-256 keyed by `randomData` over `sharedSecret`. */
function createDigest(randomData: Uint8Array, sharedSecret: Uint8Array): Uint8Array {
    return hmacSha256(randomData, sharedSecret);
}

/** Validate threshold, share count, and secret length preconditions. */
function validateParameters(
    threshold: number,
    shareCount: number,
    secretLength: number,
): void {
    if (shareCount > MAX_SHARE_COUNT) {
        throw ShamirError.tooManyShares();
    } else if (threshold < 1 || threshold > shareCount) {
        throw ShamirError.invalidThreshold();
    } else if (secretLength > MAX_SECRET_LEN) {
        throw ShamirError.secretTooLong();
    } else if (secretLength < MIN_SECRET_LEN) {
        throw ShamirError.secretTooShort();
    } else if ((secretLength & 1) !== 0) {
        throw ShamirError.secretNotEvenLength();
    }
}

/**
 * Splits a secret into shares using the Shamir secret sharing algorithm.
 *
 * @param threshold - The minimum number of shares required to reconstruct the secret.
 * @param shareCount - The total number of shares to generate.
 * @param secret - A byte array containing the secret to be split.
 * @param randomGenerator - An implementation of RandomNumberGenerator.
 * @returns An array of Uint8Array shares.
 * @throws {ShamirError} If the parameters are invalid.
 */
export function splitSecret(
    threshold: number,
    shareCount: number,
    secret: Uint8Array,
    randomGenerator: RandomNumberGenerator,
): Uint8Array[] {
    validateParameters(threshold, shareCount, secret.length);

    if (threshold === 1) {
        const result: Uint8Array[] = [];
        for (let i = 0; i < shareCount; i++) {
            result.push(Uint8Array.from(secret));
        }
        return result;
    }

    const x = new Uint8Array(shareCount);
    const y: Uint8Array[] = [];
    for (let i = 0; i < shareCount; i++) {
        y.push(new Uint8Array(secret.length));
    }
    let n = 0;
    const result: Uint8Array[] = [];
    for (let i = 0; i < shareCount; i++) {
        result.push(new Uint8Array(secret.length));
    }

    for (let index = 0; index < threshold - 2; index++) {
        randomGenerator.fillRandomData(result[index]!);
        x[n] = index;
        y[n]!.set(result[index]!);
        n++;
    }

    // generate secretLength - 4 bytes worth of random data
    const digest = new Uint8Array(secret.length);
    const digestSlice = digest.subarray(4);
    randomGenerator.fillRandomData(digestSlice);
    // put 4 bytes of digest at the top of the digest array
    const d = createDigest(digestSlice, secret);
    digest.set(d.subarray(0, 4), 0);
    x[n] = DIGEST_INDEX;
    y[n]!.set(digest);
    n++;

    x[n] = SECRET_INDEX;
    y[n]!.set(secret);
    n++;

    for (let index = threshold - 2; index < shareCount; index++) {
        const v = interpolate(n, x, secret.length, y, index);
        result[index]!.set(v);
    }

    // clean up stack
    memzero(digest);
    memzero(x);
    memzeroAll(y);

    return result;
}

/**
 * Recovers the secret from the given shares using the Shamir secret sharing algorithm.
 *
 * @param indexes - The indexes of the shares to use for recovery.
 * @param shares - The shares corresponding to the given indexes.
 * @returns The recovered secret as a Uint8Array.
 * @throws {ShamirError} If recovery fails.
 */
export function recoverSecret(
    indexes: number[],
    shares: Uint8Array[],
): Uint8Array {
    const threshold = shares.length;
    if (threshold === 0 || indexes.length !== threshold) {
        throw ShamirError.invalidThreshold();
    }
    const shareLength = shares[0]!.length;
    validateParameters(threshold, threshold, shareLength);

    if (!shares.every(share => share.length === shareLength)) {
        throw ShamirError.sharesUnequalLength();
    }

    if (threshold === 1) {
        return Uint8Array.from(shares[0]!);
    }

    const indexBytes = new Uint8Array(indexes.map(x => x & 0xFF));
    const digest = interpolate(
        threshold,
        indexBytes,
        shareLength,
        shares,
        DIGEST_INDEX,
    );
    const secret = interpolate(
        threshold,
        indexBytes,
        shareLength,
        shares,
        SECRET_INDEX,
    );
    const verify = createDigest(digest.subarray(4), secret);

    let valid = true;
    for (let i = 0; i < 4; i++) {
        valid = valid && digest[i] === verify[i];
    }
    memzero(digest);
    memzero(verify);

    if (!valid) {
        throw ShamirError.checksumFailure();
    }

    return secret;
}
