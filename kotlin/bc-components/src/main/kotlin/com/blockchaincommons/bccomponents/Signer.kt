package com.blockchaincommons.bccomponents

/**
 * A type capable of creating digital signatures.
 *
 * The [Signer] interface provides methods for signing messages with various
 * cryptographic signature schemes. Implementations of this interface can
 * sign messages using different algorithms according to the specific
 * signer type.
 */
interface Signer {
    /**
     * Signs a message with additional options specific to the signature scheme.
     *
     * @param message the message to sign
     * @param options optional signing options (algorithm-specific parameters)
     * @return the digital signature
     */
    fun signWithOptions(message: ByteArray, options: SigningOptions? = null): Signature

    /**
     * Signs a message using default options.
     *
     * This is a convenience method that calls [signWithOptions] with `null`
     * for the options parameter.
     *
     * @param message the message to sign
     * @return the digital signature
     */
    fun sign(message: ByteArray): Signature = signWithOptions(message)
}

/**
 * A type capable of verifying digital signatures.
 *
 * The [Verifier] interface provides a method to verify that a signature
 * was created by a corresponding signer for a specific message.
 */
interface Verifier {
    /**
     * Verifies a signature against a message.
     *
     * @param signature the signature to verify
     * @param message the message that was allegedly signed
     * @return `true` if the signature is valid for the message, `false` otherwise
     */
    fun verify(signature: Signature, message: ByteArray): Boolean
}
