package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_COMPRESSED
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues
import java.util.zip.CRC32
import java.util.zip.Deflater
import java.util.zip.Inflater

/**
 * A compressed binary object with integrity verification.
 *
 * [Compressed] provides efficient storage and transmission of binary data
 * using the raw DEFLATE algorithm (no zlib/gzip wrapper). It includes a
 * CRC32 checksum for integrity verification and an optional cryptographic
 * [Digest].
 *
 * CBOR array format: `[checksum, decompressed_size, compressed_data, digest?]`
 */
class Compressed private constructor(
    /** CRC32 checksum of the decompressed data. */
    val checksum: UInt,
    /** Size of the original decompressed data in bytes. */
    val decompressedSize: Int,
    /** The compressed data (or original data if compression was ineffective). */
    private val compressedData: ByteArray,
    /** Optional cryptographic digest of the content. */
    private val digest: Digest?,
) : DigestProvider,
    CborTaggedCodable,
    URCodable {

    /** The size of the compressed data in bytes. */
    val compressedSize: Int get() = compressedData.size

    /** The compression ratio (compressed / decompressed). Values < 1.0 indicate effective compression. */
    val compressionRatio: Double
        get() = compressedSize.toDouble() / decompressedSize.toDouble()

    /** Returns the optional digest, or `null` if none was associated. */
    fun digestOrNull(): Digest? = digest

    /** Whether this compressed data has an associated digest. */
    val hasDigest: Boolean get() = digest != null

    /**
     * Decompresses and returns the original data.
     *
     * @throws BcComponentsException.Compression if decompression fails or
     *   the checksum does not match.
     */
    fun decompress(): ByteArray {
        if (compressedData.size >= decompressedSize) {
            return compressedData.copyOf()
        }

        val inflater = Inflater(true) // nowrap = true for raw DEFLATE
        inflater.setInput(compressedData)
        val output = ByteArray(decompressedSize)
        try {
            val resultLength = inflater.inflate(output)
            if (resultLength != decompressedSize) {
                throw BcComponentsException.compression("corrupt compressed data")
            }
        } catch (e: java.util.zip.DataFormatException) {
            throw BcComponentsException.compression("corrupt compressed data")
        } finally {
            inflater.end()
        }

        if (computeCrc32(output) != checksum) {
            throw BcComponentsException.compression("compressed data checksum mismatch")
        }

        return output
    }

    // -- DigestProvider --

    /**
     * Returns the cryptographic digest associated with this compressed data.
     *
     * @throws IllegalStateException if no digest was associated at compression
     *   time. Use [digestOrNull] to check for the presence of a digest without
     *   throwing.
     */
    override fun digest(): Digest =
        digest ?: throw IllegalStateException("No digest associated with this Compressed")

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Compressed) return false
        return checksum == other.checksum &&
            decompressedSize == other.decompressedSize &&
            compressedData.contentEquals(other.compressedData) &&
            digest == other.digest
    }

    override fun hashCode(): Int {
        var result = checksum.hashCode()
        result = 31 * result + decompressedSize
        result = 31 * result + compressedData.contentHashCode()
        result = 31 * result + (digest?.hashCode() ?: 0)
        return result
    }

    // -- toString --

    override fun toString(): String {
        val checksumHex = "%08x".format(checksum.toInt())
        return "Compressed($checksumHex, $compressedSize/$decompressedSize, ${"%.2f".format(compressionRatio)}, ${digest?.shortDescription() ?: "None"})"
    }

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_COMPRESSED))

    override fun untaggedCbor(): Cbor {
        val elements = mutableListOf(
            Cbor.fromUInt(checksum),
            Cbor.fromInt(decompressedSize),
            Cbor.fromByteString(compressedData),
        )
        if (digest != null) {
            elements.add(digest.taggedCbor())
        }
        return Cbor.fromArray(elements)
    }

    // -- Companion --

    companion object {
        /**
         * Creates a [Compressed] from pre-computed parameters. Low-level
         * constructor for deserialization or pre-compressed data.
         *
         * @throws BcComponentsException.Compression if the compressed data
         *   is larger than the decompressed size.
         */
        fun create(
            checksum: UInt,
            decompressedSize: Int,
            compressedData: ByteArray,
            digest: Digest?,
        ): Compressed {
            if (compressedData.size > decompressedSize) {
                throw BcComponentsException.compression(
                    "compressed data is larger than decompressed size",
                )
            }
            return Compressed(checksum, decompressedSize, compressedData.copyOf(), digest)
        }

        /**
         * Compresses the given [decompressedData] using DEFLATE.
         *
         * If compression would increase the data size (common for small or
         * already-compressed inputs), the original data is stored instead.
         */
        fun fromDecompressedData(
            decompressedData: ByteArray,
            digest: Digest? = null,
        ): Compressed {
            val checksum = computeCrc32(decompressedData)
            val decompressedSize = decompressedData.size

            val deflater = Deflater(Deflater.DEFAULT_COMPRESSION, true) // nowrap = true
            deflater.setInput(decompressedData)
            deflater.finish()
            val buffer = ByteArray(decompressedData.size + 256)
            val compressedLength = deflater.deflate(buffer)
            deflater.end()
            val compressedData = buffer.copyOfRange(0, compressedLength)

            return if (compressedLength > 0 && compressedLength < decompressedSize) {
                Compressed(checksum, decompressedSize, compressedData, digest)
            } else {
                Compressed(checksum, decompressedSize, decompressedData.copyOf(), digest)
            }
        }

        /** Decodes a [Compressed] from untagged CBOR (an array). */
        fun fromUntaggedCbor(cbor: Cbor): Compressed {
            val elements = cbor.tryArray()
            if (elements.size < 3 || elements.size > 4) {
                throw com.blockchaincommons.dcbor.CborException.msg(
                    "invalid number of elements in compressed",
                )
            }
            val checksum = elements[0].tryUInt()
            val decompressedSize = elements[1].tryInt()
            val compressedData = elements[2].tryByteStringData()
            val digest = if (elements.size == 4) {
                Digest.fromTaggedCbor(elements[3])
            } else {
                null
            }
            return create(checksum, decompressedSize, compressedData, digest)
        }

        /** Decodes a [Compressed] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): Compressed =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_COMPRESSED)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Compressed] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): Compressed =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_COMPRESSED)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Compressed] from a UR. */
        fun fromUr(ur: UR): Compressed {
            ur.checkType("compressed")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [Compressed] from a UR string. */
        fun fromUrString(urString: String): Compressed =
            fromUr(UR.fromUrString(urString))

        private fun computeCrc32(data: ByteArray): UInt {
            val crc = CRC32()
            crc.update(data)
            return crc.value.toUInt()
        }
    }
}
