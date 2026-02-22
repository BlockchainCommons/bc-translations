package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_MLKEM_PRIVATE_KEY
import com.blockchaincommons.bctags.TAG_X25519_PRIVATE_KEY
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A private key used for key encapsulation mechanisms (KEM).
 *
 * [EncapsulationPrivateKey] is a sealed class representing different types of
 * private keys that can be used for key encapsulation, including:
 *
 * - [X25519]: Curve25519-based key exchange
 * - [MLKEM]: Module Lattice-based Key Encapsulation Mechanism at various
 *   security levels
 *
 * These private keys are used to decrypt (decapsulate) shared secrets that
 * have been encapsulated with the corresponding public keys.
 */
sealed class EncapsulationPrivateKey : Decrypter, ReferenceProvider {

    /** An X25519 private key. */
    data class X25519(val key: X25519PrivateKey) : EncapsulationPrivateKey()

    /** An ML-KEM private key (post-quantum). */
    data class MLKEM(val key: MLKEMPrivateKey) : EncapsulationPrivateKey()

    /**
     * Returns the encapsulation scheme associated with this private key.
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
     * Decapsulates a shared secret from a ciphertext using this private key.
     *
     * @param ciphertext the encapsulation ciphertext containing the
     *   encapsulated shared secret
     * @return the decapsulated [SymmetricKey]
     * @throws BcComponentsException.Crypto if the ciphertext type does not
     *   match the private key type, or if decapsulation fails
     */
    override fun decapsulateSharedSecret(ciphertext: EncapsulationCiphertext): SymmetricKey {
        return when {
            this is X25519 && ciphertext is EncapsulationCiphertext.X25519 ->
                key.sharedKeyWith(ciphertext.key)
            this is MLKEM && ciphertext is EncapsulationCiphertext.MLKEM ->
                key.decapsulateSharedSecret(ciphertext.ciphertext)
            else -> throw BcComponentsException.crypto(
                "Mismatched key encapsulation types. " +
                    "private key: ${encapsulationScheme()}, " +
                    "ciphertext: ${ciphertext.encapsulationScheme()}",
            )
        }
    }

    /**
     * Derives the corresponding [EncapsulationPublicKey] from this private key.
     *
     * Only supported for [X25519] keys.
     *
     * @return the corresponding [EncapsulationPublicKey]
     * @throws BcComponentsException.Crypto if the key type does not support
     *   public key derivation
     */
    fun publicKey(): EncapsulationPublicKey = when (this) {
        is X25519 -> EncapsulationPublicKey.X25519(key.publicKey())
        is MLKEM -> throw BcComponentsException.crypto(
            "Deriving ML-KEM public key not supported",
        )
    }

    // -- Decrypter --

    override fun encapsulationPrivateKey(): EncapsulationPrivateKey = this

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- toString --

    override fun toString(): String {
        val displayKey = when (this) {
            is X25519 -> key
            is MLKEM -> key
        }
        return "EncapsulationPrivateKey(${refHexShort()}, $displayKey)"
    }

    // -- CBOR --

    /**
     * Returns the CBOR tags for this key. The tag depends on the variant:
     * - [X25519] uses [TAG_X25519_PRIVATE_KEY]
     * - [MLKEM] uses [TAG_MLKEM_PRIVATE_KEY]
     */
    fun cborTags(): List<Tag> = when (this) {
        is X25519 -> tagsForValues(listOf(TAG_X25519_PRIVATE_KEY))
        is MLKEM -> tagsForValues(listOf(TAG_MLKEM_PRIVATE_KEY))
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
         * Decodes an [EncapsulationPrivateKey] from tagged CBOR.
         *
         * Dispatches based on the CBOR tag value:
         * - [TAG_X25519_PRIVATE_KEY] produces an [X25519] variant
         * - [TAG_MLKEM_PRIVATE_KEY] produces an [MLKEM] variant
         *
         * @throws com.blockchaincommons.dcbor.CborException if the CBOR is not
         *   a valid encapsulation private key
         */
        fun fromTaggedCbor(cbor: Cbor): EncapsulationPrivateKey {
            val (tagValue, _) = cbor.tryTaggedValue()
            return when (tagValue) {
                TAG_X25519_PRIVATE_KEY -> X25519(X25519PrivateKey.fromTaggedCbor(cbor))
                TAG_MLKEM_PRIVATE_KEY -> MLKEM(MLKEMPrivateKey.fromTaggedCbor(cbor))
                else -> throw com.blockchaincommons.dcbor.CborException.msg(
                    "Invalid encapsulation private key",
                )
            }
        }

        /**
         * Decodes an [EncapsulationPrivateKey] from tagged CBOR binary data.
         */
        fun fromTaggedCborData(data: ByteArray): EncapsulationPrivateKey =
            fromTaggedCbor(Cbor.tryFromData(data))
    }
}
