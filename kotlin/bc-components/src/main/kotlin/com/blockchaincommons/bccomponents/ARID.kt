package com.blockchaincommons.bccomponents

import com.blockchaincommons.bcrand.randomData
import com.blockchaincommons.bctags.TAG_ARID
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * An "Apparently Random Identifier" (ARID).
 *
 * An ARID is a cryptographically strong, universally unique identifier with
 * the following properties:
 * - Non-correlatability: The bit sequence cannot be correlated with its
 *   referent or any other ARID
 * - Neutral semantics: Contains no inherent type information
 * - Open generation: Any method of generation is allowed as long as it
 *   produces statistically random bits
 * - Minimum strength: Must be 256 bits (32 bytes) in length
 * - Cryptographic suitability: Can be used as inputs to cryptographic
 *   constructs
 *
 * Unlike digests/hashes which identify a fixed, immutable state of data,
 * ARIDs can serve as stable identifiers for mutable data structures.
 *
 * As defined in
 * [BCR-2022-002](https://github.com/BlockchainCommons/Research/blob/master/papers/bcr-2022-002-arid.md).
 */
class ARID private constructor(private val data: ByteArray) :
    ReferenceProvider,
    Comparable<ARID>,
    CborTaggedCodable,
    URCodable {

    init {
        require(data.size == ARID_SIZE) {
            "ARID data must be exactly $ARID_SIZE bytes, got ${data.size}"
        }
    }

    /** Returns a copy of the underlying 32-byte ARID data. */
    fun data(): ByteArray = data.copyOf()

    /** Returns the ARID bytes as a copy. */
    fun asBytes(): ByteArray = data.copyOf()

    /** The ARID as a 64-character lowercase hexadecimal string. */
    val hex: String get() = data.toHexString()

    /** Returns the first four bytes of the ARID as a hexadecimal string. */
    fun shortDescription(): String = data.copyOfRange(0, 4).toHexString()

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- Comparable --

    override fun compareTo(other: ARID): Int {
        for (i in data.indices) {
            val cmp = (data[i].toInt() and 0xFF) - (other.data[i].toInt() and 0xFF)
            if (cmp != 0) return cmp
        }
        return 0
    }

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is ARID) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString --

    override fun toString(): String = "ARID($hex)"

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_ARID))

    override fun untaggedCbor(): Cbor = Cbor.fromByteString(data)

    companion object {
        const val ARID_SIZE: Int = 32

        /** Creates a new random ARID. */
        fun create(): ARID {
            val data = randomData(ARID_SIZE)
            return ARID(data)
        }

        /** Restores an ARID from exactly [ARID_SIZE] bytes. */
        fun fromData(data: ByteArray): ARID {
            if (data.size != ARID_SIZE) {
                throw BcComponentsException.invalidSize("ARID", ARID_SIZE, data.size)
            }
            return ARID(data.copyOf())
        }

        /**
         * Creates an ARID from a hexadecimal string.
         *
         * @throws IllegalArgumentException if the string is not exactly 64
         *   hex digits.
         */
        fun fromHex(hex: String): ARID = fromData(hex.hexToByteArray())

        /** Decodes an [ARID] from untagged CBOR (a byte string). */
        fun fromUntaggedCbor(cbor: Cbor): ARID {
            val bytes = cbor.tryByteStringData()
            return fromData(bytes)
        }

        /** Decodes an [ARID] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): ARID =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_ARID)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [ARID] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): ARID =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_ARID)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [ARID] from a UR. */
        fun fromUr(ur: UR): ARID {
            ur.checkType("arid")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [ARID] from a UR string. */
        fun fromUrString(urString: String): ARID =
            fromUr(UR.fromUrString(urString))
    }
}
