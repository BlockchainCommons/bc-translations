package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_MLKEM_CIPHERTEXT
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A ciphertext containing an encapsulated shared secret for ML-KEM.
 *
 * [MLKEMCiphertext] represents a ciphertext produced by ML-KEM during
 * the encapsulation process. It contains an encapsulated shared secret
 * that can only be recovered by the corresponding [MLKEMPrivateKey].
 *
 * It supports multiple security levels:
 *
 * - ML-KEM-512: NIST security level 1 (roughly equivalent to AES-128), 768 bytes
 * - ML-KEM-768: NIST security level 3 (roughly equivalent to AES-192), 1088 bytes
 * - ML-KEM-1024: NIST security level 5 (roughly equivalent to AES-256), 1568 bytes
 */
sealed class MLKEMCiphertext :
    CborTaggedCodable,
    URCodable {

    /** The security level of this ciphertext. */
    abstract val level: MLKEM

    /** Returns a copy of the raw ciphertext bytes. */
    abstract fun data(): ByteArray

    /** The size of this ciphertext in bytes. */
    val size: Int get() = level.ciphertextSize()

    /** The ciphertext as a lowercase hexadecimal string. */
    val hex: String get() = data().toHexString()

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_MLKEM_CIPHERTEXT))

    override fun untaggedCbor(): Cbor =
        Cbor.fromArray(listOf(Cbor.fromInt(level.level), Cbor.fromByteString(data())))

    // -- toString --

    override fun toString(): String =
        "${level.name}Ciphertext"

    // -- Variants --

    private class MLKEM512CT(private val ctData: ByteArray) : MLKEMCiphertext() {
        override val level: MLKEM = MLKEM.MLKEM512
        override fun data(): ByteArray = ctData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLKEM512CT) return false
            return ctData.contentEquals(other.ctData)
        }
        override fun hashCode(): Int = ctData.contentHashCode()
    }

    private class MLKEM768CT(private val ctData: ByteArray) : MLKEMCiphertext() {
        override val level: MLKEM = MLKEM.MLKEM768
        override fun data(): ByteArray = ctData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLKEM768CT) return false
            return ctData.contentEquals(other.ctData)
        }
        override fun hashCode(): Int = ctData.contentHashCode()
    }

    private class MLKEM1024CT(private val ctData: ByteArray) : MLKEMCiphertext() {
        override val level: MLKEM = MLKEM.MLKEM1024
        override fun data(): ByteArray = ctData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLKEM1024CT) return false
            return ctData.contentEquals(other.ctData)
        }
        override fun hashCode(): Int = ctData.contentHashCode()
    }

    companion object {
        /**
         * Creates an [MLKEMCiphertext] from a security level and raw ciphertext
         * bytes.
         *
         * @param level the security level
         * @param data the raw ciphertext bytes
         * @return the ciphertext
         * @throws BcComponentsException.InvalidSize if the data length does not
         *   match the expected size for the given level
         */
        fun fromData(level: MLKEM, data: ByteArray): MLKEMCiphertext {
            if (data.size != level.ciphertextSize()) {
                throw BcComponentsException.invalidSize(
                    "ML-KEM ${level.name} ciphertext",
                    level.ciphertextSize(),
                    data.size,
                )
            }
            val copy = data.copyOf()
            return when (level) {
                MLKEM.MLKEM512 -> MLKEM512CT(copy)
                MLKEM.MLKEM768 -> MLKEM768CT(copy)
                MLKEM.MLKEM1024 -> MLKEM1024CT(copy)
            }
        }

        /**
         * Creates an [MLKEMCiphertext] from a security level and a hex string.
         */
        fun fromHex(level: MLKEM, hex: String): MLKEMCiphertext =
            fromData(level, hex.hexToByteArray())

        /** Decodes an [MLKEMCiphertext] from untagged CBOR (a two-element array). */
        fun fromUntaggedCbor(cbor: Cbor): MLKEMCiphertext {
            val elements = cbor.tryArray()
            if (elements.size != 2) {
                throw BcComponentsException.invalidData(
                    "MLKEMCiphertext", "must have two elements"
                )
            }
            val level = MLKEM.fromCborValue(elements[0].tryInt())
            val data = elements[1].tryByteStringData()
            return fromData(level, data)
        }

        /** Decodes an [MLKEMCiphertext] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): MLKEMCiphertext =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_MLKEM_CIPHERTEXT)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [MLKEMCiphertext] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): MLKEMCiphertext =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_MLKEM_CIPHERTEXT)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [MLKEMCiphertext] from a UR. */
        fun fromUr(ur: UR): MLKEMCiphertext {
            ur.checkType("mlkem-ciphertext")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [MLKEMCiphertext] from a UR string. */
        fun fromUrString(urString: String): MLKEMCiphertext =
            fromUr(UR.fromUrString(urString))
    }
}
