/**
 * # Shamir's Secret Sharing (SSS) for Kotlin
 *
 * This package implements Shamir's Secret Sharing scheme compatible with the
 * Blockchain Commons deterministic test suite. It provides functions to split
 * a secret into multiple shares and recover the secret from a threshold number
 * of those shares.
 *
 * ## Usage
 *
 * ```kotlin
 * val secret = "my secret belongs to me.".encodeToByteArray()
 * val shares = splitSecret(threshold = 2, shareCount = 3, secret, SecureRandomNumberGenerator())
 * val recovered = recoverSecret(listOf(0, 2), listOf(shares[0], shares[2]))
 * ```
 */
package com.blockchaincommons.bcshamir

import com.blockchaincommons.bccrypto.hmacSha256
import com.blockchaincommons.bccrypto.memzero
import com.blockchaincommons.bccrypto.memzeroAll
import com.blockchaincommons.bcrand.RandomNumberGenerator

/** The minimum length of a secret in bytes. */
const val MIN_SECRET_LEN = 16

/** The maximum length of a secret in bytes. */
const val MAX_SECRET_LEN = 32

/** The maximum number of shares that can be generated from a secret. */
const val MAX_SHARE_COUNT = 16

private const val SECRET_INDEX: UByte = 255u
private const val DIGEST_INDEX: UByte = 254u

private fun createDigest(randomData: ByteArray, sharedSecret: ByteArray): ByteArray =
    hmacSha256(randomData, sharedSecret)

private fun validateParameters(threshold: Int, shareCount: Int, secretLength: Int) {
    if (shareCount > MAX_SHARE_COUNT) {
        throw ShamirException.TooManyShares()
    }
    if (threshold < 1 || threshold > shareCount) {
        throw ShamirException.InvalidThreshold()
    }
    if (secretLength > MAX_SECRET_LEN) {
        throw ShamirException.SecretTooLong()
    }
    if (secretLength < MIN_SECRET_LEN) {
        throw ShamirException.SecretTooShort()
    }
    if (secretLength % 2 != 0) {
        throw ShamirException.SecretNotEvenLength()
    }
}

/**
 * Splits a secret into shares using Shamir's Secret Sharing algorithm.
 *
 * @param threshold The minimum number of shares required to reconstruct the
 *   secret. Must be at least 1 and at most [shareCount].
 * @param shareCount The total number of shares to generate. Must be at least
 *   [threshold] and at most [MAX_SHARE_COUNT].
 * @param secret The secret to split. Must be [MIN_SECRET_LEN]..[MAX_SECRET_LEN]
 *   bytes long with an even length.
 * @param randomGenerator Random number generator used for share generation.
 * @return A list of [shareCount] byte arrays, each the same length as [secret].
 * @throws ShamirException if any parameter constraint is violated.
 */
fun splitSecret(
    threshold: Int,
    shareCount: Int,
    secret: ByteArray,
    randomGenerator: RandomNumberGenerator,
): List<ByteArray> {
    validateParameters(threshold, shareCount, secret.size)

    if (threshold == 1) {
        return List(shareCount) { secret.copyOf() }
    }

    val x = ByteArray(shareCount)
    val y = MutableList(shareCount) { ByteArray(secret.size) }
    var n = 0
    val result = MutableList(shareCount) { ByteArray(secret.size) }

    for (index in 0 until threshold - 2) {
        randomGenerator.fillRandomData(result[index])
        x[n] = index.toByte()
        result[index].copyInto(y[n])
        n += 1
    }

    val digest = ByteArray(secret.size)
    val digestRandom = ByteArray(secret.size - 4)
    randomGenerator.fillRandomData(digestRandom)
    digestRandom.copyInto(digest, destinationOffset = 4)
    val d = createDigest(digest.copyOfRange(4, digest.size), secret)
    d.copyInto(digest, endIndex = 4)
    x[n] = DIGEST_INDEX.toByte()
    digest.copyInto(y[n])
    n += 1

    x[n] = SECRET_INDEX.toByte()
    secret.copyInto(y[n])
    n += 1

    for (index in threshold - 2 until shareCount) {
        val v = interpolate(n, x, secret.size, y, index.toUByte())
        v.copyInto(result[index])
    }

    memzero(digest)
    memzero(digestRandom)
    memzero(x)
    memzeroAll(y)

    return result
}

/**
 * Recovers a secret from the given shares using Shamir's Secret Sharing
 * algorithm.
 *
 * @param indexes The share indexes (0-based) used during splitting.
 * @param shares The share byte arrays corresponding to each index.
 * @return The recovered secret.
 * @throws ShamirException if the shares are invalid, have mismatched lengths,
 *   or the digest checksum verification fails.
 */
fun recoverSecret(indexes: List<Int>, shares: List<ByteArray>): ByteArray {
    val threshold = shares.size
    if (threshold == 0 || indexes.size != threshold) {
        throw ShamirException.InvalidThreshold()
    }

    val shareLength = shares[0].size
    validateParameters(threshold, threshold, shareLength)

    if (!shares.all { it.size == shareLength }) {
        throw ShamirException.SharesUnequalLength()
    }

    if (threshold == 1) {
        return shares[0].copyOf()
    }

    val shareIndexes = ByteArray(indexes.size) { idx -> indexes[idx].toByte() }
    val digest = interpolate(threshold, shareIndexes, shareLength, shares, DIGEST_INDEX)
    val secret = interpolate(threshold, shareIndexes, shareLength, shares, SECRET_INDEX)
    val verify = createDigest(digest.copyOfRange(4, digest.size), secret)

    var valid = true
    for (i in 0 until 4) {
        valid = valid && digest[i] == verify[i]
    }

    memzero(digest)
    memzero(verify)

    if (!valid) {
        throw ShamirException.ChecksumFailure()
    }

    return secret
}
