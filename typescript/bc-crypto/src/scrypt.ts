import { scryptSync } from 'node:crypto';

import { type BytesLike, toBytes } from './bytes.js';

/** Computes scrypt with recommended parameters. */
export function scrypt(
    pass: BytesLike,
    salt: BytesLike,
    outputLen: number,
): Uint8Array {
    return scryptOpt(pass, salt, outputLen, 15, 8, 1);
}

/** Computes scrypt with explicit parameters. */
export function scryptOpt(
    pass: BytesLike,
    salt: BytesLike,
    outputLen: number,
    logN: number,
    r: number,
    p: number,
): Uint8Array {
    if (outputLen < 0 || logN < 1 || logN >= 31 || r <= 0 || p <= 0) {
        throw new RangeError('Invalid Scrypt parameters');
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
        throw new RangeError('Invalid Scrypt parameters');
    }
}
