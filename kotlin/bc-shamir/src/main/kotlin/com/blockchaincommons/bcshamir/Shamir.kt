/**
 * Shamir's Secret Sharing (SSS) for Kotlin.
 *
 * This package translates the reference Rust implementation and preserves
 * deterministic vector behavior used by Blockchain Commons test suites.
 */
package com.blockchaincommons.bcshamir

import com.blockchaincommons.bccrypto.hmacSha256
import com.blockchaincommons.bccrypto.memzero
import com.blockchaincommons.bccrypto.memzeroAll
import com.blockchaincommons.bcrand.RandomNumberGenerator

/** The minimum length of a secret. */
const val MIN_SECRET_LEN = 16

/** The maximum length of a secret. */
const val MAX_SECRET_LEN = 32

/** The maximum number of shares that can be generated from a secret. */
const val MAX_SHARE_COUNT = 16

private const val SECRET_INDEX: UByte = 255u
private const val DIGEST_INDEX: UByte = 254u

private fun createDigest(randomData: ByteArray, sharedSecret: ByteArray): ByteArray =
    hmacSha256(randomData, sharedSecret)

private fun validateParameters(threshold: Int, shareCount: Int, secretLength: Int) {
    if (shareCount > MAX_SHARE_COUNT) {
        throw Error.TooManyShares()
    } else if (threshold < 1 || threshold > shareCount) {
        throw Error.InvalidThreshold()
    } else if (secretLength > MAX_SECRET_LEN) {
        throw Error.SecretTooLong()
    } else if (secretLength < MIN_SECRET_LEN) {
        throw Error.SecretTooShort()
    } else if ((secretLength and 1) != 0) {
        throw Error.SecretNotEvenLen()
    }
}

/**
 * Splits a secret into shares using the Shamir secret sharing algorithm.
 *
 * @param threshold The minimum number of shares required to reconstruct the
 * secret. Must be greater than or equal to 1 and less than or equal to
 * `shareCount`.
 * @param shareCount The total number of shares to generate. Must be at least
 * `threshold` and less than or equal to `MAX_SHARE_COUNT`.
 * @param secret A byte array containing the secret to be split. Must be at
 * least `MIN_SECRET_LEN` bytes long and at most `MAX_SECRET_LEN` bytes long.
 * The length must be an even number.
 * @param randomGenerator Random number generator used to generate random data.
 * @return A list of byte arrays representing secret shares.
 */
fun splitSecret(
    threshold: Int,
    shareCount: Int,
    secret: ByteArray,
    randomGenerator: RandomNumberGenerator,
): List<ByteArray> {
    validateParameters(threshold, shareCount, secret.size)

    if (threshold == 1) {
        val result = MutableList(shareCount) { ByteArray(secret.size) }
        for (share in result) {
            secret.copyInto(share)
        }
        return result
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
 * Recovers a secret from the given shares using the Shamir secret sharing
 * algorithm.
 *
 * @param indexes Indexes of shares to use for recovery.
 * @param shares Shares corresponding to [indexes].
 * @return The recovered secret bytes.
 */
fun recoverSecret(indexes: List<Int>, shares: List<ByteArray>): ByteArray {
    val threshold = shares.size
    if (threshold == 0 || indexes.size != threshold) {
        throw Error.InvalidThreshold()
    }

    val shareLength = shares[0].size
    validateParameters(threshold, threshold, shareLength)

    if (!shares.all { it.size == shareLength }) {
        throw Error.SharesUnequalLength()
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
        throw Error.ChecksumFailure()
    }

    return secret
}
