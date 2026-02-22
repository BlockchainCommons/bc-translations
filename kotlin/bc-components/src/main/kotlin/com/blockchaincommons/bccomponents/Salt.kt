package com.blockchaincommons.bccomponents

import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator
import com.blockchaincommons.bcrand.nextInClosedRange
import com.blockchaincommons.bctags.TAG_SALT
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues
import kotlin.math.ceil
import kotlin.math.max

/**
 * Random salt used to decorrelate other information.
 *
 * A [Salt] is a variable-length (minimum 8 bytes) random value used in
 * password hashing, key derivation, and other cryptographic contexts to
 * ensure uniqueness and prevent correlation.
 */
class Salt private constructor(private val data: ByteArray) :
    CborTaggedCodable,
    URCodable {

    /** The length of the salt in bytes. */
    val size: Int get() = data.size

    /** Whether the salt data is empty (not recommended). */
    val isEmpty: Boolean get() = data.isEmpty()

    /** Returns the salt bytes as a copy. */
    fun asBytes(): ByteArray = data.copyOf()

    /** The salt as a lowercase hexadecimal string. */
    val hex: String get() = data.toHexString()

    /** Returns the salt bytes as a new [ByteArray]. */
    fun toByteArray(): ByteArray = data.copyOf()

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Salt) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString --

    override fun toString(): String = "Salt(${size})"

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_SALT))

    override fun untaggedCbor(): Cbor = Cbor.fromByteString(data)

    // -- Companion --

    companion object {
        private const val MIN_SALT_SIZE = 8

        /** Creates a salt from the given byte data (no minimum check). */
        fun fromData(data: ByteArray): Salt = Salt(data.copyOf())

        /**
         * Creates a salt of a specific number of random bytes.
         *
         * @throws BcComponentsException.DataTooShort if [count] < 8.
         */
        fun createWithLength(count: Int): Salt {
            val rng = SecureRandomNumberGenerator()
            return createWithLengthUsing(count, rng)
        }

        /**
         * Creates a salt of a specific number of random bytes using the
         * given [rng].
         *
         * @throws BcComponentsException.DataTooShort if [count] < 8.
         */
        fun createWithLengthUsing(count: Int, rng: RandomNumberGenerator): Salt {
            if (count < MIN_SALT_SIZE) {
                throw BcComponentsException.dataTooShort("salt", MIN_SALT_SIZE, count)
            }
            return Salt(rng.randomData(count))
        }

        /**
         * Creates a salt with a random length chosen from the given range.
         *
         * @throws BcComponentsException.DataTooShort if range start < 8.
         */
        fun createInRange(range: IntRange): Salt {
            if (range.first < MIN_SALT_SIZE) {
                throw BcComponentsException.dataTooShort("salt", MIN_SALT_SIZE, range.first)
            }
            val rng = SecureRandomNumberGenerator()
            return createInRangeUsing(range, rng)
        }

        /**
         * Creates a salt with a random length chosen from the given range
         * using the given [rng].
         *
         * @throws BcComponentsException.DataTooShort if range start < 8.
         */
        fun createInRangeUsing(range: IntRange, rng: RandomNumberGenerator): Salt {
            if (range.first < MIN_SALT_SIZE) {
                throw BcComponentsException.dataTooShort("salt", MIN_SALT_SIZE, range.first)
            }
            val count = rng.nextInClosedRange(
                range.first.toLong(),
                range.last.toLong(),
            ).toInt()
            return createWithLengthUsing(count, rng)
        }

        /**
         * Creates a salt with a length generally proportionate to the given
         * data [size].
         */
        fun createForSize(size: Int): Salt {
            val rng = SecureRandomNumberGenerator()
            return createForSizeUsing(size, rng)
        }

        /**
         * Creates a salt with a length generally proportionate to the given
         * data [size] using the given [rng].
         */
        fun createForSizeUsing(size: Int, rng: RandomNumberGenerator): Salt {
            val count = size.toDouble()
            val minSize = max(MIN_SALT_SIZE, ceil(count * 0.05).toInt())
            val maxSize = max(minSize + MIN_SALT_SIZE, ceil(count * 0.25).toInt())
            return createInRangeUsing(minSize..maxSize, rng)
        }

        /**
         * Creates a salt from a hexadecimal string.
         *
         * @throws IllegalArgumentException if the hex string is invalid.
         */
        fun fromHex(hex: String): Salt = fromData(hex.hexToByteArray())

        /** Decodes a [Salt] from untagged CBOR (a byte string). */
        fun fromUntaggedCbor(cbor: Cbor): Salt {
            val bytes = cbor.tryByteStringData()
            return Salt(bytes)
        }

        /** Decodes a [Salt] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): Salt =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_SALT)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Salt] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): Salt =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_SALT)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Salt] from a UR. */
        fun fromUr(ur: UR): Salt {
            ur.checkType("salt")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [Salt] from a UR string. */
        fun fromUrString(urString: String): Salt =
            fromUr(UR.fromUrString(urString))
    }
}
