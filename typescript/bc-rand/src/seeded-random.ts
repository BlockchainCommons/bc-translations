import { Xoshiro256StarStar } from './xoshiro256starstar.js';
import type { RandomNumberGenerator } from './random-number-generator.js';

/**
 * A random number generator that can be used as a source of deterministic
 * pseudo-randomness for testing purposes.
 */
export class SeededRandomNumberGenerator implements RandomNumberGenerator {
    #rng: Xoshiro256StarStar;

    /**
     * Creates a new seeded random number generator.
     *
     * The seed should be a 256-bit value, represented as an array of 4
     * bigint values. For the output distribution to look random, the seed
     * should not have any obvious patterns, like all zeroes or all ones.
     *
     * This is not cryptographically secure, and should only be used for
     * testing purposes.
     *
     * @param seed - A 256-bit seed as a tuple of 4 bigint values.
     */
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

/**
 * Creates a seeded random number generator with a fixed seed.
 *
 * @returns A new {@link SeededRandomNumberGenerator} initialized with a
 *   predetermined seed for deterministic testing.
 */
export function createFakeRandomNumberGenerator(): SeededRandomNumberGenerator {
    return new SeededRandomNumberGenerator(FAKE_SEED);
}

/**
 * Creates random bytes with a fixed seed.
 *
 * @param size - The number of random bytes to generate.
 * @returns A new Uint8Array filled with deterministic random bytes.
 */
export function fakeRandomData(size: number): Uint8Array {
    return createFakeRandomNumberGenerator().randomData(size);
}
