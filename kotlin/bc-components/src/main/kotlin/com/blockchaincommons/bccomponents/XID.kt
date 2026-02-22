package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_XID
import com.blockchaincommons.bcur.Bytewords
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * An eXtensible IDentifier (XID).
 *
 * A XID is a unique 32-byte identifier for a subject entity (person,
 * organization, device, or any other entity). XIDs have the following
 * characteristics:
 *
 * - They are cryptographically tied to a public key at inception (the
 *   "inception key")
 * - They remain stable throughout their lifecycle even as their keys and
 *   permissions change
 * - They can be extended to XID documents containing keys, endpoints,
 *   permissions, and delegation info
 * - They support key rotation and multiple verification schemes
 *
 * A XID is created by taking the SHA-256 hash of the CBOR encoding of a
 * public signing key. This ensures the XID is cryptographically tied to the
 * key.
 *
 * As defined in
 * [BCR-2024-010](https://github.com/BlockchainCommons/Research/blob/master/papers/bcr-2024-010-xid.md).
 */
class XID private constructor(private val data: ByteArray) :
    ReferenceProvider,
    Comparable<XID>,
    CborTaggedCodable,
    URCodable {

    init {
        require(data.size == XID_SIZE) {
            "XID data must be exactly $XID_SIZE bytes, got ${data.size}"
        }
    }

    /** Returns a copy of the underlying 32-byte XID data. */
    fun data(): ByteArray = data.copyOf()

    /** Returns the XID bytes as a copy. */
    fun asBytes(): ByteArray = data.copyOf()

    /** The XID as a 64-character lowercase hexadecimal string. */
    val hex: String get() = data.toHexString()

    /** Returns the first four bytes of the XID as a hexadecimal string. */
    fun shortDescription(): String = refHexShort()

    /**
     * Validates the XID against the given public signing key.
     *
     * @return `true` if this XID matches the SHA-256 hash of the key's
     *   CBOR encoding
     */
    fun validate(key: SigningPublicKey): Boolean {
        val keyData = key.taggedCbor().toCborData()
        val digest = Digest.fromImage(keyData)
        return data.contentEquals(digest.data())
    }

    /**
     * Returns the first four bytes of the XID as upper-case ByteWords.
     *
     * @param prefix if `true`, the XID marker prefix is prepended
     */
    fun bytewordsIdentifier(prefix: Boolean = false): String {
        val s = Bytewords.identifier(refDataShort()).uppercase()
        return if (prefix) "\uD83C\uDD67 $s" else s
    }

    /**
     * Returns the first four bytes of the XID as Bytemoji.
     *
     * @param prefix if `true`, the XID marker prefix is prepended
     */
    fun bytemojiIdentifier(prefix: Boolean = false): String {
        val s = Bytewords.bytemojiIdentifier(refDataShort()).uppercase()
        return if (prefix) "\uD83C\uDD67 $s" else s
    }

    // -- ReferenceProvider --

    override fun reference(): Reference = Reference.fromData(data.copyOf())

    // -- Comparable --

    override fun compareTo(other: XID): Int {
        for (i in data.indices) {
            val cmp = (data[i].toInt() and 0xFF) - (other.data[i].toInt() and 0xFF)
            if (cmp != 0) return cmp
        }
        return 0
    }

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is XID) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString --

    override fun toString(): String = "XID(${shortDescription()})"

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_XID))

    override fun untaggedCbor(): Cbor = Cbor.fromByteString(data)

    companion object {
        const val XID_SIZE: Int = 32

        /**
         * Creates a new XID from the given signing public key (the
         * "genesis key").
         *
         * The XID is the SHA-256 digest of the tagged CBOR encoding of the
         * public key.
         */
        fun fromSigningPublicKey(key: SigningPublicKey): XID {
            val keyCborData = key.taggedCbor().toCborData()
            val digest = Digest.fromImage(keyCborData)
            return XID(digest.data())
        }

        /** Restores a XID from exactly [XID_SIZE] bytes. */
        fun fromData(data: ByteArray): XID {
            if (data.size != XID_SIZE) {
                throw BcComponentsException.invalidSize("XID", XID_SIZE, data.size)
            }
            return XID(data.copyOf())
        }

        /**
         * Creates a XID from a hexadecimal string.
         *
         * @throws IllegalArgumentException if the string is not exactly 64
         *   hex digits.
         */
        fun fromHex(hex: String): XID = fromData(hex.hexToByteArray())

        /** Decodes a [XID] from untagged CBOR (a byte string). */
        fun fromUntaggedCbor(cbor: Cbor): XID {
            val bytes = cbor.tryByteStringData()
            return fromData(bytes)
        }

        /** Decodes a [XID] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): XID =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_XID)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [XID] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): XID =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_XID)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [XID] from a UR. */
        fun fromUr(ur: UR): XID {
            ur.checkType("xid")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [XID] from a UR string. */
        fun fromUrString(urString: String): XID =
            fromUr(UR.fromUrString(urString))
    }
}

/**
 * A type that can provide a XID.
 */
interface XIDProvider {
    /** Returns the XID for this object. */
    fun xid(): XID
}
