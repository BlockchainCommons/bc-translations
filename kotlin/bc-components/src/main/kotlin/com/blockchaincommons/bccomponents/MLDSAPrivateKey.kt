package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_MLDSA_PRIVATE_KEY
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues
import org.bouncycastle.pqc.crypto.mldsa.MLDSAPrivateKeyParameters
import org.bouncycastle.pqc.crypto.mldsa.MLDSASigner

/**
 * A private key for the ML-DSA post-quantum digital signature algorithm.
 *
 * [MLDSAPrivateKey] represents a private key that can be used to create
 * digital signatures using ML-DSA. It supports multiple security levels:
 *
 * - ML-DSA-44: NIST security level 2 (roughly equivalent to AES-128)
 * - ML-DSA-65: NIST security level 3 (roughly equivalent to AES-192)
 * - ML-DSA-87: NIST security level 5 (roughly equivalent to AES-256)
 *
 * ML-DSA private keys should be kept secure and never exposed. They provide
 * resistance against attacks from both classical and quantum computers.
 */
sealed class MLDSAPrivateKey :
    ReferenceProvider,
    CborTaggedCodable,
    URCodable {

    /** The security level of this key. */
    abstract val level: MLDSA

    /** Returns a copy of the raw key bytes. */
    abstract fun data(): ByteArray

    /** The size of this key in bytes. */
    val size: Int get() = level.privateKeySize()

    /** The key as a lowercase hexadecimal string. */
    val hex: String get() = data().toHexString()

    /**
     * Signs a message using this ML-DSA private key.
     *
     * @param message the data to sign
     * @return an [MLDSASignature] for the message
     */
    fun sign(message: ByteArray): MLDSASignature {
        val params = level.privateKeyParamsFromEncoded(data())
        val signer = MLDSASigner()
        signer.init(true, params)
        signer.update(message, 0, message.size)
        val sigBytes = signer.generateSignature()
        return MLDSASignature.fromData(level, sigBytes)
    }

    /**
     * Derives the corresponding [MLDSAPublicKey] from this private key.
     */
    fun publicKey(): MLDSAPublicKey {
        val params = level.privateKeyParamsFromEncoded(data())
        val pubParams = params.publicKeyParameters
        return MLDSAPublicKey.fromData(level, pubParams.encoded)
    }

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_MLDSA_PRIVATE_KEY))

    override fun untaggedCbor(): Cbor =
        Cbor.fromArray(listOf(Cbor.fromInt(level.cborValue), Cbor.fromByteString(data())))

    // -- toString --

    override fun toString(): String =
        "${level.name}PrivateKey(${refHexShort()})"

    // -- Variants --

    private class MLDSA44Key(private val keyData: ByteArray) : MLDSAPrivateKey() {
        override val level: MLDSA = MLDSA.MLDSA44
        override fun data(): ByteArray = keyData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLDSA44Key) return false
            return keyData.contentEquals(other.keyData)
        }
        override fun hashCode(): Int = keyData.contentHashCode()
    }

    private class MLDSA65Key(private val keyData: ByteArray) : MLDSAPrivateKey() {
        override val level: MLDSA = MLDSA.MLDSA65
        override fun data(): ByteArray = keyData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLDSA65Key) return false
            return keyData.contentEquals(other.keyData)
        }
        override fun hashCode(): Int = keyData.contentHashCode()
    }

    private class MLDSA87Key(private val keyData: ByteArray) : MLDSAPrivateKey() {
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
         * Creates an [MLDSAPrivateKey] from a security level and raw key bytes.
         *
         * @param level the security level
         * @param data the raw key bytes
         * @return the private key
         * @throws BcComponentsException.InvalidSize if the data length does not
         *   match the expected size for the given level
         */
        fun fromData(level: MLDSA, data: ByteArray): MLDSAPrivateKey {
            if (data.size != level.privateKeySize()) {
                throw BcComponentsException.invalidSize(
                    "ML-DSA ${level.name} private key",
                    level.privateKeySize(),
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
         * Creates an [MLDSAPrivateKey] from a security level and a hex string.
         */
        fun fromHex(level: MLDSA, hex: String): MLDSAPrivateKey =
            fromData(level, hex.hexToByteArray())

        /** Decodes an [MLDSAPrivateKey] from untagged CBOR (a two-element array). */
        fun fromUntaggedCbor(cbor: Cbor): MLDSAPrivateKey {
            val elements = cbor.tryArray()
            if (elements.size != 2) {
                throw BcComponentsException.invalidData(
                    "MLDSAPrivateKey", "must have two elements"
                )
            }
            val level = MLDSA.fromCborValue(elements[0].tryInt())
            val data = elements[1].tryByteStringData()
            return fromData(level, data)
        }

        /** Decodes an [MLDSAPrivateKey] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): MLDSAPrivateKey =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_MLDSA_PRIVATE_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [MLDSAPrivateKey] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): MLDSAPrivateKey =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_MLDSA_PRIVATE_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [MLDSAPrivateKey] from a UR. */
        fun fromUr(ur: UR): MLDSAPrivateKey {
            ur.checkType("mldsa-private-key")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [MLDSAPrivateKey] from a UR string. */
        fun fromUrString(urString: String): MLDSAPrivateKey =
            fromUr(UR.fromUrString(urString))
    }
}
