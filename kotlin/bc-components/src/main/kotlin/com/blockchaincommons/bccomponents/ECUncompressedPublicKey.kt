package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE
import com.blockchaincommons.bccrypto.ecdsaCompressPublicKey
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
 * An uncompressed ECDSA public key on the secp256k1 curve.
 *
 * An [ECUncompressedPublicKey] is a 65-byte representation of a public key
 * consisting of a 0x04 prefix byte followed by 32 bytes for the x-coordinate
 * and 32 bytes for the y-coordinate.
 *
 * This format explicitly includes both coordinates of the elliptic curve
 * point, unlike the compressed format which only includes the x-coordinate
 * and a parity byte. The compressed format ([ECPublicKey]) is more
 * space-efficient and provides the same cryptographic security.
 */
class ECUncompressedPublicKey private constructor(private val keyData: ByteArray) :
    ECPublicKeyBase,
    ReferenceProvider,
    CborTaggedCodable,
    URCodable {

    init {
        require(keyData.size == KEY_SIZE) {
            "EC uncompressed public key must be exactly $KEY_SIZE bytes, got ${keyData.size}"
        }
    }

    // -- ECKeyBase --

    override fun data(): ByteArray = keyData.copyOf()

    // -- ECKey --

    /** Compresses this uncompressed public key to its 33-byte compressed form. */
    override fun publicKey(): ECPublicKey =
        ECPublicKey.fromData(ecdsaCompressPublicKey(keyData))

    // -- ECPublicKeyBase --

    /** Returns this uncompressed public key (identity operation). */
    override fun uncompressedPublicKey(): ECUncompressedPublicKey = this

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
        if (other !is ECUncompressedPublicKey) return false
        return keyData.contentEquals(other.keyData)
    }

    override fun hashCode(): Int = keyData.contentHashCode()

    // -- toString --

    override fun toString(): String = "ECUncompressedPublicKey(${refHexShort()})"

    companion object {
        const val KEY_SIZE: Int = ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE

        /** Restores an EC uncompressed public key from exactly [KEY_SIZE] bytes. */
        fun fromData(data: ByteArray): ECUncompressedPublicKey {
            if (data.size != KEY_SIZE) {
                throw BcComponentsException.invalidSize(
                    "ECDSA uncompressed public key", KEY_SIZE, data.size,
                )
            }
            return ECUncompressedPublicKey(data.copyOf())
        }

        /**
         * Creates an EC uncompressed public key from a hexadecimal string.
         *
         * @throws BcComponentsException.InvalidSize if the decoded bytes
         *   are not exactly [KEY_SIZE] bytes
         */
        fun fromHex(hex: String): ECUncompressedPublicKey =
            fromData(hex.hexToByteArray())

        /** Decodes an [ECUncompressedPublicKey] from untagged CBOR (a map). */
        fun fromUntaggedCbor(cbor: Cbor): ECUncompressedPublicKey {
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

        /** Decodes an [ECUncompressedPublicKey] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): ECUncompressedPublicKey =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_EC_KEY, TAG_EC_KEY_V1)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [ECUncompressedPublicKey] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): ECUncompressedPublicKey =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_EC_KEY, TAG_EC_KEY_V1)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [ECUncompressedPublicKey] from a UR. */
        fun fromUr(ur: UR): ECUncompressedPublicKey {
            ur.checkType("eckey")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [ECUncompressedPublicKey] from a UR string. */
        fun fromUrString(urString: String): ECUncompressedPublicKey =
            fromUr(UR.fromUrString(urString))
    }
}
