package com.blockchaincommons.bcur

import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.Tag
import kotlin.test.Test
import kotlin.test.assertEquals

/**
 * Tests the URCodable interface.
 *
 * Mirrors the Rust `ur_codable.rs` test which defines a simple `Test` type
 * that implements `CBORTaggedEncodable`/`CBORTaggedDecodable` with tag 24/"leaf",
 * encodes to a UR, and verifies the string and round-trip.
 *
 * In the Rust bc-ur design, `ur()` stores the *untagged* CBOR in the UR.
 * Decoding reads the UR type from the UR type string and reconstructs
 * the object from the untagged CBOR directly via `from_untagged_cbor`.
 */
class URCodableTest {

    /** A simple test type that encodes its string payload as tagged CBOR. */
    private data class TestLeaf(val s: String) : URCodable {
        override fun cborTags(): List<Tag> = listOf(Tag(24, "leaf"))

        override fun untaggedCbor(): Cbor = Cbor.fromString(s)

        companion object {
            fun fromUntaggedCbor(cbor: Cbor): TestLeaf =
                TestLeaf(cbor.tryText())

            fun fromUrString(urString: String): TestLeaf {
                val ur = UR.fromUrString(urString)
                // The UR holds untagged CBOR; decode directly from it.
                return fromUntaggedCbor(ur.cbor)
            }
        }
    }

    @Test
    fun testUrCodable() {
        val test = TestLeaf("test")
        val ur = test.ur()
        val urString = ur.string
        assertEquals("ur:leaf/iejyihjkjygupyltla", urString)

        val test2 = TestLeaf.fromUrString(urString)
        assertEquals(test.s, test2.s)
    }
}
