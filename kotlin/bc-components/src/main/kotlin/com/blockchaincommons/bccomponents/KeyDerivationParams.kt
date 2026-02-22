package com.blockchaincommons.bccomponents

import com.blockchaincommons.dcbor.Cbor

/**
 * Sealed class wrapping the different key derivation parameter types.
 *
 * Each variant holds the parameters for a specific key derivation method.
 * This type is used internally by [EncryptedKey] to dispatch lock/unlock
 * operations to the appropriate implementation.
 */
sealed class KeyDerivationParams {

    /** HKDF-based key derivation. */
    data class HKDF(val params: HKDFParams) : KeyDerivationParams()

    /** PBKDF2-based key derivation. */
    data class PBKDF2(val params: PBKDF2Params) : KeyDerivationParams()

    /** Scrypt-based key derivation. */
    data class Scrypt(val params: ScryptParams) : KeyDerivationParams()

    /** Argon2id-based key derivation. */
    data class Argon2id(val params: Argon2idParams) : KeyDerivationParams()

    /** Returns the [KeyDerivationMethod] associated with these parameters. */
    fun method(): KeyDerivationMethod = when (this) {
        is HKDF -> KeyDerivationMethod.HKDF
        is PBKDF2 -> KeyDerivationMethod.PBKDF2
        is Scrypt -> KeyDerivationMethod.Scrypt
        is Argon2id -> KeyDerivationMethod.Argon2id
    }

    /** Returns `true` if the derivation method is password-based. */
    fun isPasswordBased(): Boolean = when (this) {
        is PBKDF2, is Scrypt, is Argon2id -> true
        else -> false
    }

    /**
     * Derives a key from [secret] and encrypts [contentKey] with it.
     *
     * The CBOR-encoded derivation parameters are stored in the AAD of the
     * returned [EncryptedMessage].
     */
    fun lock(contentKey: SymmetricKey, secret: ByteArray): EncryptedMessage = when (this) {
        is HKDF -> params.lock(contentKey, secret)
        is PBKDF2 -> params.lock(contentKey, secret)
        is Scrypt -> params.lock(contentKey, secret)
        is Argon2id -> params.lock(contentKey, secret)
    }

    /** Encodes these parameters as CBOR. */
    fun toCbor(): Cbor = when (this) {
        is HKDF -> params.toCbor()
        is PBKDF2 -> params.toCbor()
        is Scrypt -> params.toCbor()
        is Argon2id -> params.toCbor()
    }

    override fun toString(): String = when (this) {
        is HKDF -> params.toString()
        is PBKDF2 -> params.toString()
        is Scrypt -> params.toString()
        is Argon2id -> params.toString()
    }

    companion object {
        /**
         * Decodes [KeyDerivationParams] from a CBOR array.
         *
         * The first element of the array is the method index, which
         * determines the concrete parameter type.
         */
        fun fromCbor(cbor: Cbor): KeyDerivationParams {
            val a = cbor.tryArray()
            if (a.isEmpty()) {
                throw BcComponentsException.general("Empty KeyDerivationParams array")
            }
            val index = a[0].tryInt()
            val method = KeyDerivationMethod.fromIndex(index)
                ?: throw BcComponentsException.general("Invalid KeyDerivationMethod index: $index")
            return when (method) {
                KeyDerivationMethod.HKDF -> HKDF(HKDFParams.fromCbor(cbor))
                KeyDerivationMethod.PBKDF2 -> PBKDF2(PBKDF2Params.fromCbor(cbor))
                KeyDerivationMethod.Scrypt -> Scrypt(ScryptParams.fromCbor(cbor))
                KeyDerivationMethod.Argon2id -> Argon2id(Argon2idParams.fromCbor(cbor))
            }
        }
    }
}
