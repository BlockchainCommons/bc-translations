package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_MLKEM_PRIVATE_KEY
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues
import org.bouncycastle.pqc.crypto.mlkem.MLKEMExtractor
import org.bouncycastle.pqc.crypto.mlkem.MLKEMPrivateKeyParameters

/**
 * A private key for the ML-KEM post-quantum key encapsulation mechanism.
 *
 * [MLKEMPrivateKey] represents a private key that can be used to decapsulate
 * shared secrets using ML-KEM. It supports multiple security levels:
 *
 * - ML-KEM-512: NIST security level 1 (roughly equivalent to AES-128), 1632 bytes
 * - ML-KEM-768: NIST security level 3 (roughly equivalent to AES-192), 2400 bytes
 * - ML-KEM-1024: NIST security level 5 (roughly equivalent to AES-256), 3168 bytes
 *
 * ML-KEM private keys should be kept secure and never exposed. They provide
 * resistance against attacks from both classical and quantum computers.
 */
sealed class MLKEMPrivateKey :
    ReferenceProvider,
    CborTaggedCodable,
    URCodable {

    /** The security level of this key. */
    abstract val level: MLKEM

    /** Returns a copy of the raw key bytes. */
    abstract fun data(): ByteArray

    /** The size of this key in bytes. */
    val size: Int get() = level.privateKeySize()

    /** The key as a lowercase hexadecimal string. */
    val hex: String get() = data().toHexString()

    /**
     * Decapsulates a shared secret from a ciphertext using this private key.
     *
     * @param ciphertext the ciphertext containing the encapsulated shared secret
     * @return a [SymmetricKey] containing the decapsulated shared secret
     * @throws BcComponentsException.LevelMismatch if the ciphertext's security
     *   level does not match this key's security level
     */
    fun decapsulateSharedSecret(ciphertext: MLKEMCiphertext): SymmetricKey {
        if (ciphertext.level != level) {
            throw BcComponentsException.levelMismatch()
        }
        val privParams = MLKEMPrivateKeyParameters(level.bcParameters(), data())
        val extractor = MLKEMExtractor(privParams)
        val sharedSecret = extractor.extractSecret(ciphertext.data())
        return SymmetricKey.fromData(sharedSecret)
    }

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_MLKEM_PRIVATE_KEY))

    override fun untaggedCbor(): Cbor =
        Cbor.fromArray(listOf(Cbor.fromInt(level.level), Cbor.fromByteString(data())))

    // -- toString --

    override fun toString(): String =
        "${level.name}PrivateKey(${refHexShort()})"

    // -- Variants --

    private class MLKEM512Key(private val keyData: ByteArray) : MLKEMPrivateKey() {
        override val level: MLKEM = MLKEM.MLKEM512
        override fun data(): ByteArray = keyData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLKEM512Key) return false
            return keyData.contentEquals(other.keyData)
        }
        override fun hashCode(): Int = keyData.contentHashCode()
    }

    private class MLKEM768Key(private val keyData: ByteArray) : MLKEMPrivateKey() {
        override val level: MLKEM = MLKEM.MLKEM768
        override fun data(): ByteArray = keyData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLKEM768Key) return false
            return keyData.contentEquals(other.keyData)
        }
        override fun hashCode(): Int = keyData.contentHashCode()
    }

    private class MLKEM1024Key(private val keyData: ByteArray) : MLKEMPrivateKey() {
        override val level: MLKEM = MLKEM.MLKEM1024
        override fun data(): ByteArray = keyData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLKEM1024Key) return false
            return keyData.contentEquals(other.keyData)
        }
        override fun hashCode(): Int = keyData.contentHashCode()
    }

    companion object {
        /**
         * Creates an [MLKEMPrivateKey] from a security level and raw key bytes.
         *
         * @param level the security level
         * @param data the raw key bytes
         * @return the private key
         * @throws BcComponentsException.InvalidSize if the data length does not
         *   match the expected size for the given level
         */
        fun fromData(level: MLKEM, data: ByteArray): MLKEMPrivateKey {
            if (data.size != level.privateKeySize()) {
                throw BcComponentsException.invalidSize(
                    "ML-KEM ${level.name} private key",
                    level.privateKeySize(),
                    data.size,
                )
            }
            val copy = data.copyOf()
            return when (level) {
                MLKEM.MLKEM512 -> MLKEM512Key(copy)
                MLKEM.MLKEM768 -> MLKEM768Key(copy)
                MLKEM.MLKEM1024 -> MLKEM1024Key(copy)
            }
        }

        /**
         * Creates an [MLKEMPrivateKey] from a security level and a hex string.
         */
        fun fromHex(level: MLKEM, hex: String): MLKEMPrivateKey =
            fromData(level, hex.hexToByteArray())

        /** Decodes an [MLKEMPrivateKey] from untagged CBOR (a two-element array). */
        fun fromUntaggedCbor(cbor: Cbor): MLKEMPrivateKey {
            val elements = cbor.tryArray()
            if (elements.size != 2) {
                throw BcComponentsException.invalidData(
                    "MLKEMPrivateKey", "must have two elements"
                )
            }
            val level = MLKEM.fromCborValue(elements[0].tryInt())
            val data = elements[1].tryByteStringData()
            return fromData(level, data)
        }

        /** Decodes an [MLKEMPrivateKey] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): MLKEMPrivateKey =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_MLKEM_PRIVATE_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [MLKEMPrivateKey] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): MLKEMPrivateKey =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_MLKEM_PRIVATE_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [MLKEMPrivateKey] from a UR. */
        fun fromUr(ur: UR): MLKEMPrivateKey {
            ur.checkType("mlkem-private-key")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [MLKEMPrivateKey] from a UR string. */
        fun fromUrString(urString: String): MLKEMPrivateKey =
            fromUr(UR.fromUrString(urString))
    }
}
