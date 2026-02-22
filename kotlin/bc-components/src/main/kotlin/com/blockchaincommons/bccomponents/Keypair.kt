package com.blockchaincommons.bccomponents

import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator

/**
 * Generates a key pair with the specified signature and encapsulation schemes.
 *
 * This function creates a new key pair containing both signing and
 * encapsulation (encryption) keys using the given cryptographic schemes.
 *
 * @param signatureScheme the signature scheme to use (default: [SignatureScheme.Schnorr])
 * @param encapsulationScheme the key encapsulation scheme to use (default: [EncapsulationScheme.X25519])
 * @return a pair containing the [PrivateKeys] and the corresponding [PublicKeys]
 */
fun keypair(
    signatureScheme: SignatureScheme = SignatureScheme.Schnorr,
    encapsulationScheme: EncapsulationScheme = EncapsulationScheme.X25519,
): Pair<PrivateKeys, PublicKeys> {
    val (signingPrivateKey, signingPublicKey) = signatureScheme.keypair()
    val (encapsulationPrivateKey, encapsulationPublicKey) = encapsulationScheme.keypair()
    val privateKeys = PrivateKeys(signingPrivateKey, encapsulationPrivateKey)
    val publicKeys = PublicKeys(signingPublicKey, encapsulationPublicKey)
    return privateKeys to publicKeys
}

/**
 * Generates a key pair with the specified schemes using a custom random number
 * generator.
 *
 * This function provides the most control over key pair generation by allowing
 * custom specification of both cryptographic schemes and the random number
 * generator.
 *
 * @param signatureScheme the signature scheme to use (default: [SignatureScheme.Schnorr])
 * @param encapsulationScheme the key encapsulation scheme to use (default: [EncapsulationScheme.X25519])
 * @param rng the random number generator to use
 * @return a pair containing the [PrivateKeys] and the corresponding [PublicKeys]
 */
fun keypairUsing(
    rng: RandomNumberGenerator,
    signatureScheme: SignatureScheme = SignatureScheme.Schnorr,
    encapsulationScheme: EncapsulationScheme = EncapsulationScheme.X25519,
): Pair<PrivateKeys, PublicKeys> {
    val (signingPrivateKey, signingPublicKey) = signatureScheme.keypairUsing(rng, "")
    val (encapsulationPrivateKey, encapsulationPublicKey) = encapsulationScheme.keypairUsing(rng)
    val privateKeys = PrivateKeys(signingPrivateKey, encapsulationPrivateKey)
    val publicKeys = PublicKeys(signingPublicKey, encapsulationPublicKey)
    return privateKeys to publicKeys
}
