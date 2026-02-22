package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_MLDSA_PUBLIC_KEY
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues
import org.bouncycastle.pqc.crypto.mldsa.MLDSAPublicKeyParameters
import org.bouncycastle.pqc.crypto.mldsa.MLDSASigner

/**
 * A public key for the ML-DSA post-quantum digital signature algorithm.
 *
 * [MLDSAPublicKey] represents a public key that can be used to verify digital
 * signatures created with ML-DSA. It supports multiple security levels:
 *
 * - ML-DSA-44: NIST security level 2 (roughly equivalent to AES-128)
 * - ML-DSA-65: NIST security level 3 (roughly equivalent to AES-192)
 * - ML-DSA-87: NIST security level 5 (roughly equivalent to AES-256)
 */
sealed class MLDSAPublicKey :
    ReferenceProvider,
    CborTaggedCodable,
    URCodable {

    /** The security level of this key. */
    abstract val level: MLDSA

    /** Returns a copy of the raw key bytes. */
    abstract fun data(): ByteArray

    /** The size of this key in bytes. */
    val size: Int get() = level.publicKeySize()

    /** The key as a lowercase hexadecimal string. */
    val hex: String get() = data().toHexString()

    /**
     * Verifies an ML-DSA signature for a message using this public key.
     *
     * @param signature the signature to verify
     * @param message the message that was signed
     * @return `true` if the signature is valid, `false` otherwise
     * @throws BcComponentsException.LevelMismatch if the signature's security
     *   level does not match this key's security level
     */
    fun verify(signature: MLDSASignature, message: ByteArray): Boolean {
        if (signature.level != level) {
            throw BcComponentsException.levelMismatch()
        }
        val params = MLDSAPublicKeyParameters(level.bcParameters(), data())
        val signer = MLDSASigner()
        signer.init(false, params)
        signer.update(message, 0, message.size)
        return signer.verifySignature(signature.data())
    }

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_MLDSA_PUBLIC_KEY))

    override fun untaggedCbor(): Cbor =
        Cbor.fromArray(listOf(Cbor.fromInt(level.cborValue), Cbor.fromByteString(data())))

    // -- toString --

    override fun toString(): String =
        "${level.name}PublicKey(${refHexShort()})"

    // -- Variants --

    private class MLDSA44Key(private val keyData: ByteArray) : MLDSAPublicKey() {
        override val level: MLDSA = MLDSA.MLDSA44
        override fun data(): ByteArray = keyData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLDSA44Key) return false
            return keyData.contentEquals(other.keyData)
        }
        override fun hashCode(): Int = keyData.contentHashCode()
    }

    private class MLDSA65Key(private val keyData: ByteArray) : MLDSAPublicKey() {
        override val level: MLDSA = MLDSA.MLDSA65
        override fun data(): ByteArray = keyData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLDSA65Key) return false
            return keyData.contentEquals(other.keyData)
        }
        override fun hashCode(): Int = keyData.contentHashCode()
    }

    private class MLDSA87Key(private val keyData: ByteArray) : MLDSAPublicKey() {
        override val level: MLDSA = MLDSA.MLDSA87
        override fun data(): ByteArray = keyData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLDSA87Key) return false
            return keyData.contentEquals(other.keyData)
        }
        override fun hashCode(): Int = keyData.contentHashCode()
    }

    companion object {
        /**
         * Creates an [MLDSAPublicKey] from a security level and raw key bytes.
         *
         * @param level the security level
         * @param data the raw key bytes
         * @return the public key
         * @throws BcComponentsException.InvalidSize if the data length does not
         *   match the expected size for the given level
         */
        fun fromData(level: MLDSA, data: ByteArray): MLDSAPublicKey {
            if (data.size != level.publicKeySize()) {
                throw BcComponentsException.invalidSize(
                    "ML-DSA ${level.name} public key",
                    level.publicKeySize(),
                    data.size,
                )
            }
            val copy = data.copyOf()
            return when (level) {
                MLDSA.MLDSA44 -> MLDSA44Key(copy)
                MLDSA.MLDSA65 -> MLDSA65Key(copy)
                MLDSA.MLDSA87 -> MLDSA87Key(copy)
            }
        }

        /**
         * Creates an [MLDSAPublicKey] from a security level and a hex string.
         */
        fun fromHex(level: MLDSA, hex: String): MLDSAPublicKey =
            fromData(level, hex.hexToByteArray())

        /** Decodes an [MLDSAPublicKey] from untagged CBOR (a two-element array). */
        fun fromUntaggedCbor(cbor: Cbor): MLDSAPublicKey {
            val elements = cbor.tryArray()
            if (elements.size != 2) {
                throw BcComponentsException.invalidData(
                    "MLDSAPublicKey", "must have two elements"
                )
            }
            val level = MLDSA.fromCborValue(elements[0].tryInt())
            val data = elements[1].tryByteStringData()
            return fromData(level, data)
        }

        /** Decodes an [MLDSAPublicKey] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): MLDSAPublicKey =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_MLDSA_PUBLIC_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [MLDSAPublicKey] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): MLDSAPublicKey =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_MLDSA_PUBLIC_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [MLDSAPublicKey] from a UR. */
        fun fromUr(ur: UR): MLDSAPublicKey {
            ur.checkType("mldsa-public-key")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [MLDSAPublicKey] from a UR string. */
        fun fromUrString(urString: String): MLDSAPublicKey =
            fromUr(UR.fromUrString(urString))
    }
}
