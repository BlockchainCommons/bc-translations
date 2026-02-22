package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.ECDSA_PUBLIC_KEY_SIZE
import com.blockchaincommons.bccrypto.ECDSA_SIGNATURE_SIZE
import com.blockchaincommons.bccrypto.ecdsaDecompressPublicKey
import com.blockchaincommons.bccrypto.ecdsaVerify
import com.blockchaincommons.bctags.TAG_EC_KEY
import com.blockchaincommons.bctags.TAG_EC_KEY_V1
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborMap
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A compressed ECDSA public key on the secp256k1 curve.
 *
 * An [ECPublicKey] is a 33-byte compressed representation of a public key.
 * The first byte is a prefix (0x02 or 0x03) indicating the parity of the
 * y-coordinate, followed by the 32-byte x-coordinate.
 *
 * These public keys are used to verify ECDSA signatures and identify the
 * owner of a private key without revealing it.
 */
class ECPublicKey private constructor(private val keyData: ByteArray) :
    ECPublicKeyBase,
    ReferenceProvider,
    CborTaggedCodable,
    URCodable {

    init {
        require(keyData.size == KEY_SIZE) {
            "EC public key must be exactly $KEY_SIZE bytes, got ${keyData.size}"
        }
    }

    // -- ECKeyBase --

    override fun data(): ByteArray = keyData.copyOf()

    // -- ECKey --

    /** Returns this public key (identity operation). */
    override fun publicKey(): ECPublicKey = this

    // -- ECPublicKeyBase --

    /** Converts this compressed public key to its uncompressed form. */
    override fun uncompressedPublicKey(): ECUncompressedPublicKey =
        ECUncompressedPublicKey.fromData(ecdsaDecompressPublicKey(keyData))

    /**
     * Verifies an ECDSA signature for the given message.
     *
     * @param signature a 64-byte compact ECDSA signature
     * @param message the message that was signed
     * @return `true` if the signature is valid, `false` otherwise
     */
    fun verify(signature: ByteArray, message: ByteArray): Boolean {
        require(signature.size == ECDSA_SIGNATURE_SIZE) {
            "ECDSA signature must be exactly $ECDSA_SIGNATURE_SIZE bytes, got ${signature.size}"
        }
        return ecdsaVerify(keyData, signature, message)
    }

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_EC_KEY, TAG_EC_KEY_V1))

    /**
     * Encodes as a CBOR map: {3: key_bytes}.
     *
     * Unlike the private key encoding, key 2 (private flag) is absent,
     * indicating this is a public key.
     */
    override fun untaggedCbor(): Cbor {
        val map = CborMap()
        map.insert(Cbor.fromInt(3), Cbor.fromByteString(keyData))
        return Cbor.fromMap(map)
    }

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is ECPublicKey) return false
        return keyData.contentEquals(other.keyData)
    }

    override fun hashCode(): Int = keyData.contentHashCode()

    // -- toString --

    override fun toString(): String = "ECPublicKey(${refHexShort()})"

    companion object {
        const val KEY_SIZE: Int = ECDSA_PUBLIC_KEY_SIZE

        /** Restores an EC public key from exactly [KEY_SIZE] bytes. */
        fun fromData(data: ByteArray): ECPublicKey {
            if (data.size != KEY_SIZE) {
                throw BcComponentsException.invalidSize(
                    "ECDSA public key", KEY_SIZE, data.size,
                )
            }
            return ECPublicKey(data.copyOf())
        }

        /**
         * Creates an EC public key from a hexadecimal string.
         *
         * @throws BcComponentsException.InvalidSize if the decoded bytes
         *   are not exactly [KEY_SIZE] bytes
         */
        fun fromHex(hex: String): ECPublicKey =
            fromData(hex.hexToByteArray())

        /** Decodes an [ECPublicKey] from untagged CBOR (a map). */
        fun fromUntaggedCbor(cbor: Cbor): ECPublicKey {
            val map = cbor.tryMap()
            // Public key maps must NOT have key 2 (private flag)
            val isPrivate: Boolean? = map.get<Int, Boolean>(2)
            if (isPrivate == true) {
                throw BcComponentsException.invalidData(
                    "EC key", "expected public key but found private key flag",
                )
            }
            val bytes: ByteArray = map.extract<Int, ByteArray>(3)
            return fromData(bytes)
        }

        /** Decodes an [ECPublicKey] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): ECPublicKey =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_EC_KEY, TAG_EC_KEY_V1)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [ECPublicKey] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): ECPublicKey =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_EC_KEY, TAG_EC_KEY_V1)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [ECPublicKey] from a UR. */
        fun fromUr(ur: UR): ECPublicKey {
            ur.checkType("eckey")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [ECPublicKey] from a UR string. */
        fun fromUrString(urString: String): ECPublicKey =
            fromUr(UR.fromUrString(urString))
    }
}
