package com.blockchaincommons.bcrand

import java.security.SecureRandom

private val secureRandom = SecureRandom()

/** Return [size] cryptographically strong random bytes. */
fun randomData(size: Int): ByteArray =
    ByteArray(size).also { secureRandom.nextBytes(it) }

/** Fill [data] with cryptographically strong random bytes. */
fun fillRandomData(data: ByteArray) {
    secureRandom.nextBytes(data)
}

/**
 * A cryptographically secure random number generator.
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

/** Return a thread-safe cryptographically secure RNG. */
fun threadRng(): SecureRandomNumberGenerator = SecureRandomNumberGenerator()
