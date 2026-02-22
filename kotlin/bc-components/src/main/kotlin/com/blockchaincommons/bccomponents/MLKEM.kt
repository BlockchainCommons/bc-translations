package com.blockchaincommons.bccomponents

import org.bouncycastle.pqc.crypto.mlkem.MLKEMKeyGenerationParameters
import org.bouncycastle.pqc.crypto.mlkem.MLKEMKeyPairGenerator
import org.bouncycastle.pqc.crypto.mlkem.MLKEMParameters
import org.bouncycastle.pqc.crypto.mlkem.MLKEMPrivateKeyParameters
import org.bouncycastle.pqc.crypto.mlkem.MLKEMPublicKeyParameters
import java.security.SecureRandom

/**
 * Security levels for the ML-KEM post-quantum key encapsulation mechanism.
 *
 * ML-KEM (Module Lattice-based Key Encapsulation Mechanism) is a post-quantum
 * key encapsulation mechanism standardized by NIST. It provides resistance
 * against attacks from both classical and quantum computers.
 *
 * Each security level offers different trade-offs between security,
 * performance, and key/ciphertext sizes:
 *
 * - [MLKEM512]: NIST security level 1 (roughly equivalent to AES-128)
 * - [MLKEM768]: NIST security level 3 (roughly equivalent to AES-192)
 * - [MLKEM1024]: NIST security level 5 (roughly equivalent to AES-256)
 *
 * The numeric [cborValue] values (512, 768, 1024) correspond to the parameter
 * sets and are used in CBOR serialization.
 */
enum class MLKEM(val cborValue: Int) {
    /** ML-KEM-512 (NIST security level 1, roughly equivalent to AES-128). */
    MLKEM512(512),

    /** ML-KEM-768 (NIST security level 3, roughly equivalent to AES-192). */
    MLKEM768(768),

    /** ML-KEM-1024 (NIST security level 5, roughly equivalent to AES-256). */
    MLKEM1024(1024);

    /** Returns the size of a private key in bytes for this security level. */
    fun privateKeySize(): Int = when (this) {
        MLKEM512 -> 1632
        MLKEM768 -> 2400
        MLKEM1024 -> 3168
    }

    /** Returns the size of a public key in bytes for this security level. */
    fun publicKeySize(): Int = when (this) {
        MLKEM512 -> 800
        MLKEM768 -> 1184
        MLKEM1024 -> 1568
    }

    /** Returns the size of a ciphertext in bytes for this security level. */
    fun ciphertextSize(): Int = when (this) {
        MLKEM512 -> 768
        MLKEM768 -> 1088
        MLKEM1024 -> 1568
    }

    /**
     * Returns the size of a shared secret in bytes for this security level.
     *
     * This is 32 bytes for all security levels.
     */
    fun sharedSecretSize(): Int = SHARED_SECRET_SIZE

    /**
     * Returns the Bouncy Castle [MLKEMParameters] for this security level.
     */
    internal fun bcParameters(): MLKEMParameters = when (this) {
        MLKEM512 -> MLKEMParameters.ml_kem_512
        MLKEM768 -> MLKEMParameters.ml_kem_768
        MLKEM1024 -> MLKEMParameters.ml_kem_1024
    }

    /**
     * Generates a new ML-KEM keypair with this security level.
     *
     * @return a pair of (private key, public key)
     */
    fun keypair(): Pair<MLKEMPrivateKey, MLKEMPublicKey> {
        val keyGen = MLKEMKeyPairGenerator()
        keyGen.init(MLKEMKeyGenerationParameters(SecureRandom(), bcParameters()))
        val pair = keyGen.generateKeyPair()
        val privParams = pair.private as MLKEMPrivateKeyParameters
        val pubParams = pair.public as MLKEMPublicKeyParameters
        return MLKEMPrivateKey.fromData(this, privParams.encoded) to
            MLKEMPublicKey.fromData(this, pubParams.encoded)
    }

    companion object {
        /** The size of a shared secret in bytes (32 bytes for all levels). */
        const val SHARED_SECRET_SIZE: Int = 32

        /**
         * Creates an [MLKEM] level from a CBOR-encoded value (512, 768, or 1024).
         *
         * @throws BcComponentsException.PostQuantum if the value is not a valid
         *   ML-KEM level
         */
        fun fromCborValue(value: Int): MLKEM =
            entries.find { it.cborValue == value }
                ?: throw BcComponentsException.postQuantum(
                    "Invalid MLKEM level: $value"
                )
    }
}
