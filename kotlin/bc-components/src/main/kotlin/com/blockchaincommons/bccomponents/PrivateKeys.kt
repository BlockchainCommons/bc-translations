package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_PRIVATE_KEYS
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborCase
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * An interface for types that can provide a complete set of private cryptographic keys.
 *
 * Types implementing this interface can be used as a source of [PrivateKeys],
 * which contain both signing and encryption private keys.
 */
interface PrivateKeysProvider {
    /**
     * Returns a complete set of private keys for cryptographic operations.
     */
    fun privateKeys(): PrivateKeys
}

/**
 * A container combining signing and encapsulation private keys.
 *
 * [PrivateKeys] packages a [SigningPrivateKey] for creating digital signatures
 * with an [EncapsulationPrivateKey] for decrypting messages, providing a
 * complete private key set for cryptographic operations.
 *
 * This type is typically used alongside its public counterpart, [PublicKeys],
 * to enable secure communication between entities.
 */
class PrivateKeys(
    val signingPrivateKey: SigningPrivateKey,
    val encapsulationPrivateKey: EncapsulationPrivateKey,
) : PrivateKeysProvider, Signer, Decrypter, ReferenceProvider, CborTaggedCodable, URCodable {

    /**
     * Derives the corresponding [PublicKeys] from this private key set.
     */
    fun publicKeys(): PublicKeys =
        PublicKeys(
            signingPrivateKey.publicKey(),
            encapsulationPrivateKey.publicKey(),
        )

    // -- PrivateKeysProvider --

    override fun privateKeys(): PrivateKeys = this

    // -- Signer --

    override fun signWithOptions(message: ByteArray, options: SigningOptions?): Signature =
        signingPrivateKey.signWithOptions(message, options)

    // -- Decrypter --

    override fun encapsulationPrivateKey(): EncapsulationPrivateKey =
        encapsulationPrivateKey

    override fun decapsulateSharedSecret(ciphertext: EncapsulationCiphertext): SymmetricKey =
        encapsulationPrivateKey.decapsulateSharedSecret(ciphertext)

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is PrivateKeys) return false
        return signingPrivateKey == other.signingPrivateKey &&
            encapsulationPrivateKey == other.encapsulationPrivateKey
    }

    override fun hashCode(): Int {
        var result = signingPrivateKey.hashCode()
        result = 31 * result + encapsulationPrivateKey.hashCode()
        return result
    }

    override fun toString(): String =
        "PrivateKeys(${refHexShort()}, $signingPrivateKey, $encapsulationPrivateKey)"

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_PRIVATE_KEYS))

    /**
     * Encodes as a CBOR array: [signing_key_tagged_cbor, encapsulation_key_tagged_cbor].
     */
    override fun untaggedCbor(): Cbor {
        val signingKeyCbor = signingPrivateKey.taggedCbor()
        val encapsulationKeyCbor = encapsulationPrivateKey.taggedCbor()
        return Cbor.fromArray(listOf(signingKeyCbor, encapsulationKeyCbor))
    }

    companion object {
        /** Decodes [PrivateKeys] from untagged CBOR (an array). */
        fun fromUntaggedCbor(cbor: Cbor): PrivateKeys {
            val case = cbor.cborCase
            if (case !is CborCase.Array) {
                throw BcComponentsException.invalidData("PrivateKeys", "expected array")
            }
            val elements = case.value
            if (elements.size != 2) {
                throw BcComponentsException.invalidData(
                    "PrivateKeys", "expected array of length 2, got ${elements.size}",
                )
            }
            val signingPrivateKey = SigningPrivateKey.fromTaggedCbor(elements[0])
            val encapsulationPrivateKey = EncapsulationPrivateKey.fromTaggedCbor(elements[1])
            return PrivateKeys(signingPrivateKey, encapsulationPrivateKey)
        }

        /** Decodes [PrivateKeys] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): PrivateKeys =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_PRIVATE_KEYS)),
            ) { fromUntaggedCbor(it) }

        /** Decodes [PrivateKeys] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): PrivateKeys =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_PRIVATE_KEYS)),
            ) { fromUntaggedCbor(it) }

        /** Decodes [PrivateKeys] from a UR. */
        fun fromUr(ur: UR): PrivateKeys {
            ur.checkType("crypto-prvkeys")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes [PrivateKeys] from a UR string. */
        fun fromUrString(urString: String): PrivateKeys =
            fromUr(UR.fromUrString(urString))
    }
}
