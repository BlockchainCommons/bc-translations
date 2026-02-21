package com.blockchaincommons.dcbor

/**
 * Sealed class representing all possible CBOR data types (major types 0-7).
 */
sealed class CborCase {
    data class Unsigned(val value: ULong) : CborCase()
    data class Negative(val value: ULong) : CborCase()
    data class CborByteString(val value: ByteString) : CborCase()
    data class Text(val value: String) : CborCase()
    data class Array(val value: List<Cbor>) : CborCase()
    data class CborMap(val value: com.blockchaincommons.dcbor.CborMap) : CborCase()
    data class Tagged(val tag: Tag, val item: Cbor) : CborCase()
    data class CborSimple(val value: Simple) : CborCase()
}
