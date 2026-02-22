package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith

/**
 * Tests for [URI].
 *
 * Tests cover creation, validation, CBOR roundtrip, and string representation.
 * The Rust `id/uri.rs` module has no inline tests; these tests validate the
 * Kotlin translation against the Rust API surface and expected behavior.
 */
class URITest {

    @Test
    fun testCreation() {
        val uri = URI.fromString("https://example.com")
        assertEquals("https://example.com", uri.string)
        assertEquals("https://example.com", uri.toString())
    }

    @Test
    fun testInvalidUri() {
        assertFailsWith<BcComponentsException> {
            URI.fromString("not a valid uri")
        }
    }

    @Test
    fun testNoScheme() {
        assertFailsWith<BcComponentsException> {
            URI.fromString("example.com")
        }
    }

    @Test
    fun testCborRoundtrip() {
        registerTags()
        val uri = URI.fromString("https://example.com/path?query=value")
        val cbor = uri.taggedCbor()
        val decoded = URI.fromTaggedCbor(cbor)
        assertEquals(uri, decoded)
    }

    @Test
    fun testEquality() {
        val uri1 = URI.fromString("https://example.com")
        val uri2 = URI.fromString("https://example.com")
        assertEquals(uri1, uri2)
    }

    @Test
    fun testVariousSchemes() {
        val httpUri = URI.fromString("http://example.com")
        assertEquals("http://example.com", httpUri.toString())

        val ftpUri = URI.fromString("ftp://files.example.com")
        assertEquals("ftp://files.example.com", ftpUri.toString())

        val mailtoUri = URI.fromString("mailto:user@example.com")
        assertEquals("mailto:user@example.com", mailtoUri.toString())
    }
}
