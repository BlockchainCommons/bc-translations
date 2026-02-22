package com.blockchaincommons.sskr

import com.blockchaincommons.bcshamir.ShamirException

/** Errors that can occur when using the SSKR library. */
sealed class SskrException(message: String, cause: Throwable? = null) : Exception(message, cause) {
    class DuplicateMemberIndex : SskrException(
        "When combining shares, the provided shares contained a duplicate member index",
    )

    class GroupSpecInvalid : SskrException("Invalid group specification")

    class GroupCountInvalid : SskrException("When creating a split spec, the group count is invalid")

    class GroupThresholdInvalid : SskrException("SSKR group threshold is invalid")

    class MemberCountInvalid : SskrException("SSKR member count is invalid")

    class MemberThresholdInvalid : SskrException("SSKR member threshold is invalid")

    class NotEnoughGroups : SskrException("SSKR shares did not contain enough groups")

    class SecretLengthNotEven : SskrException("SSKR secret is not of even length")

    class SecretTooLong : SskrException("SSKR secret is too long")

    class SecretTooShort : SskrException("SSKR secret is too short")

    class ShareLengthInvalid : SskrException("SSKR shares did not contain enough serialized bytes")

    class ShareReservedBitsInvalid : SskrException("SSKR shares contained invalid reserved bits")

    class SharesEmpty : SskrException("SSKR shares were empty")

    class ShareSetInvalid : SskrException("SSKR shares were invalid")

    class ShamirError(val error: ShamirException) :
        SskrException("SSKR Shamir error: ${error.message}", error)
}
