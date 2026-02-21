import { scryptSync } from 'node:crypto';

import { type BytesLike, toBytes } from './bytes.js';

/** Derives a key using scrypt with recommended parameters (N=2^15, r=8, p=1). */
export function scrypt(
    pass: BytesLike,
    salt: BytesLike,
    outputLen: number,
): Uint8Array {
    return scryptWithParams(pass, salt, outputLen, 15, 8, 1);
}

/**
 * Derives a key using scrypt with caller-specified parameters.
 *
 * @param logN - Base-2 logarithm of the CPU/memory cost parameter (1..30).
 * @param r    - Block size parameter.
 * @param p    - Parallelization parameter.
 * @throws {RangeError} If any parameter is out of range.
 */
export function scryptWithParams(
    pass: BytesLike,
    salt: BytesLike,
    outputLen: number,
    logN: number,
    r: number,
    p: number,
): Uint8Array {
    if (outputLen < 0 || logN < 1 || logN >= 31 || r <= 0 || p <= 0) {
        throw new RangeError('Invalid scrypt parameters');
    }

    const n = 2 ** logN;
    const maxmem = Math.max(64 * 1024 * 1024, 128 * n * r * p + 1024);

    try {
        return new Uint8Array(
            scryptSync(toBytes(pass), toBytes(salt), outputLen, {
                N: n,
                r,
                p,
                maxmem,
            }),
        );
    } catch {
        throw new RangeError('Invalid scrypt parameters');
    }
}
