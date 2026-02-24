package com.blockchaincommons.provenancemark

import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator
import com.blockchaincommons.dcbor.Cbor

const val PROVENANCE_SEED_LENGTH: Int = 32

class ProvenanceSeed private constructor(private val bytes: ByteArray) {
    init {
        require(bytes.size == PROVENANCE_SEED_LENGTH) {
            "ProvenanceSeed must be $PROVENANCE_SEED_LENGTH bytes"
        }
    }

    fun toBytes(): ByteArray = bytes.copyOf()

    fun hex(): String = bytes.toHex()

    fun toCbor(): Cbor = Cbor.fromByteString(bytes)

    fun toBase64(): String = bytes.toBase64()

    fun toJson(): String = JsonSupport.mapper.writeValueAsString(toBase64())

    override fun toString(): String = hex()

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is ProvenanceSeed) return false
        return bytes.contentEquals(other.bytes)
    }

    override fun hashCode(): Int = bytes.contentHashCode()

    companion object {
        fun new(): ProvenanceSeed {
            val rng = SecureRandomNumberGenerator()
            return newUsing(rng)
        }

        fun newUsing(rng: RandomNumberGenerator): ProvenanceSeed {
            val data = rng.randomData(PROVENANCE_SEED_LENGTH)
            return fromBytes(data)
        }

        fun newWithPassphrase(passphrase: String): ProvenanceSeed {
            return fromBytes(CryptoUtils.extendKey(passphrase.toByteArray(Charsets.UTF_8)))
        }

        fun fromBytes(bytes: ByteArray): ProvenanceSeed {
            if (bytes.size != PROVENANCE_SEED_LENGTH) {
                throw ProvenanceMarkException.InvalidSeedLength(bytes.size)
            }
            return ProvenanceSeed(bytes.copyOf())
        }

        fun fromCbor(cbor: Cbor): ProvenanceSeed {
            return try {
                fromBytes(cbor.tryByteStringData())
            } catch (e: ProvenanceMarkException) {
                throw ProvenanceMarkException.Cbor(e.message ?: "invalid seed cbor")
            } catch (e: Exception) {
                throw ProvenanceMarkException.Cbor(e.message ?: "invalid seed cbor")
            }
        }

        fun fromBase64(value: String): ProvenanceSeed {
            return fromBytes(value.fromBase64())
        }

        fun fromJson(json: String): ProvenanceSeed {
            val value = JsonSupport.mapper.readValue(json, String::class.java)
            return fromBase64(value)
        }
    }
}
