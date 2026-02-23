@file:OptIn(ExperimentalStdlibApi::class)

package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.Digest
import com.blockchaincommons.bctags.TAG_SEED
import com.blockchaincommons.dcbor.*
import com.blockchaincommons.knownvalues.*

/**
 * A simplified Seed domain object used in tests.
 *
 * This mirrors the test_seed.rs helper in the Rust test suite: a minimal
 * Seed type that supports CBOR-tagged encoding and Envelope round-tripping
 * without pulling in the full bc-components [com.blockchaincommons.bccomponents.Seed]
 * which enforces a minimum seed length.
 */
class TestSeed(
    val data: ByteArray,
    var name: String = "",
    var note: String = "",
    var creationDate: CborDate? = null,
) : CborTaggedEncodable {

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_SEED))

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

    /** Converts this seed to a Gordian Envelope. */
    fun toEnvelope(): Envelope {
        var e = Envelope.from(Cbor.fromByteString(data))
            .addType(SEED_TYPE)
        creationDate?.let { e = e.addAssertion(DATE, it) }
        if (name.isNotEmpty()) {
            e = e.addAssertion(NAME, name)
        }
        if (note.isNotEmpty()) {
            e = e.addAssertion(NOTE, note)
        }
        return e
    }

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is TestSeed) return false
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

    override fun toString(): String = "TestSeed(${data.size} bytes)"

    companion object {
        /** Parses a [TestSeed] from an envelope. */
        fun fromEnvelope(envelope: Envelope): TestSeed {
            envelope.checkTypeValue(SEED_TYPE)
            val data = envelope.subject().tryLeaf().tryByteStringData()
            val name: String = envelope.extractOptionalObjectForPredicate(NAME) ?: ""
            val note: String = envelope.extractOptionalObjectForPredicate(NOTE) ?: ""
            val date: CborDate? = envelope.extractOptionalObjectForPredicate<CborDate>(DATE)
            return TestSeed(data, name, note, date)
        }

        /** Decodes a [TestSeed] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): TestSeed {
            val tags = tagsForValues(listOf(TAG_SEED))
            return CborTaggedUtils.fromTaggedCbor(cbor, tags) { untagged ->
                val map = untagged.tryMap()
                val seedData = map.extract<Int, Cbor>(1).tryByteStringData()
                val creationDate: CborDate? = try {
                    val dateCbor = map.get<Int, Cbor>(2)
                    if (dateCbor != null) CborDate.fromTaggedCbor(dateCbor) else null
                } catch (_: Exception) { null }
                val name: String = map.get<Int, String>(3) ?: ""
                val note: String = map.get<Int, String>(4) ?: ""
                TestSeed(seedData, name, note, creationDate)
            }
        }
    }
}
