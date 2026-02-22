package com.blockchaincommons.bccomponents

import org.bouncycastle.pqc.crypto.mldsa.MLDSAKeyGenerationParameters
import org.bouncycastle.pqc.crypto.mldsa.MLDSAKeyPairGenerator
import org.bouncycastle.pqc.crypto.mldsa.MLDSAParameters
import org.bouncycastle.pqc.crypto.mldsa.MLDSAPrivateKeyParameters
import org.bouncycastle.pqc.crypto.mldsa.MLDSAPublicKeyParameters
import java.security.SecureRandom

/**
 * Security levels for the ML-DSA post-quantum digital signature algorithm.
 *
 * ML-DSA (Module Lattice-based Digital Signature Algorithm) is a post-quantum
 * digital signature algorithm standardized by NIST. It provides resistance
 * against attacks from both classical and quantum computers.
 *
 * Each security level offers different trade-offs between security,
 * performance, and key/signature sizes:
 *
 * - [MLDSA44]: NIST security level 2 (roughly equivalent to AES-128)
 * - [MLDSA65]: NIST security level 3 (roughly equivalent to AES-192)
 * - [MLDSA87]: NIST security level 5 (roughly equivalent to AES-256)
 *
 * The numeric [cborValue] values (2, 3, 5) correspond to the NIST security
 * levels and are used in CBOR serialization.
 */
enum class MLDSA(val cborValue: Int) {
    /** ML-DSA Level 2 (NIST security level 2, roughly equivalent to AES-128). */
    MLDSA44(2),

    /** ML-DSA Level 3 (NIST security level 3, roughly equivalent to AES-192). */
    MLDSA65(3),

    /** ML-DSA Level 5 (NIST security level 5, roughly equivalent to AES-256). */
    MLDSA87(5);

    /** Returns the size of a private key in bytes for this security level. */
    fun privateKeySize(): Int = when (this) {
        MLDSA44 -> 2560
        MLDSA65 -> 4032
        MLDSA87 -> 4896
    }

    /** Returns the size of a public key in bytes for this security level. */
    fun publicKeySize(): Int = when (this) {
        MLDSA44 -> 1312
        MLDSA65 -> 1952
        MLDSA87 -> 2592
    }

    /** Returns the size of a signature in bytes for this security level. */
    fun signatureSize(): Int = when (this) {
        MLDSA44 -> 2420
        MLDSA65 -> 3309
        MLDSA87 -> 4627
    }

    /**
     * Returns the Bouncy Castle [MLDSAParameters] for this security level.
     */
    internal fun bcParameters(): MLDSAParameters = when (this) {
        MLDSA44 -> MLDSAParameters.ml_dsa_44
        MLDSA65 -> MLDSAParameters.ml_dsa_65
        MLDSA87 -> MLDSAParameters.ml_dsa_87
    }

    /**
     * Returns the sizes of the individual key components (rho, K, tr, s1, s2, t0)
     * for this security level. The expanded private key is the concatenation of
     * these components in order.
     */
    internal fun componentSizes(): IntArray = when (this) {
        MLDSA44 -> intArrayOf(32, 32, 64, 384, 384, 1664)
        MLDSA65 -> intArrayOf(32, 32, 64, 640, 768, 2496)
        MLDSA87 -> intArrayOf(32, 32, 64, 672, 768, 3328)
    }

    /**
     * Reconstructs a [MLDSAPrivateKeyParameters] from expanded key bytes.
     *
     * The expanded key format is `rho || K || tr || s1 || s2 || t0`.
     * Bouncy Castle's single-byte-array constructor expects a seed (32 bytes),
     * so we must decompose the expanded key into its individual components
     * and use the component-wise constructor.
     */
    internal fun privateKeyParamsFromEncoded(encoded: ByteArray): MLDSAPrivateKeyParameters {
        val sizes = componentSizes()
        var offset = 0
        val components = Array(sizes.size) { i ->
            val component = encoded.copyOfRange(offset, offset + sizes[i])
            offset += sizes[i]
            component
        }
        return MLDSAPrivateKeyParameters(
            bcParameters(),
            components[0], // rho
            components[1], // K
            components[2], // tr
            components[3], // s1
            components[4], // s2
            components[5], // t0
            null,          // t1 (not needed for signing)
        )
    }

    /**
     * Generates a new ML-DSA keypair with this security level.
     *
     * @return a pair of (private key, public key)
     */
    fun keypair(): Pair<MLDSAPrivateKey, MLDSAPublicKey> {
        val keyGen = MLDSAKeyPairGenerator()
        keyGen.init(MLDSAKeyGenerationParameters(SecureRandom(), bcParameters()))
        val pair = keyGen.generateKeyPair()
        val privParams = pair.private as MLDSAPrivateKeyParameters
        val pubParams = pair.public as MLDSAPublicKeyParameters
        return MLDSAPrivateKey.fromData(this, privParams.encoded) to
            MLDSAPublicKey.fromData(this, pubParams.encoded)
    }

    companion object {
        /**
         * Creates an [MLDSA] level from a CBOR-encoded value (2, 3, or 5).
         *
         * @throws BcComponentsException.PostQuantum if the value is not a valid
         *   ML-DSA level
         */
        fun fromCborValue(value: Int): MLDSA =
            entries.find { it.cborValue == value }
                ?: throw BcComponentsException.postQuantum(
                    "Invalid MLDSA level: $value"
                )
    }
}
