package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_URI
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A validated Uniform Resource Identifier (URI).
 *
 * A [URI] is a string of characters that unambiguously identifies a particular
 * resource. This implementation validates URIs using [java.net.URI] to ensure
 * conformance to the URI specification.
 *
 * Note: This is the bc-components URI type, not [java.net.URI].
 */
class URI private constructor(
    /** The URI string. */
    val string: String,
) : CborTaggedCodable, URCodable {

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is URI) return false
        return string == other.string
    }

    override fun hashCode(): Int = string.hashCode()

    // -- toString --

    override fun toString(): String = string

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_URI))

    override fun untaggedCbor(): Cbor = Cbor.fromString(string)

    companion object {
        /**
         * Creates a new [URI] from a string, validating it.
         *
         * @throws BcComponentsException.InvalidData if the string is not a
         *   valid URI
         */
        fun fromString(uri: String): URI {
            try {
                val parsed = java.net.URI(uri)
                // A valid URI must have a scheme
                if (parsed.scheme == null) {
                    throw BcComponentsException.invalidData("URI", "invalid URI format: no scheme")
                }
            } catch (e: java.net.URISyntaxException) {
                throw BcComponentsException.invalidData("URI", "invalid URI format: ${e.message}")
            }
            return URI(uri)
        }

        /** Decodes a [URI] from untagged CBOR (a text string). */
        fun fromUntaggedCbor(cbor: Cbor): URI {
            val text = cbor.tryText()
            return fromString(text)
        }

        /** Decodes a [URI] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): URI =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_URI)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [URI] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): URI =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_URI)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [URI] from a UR. */
        fun fromUr(ur: UR): URI {
            ur.checkType("url")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [URI] from a UR string. */
        fun fromUrString(urString: String): URI =
            fromUr(UR.fromUrString(urString))
    }
}
