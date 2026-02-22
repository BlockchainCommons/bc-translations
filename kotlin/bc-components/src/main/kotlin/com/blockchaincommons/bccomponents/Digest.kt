package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.sha256
import com.blockchaincommons.bctags.TAG_DIGEST
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.bcur.URDecodable
import com.blockchaincommons.bcur.UREncodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedDecodable
import com.blockchaincommons.dcbor.CborTaggedEncodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A cryptographically secure digest, implemented with SHA-256.
 *
 * A [Digest] represents the cryptographic hash of some data. SHA-256 is
 * used, producing a 32-byte hash value. Digests are used throughout the
 * library for data verification and as unique identifiers derived from data.
 */
class Digest private constructor(private val data: ByteArray) :
    DigestProvider,
    Comparable<Digest>,
    CborTaggedCodable,
    URCodable {

    init {
        require(data.size == DIGEST_SIZE) {
            "Digest data must be exactly $DIGEST_SIZE bytes, got ${data.size}"
        }
    }

    /** Returns a copy of the underlying 32-byte digest data. */
    fun data(): ByteArray = data.copyOf()

    /** Returns the digest bytes as a read-only slice. */
    fun asBytes(): ByteArray = data.copyOf()

    /** Validates that this digest matches the SHA-256 hash of [image]. */
    fun validate(image: ByteArray): Boolean = this == fromImage(image)

    /** The digest as a 64-character lowercase hexadecimal string. */
    val hex: String get() = data.toHexString()

    /** Returns the first four bytes of the digest as a hexadecimal string. */
    fun shortDescription(): String = data.copyOfRange(0, 4).toHexString()

    /** Returns the digest bytes as a new [ByteArray]. */
    fun toByteArray(): ByteArray = data.copyOf()

    // -- DigestProvider --

    override fun digest(): Digest = this

    // -- Comparable --

    override fun compareTo(other: Digest): Int {
        for (i in data.indices) {
            val cmp = (data[i].toInt() and 0xFF) - (other.data[i].toInt() and 0xFF)
            if (cmp != 0) return cmp
        }
        return 0
    }

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Digest) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString / debug --

    override fun toString(): String = "Digest($hex)"

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_DIGEST))

    override fun untaggedCbor(): Cbor = Cbor.fromByteString(data)

    // -- Companion --

    companion object {
        const val DIGEST_SIZE: Int = 32

        /** Creates a digest from exactly [DIGEST_SIZE] bytes. */
        fun fromData(data: ByteArray): Digest {
            require(data.size == DIGEST_SIZE) {
                "Digest data must be exactly $DIGEST_SIZE bytes, got ${data.size}"
            }
            return Digest(data.copyOf())
        }

        /**
         * Creates a digest from a byte array, returning an error if the
         * length is not [DIGEST_SIZE].
         */
        fun fromDataChecked(data: ByteArray): Digest {
            if (data.size != DIGEST_SIZE) {
                throw BcComponentsException.invalidSize("digest", DIGEST_SIZE, data.size)
            }
            return Digest(data.copyOf())
        }

        /** Creates a digest by computing the SHA-256 hash of [image]. */
        fun fromImage(image: ByteArray): Digest = Digest(sha256(image))

        /**
         * Creates a digest by concatenating the given byte parts and
         * computing their SHA-256 hash.
         */
        fun fromImageParts(parts: List<ByteArray>): Digest {
            val totalSize = parts.sumOf { it.size }
            val buf = ByteArray(totalSize)
            var offset = 0
            for (part in parts) {
                part.copyInto(buf, offset)
                offset += part.size
            }
            return fromImage(buf)
        }

        /**
         * Creates a digest by concatenating the data of the given digests
         * and computing their SHA-256 hash.
         */
        fun fromDigests(digests: List<Digest>): Digest {
            val buf = ByteArray(digests.size * DIGEST_SIZE)
            var offset = 0
            for (d in digests) {
                d.data.copyInto(buf, offset)
                offset += DIGEST_SIZE
            }
            return fromImage(buf)
        }

        /**
         * Creates a digest from a hexadecimal string.
         *
         * @throws IllegalArgumentException if the string is not exactly 64
         *   hex digits.
         */
        fun fromHex(hex: String): Digest = fromData(hex.hexToByteArray())

        /**
         * Validates [image] against an optional digest.
         *
         * Returns `true` if [digest] is `null` or if it matches the image's
         * SHA-256 hash. Returns `false` otherwise.
         */
        fun validate(image: ByteArray, digest: Digest?): Boolean =
            digest?.validate(image) ?: true

        /** Decodes a [Digest] from untagged CBOR (a byte string). */
        fun fromUntaggedCbor(cbor: Cbor): Digest {
            val bytes = cbor.tryByteStringData()
            return fromDataChecked(bytes)
        }

        /** Decodes a [Digest] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): Digest =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_DIGEST)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Digest] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): Digest =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_DIGEST)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Digest] from a UR. */
        fun fromUr(ur: UR): Digest {
            ur.checkType("digest")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [Digest] from a UR string. */
        fun fromUrString(urString: String): Digest =
            fromUr(UR.fromUrString(urString))
    }
}
