package com.blockchaincommons.bccomponents

import com.blockchaincommons.dcbor.CborException

/**
 * Errors that can occur when using the bc-components library.
 *
 * This sealed class hierarchy models the various error conditions from the
 * Rust `bc_components::Error` enum. Only the error variants needed by the
 * default-features subset are included; additional variants can be added as
 * later translation units are completed.
 */
sealed class BcComponentsException(
    message: String,
    cause: Throwable? = null,
) : Exception(message, cause) {

    /** Invalid data size for the specified type. */
    class InvalidSize(
        val dataType: String,
        val expected: Int,
        val actual: Int,
    ) : BcComponentsException(
        "invalid $dataType size: expected $expected, got $actual",
    )

    /** Invalid data format or content. */
    class InvalidData(
        val dataType: String,
        val reason: String,
    ) : BcComponentsException("invalid $dataType: $reason")

    /** Data too short for the specified type. */
    class DataTooShort(
        val dataType: String,
        val minimum: Int,
        val actual: Int,
    ) : BcComponentsException(
        "data too short: $dataType expected at least $minimum, got $actual",
    )

    /** Cryptographic operation failed. */
    class Crypto(val msg: String) : BcComponentsException(
        "cryptographic operation failed: $msg",
    )

    /** CBOR encoding or decoding error. */
    class Cbor(val error: CborException) : BcComponentsException(
        "CBOR error: ${error.message}",
        error,
    )

    /** SSKR error. */
    class Sskr(val error: com.blockchaincommons.sskr.SskrException) :
        BcComponentsException("SSKR error: ${error.message}", error)

    /** Data compression/decompression failed. */
    class Compression(val msg: String) : BcComponentsException(
        "compression error: $msg",
    )

    /** Post-quantum cryptographic operation failed. */
    class PostQuantum(val msg: String) : BcComponentsException(
        "post-quantum error: $msg",
    )

    /** Security level mismatch between key and signature/ciphertext. */
    class LevelMismatch : BcComponentsException(
        "security level mismatch between key and signature/ciphertext",
    )

    /** General error with custom message. */
    class General(val msg: String) : BcComponentsException(msg)

    companion object {
        fun general(msg: String): BcComponentsException = General(msg)

        fun invalidSize(dataType: String, expected: Int, actual: Int): BcComponentsException =
            InvalidSize(dataType, expected, actual)

        fun invalidData(dataType: String, reason: String): BcComponentsException =
            InvalidData(dataType, reason)

        fun dataTooShort(dataType: String, minimum: Int, actual: Int): BcComponentsException =
            DataTooShort(dataType, minimum, actual)

        fun crypto(msg: String): BcComponentsException = Crypto(msg)

        fun compression(msg: String): BcComponentsException = Compression(msg)

        fun postQuantum(msg: String): BcComponentsException = PostQuantum(msg)

        fun levelMismatch(): BcComponentsException = LevelMismatch()
    }
}

/**
 * Converts a [BcComponentsException] into a [CborException] for interop
 * with dCBOR interface implementations.
 */
fun BcComponentsException.toCborException(): CborException = when (this) {
    is BcComponentsException.Cbor -> error
    else -> CborException.msg(message ?: "unknown error")
}
