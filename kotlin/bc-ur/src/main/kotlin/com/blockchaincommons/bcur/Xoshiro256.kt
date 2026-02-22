package com.blockchaincommons.bcur

import java.security.MessageDigest

/**
 * Xoshiro256** pseudo-random number generator.
 *
 * Seeded from a SHA-256 hash of input data. Used internally for
 * deterministic fragment selection in fountain codes.
 */
internal class Xoshiro256 private constructor(private val s: ULongArray) {
    init {
        require(s.size == 4)
    }

    /** Returns the next 64-bit random value. */
    fun next(): ULong {
        val result = (s[1] * 5u).rotateLeft(7) * 9u
        val t = s[1] shl 17

        s[2] = s[2] xor s[0]
        s[3] = s[3] xor s[1]
        s[1] = s[1] xor s[2]
        s[0] = s[0] xor s[3]

        s[2] = s[2] xor t
        s[3] = s[3].rotateLeft(45)

        return result
    }

    /** Returns a random double in [0, 1). */
    fun nextDouble(): Double =
        next().toDouble() / (ULong.MAX_VALUE.toDouble() + 1.0)

    /** Returns a random integer in [low, high]. */
    fun nextInt(low: ULong, high: ULong): ULong =
        (nextDouble() * (high - low + 1u).toDouble()).toULong() + low

    /** Returns a new list with elements of [items] in random order. */
    fun <T> shuffled(items: List<T>): List<T> {
        val remaining = items.toMutableList()
        val result = mutableListOf<T>()
        while (remaining.isNotEmpty()) {
            val index = nextInt(0u, (remaining.size - 1).toULong()).toInt()
            result.add(remaining.removeAt(index))
        }
        return result
    }

    /**
     * Chooses a random degree for fountain encoding using harmonic weights.
     *
     * @param length the number of fragments
     * @return a degree in [1, length]
     */
    fun chooseDegree(length: Int): Int {
        val weights = (1..length).map { 1.0 / it.toDouble() }
        val sampler = WeightedSampler(weights)
        return sampler.next(this) + 1
    }

    // ---- Test utilities (package-private) ----

    internal fun nextByte(): UByte = nextInt(0u, 255u).toUByte()

    internal fun nextBytes(n: Int): ByteArray =
        ByteArray(n) { nextByte().toByte() }

    companion object {
        /** Creates a Xoshiro256 seeded from the SHA-256 hash of a string. */
        fun fromString(seed: String): Xoshiro256 =
            fromBytes(seed.toByteArray(Charsets.UTF_8))

        /** Creates a Xoshiro256 seeded from the SHA-256 hash of bytes. */
        fun fromBytes(bytes: ByteArray): Xoshiro256 {
            val hash = MessageDigest.getInstance("SHA-256").digest(bytes)
            return fromHash(hash)
        }

        /** Creates a Xoshiro256 seeded from the CRC32 of bytes. */
        internal fun fromCrc(bytes: ByteArray): Xoshiro256 {
            val crcBytes = Crc32.checksum(bytes).toBytesBigEndian()
            return fromBytes(crcBytes)
        }

        /**
         * Creates a Xoshiro256 from a 32-byte hash.
         *
         * Reads each 8-byte chunk as a big-endian u64 to form the state,
         * matching the Rust implementation's byte-order transform.
         */
        private fun fromHash(hash: ByteArray): Xoshiro256 {
            require(hash.size == 32)
            val state = ULongArray(4)
            for (i in 0 until 4) {
                var v = 0uL
                for (n in 0 until 8) {
                    v = (v shl 8) or (hash[8 * i + n].toUByte().toULong())
                }
                state[i] = v
            }
            return Xoshiro256(state)
        }

        /** Creates a deterministic test message. */
        internal fun makeMessage(seed: String, size: Int): ByteArray {
            val xoshiro = fromString(seed)
            return xoshiro.nextBytes(size)
        }
    }
}

private fun ULong.rotateLeft(bits: Int): ULong =
    (this shl bits) or (this shr (64 - bits))
