package com.blockchaincommons.bcrand

private const val UINT_BITS = 32
private const val ULONG_BITS = 64
private const val UINT_MASK = 0xFFFF_FFFFuL

private fun requireSupportedBits(bits: Int) {
    require(bits == UINT_BITS || bits == ULONG_BITS) {
        "bits must be 32 or 64, got $bits"
    }
}

private fun maskFor(bits: Int): ULong =
    if (bits == ULONG_BITS) ULong.MAX_VALUE else (1uL shl bits) - 1uL

/**
 * Performs wide multiplication of two unsigned values with the given bit width.
 * Returns (low, high) pair.
 */
private fun wideMul(a: ULong, b: ULong, bits: Int): Pair<ULong, ULong> {
    return when (bits) {
        UINT_BITS -> {
            val product = (a and UINT_MASK) * (b and UINT_MASK)
            (product and UINT_MASK) to (product shr UINT_BITS)
        }
        ULONG_BITS -> {
            val low = a * b
            val high = Math.unsignedMultiplyHigh(a.toLong(), b.toLong()).toULong()
            low to high
        }
        else -> error("bits must be 32 or 64, got $bits")
    }
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
    requireSupportedBits(bits)
    require(upperBound > 0uL) { "upperBound must be positive, got $upperBound" }

    val mask = maskFor(bits)

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
    requireSupportedBits(bits)
    require(start < end) { "start must be less than end, got [$start, $end)" }

    val mask = maskFor(bits)
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
    requireSupportedBits(bits)
    require(start <= end) { "start must be <= end, got [$start, $end]" }

    val mask = maskFor(bits)
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

fun RandomNumberGenerator.nextWithUpperBound(upperBound: ULong, bits: Int = 64): ULong =
    rngNextWithUpperBound(this, upperBound, bits)

fun RandomNumberGenerator.nextInRange(start: Long, end: Long, bits: Int = 64): Long =
    rngNextInRange(this, start, end, bits)

fun RandomNumberGenerator.nextInClosedRange(start: Long, end: Long, bits: Int = 64): Long =
    rngNextInClosedRange(this, start, end, bits)

fun RandomNumberGenerator.randomArray(size: Int): ByteArray =
    rngRandomArray(this, size)

fun RandomNumberGenerator.randomBool(): Boolean =
    rngRandomBool(this)

fun RandomNumberGenerator.randomU32(): UInt =
    rngRandomU32(this)
