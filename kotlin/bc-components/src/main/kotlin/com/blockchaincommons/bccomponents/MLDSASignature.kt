package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_MLDSA_SIGNATURE
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A digital signature created with the ML-DSA post-quantum signature algorithm.
 *
 * [MLDSASignature] represents a digital signature created using ML-DSA.
 * It supports multiple security levels:
 *
 * - ML-DSA-44: NIST security level 2 (roughly equivalent to AES-128)
 * - ML-DSA-65: NIST security level 3 (roughly equivalent to AES-192)
 * - ML-DSA-87: NIST security level 5 (roughly equivalent to AES-256)
 *
 * ML-DSA signatures can be verified using the corresponding [MLDSAPublicKey].
 */
sealed class MLDSASignature :
    CborTaggedCodable,
    URCodable {

    /** The security level of this signature. */
    abstract val level: MLDSA

    /** Returns a copy of the raw signature bytes. */
    abstract fun data(): ByteArray

    /** The size of this signature in bytes. */
    val size: Int get() = level.signatureSize()

    /** The signature as a lowercase hexadecimal string. */
    val hex: String get() = data().toHexString()

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_MLDSA_SIGNATURE))

    override fun untaggedCbor(): Cbor =
        Cbor.fromArray(listOf(Cbor.fromInt(level.cborValue), Cbor.fromByteString(data())))

    // -- toString --

    override fun toString(): String =
        "${level.name}Signature"

    // -- Variants --

    private class MLDSA44Sig(private val sigData: ByteArray) : MLDSASignature() {
        override val level: MLDSA = MLDSA.MLDSA44
        override fun data(): ByteArray = sigData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLDSA44Sig) return false
            return sigData.contentEquals(other.sigData)
        }
        override fun hashCode(): Int = sigData.contentHashCode()
    }

    private class MLDSA65Sig(private val sigData: ByteArray) : MLDSASignature() {
        override val level: MLDSA = MLDSA.MLDSA65
        override fun data(): ByteArray = sigData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLDSA65Sig) return false
            return sigData.contentEquals(other.sigData)
        }
        override fun hashCode(): Int = sigData.contentHashCode()
    }

    private class MLDSA87Sig(private val sigData: ByteArray) : MLDSASignature() {
        override val level: MLDSA = MLDSA.MLDSA87
        override fun data(): ByteArray = sigData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLDSA87Sig) return false
            return sigData.contentEquals(other.sigData)
        }
        override fun hashCode(): Int = sigData.contentHashCode()
    }

    companion object {
        /**
         * Creates an [MLDSASignature] from a security level and raw signature
         * bytes.
         *
         * @param level the security level
         * @param data the raw signature bytes
         * @return the signature
         * @throws BcComponentsException.InvalidSize if the data length does not
         *   match the expected size for the given level
         */
        fun fromData(level: MLDSA, data: ByteArray): MLDSASignature {
            if (data.size != level.signatureSize()) {
                throw BcComponentsException.invalidSize(
                    "ML-DSA ${level.name} signature",
                    level.signatureSize(),
                    data.size,
                )
            }
            val copy = data.copyOf()
            return when (level) {
                MLDSA.MLDSA44 -> MLDSA44Sig(copy)
                MLDSA.MLDSA65 -> MLDSA65Sig(copy)
                MLDSA.MLDSA87 -> MLDSA87Sig(copy)
            }
        }

        /**
         * Creates an [MLDSASignature] from a security level and a hex string.
         */
        fun fromHex(level: MLDSA, hex: String): MLDSASignature =
            fromData(level, hex.hexToByteArray())

        /** Decodes an [MLDSASignature] from untagged CBOR (a two-element array). */
        fun fromUntaggedCbor(cbor: Cbor): MLDSASignature {
            val elements = cbor.tryArray()
            if (elements.size != 2) {
                throw BcComponentsException.invalidData(
                    "MLDSASignature", "must have two elements"
                )
            }
            val level = MLDSA.fromCborValue(elements[0].tryInt())
            val data = elements[1].tryByteStringData()
            return fromData(level, data)
        }

        /** Decodes an [MLDSASignature] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): MLDSASignature =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_MLDSA_SIGNATURE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [MLDSASignature] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): MLDSASignature =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_MLDSA_SIGNATURE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [MLDSASignature] from a UR. */
        fun fromUr(ur: UR): MLDSASignature {
            ur.checkType("mldsa-signature")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [MLDSASignature] from a UR string. */
        fun fromUrString(urString: String): MLDSASignature =
            fromUr(UR.fromUrString(urString))
    }
}
