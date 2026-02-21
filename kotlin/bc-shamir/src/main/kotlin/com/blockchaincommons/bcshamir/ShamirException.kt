package com.blockchaincommons.bcshamir

/**
 * Exception hierarchy for Shamir secret sharing errors.
 *
 * All exceptions thrown by [splitSecret] and [recoverSecret] are subtypes of
 * this sealed class.
 */
sealed class ShamirException(message: String) : Exception(message) {
    class SecretTooLong : ShamirException("Secret length exceeds maximum of $MAX_SECRET_LEN bytes")

    class TooManyShares : ShamirException("Share count exceeds maximum of $MAX_SHARE_COUNT")

    class InterpolationFailure : ShamirException("Interpolation failed")

    class ChecksumFailure : ShamirException("Checksum verification failed during secret recovery")

    class SecretTooShort : ShamirException("Secret length is below minimum of $MIN_SECRET_LEN bytes")

    class SecretNotEvenLength : ShamirException("Secret length must be even")

    class InvalidThreshold : ShamirException("Threshold must be between 1 and shareCount (inclusive)")

    class SharesUnequalLength : ShamirException("All shares must have the same length")
}
