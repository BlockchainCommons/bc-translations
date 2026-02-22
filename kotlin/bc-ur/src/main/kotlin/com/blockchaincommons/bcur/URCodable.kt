package com.blockchaincommons.bcur

import com.blockchaincommons.dcbor.CborTaggedEncodable
import com.blockchaincommons.dcbor.CborTaggedDecodable
import com.blockchaincommons.dcbor.CborTaggedCodable

/**
 * A type that can be encoded as a UR.
 *
 * Extends [CborTaggedEncodable] by providing UR encoding based on
 * the type's CBOR tag name.
 */
interface UREncodable : CborTaggedEncodable {
    /** Returns the UR representation of this object. */
    fun ur(): UR {
        val tag = cborTags().first()
        val name = tag.name
            ?: throw IllegalStateException(
                "CBOR tag ${tag.value} must have a name. Did you call registerTags()?"
            )
        return UR(URType(name), untaggedCbor())
    }

    /** Returns the UR string representation of this object. */
    fun urString(): String = ur().string
}

/**
 * A type that can be decoded from a UR.
 *
 * Extends [CborTaggedDecodable]. Implementations should provide companion
 * factory methods `fromUr(ur)` and `fromUrString(urString)`.
 */
interface URDecodable : CborTaggedDecodable

/**
 * A type that can be both encoded to and decoded from a UR.
 */
interface URCodable : UREncodable, URDecodable
