package com.blockchaincommons.bccomponents

/**
 * Supported key derivation methods.
 *
 * CDDL:
 * ```
 * KeyDerivationMethod = HKDF / PBKDF2 / Scrypt / Argon2id
 * HKDF = 0
 * PBKDF2 = 1
 * Scrypt = 2
 * Argon2id = 3
 * ```
 */
enum class KeyDerivationMethod(val index: Int) {
    HKDF(0),
    PBKDF2(1),
    Scrypt(2),
    Argon2id(3);

    companion object {
        /**
         * Returns the [KeyDerivationMethod] for the given zero-based index,
         * or `null` if the index is not recognised.
         */
        fun fromIndex(index: Int): KeyDerivationMethod? =
            entries.firstOrNull { it.index == index }
    }
}
