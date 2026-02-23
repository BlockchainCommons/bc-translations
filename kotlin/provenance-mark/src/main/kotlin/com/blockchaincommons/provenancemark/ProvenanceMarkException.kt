package com.blockchaincommons.provenancemark

sealed class ProvenanceMarkException(message: String, cause: Throwable? = null) : Exception(message, cause) {
    class InvalidSeedLength(actual: Int) :
        ProvenanceMarkException("invalid seed length: expected 32 bytes, got $actual bytes")

    class DuplicateKey(key: String) :
        ProvenanceMarkException("duplicate key: $key")

    class MissingKey(key: String) :
        ProvenanceMarkException("missing key: $key")

    class InvalidKey(key: String) :
        ProvenanceMarkException("invalid key: $key")

    class ExtraKeys(expected: Int, actual: Int) :
        ProvenanceMarkException("wrong number of keys: expected $expected, got $actual")

    class InvalidKeyLength(expected: Int, actual: Int) :
        ProvenanceMarkException("invalid key length: expected $expected, got $actual")

    class InvalidNextKeyLength(expected: Int, actual: Int) :
        ProvenanceMarkException("invalid next key length: expected $expected, got $actual")

    class InvalidChainIdLength(expected: Int, actual: Int) :
        ProvenanceMarkException("invalid chain ID length: expected $expected, got $actual")

    class InvalidMessageLength(expected: Int, actual: Int) :
        ProvenanceMarkException("invalid message length: expected at least $expected, got $actual")

    class InvalidInfoCbor :
        ProvenanceMarkException("invalid CBOR data in info field")

    class DateOutOfRange(details: String) :
        ProvenanceMarkException("date out of range: $details")

    class InvalidDate(details: String) :
        ProvenanceMarkException("invalid date: $details")

    class MissingUrlParameter(parameter: String) :
        ProvenanceMarkException("missing required URL parameter: $parameter")

    class YearOutOfRange(year: Int) :
        ProvenanceMarkException("year out of range for 2-byte serialization: must be between 2023-2150, got $year")

    class InvalidMonthOrDay(year: Int, month: Int, day: Int) :
        ProvenanceMarkException("invalid month ($month) or day ($day) for year $year")

    class ResolutionError(details: String) :
        ProvenanceMarkException("resolution serialization error: $details")

    class Bytewords(details: String) :
        ProvenanceMarkException("bytewords error: $details")

    class Cbor(details: String) :
        ProvenanceMarkException("CBOR error: $details")

    class Url(details: String) :
        ProvenanceMarkException("URL parsing error: $details")

    class Base64(details: String) :
        ProvenanceMarkException("base64 decoding error: $details")

    class Json(details: String) :
        ProvenanceMarkException("JSON error: $details")

    class IntegerConversion(details: String) :
        ProvenanceMarkException("integer conversion error: $details")

    class Envelope(details: String) :
        ProvenanceMarkException("envelope error: $details")

    class Validation(val issue: ValidationIssue) :
        ProvenanceMarkException("validation error: $issue")
}

typealias Result<T> = kotlin.Result<T>
