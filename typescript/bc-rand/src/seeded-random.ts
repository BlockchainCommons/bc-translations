import { Xoshiro256StarStar } from './xoshiro256starstar.js';
import type { RandomNumberGenerator } from './random-number-generator.js';

/**
 * A deterministic pseudo-random number generator for testing purposes.
 *
 * Seeded with a 256-bit value (4 x bigint). For the output to look random,
 * the seed should not have obvious patterns like all zeroes.
 *
 * This is NOT cryptographically secure and should only be used for testing.
 */
export class SeededRandomNumberGenerator implements RandomNumberGenerator {
    #rng: Xoshiro256StarStar;

    constructor(seed: [bigint, bigint, bigint, bigint]) {
        this.#rng = new Xoshiro256StarStar(seed[0], seed[1], seed[2], seed[3]);
    }

    nextU32(): number {
        return Number(this.nextU64() & 0xFFFFFFFFn);
    }

    nextU64(): bigint {
        return this.#rng.nextU64();
    }

    randomData(size: number): Uint8Array {
        // Byte-by-byte generation matching Swift implementation
        // for cross-platform test vector compatibility.
        const data = new Uint8Array(size);
        for (let i = 0; i < size; i++) {
            data[i] = Number(this.nextU64() & 0xFFn);
        }
        return data;
    }

    fillRandomData(data: Uint8Array): void {
        for (let i = 0; i < data.length; i++) {
            data[i] = Number(this.nextU64() & 0xFFn);
        }
    }
}

const FAKE_SEED: [bigint, bigint, bigint, bigint] = [
    17295166580085024720n,
    422929670265678780n,
    5577237070365765850n,
    7953171132032326923n,
];

/** Creates a seeded RNG with a fixed seed for deterministic testing. */
export function createFakeRandomNumberGenerator(): SeededRandomNumberGenerator {
    return new SeededRandomNumberGenerator(FAKE_SEED);
}

/** Returns deterministic random bytes using a fixed seed. */
export function fakeRandomData(size: number): Uint8Array {
    return createFakeRandomNumberGenerator().randomData(size);
}
