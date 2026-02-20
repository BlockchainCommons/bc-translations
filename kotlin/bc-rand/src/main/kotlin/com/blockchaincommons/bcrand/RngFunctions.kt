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
 * Return a random value in `[0, upperBound)` using Lemire's method.
 *
 * [bits] is the unsigned integer width used for the algorithm (e.g. 32 for
 * u32 semantics, 64 for u64).
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
 * Return a random value in the half-open range `[start, end)`.
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

/**
 * Return a random value in the closed range `[start, end]`.
 */
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

/** Return [size] random bytes from [rng]. */
fun rngRandomData(rng: RandomNumberGenerator, size: Int): ByteArray {
    val data = ByteArray(size)
    rng.fillRandomData(data)
    return data
}

/** Fill [data] with random bytes from [rng]. */
fun rngFillRandomData(rng: RandomNumberGenerator, data: ByteArray) {
    rng.fillRandomData(data)
}

/** Alias for [rngRandomData]. */
fun rngRandomArray(rng: RandomNumberGenerator, size: Int): ByteArray =
    rngRandomData(rng, size)

/** Return a random boolean with equal probability. */
fun rngRandomBool(rng: RandomNumberGenerator): Boolean =
    rng.nextU32() % 2u == 0u

/** Return a random unsigned 32-bit integer from [rng]. */
fun rngRandomU32(rng: RandomNumberGenerator): UInt =
    rng.nextU32()
