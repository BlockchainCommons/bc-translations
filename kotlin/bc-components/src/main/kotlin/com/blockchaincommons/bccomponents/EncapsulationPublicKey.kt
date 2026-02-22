package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_MLKEM_PUBLIC_KEY
import com.blockchaincommons.bctags.TAG_X25519_PUBLIC_KEY
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A public key used for key encapsulation mechanisms (KEM).
 *
 * [EncapsulationPublicKey] is a sealed class representing different types of
 * public keys that can be used for key encapsulation, including:
 *
 * - [X25519]: Curve25519-based key exchange
 * - [MLKEM]: Module Lattice-based Key Encapsulation Mechanism at various
 *   security levels
 *
 * These public keys are used to encrypt (encapsulate) shared secrets that can
 * only be decrypted (decapsulated) by the corresponding private key holder.
 */
sealed class EncapsulationPublicKey : Encrypter, ReferenceProvider {

    /** An X25519 public key. */
    data class X25519(val key: X25519PublicKey) : EncapsulationPublicKey()

    /** An ML-KEM public key (post-quantum). */
    data class MLKEM(val key: MLKEMPublicKey) : EncapsulationPublicKey()

    /**
     * Returns the encapsulation scheme associated with this public key.
     */
    fun encapsulationScheme(): EncapsulationScheme = when (this) {
        is X25519 -> EncapsulationScheme.X25519
        is MLKEM -> when (key.level) {
            com.blockchaincommons.bccomponents.MLKEM.MLKEM512 -> EncapsulationScheme.MLKEM512
            com.blockchaincommons.bccomponents.MLKEM.MLKEM768 -> EncapsulationScheme.MLKEM768
            com.blockchaincommons.bccomponents.MLKEM.MLKEM1024 -> EncapsulationScheme.MLKEM1024
        }
    }

    /**
     * Encapsulates a new shared secret using this public key.
     *
     * The encapsulation process differs based on the key type:
     * - For [X25519]: Generates an ephemeral private/public key pair, derives a
     *   shared secret using Diffie-Hellman, and returns the shared secret along
     *   with the ephemeral public key as the ciphertext.
     * - For [MLKEM]: Uses the KEM encapsulation algorithm to generate and
     *   encapsulate a random shared secret.
     *
     * @return a pair containing the generated shared secret as a [SymmetricKey]
     *   and the [EncapsulationCiphertext] that can be sent to the private key
     *   holder
     */
    override fun encapsulateNewSharedSecret(): Pair<SymmetricKey, EncapsulationCiphertext> {
        return when (this) {
            is X25519 -> {
                val ephemeral = X25519PrivateKey.create()
                val ephemeralPublic = ephemeral.publicKey()
                val sharedKey = ephemeral.sharedKeyWith(key)
                sharedKey to EncapsulationCiphertext.X25519(ephemeralPublic)
            }
            is MLKEM -> {
                val (sharedKey, ct) = key.encapsulateNewSharedSecret()
                sharedKey to EncapsulationCiphertext.MLKEM(ct)
            }
        }
    }

    // -- Encrypter --

    override fun encapsulationPublicKey(): EncapsulationPublicKey = this

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- toString --

    override fun toString(): String {
        val displayKey = when (this) {
            is X25519 -> key
            is MLKEM -> key
        }
        return "EncapsulationPublicKey(${refHexShort()}, $displayKey)"
    }

    // -- CBOR --

    /**
     * Returns the CBOR tags for this key. The tag depends on the variant:
     * - [X25519] uses [TAG_X25519_PUBLIC_KEY]
     * - [MLKEM] uses [TAG_MLKEM_PUBLIC_KEY]
     */
    fun cborTags(): List<Tag> = when (this) {
        is X25519 -> tagsForValues(listOf(TAG_X25519_PUBLIC_KEY))
        is MLKEM -> tagsForValues(listOf(TAG_MLKEM_PUBLIC_KEY))
    }

    /** Returns the untagged CBOR representation, dispatching to the inner key. */
    fun untaggedCbor(): Cbor = when (this) {
        is X25519 -> key.untaggedCbor()
        is MLKEM -> key.untaggedCbor()
    }

    /** Returns the tagged CBOR representation, dispatching to the inner key. */
    fun taggedCbor(): Cbor = when (this) {
        is X25519 -> key.taggedCbor()
        is MLKEM -> key.taggedCbor()
    }

    /** Returns the tagged CBOR encoding as binary data. */
    fun taggedCborData(): ByteArray = taggedCbor().toCborData()

    companion object {
        /**
         * Decodes an [EncapsulationPublicKey] from tagged CBOR.
         *
         * Dispatches based on the CBOR tag value:
         * - [TAG_X25519_PUBLIC_KEY] produces an [X25519] variant
         * - [TAG_MLKEM_PUBLIC_KEY] produces an [MLKEM] variant
         *
         * @throws com.blockchaincommons.dcbor.CborException if the CBOR is not
         *   a valid encapsulation public key
         */
        fun fromTaggedCbor(cbor: Cbor): EncapsulationPublicKey {
            val (tagValue, _) = cbor.tryTaggedValue()
            return when (tagValue) {
                TAG_X25519_PUBLIC_KEY -> X25519(X25519PublicKey.fromTaggedCbor(cbor))
                TAG_MLKEM_PUBLIC_KEY -> MLKEM(MLKEMPublicKey.fromTaggedCbor(cbor))
                else -> throw com.blockchaincommons.dcbor.CborException.msg(
                    "Invalid encapsulation public key",
                )
            }
        }

        /**
         * Decodes an [EncapsulationPublicKey] from tagged CBOR binary data.
         */
        fun fromTaggedCborData(data: ByteArray): EncapsulationPublicKey =
            fromTaggedCbor(Cbor.tryFromData(data))
    }
}
