package com.blockchaincommons.bccomponents

/**
 * Base interface for all elliptic curve keys on the secp256k1 curve.
 *
 * Provides common functionality for EC private and public keys, including
 * access to the underlying key data, hexadecimal conversion, and
 * factory construction from binary or hex-encoded data.
 *
 * All EC key types have a fixed size depending on their specific type:
 * - EC private keys: 32 bytes
 * - EC compressed public keys: 33 bytes
 * - EC uncompressed public keys: 65 bytes
 * - Schnorr public keys: 32 bytes
 */
interface ECKeyBase {
    /** Returns the key's binary data as a byte array copy. */
    fun data(): ByteArray

    /** The key as a lowercase hexadecimal string. */
    val hex: String get() = data().toHexString()
}

/**
 * An EC key that can derive the corresponding compressed public key.
 *
 * Implemented by [ECPrivateKey] (generates the public key from the secret),
 * [ECPublicKey] (returns itself), and [ECUncompressedPublicKey] (compresses).
 */
interface ECKey : ECKeyBase {
    /** Returns the compressed public key corresponding to this key. */
    fun publicKey(): ECPublicKey
}

/**
 * An EC public key that can provide its uncompressed form.
 *
 * EC public keys can be represented in compressed (33 bytes) or
 * uncompressed (65 bytes) format. This interface exposes the conversion.
 */
interface ECPublicKeyBase : ECKey {
    /** Returns the uncompressed public key representation. */
    fun uncompressedPublicKey(): ECUncompressedPublicKey
}
