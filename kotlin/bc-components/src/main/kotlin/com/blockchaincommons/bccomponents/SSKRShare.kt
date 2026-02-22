package com.blockchaincommons.bccomponents

import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator
import com.blockchaincommons.bctags.TAG_SSKR_SHARE
import com.blockchaincommons.bctags.TAG_SSKR_SHARE_V1
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A share of a secret split using Sharded Secret Key Reconstruction (SSKR).
 *
 * SSKR is a protocol for splitting a secret into multiple shares across one or
 * more groups, such that the secret can be reconstructed only when a threshold
 * number of shares from a threshold number of groups are combined.
 *
 * Each SSKR share contains:
 * - A unique identifier for the split
 * - Metadata about the group structure (thresholds, counts, indices)
 * - A portion of the secret data
 *
 * SSKR shares follow a specific binary format that includes a 5-byte metadata
 * header followed by the share value. The metadata encodes information about
 * group thresholds, member thresholds, and the position of this share within
 * the overall structure.
 */
class SSKRShare private constructor(private val data: ByteArray) :
    CborTaggedCodable,
    URCodable {

    /** Returns a copy of the raw binary data of this share. */
    fun asBytes(): ByteArray = data.copyOf()

    /** The data of this share as a lowercase hexadecimal string. */
    val hex: String get() = data.toHexString()

    /**
     * Returns the unique identifier of the split to which this share belongs.
     *
     * The identifier is a 16-bit value that is the same for all shares in a
     * split and is used to verify that shares belong together when combining
     * them.
     */
    fun identifier(): Int =
        ((data[0].toInt() and 0xFF) shl 8) or (data[1].toInt() and 0xFF)

    /** Returns the unique identifier of the split as a hexadecimal string. */
    fun identifierHex(): String = data.copyOfRange(0, 2).toHexString()

    /**
     * Returns the minimum number of groups whose quorum must be met to
     * reconstruct the secret.
     *
     * This value is encoded as GroupThreshold - 1 in the metadata, so the
     * actual threshold value is one more than the encoded value.
     */
    fun groupThreshold(): Int = ((data[2].toInt() and 0xFF) ushr 4) + 1

    /**
     * Returns the total number of groups in the split.
     *
     * This value is encoded as GroupCount - 1 in the metadata, so the actual
     * count is one more than the encoded value.
     */
    fun groupCount(): Int = (data[2].toInt() and 0x0F) + 1

    /**
     * Returns the zero-based index of the group to which this share belongs.
     */
    fun groupIndex(): Int = (data[3].toInt() and 0xFF) ushr 4

    /**
     * Returns the minimum number of shares within the group to which this
     * share belongs that must be combined to meet the group threshold.
     *
     * This value is encoded as MemberThreshold - 1 in the metadata, so the
     * actual threshold value is one more than the encoded value.
     */
    fun memberThreshold(): Int = (data[3].toInt() and 0x0F) + 1

    /**
     * Returns the zero-based index of this share within the group to which
     * it belongs.
     */
    fun memberIndex(): Int = data[4].toInt() and 0x0F

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is SSKRShare) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString --

    override fun toString(): String = "SSKRShare(${identifierHex()})"

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_SSKR_SHARE, TAG_SSKR_SHARE_V1))

    override fun untaggedCbor(): Cbor = Cbor.fromByteString(data)

    // -- Companion --

    companion object {
        /**
         * Creates a new [SSKRShare] from raw binary data.
         *
         * @param data the raw binary data of the SSKR share, including both
         *   metadata (5 bytes) and share value
         * @return a new [SSKRShare] instance containing a copy of the data
         */
        fun fromData(data: ByteArray): SSKRShare = SSKRShare(data.copyOf())

        /**
         * Creates a new [SSKRShare] from a hexadecimal string.
         *
         * @param hex a hexadecimal string representing the SSKR share data
         * @return a new [SSKRShare] instance
         */
        fun fromHex(hex: String): SSKRShare = fromData(hex.hexToByteArray())

        /** Decodes an [SSKRShare] from untagged CBOR (a byte string). */
        fun fromUntaggedCbor(cbor: Cbor): SSKRShare {
            val bytes = cbor.tryByteStringData()
            return SSKRShare(bytes)
        }

        /** Decodes an [SSKRShare] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): SSKRShare =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_SSKR_SHARE, TAG_SSKR_SHARE_V1)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [SSKRShare] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): SSKRShare =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_SSKR_SHARE, TAG_SSKR_SHARE_V1)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [SSKRShare] from a UR. */
        fun fromUr(ur: UR): SSKRShare {
            ur.checkType("sskr")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [SSKRShare] from a UR string. */
        fun fromUrString(urString: String): SSKRShare =
            fromUr(UR.fromUrString(urString))
    }
}

// -- Re-exported types from the sskr library --

/** Re-export of [com.blockchaincommons.sskr.Spec] for use as SSKRSpec. */
typealias SSKRSpec = com.blockchaincommons.sskr.Spec

/** Re-export of [com.blockchaincommons.sskr.Secret] for use as SSKRSecret. */
typealias SSKRSecret = com.blockchaincommons.sskr.Secret

/** Re-export of [com.blockchaincommons.sskr.GroupSpec] for use as SSKRGroupSpec. */
typealias SSKRGroupSpec = com.blockchaincommons.sskr.GroupSpec

// -- Top-level SSKR functions --

/**
 * Generates SSKR shares for the given [spec] and [masterSecret].
 *
 * Uses [SecureRandomNumberGenerator] for share generation.
 *
 * @param spec the split specification defining groups and thresholds
 * @param masterSecret the secret to split into shares
 * @return a list of groups, each containing [SSKRShare] instances
 * @throws com.blockchaincommons.sskr.SskrException if the spec or secret is
 *   invalid
 */
fun sskrGenerate(spec: SSKRSpec, masterSecret: SSKRSecret): List<List<SSKRShare>> {
    val rng = SecureRandomNumberGenerator()
    return sskrGenerateUsing(spec, masterSecret, rng)
}

/**
 * Generates SSKR shares using a custom random number generator.
 *
 * @param spec the split specification defining groups and thresholds
 * @param masterSecret the secret to split into shares
 * @param rng the random number generator to use for share generation
 * @return a list of groups, each containing [SSKRShare] instances
 * @throws com.blockchaincommons.sskr.SskrException if the spec or secret is
 *   invalid
 */
fun sskrGenerateUsing(
    spec: SSKRSpec,
    masterSecret: SSKRSecret,
    rng: RandomNumberGenerator,
): List<List<SSKRShare>> {
    val shares = com.blockchaincommons.sskr.sskrGenerateUsing(spec, masterSecret, rng)
    return shares.map { group -> group.map { SSKRShare.fromData(it) } }
}

/**
 * Combines SSKR shares to reconstruct the original secret.
 *
 * The shares must meet the group and member thresholds specified when the
 * shares were generated.
 *
 * @param shares the shares to combine
 * @return the reconstructed [SSKRSecret]
 * @throws com.blockchaincommons.sskr.SskrException if the shares cannot be
 *   combined
 */
fun sskrCombine(shares: List<SSKRShare>): SSKRSecret {
    val shareData = shares.map { it.asBytes() }
    return com.blockchaincommons.sskr.sskrCombine(shareData)
}
