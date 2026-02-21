package com.blockchaincommons.dcbor

/**
 * A type that can be encoded to CBOR.
 */
interface CborEncodable {
    fun toCbor(): Cbor
    fun toCborData(): ByteArray = toCbor().toCborData()
}

/**
 * A type that can be decoded from CBOR.
 */
interface CborDecodable<T> {
    fun fromCbor(cbor: Cbor): T
}

/**
 * Marker interface for types that are both encodable and decodable.
 */
interface CborCodable<T> : CborEncodable, CborDecodable<T>
