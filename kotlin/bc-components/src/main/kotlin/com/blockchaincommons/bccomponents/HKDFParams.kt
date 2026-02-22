package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.hkdfHmacSha256
import com.blockchaincommons.bccrypto.hkdfHmacSha512
import com.blockchaincommons.dcbor.Cbor

/**
 * HKDF-based key derivation parameters.
 *
 * CDDL:
 * ```
 * HKDFParams = [0, Salt, HashType]
 * ```
 *
 * @property salt the salt used for key derivation
 * @property hashType the hash algorithm (SHA-256 or SHA-512)
 */
class HKDFParams(
    val salt: Salt,
    val hashType: HashType = HashType.SHA256,
) : KeyDerivation {

    /** Creates default parameters with a random 16-byte salt and SHA-256. */
    constructor() : this(Salt.createWithLength(SALT_LEN), HashType.SHA256)

    override fun lock(contentKey: SymmetricKey, secret: ByteArray): EncryptedMessage {
        val derivedKey = deriveKey(secret)
        val encodedMethod = toCbor().toCborData()
        return derivedKey.encrypt(contentKey.data(), encodedMethod, null)
    }

    override fun unlock(encryptedMessage: EncryptedMessage, secret: ByteArray): SymmetricKey {
        val derivedKey = deriveKey(secret)
        val decrypted = derivedKey.decrypt(encryptedMessage)
        return SymmetricKey.fromData(decrypted)
    }

    private fun deriveKey(secret: ByteArray): SymmetricKey {
        val derived = when (hashType) {
            HashType.SHA256 -> hkdfHmacSha256(secret, salt.asBytes(), 32)
            HashType.SHA512 -> hkdfHmacSha512(secret, salt.asBytes(), 32)
        }
        return SymmetricKey.fromData(derived)
    }

    /** Encodes these parameters as a CBOR array. */
    fun toCbor(): Cbor = Cbor.fromArray(listOf(
        Cbor.fromInt(KeyDerivationMethod.HKDF.index),
        salt.untaggedCbor(),
        hashType.toCbor(),
    ))

    override fun toString(): String = "HKDF($hashType)"

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is HKDFParams) return false
        return salt == other.salt && hashType == other.hashType
    }

    override fun hashCode(): Int = 31 * salt.hashCode() + hashType.hashCode()

    companion object {
        const val SALT_LEN = 16

        /**
         * Decodes [HKDFParams] from a CBOR array.
         *
         * The expected format is `[0, salt_bytes, hash_type_int]`.
         */
        fun fromCbor(cbor: Cbor): HKDFParams {
            val a = cbor.tryArray()
            if (a.size != 3) {
                throw BcComponentsException.general("Invalid HKDFParams: expected 3 elements, got ${a.size}")
            }
            // a[0] is the method index (already consumed by caller)
            val salt = Salt.fromUntaggedCbor(a[1])
            val hashType = HashType.fromCbor(a[2])
            return HKDFParams(salt, hashType)
        }
    }
}
