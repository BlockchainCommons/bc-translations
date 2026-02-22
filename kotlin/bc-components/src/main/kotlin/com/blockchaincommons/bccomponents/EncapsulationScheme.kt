package com.blockchaincommons.bccomponents

import com.blockchaincommons.bcrand.RandomNumberGenerator

/**
 * Supported key encapsulation mechanisms.
 *
 * Key Encapsulation Mechanisms (KEMs) are cryptographic algorithms designed to
 * securely establish a shared secret between parties in public-key
 * cryptography. They are often used to encapsulate (wrap) symmetric keys for
 * secure key exchange.
 *
 * This enum represents the various KEM schemes supported:
 * - [X25519]: A Diffie-Hellman key exchange mechanism using the Curve25519
 *   elliptic curve
 * - [MLKEM512], [MLKEM768], [MLKEM1024]: Module Lattice-based Key
 *   Encapsulation Mechanism at various security levels (post-quantum)
 */
enum class EncapsulationScheme {
    /** X25519 key agreement (default). */
    X25519,

    /** ML-KEM512 post-quantum key encapsulation (NIST level 1). */
    MLKEM512,

    /** ML-KEM768 post-quantum key encapsulation (NIST level 3). */
    MLKEM768,

    /** ML-KEM1024 post-quantum key encapsulation (NIST level 5). */
    MLKEM1024;

    /**
     * Generates a new random key pair for this encapsulation scheme.
     *
     * @return a pair containing the private key and public key for the
     *   selected encapsulation scheme
     */
    fun keypair(): Pair<EncapsulationPrivateKey, EncapsulationPublicKey> {
        return when (this) {
            X25519 -> {
                val (priv, pub) = X25519PrivateKey.keypair()
                EncapsulationPrivateKey.X25519(priv) to EncapsulationPublicKey.X25519(pub)
            }
            MLKEM512 -> {
                val (priv, pub) = MLKEM.MLKEM512.keypair()
                EncapsulationPrivateKey.MLKEM(priv) to EncapsulationPublicKey.MLKEM(pub)
            }
            MLKEM768 -> {
                val (priv, pub) = MLKEM.MLKEM768.keypair()
                EncapsulationPrivateKey.MLKEM(priv) to EncapsulationPublicKey.MLKEM(pub)
            }
            MLKEM1024 -> {
                val (priv, pub) = MLKEM.MLKEM1024.keypair()
                EncapsulationPrivateKey.MLKEM(priv) to EncapsulationPublicKey.MLKEM(pub)
            }
        }
    }

    /**
     * Generates a deterministic key pair using the provided random number
     * generator.
     *
     * Currently only [X25519] supports deterministic key generation.
     *
     * @param rng the random number generator to use
     * @return a pair containing the private key and public key
     * @throws BcComponentsException.General if deterministic key generation
     *   is not supported for this scheme
     */
    fun keypairUsing(rng: RandomNumberGenerator): Pair<EncapsulationPrivateKey, EncapsulationPublicKey> {
        return when (this) {
            X25519 -> {
                val (priv, pub) = X25519PrivateKey.keypairUsing(rng)
                EncapsulationPrivateKey.X25519(priv) to EncapsulationPublicKey.X25519(pub)
            }
            else -> throw BcComponentsException.general(
                "Deterministic keypair generation not supported for this encapsulation scheme",
            )
        }
    }
}
