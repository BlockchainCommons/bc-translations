package com.blockchaincommons.dcbor

/**
 * Comprehensive set of errors for CBOR encoding and decoding, enforcing
 * dCBOR deterministic encoding rules.
 */
sealed class CborException(message: String) : Exception(message) {
    class Underrun : CborException("early end of CBOR data")
    class UnsupportedHeaderValue(val headerValue: UByte) : CborException("unsupported value in CBOR header")
    class NonCanonicalNumeric : CborException("a CBOR numeric value was encoded in non-canonical form")
    class InvalidSimpleValue : CborException("an invalid CBOR simple value was encountered")
    class InvalidString(val utf8Error: String) : CborException("an invalidly-encoded UTF-8 string was encountered in the CBOR ($utf8Error)")
    class NonCanonicalString : CborException("a CBOR string was not encoded in Unicode Canonical Normalization Form C")
    class UnusedData(val count: Int) : CborException("the decoded CBOR had $count extra bytes at the end")
    class MisorderedMapKey : CborException("the decoded CBOR map has keys that are not in canonical order")
    class DuplicateMapKey : CborException("the decoded CBOR map has a duplicate key")
    class MissingMapKey : CborException("missing CBOR map key")
    class OutOfRange : CborException("the CBOR numeric value could not be represented in the specified numeric type")
    class WrongType : CborException("the decoded CBOR value was not the expected type")
    class WrongTag(val expected: Tag, val actual: Tag) : CborException("expected CBOR tag $expected, but got $actual")
    class InvalidUtf8(val error: String) : CborException("invalid UTF-8 string: $error")
    class InvalidDate(val dateString: String) : CborException("invalid ISO 8601 date string: $dateString")
    class Custom(val msg: String) : CborException(msg)

    companion object {
        fun msg(s: String): CborException = Custom(s)
    }
}
