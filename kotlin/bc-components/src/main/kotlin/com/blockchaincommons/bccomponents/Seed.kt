package com.blockchaincommons.bccomponents

import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator
import com.blockchaincommons.bctags.TAG_SEED
import com.blockchaincommons.bctags.TAG_SEED_V1
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborDate
import com.blockchaincommons.dcbor.CborMap
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A cryptographic seed for deterministic key generation.
 *
 * A [Seed] is a source of entropy used to generate cryptographic keys in a
 * deterministic manner. It includes the random seed data along with optional
 * metadata: a name, a note, and a creation date.
 *
 * The minimum seed length is [MIN_SEED_LENGTH] bytes to ensure sufficient
 * security and entropy.
 *
 * CBOR map format:
 * - `1`: seed data (required)
 * - `2`: creation date (optional)
 * - `3`: name (optional, omitted if empty)
 * - `4`: note (optional, omitted if empty)
 */
class Seed private constructor(
    private val data: ByteArray,
    // Mutable to match the Rust API which provides set_name/set_note/set_creation_date.
    /** The human-readable name of the seed. */
    var name: String,
    /** Additional notes about the seed. */
    var note: String,
    /** The creation date of the seed. */
    var creationDate: CborDate?,
) : PrivateKeyDataProvider,
    CborTaggedCodable,
    URCodable {

    /** Returns the seed data as a new [ByteArray]. */
    fun toByteArray(): ByteArray = data.copyOf()

    // -- PrivateKeyDataProvider --

    override fun privateKeyData(): ByteArray = data.copyOf()

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Seed) return false
        return data.contentEquals(other.data) &&
            name == other.name &&
            note == other.note &&
            creationDate == other.creationDate
    }

    override fun hashCode(): Int {
        var result = data.contentHashCode()
        result = 31 * result + name.hashCode()
        result = 31 * result + note.hashCode()
        result = 31 * result + (creationDate?.hashCode() ?: 0)
        return result
    }

    // -- toString --

    override fun toString(): String = "Seed(${data.size} bytes)"

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_SEED, TAG_SEED_V1))

    override fun untaggedCbor(): Cbor {
        val map = CborMap()
        map.insert(Cbor.fromInt(1), Cbor.fromByteString(data))
        creationDate?.let { map.insert(Cbor.fromInt(2), it.taggedCbor()) }
        if (name.isNotEmpty()) {
            map.insert(Cbor.fromInt(3), Cbor.fromString(name))
        }
        if (note.isNotEmpty()) {
            map.insert(Cbor.fromInt(4), Cbor.fromString(note))
        }
        return Cbor.fromMap(map)
    }

    // -- Companion --

    companion object {
        const val MIN_SEED_LENGTH: Int = 16

        /**
         * Creates a new random seed with [MIN_SEED_LENGTH] bytes.
         */
        fun create(): Seed = createWithLength(MIN_SEED_LENGTH)

        /**
         * Creates a new random seed with [count] bytes.
         *
         * @throws BcComponentsException.DataTooShort if [count] < [MIN_SEED_LENGTH].
         */
        fun createWithLength(count: Int): Seed {
            val rng = SecureRandomNumberGenerator()
            return createWithLengthUsing(count, rng)
        }

        /**
         * Creates a new random seed with [count] bytes using the given [rng].
         *
         * @throws BcComponentsException.DataTooShort if [count] < [MIN_SEED_LENGTH].
         */
        fun createWithLengthUsing(count: Int, rng: RandomNumberGenerator): Seed {
            val seedData = rng.randomData(count)
            return fromData(seedData)
        }

        /**
         * Creates a new seed from the given data and optional metadata.
         *
         * @throws BcComponentsException.DataTooShort if [data] length < [MIN_SEED_LENGTH].
         */
        fun fromData(
            data: ByteArray,
            name: String? = null,
            note: String? = null,
            creationDate: CborDate? = null,
        ): Seed {
            if (data.size < MIN_SEED_LENGTH) {
                throw BcComponentsException.dataTooShort(
                    "seed",
                    MIN_SEED_LENGTH,
                    data.size,
                )
            }
            return Seed(
                data = data.copyOf(),
                name = name ?: "",
                note = note ?: "",
                creationDate = creationDate,
            )
        }

        /** Decodes a [Seed] from untagged CBOR (a map). */
        fun fromUntaggedCbor(cbor: Cbor): Seed {
            val map = cbor.tryMap()
            val seedData: ByteArray = map.extract<Int, Cbor>(1).tryByteStringData()
            if (seedData.isEmpty()) {
                throw com.blockchaincommons.dcbor.CborException.msg("Seed data is empty")
            }
            val creationDate: CborDate? = try {
                val dateCbor = map.get<Int, Cbor>(2)
                if (dateCbor != null) CborDate.fromTaggedCbor(dateCbor) else null
            } catch (_: Exception) {
                null
            }
            val name: String? = map.get<Int, String>(3)
            val note: String? = map.get<Int, String>(4)
            return fromData(seedData, name, note, creationDate)
        }

        /** Decodes a [Seed] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): Seed =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_SEED, TAG_SEED_V1)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Seed] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): Seed =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_SEED, TAG_SEED_V1)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Seed] from a UR. */
        fun fromUr(ur: UR): Seed {
            ur.checkType("seed")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [Seed] from a UR string. */
        fun fromUrString(urString: String): Seed =
            fromUr(UR.fromUrString(urString))
    }
}
