package com.blockchaincommons.bcur

import com.blockchaincommons.dcbor.CborException

/** Exception hierarchy for UR operations. */
sealed class URException(message: String, cause: Throwable? = null) : Exception(message, cause) {
    class DecoderError(message: String) : URException("UR decoder error ($message)")
    class BytewordsError(message: String) : URException("Bytewords error ($message)")
    class CborError(val cborException: CborException) :
        URException("CBOR error (${cborException.message})", cborException)
    class InvalidScheme : URException("invalid UR scheme")
    class TypeUnspecified : URException("no UR type specified")
    class InvalidType : URException("invalid UR type")
    class NotSinglePart : URException("UR is not a single-part")
    class UnexpectedType(val expected: String, val found: String) :
        URException("expected UR type $expected, but found $found")
}
