package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.scrypt
import com.blockchaincommons.dcbor.Cbor

/**
 * Scrypt-based key derivation parameters.
 *
 * CDDL:
 * ```
 * ScryptParams = [2, Salt, log_n: uint, r: uint, p: uint]
 * ```
 *
 * @property salt the salt used for key derivation
 * @property logN the log2 of the CPU/memory cost parameter
 * @property r the block size parameter
 * @property p the parallelisation parameter
 */
class ScryptParams(
    val salt: Salt,
    val logN: Int = DEFAULT_LOG_N,
    val r: Int = DEFAULT_R,
    val p: Int = DEFAULT_P,
) : KeyDerivation {

    /** Creates default parameters with a random 16-byte salt and standard cost factors. */
    constructor() : this(Salt.createWithLength(SALT_LEN), DEFAULT_LOG_N, DEFAULT_R, DEFAULT_P)

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
        val derived = scrypt(secret, salt.asBytes(), 32, logN, r, p)
        return SymmetricKey.fromData(derived)
    }

    /** Encodes these parameters as a CBOR array. */
    fun toCbor(): Cbor = Cbor.fromArray(listOf(
        Cbor.fromInt(KeyDerivationMethod.Scrypt.index),
        salt.untaggedCbor(),
        Cbor.fromInt(logN),
        Cbor.fromInt(r),
        Cbor.fromInt(p),
    ))

    override fun toString(): String = "Scrypt"

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is ScryptParams) return false
        return salt == other.salt &&
            logN == other.logN &&
            r == other.r &&
            p == other.p
    }

    override fun hashCode(): Int {
        var result = salt.hashCode()
        result = 31 * result + logN
        result = 31 * result + r
        result = 31 * result + p
        return result
    }

    companion object {
        const val SALT_LEN = 16
        const val DEFAULT_LOG_N = 15
        const val DEFAULT_R = 8
        const val DEFAULT_P = 1

        /**
         * Decodes [ScryptParams] from a CBOR array.
         *
         * The expected format is `[2, salt_bytes, log_n, r, p]`.
         */
        fun fromCbor(cbor: Cbor): ScryptParams {
            val a = cbor.tryArray()
            if (a.size != 5) {
                throw BcComponentsException.general("Invalid ScryptParams: expected 5 elements, got ${a.size}")
            }
            // a[0] is the method index (already consumed by caller)
            val salt = Salt.fromUntaggedCbor(a[1])
            val logN = a[2].tryInt()
            val r = a[3].tryInt()
            val p = a[4].tryInt()
            return ScryptParams(salt, logN, r, p)
        }
    }
}
