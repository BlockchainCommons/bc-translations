package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_REFERENCE
import com.blockchaincommons.bcur.Bytewords
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A globally unique reference to a globally unique object.
 *
 * [Reference] provides a cryptographically secure way to uniquely identify
 * objects based on their content. It is a 32-byte identifier, typically
 * derived from the SHA-256 hash of the object's CBOR-serialized form.
 */
class Reference private constructor(private val data: ByteArray) :
    DigestProvider,
    ReferenceProvider,
    Comparable<Reference>,
    CborTaggedCodable,
    URCodable {

    init {
        require(data.size == REFERENCE_SIZE) {
            "Reference data must be exactly $REFERENCE_SIZE bytes, got ${data.size}"
        }
    }

    /** Returns a copy of the underlying 32-byte reference data. */
    fun data(): ByteArray = data.copyOf()

    /** Returns the reference bytes as a copy. */
    fun asBytes(): ByteArray = data.copyOf()

    /** Returns the reference bytes as a new [ByteArray]. */
    fun toByteArray(): ByteArray = data.copyOf()

    /** Returns the full reference as a hexadecimal string. */
    override fun refHex(): String = data.toHexString()

    /** Returns the first four bytes of the reference. */
    override fun refDataShort(): ByteArray = data.copyOfRange(0, 4)

    /** Returns the first four bytes of the reference as a hexadecimal string. */
    override fun refHexShort(): String = data.copyOfRange(0, 4).toHexString()

    /**
     * Returns the first four bytes of the reference as upper-case ByteWords.
     *
     * @param prefix an optional prefix to prepend
     */
    fun bytewordsIdentifier(prefix: String? = null): String {
        val s = Bytewords.identifier(refDataShort()).uppercase()
        return if (prefix != null) "$prefix $s" else s
    }

    /**
     * Returns the first four bytes of the reference as Bytemoji.
     *
     * @param prefix an optional prefix to prepend
     */
    fun bytemojiIdentifier(prefix: String? = null): String {
        val s = Bytewords.bytemojiIdentifier(refDataShort()).uppercase()
        return if (prefix != null) "$prefix $s" else s
    }

    // -- DigestProvider --

    override fun digest(): Digest =
        Digest.fromImage(taggedCbor().toCborData())

    // -- ReferenceProvider --

    override fun reference(): Reference = fromDigest(digest())

    // -- Comparable --

    override fun compareTo(other: Reference): Int {
        for (i in data.indices) {
            val cmp = (data[i].toInt() and 0xFF) - (other.data[i].toInt() and 0xFF)
            if (cmp != 0) return cmp
        }
        return 0
    }

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Reference) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString --

    override fun toString(): String = "Reference(${refHexShort()})"

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_REFERENCE))

    override fun untaggedCbor(): Cbor = Cbor.fromByteString(data)

    // -- Companion --

    companion object {
        const val REFERENCE_SIZE: Int = 32

        /** Creates a reference from exactly [REFERENCE_SIZE] bytes. */
        fun fromData(data: ByteArray): Reference {
            require(data.size == REFERENCE_SIZE) {
                "Reference data must be exactly $REFERENCE_SIZE bytes, got ${data.size}"
            }
            return Reference(data.copyOf())
        }

        /** Creates a reference from the given digest. */
        fun fromDigest(digest: Digest): Reference =
            Reference(digest.data())

        /**
         * Creates a reference from a hexadecimal string.
         *
         * @throws IllegalArgumentException if the string is not exactly 64 hex digits.
         */
        fun fromHex(hex: String): Reference = fromData(hex.hexToByteArray())

        /** Decodes a [Reference] from untagged CBOR (a byte string). */
        fun fromUntaggedCbor(cbor: Cbor): Reference {
            val bytes = cbor.tryByteStringData()
            if (bytes.size != REFERENCE_SIZE) {
                throw BcComponentsException.invalidSize("reference", REFERENCE_SIZE, bytes.size)
            }
            return Reference(bytes)
        }

        /** Decodes a [Reference] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): Reference =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_REFERENCE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Reference] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): Reference =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_REFERENCE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Reference] from a UR. */
        fun fromUr(ur: UR): Reference {
            ur.checkType("reference")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [Reference] from a UR string. */
        fun fromUrString(urString: String): Reference =
            fromUr(UR.fromUrString(urString))
    }
}
