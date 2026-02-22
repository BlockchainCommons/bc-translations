package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.argon2id
import com.blockchaincommons.dcbor.Cbor

/**
 * Argon2id-based key derivation parameters.
 *
 * CDDL:
 * ```
 * Argon2idParams = [3, Salt]
 * ```
 *
 * @property salt the salt used for key derivation
 */
class Argon2idParams(
    val salt: Salt,
) : KeyDerivation {

    /** Creates default parameters with a random 16-byte salt. */
    constructor() : this(Salt.createWithLength(SALT_LEN))

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
        val derived = argon2id(secret, salt.asBytes(), 32)
        return SymmetricKey.fromData(derived)
    }

    /** Encodes these parameters as a CBOR array. */
    fun toCbor(): Cbor = Cbor.fromArray(listOf(
        Cbor.fromInt(KeyDerivationMethod.Argon2id.index),
        salt.untaggedCbor(),
    ))

    override fun toString(): String = "Argon2id"

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Argon2idParams) return false
        return salt == other.salt
    }

    override fun hashCode(): Int = salt.hashCode()

    companion object {
        const val SALT_LEN = 16

        /**
         * Decodes [Argon2idParams] from a CBOR array.
         *
         * The expected format is `[3, salt_bytes]`.
         */
        fun fromCbor(cbor: Cbor): Argon2idParams {
            val a = cbor.tryArray()
            if (a.size != 2) {
                throw BcComponentsException.general("Invalid Argon2idParams: expected 2 elements, got ${a.size}")
            }
            // a[0] is the method index (already consumed by caller)
            val salt = Salt.fromUntaggedCbor(a[1])
            return Argon2idParams(salt)
        }
    }
}
