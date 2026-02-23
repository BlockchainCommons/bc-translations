package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.*
import com.blockchaincommons.knownvalues.HAS_SECRET

/**
 * Secret-based encryption extension for Gordian Envelopes.
 *
 * Provides methods for locking envelopes with password or other
 * secret-derived keys, and for unlocking them.
 */

/** Locks the envelope's subject with a secret using the given key derivation method. */
fun Envelope.lockSubject(
    method: KeyDerivationMethod,
    secret: ByteArray,
): Envelope {
    val contentKey = SymmetricKey.create()
    val encryptedKey = EncryptedKey.lock(method, secret, contentKey)
    return encryptSubject(contentKey)
        .addAssertion(HAS_SECRET, encryptedKey)
}

/** Locks the envelope's subject with a password using the given key derivation method. */
fun Envelope.lockSubject(
    method: KeyDerivationMethod,
    password: String,
): Envelope = lockSubject(method, password.toByteArray(Charsets.UTF_8))

/** Unlocks the envelope's subject with a secret. */
fun Envelope.unlockSubject(secret: ByteArray): Envelope {
    for (assertion in assertionsWithPredicate(HAS_SECRET)) {
        val obj = assertion.asObject() ?: continue
        if (obj.isObscured()) continue
        try {
            val encryptedKey = obj.extractSubject<EncryptedKey>()
            val contentKey = encryptedKey.unlock(secret)
            return decryptSubject(contentKey)
        } catch (_: Exception) {
            // Try next assertion
        }
    }
    throw EnvelopeException.UnknownSecret()
}

/** Unlocks the envelope's subject with a password. */
fun Envelope.unlockSubject(password: String): Envelope =
    unlockSubject(password.toByteArray(Charsets.UTF_8))

/** Returns true if the envelope has a password-based `hasSecret` assertion. */
fun Envelope.isLockedWithPassword(): Boolean =
    assertionsWithPredicate(HAS_SECRET).any { assertion ->
        try {
            val obj = assertion.asObject() ?: return@any false
            val encryptedKey = obj.extractSubject<EncryptedKey>()
            encryptedKey.isPasswordBased()
        } catch (_: Exception) { false }
    }

/** Returns true if the envelope has an SSH agent-based `hasSecret` assertion. */
fun Envelope.isLockedWithSshAgent(): Boolean =
    assertionsWithPredicate(HAS_SECRET).any { assertion ->
        try {
            val obj = assertion.asObject() ?: return@any false
            val encryptedKey = obj.extractSubject<EncryptedKey>()
            encryptedKey.isSshAgent()
        } catch (_: Exception) { false }
    }

/** Adds a `hasSecret` assertion to an already-encrypted envelope. */
fun Envelope.addSecret(
    method: KeyDerivationMethod,
    secret: ByteArray,
    contentKey: SymmetricKey,
): Envelope {
    val encryptedKey = EncryptedKey.lock(method, secret, contentKey)
    return addAssertion(HAS_SECRET, encryptedKey)
}

/** Adds a `hasSecret` assertion using a password. */
fun Envelope.addSecret(
    method: KeyDerivationMethod,
    password: String,
    contentKey: SymmetricKey,
): Envelope = addSecret(method, password.toByteArray(Charsets.UTF_8), contentKey)

/** Convenience: wrap + lock subject. */
fun Envelope.lock(
    method: KeyDerivationMethod,
    secret: ByteArray,
): Envelope = wrap().lockSubject(method, secret)

/** Convenience: wrap + lock subject with password. */
fun Envelope.lock(
    method: KeyDerivationMethod,
    password: String,
): Envelope = lock(method, password.toByteArray(Charsets.UTF_8))

/** Convenience: unlock subject + unwrap. */
fun Envelope.unlock(secret: ByteArray): Envelope =
    unlockSubject(secret).unwrap()

/** Convenience: unlock with password + unwrap. */
fun Envelope.unlock(password: String): Envelope =
    unlock(password.toByteArray(Charsets.UTF_8))
