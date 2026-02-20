const MASK64 = 0xFFFFFFFFFFFFFFFFn;

function rotl(x: bigint, k: number): bigint {
    return ((x << BigInt(k)) | (x >> BigInt(64 - k))) & MASK64;
}

/**
 * Internal implementation of the Xoshiro256** PRNG algorithm.
 * Not part of the public API.
 */
export class Xoshiro256StarStar {
    #s: [bigint, bigint, bigint, bigint];

    constructor(s0: bigint, s1: bigint, s2: bigint, s3: bigint) {
        this.#s = [s0 & MASK64, s1 & MASK64, s2 & MASK64, s3 & MASK64];
    }

    nextU64(): bigint {
        const s = this.#s;
        const result = (rotl((s[1] * 5n) & MASK64, 7) * 9n) & MASK64;
        const t = (s[1] << 17n) & MASK64;

        s[2] ^= s[0];
        s[3] ^= s[1];
        s[1] ^= s[2];
        s[0] ^= s[3];
        s[2] ^= t;
        s[3] = rotl(s[3], 45);

        return result;
    }
}
