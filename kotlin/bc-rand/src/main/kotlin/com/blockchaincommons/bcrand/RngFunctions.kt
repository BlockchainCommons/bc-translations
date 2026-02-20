package com.blockchaincommons.bcrand

import java.math.BigInteger

private val BIG_MASK_64 = BigInteger("FFFFFFFFFFFFFFFF", 16)

/**
 * Performs wide multiplication of two unsigned values with the given bit width.
 * Returns (low, high) pair.
 */
private fun wideMul(a: ULong, b: ULong, bits: Int): Pair<ULong, ULong> {
    if (bits <= 32) {
        val product = a * b
        val mask = (1uL shl bits) - 1uL
        return (product and mask) to (product shr bits)
    }
    val aBig = BigInteger(a.toString())
    val bBig = BigInteger(b.toString())
    val product = aBig.multiply(bBig)
    val low = product.and(BIG_MASK_64).toLong().toULong()
    val high = product.shiftRight(64).toLong().toULong()
    return low to high
}

/**
 * Returns a random value that is less than the given upper bound.
 *
 * Uses Lemire's "nearly divisionless" method for generating random integers
 * in an interval.
 *
 * @param rng The random number generator to use.
 * @param upperBound The upper bound for the randomly generated value. Must be non-zero.
 * @param bits The unsigned integer width used for the algorithm (e.g. 32 or 64).
 * @return A random value in the range `[0, upperBound)`. Every value in that range
 *   is equally likely to be returned.
 */
fun rngNextWithUpperBound(
    rng: RandomNumberGenerator,
    upperBound: ULong,
    bits: Int = 64,
): ULong {
    require(upperBound > 0uL) { "upperBound must be positive, got $upperBound" }

    val mask = if (bits == 64) ULong.MAX_VALUE else (1uL shl bits) - 1uL

    var random = rng.nextU64() and mask
    var (mLow, mHigh) = wideMul(random, upperBound, bits)

    if (mLow < upperBound) {
        val t = ((0uL - upperBound) and mask) % upperBound
        while (mLow < t) {
            random = rng.nextU64() and mask
            val m = wideMul(random, upperBound, bits)
            mLow = m.first
            mHigh = m.second
        }
    }
    return mHigh
}

/**
 * Returns a random value within the specified range, using the given
 * generator as a source for randomness.
 *
 * @param rng The random number generator to use when creating the new random value.
 * @param start The inclusive lower bound of the range.
 * @param end The exclusive upper bound of the range.
 * @param bits The unsigned integer width used for the algorithm (e.g. 32 or 64).
 * @return A random value within the bounds of `[start, end)`.
 */
fun rngNextInRange(
    rng: RandomNumberGenerator,
    start: Long,
    end: Long,
    bits: Int = 64,
): Long {
    require(start < end) { "start must be less than end, got [$start, $end)" }

    val mask = if (bits == 64) ULong.MAX_VALUE else (1uL shl bits) - 1uL
    val delta = (end - start).toULong() and mask

    if (delta == mask) {
        return start + (rng.nextU64() and mask).toLong()
    }

    val random = rngNextWithUpperBound(rng, delta, bits)
    return start + random.toLong()
}

fun rngNextInClosedRange(
    rng: RandomNumberGenerator,
    start: Long,
    end: Long,
    bits: Int = 64,
): Long {
    require(start <= end) { "start must be <= end, got [$start, $end]" }

    val mask = if (bits == 64) ULong.MAX_VALUE else (1uL shl bits) - 1uL
    val delta = (end - start).toULong() and mask

    if (delta == mask) {
        return start + (rng.nextU64() and mask).toLong()
    }

    val random = rngNextWithUpperBound(rng, delta + 1uL, bits)
    return start + random.toLong()
}

/**
 * Returns a byte array of random bytes of the given size.
 *
 * @param rng The random number generator to use.
 * @param size The number of random bytes to generate.
 * @return A new [ByteArray] containing [size] random bytes.
 */
fun rngRandomData(rng: RandomNumberGenerator, size: Int): ByteArray {
    val data = ByteArray(size)
    rng.fillRandomData(data)
    return data
}

/**
 * Fills the given byte array with random bytes.
 *
 * @param rng The random number generator to use.
 * @param data The byte array to fill with random bytes.
 */
fun rngFillRandomData(rng: RandomNumberGenerator, data: ByteArray) {
    rng.fillRandomData(data)
}

fun rngRandomArray(rng: RandomNumberGenerator, size: Int): ByteArray =
    rngRandomData(rng, size)

fun rngRandomBool(rng: RandomNumberGenerator): Boolean =
    rng.nextU32() % 2u == 0u

fun rngRandomU32(rng: RandomNumberGenerator): UInt =
    rng.nextU32()
