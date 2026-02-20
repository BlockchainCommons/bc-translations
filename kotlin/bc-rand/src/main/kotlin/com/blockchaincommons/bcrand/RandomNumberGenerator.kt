/**
 * # Blockchain Commons Random Number Utilities
 *
 * `bc-rand` exposes a uniform API for the random number primitives used in
 * higher-level [Blockchain Commons](https://blockchaincommons.com) projects,
 * including a cryptographically strong random number generator
 * [SecureRandomNumberGenerator] and a deterministic random number generator
 * [SeededRandomNumberGenerator].
 *
 * These primitive random number generators extend the [RandomNumberGenerator]
 * abstract class to produce random numbers compatible with the deterministic
 * random number generator used for cross-platform testing.
 *
 * The package also includes several convenience functions for generating secure
 * and deterministic random numbers.
 */
package com.blockchaincommons.bcrand

abstract class RandomNumberGenerator {

    abstract fun nextU32(): UInt

    abstract fun nextU64(): ULong

    /**
     * Returns a byte array of random bytes of the given size.
     *
     * @param size The number of random bytes to generate.
     * @return A new [ByteArray] containing [size] random bytes.
     */
    open fun randomData(size: Int): ByteArray {
        val data = ByteArray(size)
        fillRandomData(data)
        return data
    }

    open fun fillRandomData(data: ByteArray) {
        var i = 0
        while (i + 8 <= data.size) {
            val v = nextU64()
            for (j in 0 until 8) {
                data[i + j] = (v shr (j * 8)).toByte()
            }
            i += 8
        }
        if (i < data.size) {
            val v = nextU64()
            for (j in 0 until data.size - i) {
                data[i + j] = (v shr (j * 8)).toByte()
            }
        }
    }
}
