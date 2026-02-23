package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.*

/**
 * Seal/unseal extension for Gordian Envelopes.
 *
 * Provides convenience methods that combine signing and recipient-based
 * encryption into a single operation.
 */

/** Seals an envelope by signing it and encrypting to the recipient. */
fun Envelope.seal(sender: Signer, recipient: Encrypter): Envelope =
    sign(sender).encryptToRecipient(recipient)

/** Seals with optional signing options. */
fun Envelope.sealOpt(
    sender: Signer,
    recipient: Encrypter,
    options: SigningOptions? = null,
): Envelope = signOpt(sender, options).encryptToRecipient(recipient)

/** Unseals by decrypting with recipient key and verifying sender signature. */
fun Envelope.unseal(sender: Verifier, recipient: Decrypter): Envelope =
    decryptToRecipient(recipient).verify(sender)
