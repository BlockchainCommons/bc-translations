package com.blockchaincommons.bccomponents

import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator

/**
 * Supported digital signature schemes.
 *
 * This enum represents the various signature schemes supported in this
 * library, including elliptic curve schemes (ECDSA, Schnorr), Edwards
 * curve schemes (Ed25519), post-quantum schemes (ML-DSA), and
 * SSH-specific algorithms.
 */
enum class SignatureScheme {
    /** BIP-340 Schnorr signature scheme, used in Bitcoin Taproot (default). */
    Schnorr,

    /** ECDSA signature scheme using the secp256k1 curve. */
    ECDSA,

    /** Ed25519 signature scheme (RFC 8032). */
    Ed25519,

    /** ML-DSA44 post-quantum signature scheme (NIST level 2). */
    MLDSA44,

    /** ML-DSA65 post-quantum signature scheme (NIST level 3). */
    MLDSA65,

    /** ML-DSA87 post-quantum signature scheme (NIST level 5). */
    MLDSA87,

    /** Ed25519 signature scheme for SSH. */
    SshEd25519,

    /** DSA signature scheme for SSH. */
    SshDsa,

    /** ECDSA signature scheme with NIST P-256 curve for SSH. */
    SshEcdsaP256,

    /** ECDSA signature scheme with NIST P-384 curve for SSH. */
    SshEcdsaP384;

    /**
     * Creates a new key pair for this signature scheme using the system's
     * secure random number generator.
     *
     * @return a pair of signing private key and its corresponding public key
     * @throws UnsupportedOperationException for SSH variants that are not
     *   yet fully translated
     */
    fun keypair(): Pair<SigningPrivateKey, SigningPublicKey> {
        return when (this) {
            MLDSA44 -> {
                val (privKey, pubKey) = MLDSA.MLDSA44.keypair()
                SigningPrivateKey.MLDSAKey(privKey) to SigningPublicKey.MLDSAKey(pubKey)
            }
            MLDSA65 -> {
                val (privKey, pubKey) = MLDSA.MLDSA65.keypair()
                SigningPrivateKey.MLDSAKey(privKey) to SigningPublicKey.MLDSAKey(pubKey)
            }
            MLDSA87 -> {
                val (privKey, pubKey) = MLDSA.MLDSA87.keypair()
                SigningPrivateKey.MLDSAKey(privKey) to SigningPublicKey.MLDSAKey(pubKey)
            }
            else -> keypairUsing(SecureRandomNumberGenerator(), "")
        }
    }

    /**
     * Creates a key pair for this signature scheme using a provided random
     * number generator with an optional comment.
     *
     * The comment is only used for SSH keys and is ignored for other schemes.
     *
     * @param rng the random number generator to use
     * @param comment an optional string comment for SSH keys
     * @return a pair of signing private key and its corresponding public key
     * @throws UnsupportedOperationException for SSH variants and ML-DSA
     *   variants (which do not support deterministic generation with a
     *   custom RNG)
     */
    fun keypairUsing(
        rng: RandomNumberGenerator,
        comment: String = "",
    ): Pair<SigningPrivateKey, SigningPublicKey> {
        return when (this) {
            Schnorr -> {
                val ecKey = ECPrivateKey.createUsing(rng)
                val privateKey = SigningPrivateKey.SchnorrKey(ecKey)
                val publicKey = privateKey.publicKey()
                privateKey to publicKey
            }
            ECDSA -> {
                val ecKey = ECPrivateKey.createUsing(rng)
                val privateKey = SigningPrivateKey.ECDSAKey(ecKey)
                val publicKey = privateKey.publicKey()
                privateKey to publicKey
            }
            Ed25519 -> {
                val edKey = Ed25519PrivateKey.createUsing(rng)
                val privateKey = SigningPrivateKey.Ed25519Key(edKey)
                val publicKey = privateKey.publicKey()
                privateKey to publicKey
            }
            MLDSA44, MLDSA65, MLDSA87 -> {
                throw UnsupportedOperationException(
                    "Deterministic keypair generation not supported for ML-DSA"
                )
            }
            SshEd25519, SshDsa, SshEcdsaP256, SshEcdsaP384 -> {
                throw UnsupportedOperationException(
                    "SSH key generation not yet supported"
                )
            }
        }
    }
}
