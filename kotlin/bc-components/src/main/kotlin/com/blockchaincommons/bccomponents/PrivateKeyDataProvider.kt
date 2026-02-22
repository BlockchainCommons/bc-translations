package com.blockchaincommons.bccomponents

/**
 * A type that can provide unique data for cryptographic key derivation.
 *
 * Types implementing [PrivateKeyDataProvider] can be used as seed material
 * for cryptographic key derivation. The provided data should be sufficiently
 * random and unpredictable to ensure the security of the derived keys.
 *
 * Use cases include deterministic key generation, key recovery mechanisms,
 * key derivation hierarchies, and hierarchical deterministic wallets.
 */
interface PrivateKeyDataProvider {
    /**
     * Returns unique data from which cryptographic keys can be derived.
     *
     * The returned data should have sufficient entropy to serve as the basis
     * for secure cryptographic key derivation.
     */
    fun privateKeyData(): ByteArray
}
