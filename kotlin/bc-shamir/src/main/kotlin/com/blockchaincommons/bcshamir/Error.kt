package com.blockchaincommons.bcshamir

sealed class Error(message: String) : Exception(message) {
    class SecretTooLong : Error("secret is too long")

    class TooManyShares : Error("too many shares")

    class InterpolationFailure : Error("interpolation failed")

    class ChecksumFailure : Error("checksum failure")

    class SecretTooShort : Error("secret is too short")

    class SecretNotEvenLen : Error("secret is not of even length")

    class InvalidThreshold : Error("invalid threshold")

    class SharesUnequalLength : Error("shares have unequal length")
}
