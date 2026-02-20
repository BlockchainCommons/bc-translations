package com.blockchaincommons.bcrand

/**
 * Xoshiro256** PRNG implementation.
 *
 * Internal — not part of the public API.
 * Reference: https://prng.di.unimi.it/xoshiro256starstar.c
 */
internal class Xoshiro256StarStar(s0: ULong, s1: ULong, s2: ULong, s3: ULong) {

    private val s = ulongArrayOf(s0, s1, s2, s3)

    fun nextU64(): ULong {
        val result = rotl64(s[1] * 5u, 7) * 9u
        val t = s[1] shl 17

        s[2] = s[2] xor s[0]
        s[3] = s[3] xor s[1]
        s[1] = s[1] xor s[2]
        s[0] = s[0] xor s[3]
        s[2] = s[2] xor t
        s[3] = rotl64(s[3], 45)

        return result
    }

    private fun rotl64(x: ULong, k: Int): ULong =
        (x shl k) or (x shr (64 - k))
}
