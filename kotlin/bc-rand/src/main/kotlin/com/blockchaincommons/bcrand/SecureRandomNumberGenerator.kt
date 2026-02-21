package com.blockchaincommons.bcrand

import java.security.SecureRandom

private val secureRandom = SecureRandom()

/**
 * Generates a byte array of cryptographically strong random bytes of the given size.
 *
 * @param size The number of random bytes to generate.
 * @return A new [ByteArray] containing [size] cryptographically strong random bytes.
 */
fun randomData(size: Int): ByteArray =
    ByteArray(size).also { secureRandom.nextBytes(it) }

/**
 * Fills the given byte array with cryptographically strong random bytes.
 *
 * @param data The byte array to fill with random bytes.
 */
fun fillRandomData(data: ByteArray) {
    secureRandom.nextBytes(data)
}

/**
 * A random number generator that can be used as a source of
 * cryptographically-strong randomness.
 *
 * Backed by [java.security.SecureRandom].
 */
class SecureRandomNumberGenerator : RandomNumberGenerator() {

    override fun nextU32(): UInt = secureRandom.nextInt().toUInt()

    override fun nextU64(): ULong = secureRandom.nextLong().toULong()

    override fun randomData(size: Int): ByteArray =
        ByteArray(size).also { secureRandom.nextBytes(it) }

    override fun fillRandomData(data: ByteArray) {
        secureRandom.nextBytes(data)
    }
}

fun secureRandomNumberGenerator(): SecureRandomNumberGenerator =
    SecureRandomNumberGenerator()
