const MASK64 = 0xFFFFFFFFFFFFFFFFn;

/** Supported bit widths for bounded random number generation. */
export type BitWidth = 8 | 16 | 32 | 64;

const BITMASKS: Record<BitWidth, bigint> = {
    8: 0xFFn,
    16: 0xFFFFn,
    32: 0xFFFFFFFFn,
    64: MASK64,
};

/**
 * A source of random numbers.
 *
 * Implementors provide `nextU32` and `nextU64` for raw random values,
 * and `randomData`/`fillRandomData` for generating random byte sequences.
 */
export interface RandomNumberGenerator {
    /** Returns a random 32-bit unsigned integer. */
    nextU32(): number;
    /** Returns a random 64-bit unsigned integer. */
    nextU64(): bigint;
    /** Returns random bytes of the given size as a Uint8Array. */
    randomData(size: number): Uint8Array;
    /** Fills the given Uint8Array with random bytes. */
    fillRandomData(data: Uint8Array): void;
}

/**
 * Returns random bytes of the given size as a Uint8Array.
 *
 * @param rng - The random number generator to use.
 * @param size - The number of random bytes to generate.
 * @returns A new Uint8Array filled with random bytes.
 */
export function rngRandomData(
    rng: RandomNumberGenerator,
    size: number,
): Uint8Array {
    const data = new Uint8Array(size);
    rng.fillRandomData(data);
    return data;
}

/**
 * Fills the given Uint8Array with random bytes.
 *
 * @param rng - The random number generator to use.
 * @param data - The Uint8Array to fill with random bytes.
 */
export function rngFillRandomData(
    rng: RandomNumberGenerator,
    data: Uint8Array,
): void {
    rng.fillRandomData(data);
}

function wideMul(a: bigint, b: bigint, bits: BitWidth): [bigint, bigint] {
    const mask = BITMASKS[bits];
    const product = a * b;
    return [product & mask, (product >> BigInt(bits)) & mask];
}

/**
 * Returns a random value that is less than the given upper bound.
 *
 * Every value in the range `[0, upperBound)` is equally likely to be returned.
 * Uses Lemire's "nearly divisionless" method for unbiased bounded generation.
 *
 * @param rng - The random number generator to use.
 * @param upperBound - The exclusive upper bound. Must be non-zero.
 * @param bits - The bit width for masking (8, 16, 32, or 64). Defaults to 64.
 * @returns A random bigint in `[0, upperBound)`.
 * @throws {RangeError} If upperBound is zero.
 */
export function rngNextWithUpperBound(
    rng: RandomNumberGenerator,
    upperBound: bigint,
    bits: BitWidth = 64,
): bigint {
    if (upperBound === 0n) {
        throw new RangeError("Upper bound must be non-zero");
    }

    const bitmask = BITMASKS[bits];
    let random = rng.nextU64() & bitmask;
    let [lo, hi] = wideMul(random, upperBound, bits);

    if (lo < upperBound) {
        const t = ((0n - upperBound) & bitmask) % upperBound;
        while (lo < t) {
            random = rng.nextU64() & bitmask;
            [lo, hi] = wideMul(random, upperBound, bits);
        }
    }

    return hi;
}

/**
 * Returns a random value within the specified range, using the given
 * generator as a source for randomness.
 *
 * @param rng - The random number generator to use.
 * @param start - The inclusive lower bound.
 * @param end - The exclusive upper bound.
 * @param bits - The bit width for masking (8, 16, 32, or 64). Defaults to 64.
 * @returns A random bigint in `[start, end)`.
 * @throws {RangeError} If start >= end.
 */
export function rngNextInRange(
    rng: RandomNumberGenerator,
    start: bigint,
    end: bigint,
    bits: BitWidth = 64,
): bigint {
    if (start >= end) {
        throw new RangeError("Start must be less than end");
    }

    const bitmask = BITMASKS[bits];
    const delta = (end - start) & bitmask;

    if (delta === bitmask) {
        return rng.nextU64() & bitmask;
    }

    const random = rngNextWithUpperBound(rng, delta, bits);
    return start + random;
}

/**
 * Returns a uniformly distributed random value in the closed range `[start, end]`.
 *
 * @param rng - The random number generator to use.
 * @param start - The inclusive lower bound.
 * @param end - The inclusive upper bound.
 * @param bits - The bit width for masking (8, 16, 32, or 64). Defaults to 64.
 * @returns A random bigint in `[start, end]`.
 * @throws {RangeError} If start > end.
 */
export function rngNextInClosedRange(
    rng: RandomNumberGenerator,
    start: bigint,
    end: bigint,
    bits: BitWidth = 64,
): bigint {
    if (start > end) {
        throw new RangeError("Start must be less than or equal to end");
    }

    const bitmask = BITMASKS[bits];
    const delta = (end - start) & bitmask;

    if (delta === bitmask) {
        return rng.nextU64() & bitmask;
    }

    const random = rngNextWithUpperBound(rng, delta + 1n, bits);
    return start + random;
}

/** Returns a new Uint8Array of the given size filled with random bytes. */
export function rngRandomArray(
    rng: RandomNumberGenerator,
    size: number,
): Uint8Array {
    const data = new Uint8Array(size);
    rng.fillRandomData(data);
    return data;
}

/** Returns a random boolean. */
export function rngRandomBool(rng: RandomNumberGenerator): boolean {
    return rng.nextU32() % 2 === 0;
}

/** Returns a random 32-bit unsigned integer. */
export function rngRandomU32(rng: RandomNumberGenerator): number {
    return rng.nextU32();
}
