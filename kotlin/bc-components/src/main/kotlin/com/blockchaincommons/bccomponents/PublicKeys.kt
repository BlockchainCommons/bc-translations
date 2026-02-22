package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_PUBLIC_KEYS
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborCase
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * An interface for types that can provide a complete set of public cryptographic keys.
 *
 * Types implementing this interface can be used as a source of [PublicKeys],
 * which contain both verification and encryption public keys.
 */
interface PublicKeysProvider {
    /**
     * Returns a complete set of public keys for cryptographic operations.
     */
    fun publicKeys(): PublicKeys
}

/**
 * A container combining signing and encapsulation public keys.
 *
 * [PublicKeys] packages a [SigningPublicKey] for verifying digital signatures
 * with an [EncapsulationPublicKey] for encrypting messages, providing a
 * complete public key set for secure communication with an entity.
 *
 * This type is designed to be freely shared across networks and systems,
 * allowing others to securely communicate with the key owner, who holds the
 * corresponding [PrivateKeys] instance.
 */
class PublicKeys(
    val signingPublicKey: SigningPublicKey,
    val encapsulationPublicKey: EncapsulationPublicKey,
) : PublicKeysProvider, Verifier, Encrypter, ReferenceProvider, CborTaggedCodable, URCodable {

    // -- PublicKeysProvider --

    override fun publicKeys(): PublicKeys = this

    // -- Verifier --

    override fun verify(signature: Signature, message: ByteArray): Boolean =
        signingPublicKey.verify(signature, message)

    // -- Encrypter --

    override fun encapsulationPublicKey(): EncapsulationPublicKey =
        encapsulationPublicKey

    override fun encapsulateNewSharedSecret(): Pair<SymmetricKey, EncapsulationCiphertext> =
        encapsulationPublicKey.encapsulateNewSharedSecret()

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is PublicKeys) return false
        return signingPublicKey == other.signingPublicKey &&
            encapsulationPublicKey == other.encapsulationPublicKey
    }

    override fun hashCode(): Int {
        var result = signingPublicKey.hashCode()
        result = 31 * result + encapsulationPublicKey.hashCode()
        return result
    }

    override fun toString(): String =
        "PublicKeys(${refHexShort()}, $signingPublicKey, $encapsulationPublicKey)"

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_PUBLIC_KEYS))

    /**
     * Encodes as a CBOR array: [signing_key_tagged_cbor, encapsulation_key_tagged_cbor].
     */
    override fun untaggedCbor(): Cbor {
        val signingKeyCbor = signingPublicKey.taggedCbor()
        val encapsulationKeyCbor = encapsulationPublicKey.taggedCbor()
        return Cbor.fromArray(listOf(signingKeyCbor, encapsulationKeyCbor))
    }

    companion object {
        /** Decodes [PublicKeys] from untagged CBOR (an array). */
        fun fromUntaggedCbor(cbor: Cbor): PublicKeys {
            val case = cbor.cborCase
            if (case !is CborCase.Array) {
                throw BcComponentsException.invalidData("PublicKeys", "expected array")
            }
            val elements = case.value
            if (elements.size != 2) {
                throw BcComponentsException.invalidData(
                    "PublicKeys", "expected array of length 2, got ${elements.size}",
                )
            }
            val signingPublicKey = SigningPublicKey.fromTaggedCbor(elements[0])
            val encapsulationPublicKey = EncapsulationPublicKey.fromTaggedCbor(elements[1])
            return PublicKeys(signingPublicKey, encapsulationPublicKey)
        }

        /** Decodes [PublicKeys] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): PublicKeys =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_PUBLIC_KEYS)),
            ) { fromUntaggedCbor(it) }

        /** Decodes [PublicKeys] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): PublicKeys =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_PUBLIC_KEYS)),
            ) { fromUntaggedCbor(it) }

        /** Decodes [PublicKeys] from a UR. */
        fun fromUr(ur: UR): PublicKeys {
            ur.checkType("crypto-pubkeys")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes [PublicKeys] from a UR string. */
        fun fromUrString(urString: String): PublicKeys =
            fromUr(UR.fromUrString(urString))
    }
}
