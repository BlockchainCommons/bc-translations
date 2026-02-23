package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.*
import com.blockchaincommons.dcbor.ByteString
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborDate
import com.blockchaincommons.dcbor.CborMap
import com.blockchaincommons.knownvalues.KnownValue

/**
 * A type that can be encoded as a Gordian Envelope.
 *
 * Implementations define how to convert a value into an [Envelope].
 */
interface EnvelopeEncodable {
    /** Converts this value into a Gordian Envelope. */
    fun toEnvelope(): Envelope
}

// -- Envelope itself is EnvelopeEncodable --

// -- Extension functions to make common types EnvelopeEncodable --

/** Wraps a [String] as a leaf envelope. */
fun String.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromString(this))

/** Wraps an [Int] as a leaf envelope. */
fun Int.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromInt(this))

/** Wraps a [Long] as a leaf envelope. */
fun Long.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromLong(this))

/** Wraps a [UInt] as a leaf envelope. */
fun UInt.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromUInt(this))

/** Wraps a [ULong] as a leaf envelope. */
fun ULong.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromUnsigned(this))

/** Wraps a [UShort] as a leaf envelope. */
fun UShort.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromUShort(this))

/** Wraps a [UByte] as a leaf envelope. */
fun UByte.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromUByte(this))

/** Wraps a [Byte] as a leaf envelope. */
fun Byte.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromInt(this.toInt()))

/** Wraps a [Short] as a leaf envelope. */
fun Short.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromInt(this.toInt()))

/** Wraps a [Boolean] as a leaf envelope. */
fun Boolean.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromBoolean(this))

/** Wraps a [Double] as a leaf envelope. */
fun Double.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromDouble(this))

/** Wraps a [Float] as a leaf envelope. */
fun Float.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromFloat(this))

/** Wraps a [ByteArray] as a leaf envelope (byte string). */
fun ByteArray.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromByteString(this))

/** Wraps a [ByteString] as a leaf envelope. */
fun ByteString.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromByteString(this))

/** Wraps a [Cbor] as a leaf envelope. */
fun Cbor.toEnvelope(): Envelope = Envelope.newLeaf(this)

/** Wraps a [CborDate] as a leaf envelope. */
fun CborDate.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps a [CborMap] as a leaf envelope. */
fun CborMap.toEnvelope(): Envelope = Envelope.newLeaf(Cbor.fromMap(this))

// -- bc-components types --

/** Wraps a [Digest] as a leaf envelope via tagged CBOR. */
fun Digest.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps a [Salt] as a leaf envelope via tagged CBOR. */
fun Salt.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps a [Nonce] as a leaf envelope via tagged CBOR. */
fun Nonce.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps an [ARID] as a leaf envelope via tagged CBOR. */
fun ARID.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps a [URI] as a leaf envelope via tagged CBOR. */
fun URI.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps a [UUID] as a leaf envelope via tagged CBOR. */
fun UUID.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps a [XID] as a leaf envelope via tagged CBOR. */
fun XID.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps a [Reference] as a leaf envelope via tagged CBOR. */
fun Reference.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps a [PublicKeys] as a leaf envelope via tagged CBOR. */
fun PublicKeys.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps a [PrivateKeys] as a leaf envelope via tagged CBOR. */
fun PrivateKeys.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps a [PrivateKeyBase] as a leaf envelope via tagged CBOR. */
fun PrivateKeyBase.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps a [SealedMessage] as a leaf envelope via tagged CBOR. */
fun SealedMessage.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps an [EncryptedKey] as a leaf envelope via tagged CBOR. */
fun EncryptedKey.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps a [Signature] as a leaf envelope via tagged CBOR. */
fun Signature.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

/** Wraps an [SSKRShare] as a leaf envelope via tagged CBOR. */
fun SSKRShare.toEnvelope(): Envelope = Envelope.newLeaf(this.taggedCbor())

// -- Assertion is EnvelopeEncodable --

/** Wraps an [Assertion] as an envelope. */
fun Assertion.toEnvelope(): Envelope = Envelope.newWithAssertion(this)

// -- KnownValue is EnvelopeEncodable --

/** Wraps a [KnownValue] as a known-value envelope. */
fun KnownValue.toEnvelope(): Envelope = Envelope.newWithKnownValue(this)

/**
 * Adapter that wraps any value into an [EnvelopeEncodable].
 *
 * This is used internally to accept various Kotlin types as envelope-encodable
 * values in methods like [Envelope.addAssertion].
 */
internal class EnvelopeEncodableWrapper(private val envelope: Envelope) : EnvelopeEncodable {
    override fun toEnvelope(): Envelope = envelope
}

/**
 * Converts any supported type to an [EnvelopeEncodable].
 *
 * Supports: [Envelope], [EnvelopeEncodable], [String], [Int], [Long], [UInt],
 * [ULong], [Boolean], [Double], [Float], [ByteArray], [Cbor], [Assertion],
 * [KnownValue], and bc-components types.
 */
fun Any.asEnvelopeEncodable(): EnvelopeEncodable = when (this) {
    is EnvelopeEncodable -> this
    is Envelope -> EnvelopeEncodableWrapper(this)
    is String -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Int -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Long -> EnvelopeEncodableWrapper(this.toEnvelope())
    is UInt -> EnvelopeEncodableWrapper(this.toEnvelope())
    is ULong -> EnvelopeEncodableWrapper(this.toEnvelope())
    is UShort -> EnvelopeEncodableWrapper(this.toEnvelope())
    is UByte -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Byte -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Short -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Boolean -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Double -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Float -> EnvelopeEncodableWrapper(this.toEnvelope())
    is ByteArray -> EnvelopeEncodableWrapper(this.toEnvelope())
    is ByteString -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Cbor -> EnvelopeEncodableWrapper(this.toEnvelope())
    is CborDate -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Assertion -> EnvelopeEncodableWrapper(this.toEnvelope())
    is KnownValue -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Digest -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Salt -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Nonce -> EnvelopeEncodableWrapper(this.toEnvelope())
    is ARID -> EnvelopeEncodableWrapper(this.toEnvelope())
    is URI -> EnvelopeEncodableWrapper(this.toEnvelope())
    is UUID -> EnvelopeEncodableWrapper(this.toEnvelope())
    is XID -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Reference -> EnvelopeEncodableWrapper(this.toEnvelope())
    is PublicKeys -> EnvelopeEncodableWrapper(this.toEnvelope())
    is PrivateKeys -> EnvelopeEncodableWrapper(this.toEnvelope())
    is PrivateKeyBase -> EnvelopeEncodableWrapper(this.toEnvelope())
    is SealedMessage -> EnvelopeEncodableWrapper(this.toEnvelope())
    is EncryptedKey -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Signature -> EnvelopeEncodableWrapper(this.toEnvelope())
    is SSKRShare -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Function -> EnvelopeEncodableWrapper(this.toEnvelope())
    is Parameter -> EnvelopeEncodableWrapper(this.toEnvelope())
    else -> throw IllegalArgumentException("Type ${this::class.simpleName} is not EnvelopeEncodable")
}
