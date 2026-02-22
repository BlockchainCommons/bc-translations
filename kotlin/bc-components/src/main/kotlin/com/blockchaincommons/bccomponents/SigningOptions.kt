package com.blockchaincommons.bccomponents

/**
 * Options for configuring signature creation.
 *
 * Different signature schemes may require specific options. Currently,
 * only Schnorr signatures support auxiliary random data for nonce
 * generation.
 */
sealed class SigningOptions {

    /**
     * Options for Schnorr signatures.
     *
     * @param auxRand auxiliary random data used for nonce generation
     *   in BIP-340 Schnorr signatures
     */
    class SchnorrAuxRand(val auxRand: ByteArray) : SigningOptions() {

        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is SchnorrAuxRand) return false
            return auxRand.contentEquals(other.auxRand)
        }

        override fun hashCode(): Int = auxRand.contentHashCode()

        override fun toString(): String =
            "SchnorrAuxRand(${auxRand.toHexString()})"
    }
}
