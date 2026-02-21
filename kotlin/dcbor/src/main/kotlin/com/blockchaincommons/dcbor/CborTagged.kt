package com.blockchaincommons.dcbor

/**
 * A type that has an associated CBOR tag.
 *
 * The [cborTags] method returns a list of tags in order of preference.
 * The first tag is used for encoding. All tags are accepted for decoding,
 * enabling backward compatibility with older tag versions.
 */
interface CborTagged {
    fun cborTags(): List<Tag>
}

/**
 * A type that can be encoded as a tagged CBOR value.
 */
interface CborTaggedEncodable : CborTagged {
    /** Returns the CBOR encoding of this instance without its tag. */
    fun untaggedCbor(): Cbor

    /** Returns the CBOR encoding of this instance with its preferred tag. */
    fun taggedCbor(): Cbor {
        return Cbor(CborCase.Tagged(cborTags()[0], untaggedCbor()))
    }

    /** Returns the tagged CBOR encoding as binary data. */
    fun taggedCborData(): ByteArray = taggedCbor().toCborData()
}

/**
 * A type that can be decoded from a tagged CBOR value.
 */
interface CborTaggedDecodable : CborTagged {
    /**
     * Decode from untagged CBOR (after the tag has been verified and removed).
     * Implementations define how to interpret the CBOR content.
     */
    // Implementations provide companion factory methods; see CborTaggedUtils.
}

/**
 * A type that can be both encoded to and decoded from tagged CBOR.
 *
 * Marker interface for types implementing both [CborTaggedEncodable] and [CborTaggedDecodable].
 */
interface CborTaggedCodable : CborTaggedEncodable, CborTaggedDecodable

/**
 * Utility for decoding tagged CBOR values.
 */
object CborTaggedUtils {
    /**
     * Decode a tagged CBOR value, verifying the tag matches one of the expected tags.
     */
    fun <T> fromTaggedCbor(cbor: Cbor, tags: List<Tag>, decoder: (Cbor) -> T): T {
        val (tag, item) = cbor.tryTagged()
        if (tags.contains(tag)) {
            return decoder(item)
        }
        throw CborException.WrongTag(tags[0], tag)
    }

    /**
     * Decode a tagged CBOR value from binary data.
     */
    fun <T> fromTaggedCborData(data: ByteArray, tags: List<Tag>, decoder: (Cbor) -> T): T {
        return fromTaggedCbor(Cbor.tryFromData(data), tags, decoder)
    }
}
