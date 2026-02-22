package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.pbkdf2HmacSha256
import com.blockchaincommons.bccrypto.pbkdf2HmacSha512
import com.blockchaincommons.dcbor.Cbor

/**
 * PBKDF2-based key derivation parameters.
 *
 * CDDL:
 * ```
 * PBKDF2Params = [1, Salt, iterations: uint, HashType]
 * ```
 *
 * @property salt the salt used for key derivation
 * @property iterations the number of PBKDF2 iterations
 * @property hashType the hash algorithm (SHA-256 or SHA-512)
 */
class PBKDF2Params(
    val salt: Salt,
    val iterations: Int = DEFAULT_ITERATIONS,
    val hashType: HashType = HashType.SHA256,
) : KeyDerivation {

    /** Creates default parameters with a random 16-byte salt, 100 000 iterations, and SHA-256. */
    constructor() : this(Salt.createWithLength(SALT_LEN), DEFAULT_ITERATIONS, HashType.SHA256)

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
            HashType.SHA256 -> pbkdf2HmacSha256(secret, salt.asBytes(), iterations, 32)
            HashType.SHA512 -> pbkdf2HmacSha512(secret, salt.asBytes(), iterations, 32)
        }
        return SymmetricKey.fromData(derived)
    }

    /** Encodes these parameters as a CBOR array. */
    fun toCbor(): Cbor = Cbor.fromArray(listOf(
        Cbor.fromInt(KeyDerivationMethod.PBKDF2.index),
        salt.untaggedCbor(),
        Cbor.fromInt(iterations),
        hashType.toCbor(),
    ))

    override fun toString(): String = "PBKDF2($hashType)"

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is PBKDF2Params) return false
        return salt == other.salt &&
            iterations == other.iterations &&
            hashType == other.hashType
    }

    override fun hashCode(): Int {
        var result = salt.hashCode()
        result = 31 * result + iterations
        result = 31 * result + hashType.hashCode()
        return result
    }

    companion object {
        const val SALT_LEN = 16
        const val DEFAULT_ITERATIONS = 100_000

        /**
         * Decodes [PBKDF2Params] from a CBOR array.
         *
         * The expected format is `[1, salt_bytes, iterations, hash_type_int]`.
         */
        fun fromCbor(cbor: Cbor): PBKDF2Params {
            val a = cbor.tryArray()
            if (a.size != 4) {
                throw BcComponentsException.general("Invalid PBKDF2Params: expected 4 elements, got ${a.size}")
            }
            // a[0] is the method index (already consumed by caller)
            val salt = Salt.fromUntaggedCbor(a[1])
            val iterations = a[2].tryInt()
            val hashType = HashType.fromCbor(a[3])
            return PBKDF2Params(salt, iterations, hashType)
        }
    }
}
