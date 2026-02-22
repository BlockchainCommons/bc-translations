package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_MLKEM_PUBLIC_KEY
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues
import org.bouncycastle.pqc.crypto.mlkem.MLKEMGenerator
import org.bouncycastle.pqc.crypto.mlkem.MLKEMPublicKeyParameters
import java.security.SecureRandom

/**
 * A public key for the ML-KEM post-quantum key encapsulation mechanism.
 *
 * [MLKEMPublicKey] represents a public key that can be used to encapsulate
 * shared secrets using ML-KEM. It supports multiple security levels:
 *
 * - ML-KEM-512: NIST security level 1 (roughly equivalent to AES-128), 800 bytes
 * - ML-KEM-768: NIST security level 3 (roughly equivalent to AES-192), 1184 bytes
 * - ML-KEM-1024: NIST security level 5 (roughly equivalent to AES-256), 1568 bytes
 */
sealed class MLKEMPublicKey :
    ReferenceProvider,
    CborTaggedCodable,
    URCodable {

    /** The security level of this key. */
    abstract val level: MLKEM

    /** Returns a copy of the raw key bytes. */
    abstract fun data(): ByteArray

    /** The size of this key in bytes. */
    val size: Int get() = level.publicKeySize()

    /** The key as a lowercase hexadecimal string. */
    val hex: String get() = data().toHexString()

    /**
     * Encapsulates a new shared secret using this public key.
     *
     * Generates a random shared secret and encapsulates it, producing a
     * ciphertext that can only be decapsulated by the corresponding private
     * key.
     *
     * @return a pair of (shared secret as [SymmetricKey], [MLKEMCiphertext])
     */
    fun encapsulateNewSharedSecret(): Pair<SymmetricKey, MLKEMCiphertext> {
        val pubParams = MLKEMPublicKeyParameters(level.bcParameters(), data())
        val generator = MLKEMGenerator(SecureRandom())
        val encapsulated = generator.generateEncapsulated(pubParams)
        val sharedSecret = SymmetricKey.fromData(encapsulated.secret)
        val ciphertext = MLKEMCiphertext.fromData(level, encapsulated.encapsulation)
        return sharedSecret to ciphertext
    }

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_MLKEM_PUBLIC_KEY))

    override fun untaggedCbor(): Cbor =
        Cbor.fromArray(listOf(Cbor.fromInt(level.cborValue), Cbor.fromByteString(data())))

    // -- toString --

    override fun toString(): String =
        "${level.name}PublicKey(${refHexShort()})"

    // -- Variants --

    private class MLKEM512Key(private val keyData: ByteArray) : MLKEMPublicKey() {
        override val level: MLKEM = MLKEM.MLKEM512
        override fun data(): ByteArray = keyData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLKEM512Key) return false
            return keyData.contentEquals(other.keyData)
        }
        override fun hashCode(): Int = keyData.contentHashCode()
    }

    private class MLKEM768Key(private val keyData: ByteArray) : MLKEMPublicKey() {
        override val level: MLKEM = MLKEM.MLKEM768
        override fun data(): ByteArray = keyData.copyOf()
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLKEM768Key) return false
            return keyData.contentEquals(other.keyData)
        }
        override fun hashCode(): Int = keyData.contentHashCode()
    }

    private class MLKEM1024Key(private val keyData: ByteArray) : MLKEMPublicKey() {
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
         * Creates an [MLKEMPublicKey] from a security level and raw key bytes.
         *
         * @param level the security level
         * @param data the raw key bytes
         * @return the public key
         * @throws BcComponentsException.InvalidSize if the data length does not
         *   match the expected size for the given level
         */
        fun fromData(level: MLKEM, data: ByteArray): MLKEMPublicKey {
            if (data.size != level.publicKeySize()) {
                throw BcComponentsException.invalidSize(
                    "ML-KEM ${level.name} public key",
                    level.publicKeySize(),
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
         * Creates an [MLKEMPublicKey] from a security level and a hex string.
         */
        fun fromHex(level: MLKEM, hex: String): MLKEMPublicKey =
            fromData(level, hex.hexToByteArray())

        /** Decodes an [MLKEMPublicKey] from untagged CBOR (a two-element array). */
        fun fromUntaggedCbor(cbor: Cbor): MLKEMPublicKey {
            val elements = cbor.tryArray()
            if (elements.size != 2) {
                throw BcComponentsException.invalidData(
                    "MLKEMPublicKey", "must have two elements"
                )
            }
            val level = MLKEM.fromCborValue(elements[0].tryInt())
            val data = elements[1].tryByteStringData()
            return fromData(level, data)
        }

        /** Decodes an [MLKEMPublicKey] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): MLKEMPublicKey =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_MLKEM_PUBLIC_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [MLKEMPublicKey] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): MLKEMPublicKey =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_MLKEM_PUBLIC_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [MLKEMPublicKey] from a UR. */
        fun fromUr(ur: UR): MLKEMPublicKey {
            ur.checkType("mlkem-public-key")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [MLKEMPublicKey] from a UR string. */
        fun fromUrString(urString: String): MLKEMPublicKey =
            fromUr(UR.fromUrString(urString))
    }
}
