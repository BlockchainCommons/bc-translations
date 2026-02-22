package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_MLKEM_CIPHERTEXT
import com.blockchaincommons.bctags.TAG_X25519_PUBLIC_KEY
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A ciphertext produced by a key encapsulation mechanism (KEM).
 *
 * [EncapsulationCiphertext] represents the output of a key encapsulation
 * operation where a shared secret has been encapsulated for secure
 * transmission. The ciphertext can only be used to recover the shared secret
 * by the holder of the corresponding private key.
 *
 * This sealed class has two variants:
 * - [X25519]: For X25519 key agreement, this is the ephemeral public key
 *   generated during encapsulation.
 * - [MLKEM]: For ML-KEM post-quantum key encapsulation, this is the ML-KEM
 *   ciphertext.
 */
sealed class EncapsulationCiphertext {

    /** X25519 key agreement ciphertext (ephemeral public key). */
    data class X25519(val key: X25519PublicKey) : EncapsulationCiphertext()

    /** ML-KEM post-quantum ciphertext. */
    data class MLKEM(val ciphertext: MLKEMCiphertext) : EncapsulationCiphertext()

    /**
     * Returns the encapsulation scheme associated with this ciphertext.
     */
    fun encapsulationScheme(): EncapsulationScheme = when (this) {
        is X25519 -> EncapsulationScheme.X25519
        is MLKEM -> when (ciphertext.level) {
            com.blockchaincommons.bccomponents.MLKEM.MLKEM512 -> EncapsulationScheme.MLKEM512
            com.blockchaincommons.bccomponents.MLKEM.MLKEM768 -> EncapsulationScheme.MLKEM768
            com.blockchaincommons.bccomponents.MLKEM.MLKEM1024 -> EncapsulationScheme.MLKEM1024
        }
    }

    /** Returns `true` if this is an X25519 ciphertext. */
    fun isX25519(): Boolean = this is X25519

    /** Returns `true` if this is an ML-KEM ciphertext. */
    fun isMlkem(): Boolean = this is MLKEM

    /**
     * Returns the X25519 public key if this is an X25519 ciphertext.
     *
     * @throws BcComponentsException.Crypto if this is not an X25519 ciphertext
     */
    fun x25519PublicKey(): X25519PublicKey = when (this) {
        is X25519 -> key
        is MLKEM -> throw BcComponentsException.crypto("Invalid key encapsulation type")
    }

    /**
     * Returns the ML-KEM ciphertext if this is an ML-KEM ciphertext.
     *
     * @throws BcComponentsException.Crypto if this is not an ML-KEM ciphertext
     */
    fun mlkemCiphertext(): MLKEMCiphertext = when (this) {
        is MLKEM -> ciphertext
        is X25519 -> throw BcComponentsException.crypto("Invalid key encapsulation type")
    }

    // -- CBOR --

    /**
     * Returns the CBOR tags for this ciphertext. The tag depends on the variant:
     * - [X25519] uses [TAG_X25519_PUBLIC_KEY]
     * - [MLKEM] uses [TAG_MLKEM_CIPHERTEXT]
     */
    fun cborTags(): List<Tag> = when (this) {
        is X25519 -> tagsForValues(listOf(TAG_X25519_PUBLIC_KEY))
        is MLKEM -> tagsForValues(listOf(TAG_MLKEM_CIPHERTEXT))
    }

    /** Returns the untagged CBOR representation, dispatching to the inner type. */
    fun untaggedCbor(): Cbor = when (this) {
        is X25519 -> key.untaggedCbor()
        is MLKEM -> ciphertext.untaggedCbor()
    }

    /** Returns the tagged CBOR representation, dispatching to the inner type. */
    fun taggedCbor(): Cbor = when (this) {
        is X25519 -> key.taggedCbor()
        is MLKEM -> ciphertext.taggedCbor()
    }

    /** Returns the tagged CBOR encoding as binary data. */
    fun taggedCborData(): ByteArray = taggedCbor().toCborData()

    companion object {
        /**
         * Decodes an [EncapsulationCiphertext] from tagged CBOR.
         *
         * Dispatches based on the CBOR tag value:
         * - [TAG_X25519_PUBLIC_KEY] produces an [X25519] variant
         * - [TAG_MLKEM_CIPHERTEXT] produces an [MLKEM] variant
         *
         * @throws com.blockchaincommons.dcbor.CborException if the CBOR is not
         *   a valid encapsulation ciphertext
         */
        fun fromTaggedCbor(cbor: Cbor): EncapsulationCiphertext {
            val (tagValue, _) = cbor.tryTaggedValue()
            return when (tagValue) {
                TAG_X25519_PUBLIC_KEY -> X25519(X25519PublicKey.fromTaggedCbor(cbor))
                TAG_MLKEM_CIPHERTEXT -> MLKEM(MLKEMCiphertext.fromTaggedCbor(cbor))
                else -> throw com.blockchaincommons.dcbor.CborException.msg(
                    "Invalid encapsulation ciphertext",
                )
            }
        }

        /**
         * Decodes an [EncapsulationCiphertext] from tagged CBOR binary data.
         */
        fun fromTaggedCborData(data: ByteArray): EncapsulationCiphertext =
            fromTaggedCbor(Cbor.tryFromData(data))
    }
}
