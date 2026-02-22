package com.blockchaincommons.bccomponents

import com.blockchaincommons.dcbor.Cbor

/**
 * Supported hash types for key derivation functions.
 *
 * CDDL:
 * ```
 * HashType = SHA256 / SHA512
 * SHA256 = 0
 * SHA512 = 1
 * ```
 */
enum class HashType(val cborValue: Int) {
    SHA256(0),
    SHA512(1);

    /** Encodes this hash type as a CBOR integer. */
    fun toCbor(): Cbor = Cbor.fromInt(cborValue)

    override fun toString(): String = name

    companion object {
        /**
         * Decodes a [HashType] from a CBOR integer value.
         *
         * @throws BcComponentsException.General if the value does not
         *   correspond to a known hash type.
         */
        fun fromCborValue(value: Int): HashType =
            entries.firstOrNull { it.cborValue == value }
                ?: throw BcComponentsException.general("Invalid HashType: $value")

        /** Decodes a [HashType] from a CBOR item. */
        fun fromCbor(cbor: Cbor): HashType = fromCborValue(cbor.tryInt())
    }
}
