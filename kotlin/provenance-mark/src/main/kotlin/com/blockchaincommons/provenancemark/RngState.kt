package com.blockchaincommons.provenancemark

import com.blockchaincommons.dcbor.Cbor

const val RNG_STATE_LENGTH: Int = 32

class RngState private constructor(private val bytes: ByteArray) {
    init {
        require(bytes.size == RNG_STATE_LENGTH) { "RngState must be $RNG_STATE_LENGTH bytes" }
    }

    fun toBytes(): ByteArray = bytes.copyOf()

    fun hex(): String = bytes.toHex()

    fun toCbor(): Cbor = Cbor.fromByteString(bytes)

    fun toBase64(): String = bytes.toBase64()

    fun toJson(): String = JsonSupport.mapper.writeValueAsString(toBase64())

    override fun toString(): String = "RngState(${hex()})"

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is RngState) return false
        return bytes.contentEquals(other.bytes)
    }

    override fun hashCode(): Int = bytes.contentHashCode()

    companion object {
        fun fromBytes(bytes: ByteArray): RngState {
            if (bytes.size != RNG_STATE_LENGTH) {
                throw ProvenanceMarkException.Cbor(
                    "invalid RNG state length: expected $RNG_STATE_LENGTH bytes, got ${bytes.size} bytes"
                )
            }
            return RngState(bytes.copyOf())
        }

        fun fromSlice(bytes: ByteArray): RngState = fromBytes(bytes)

        fun fromCbor(cbor: Cbor): RngState {
            return try {
                fromSlice(cbor.tryByteStringData())
            } catch (e: ProvenanceMarkException) {
                throw ProvenanceMarkException.Cbor(e.message ?: "invalid rng-state cbor")
            } catch (e: Exception) {
                throw ProvenanceMarkException.Cbor(e.message ?: "invalid rng-state cbor")
            }
        }

        fun fromBase64(value: String): RngState = fromSlice(value.fromBase64())

        fun fromJson(json: String): RngState {
            val value = JsonSupport.mapper.readValue(json, String::class.java)
            return fromBase64(value)
        }
    }
}
