package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.*
import com.blockchaincommons.knownvalues.HAS_RECIPIENT

/**
 * Public key encryption extension for Gordian Envelopes.
 *
 * Provides methods for encrypting envelopes to one or more recipients
 * using key encapsulation, and for decrypting envelopes as a recipient.
 */

/** Adds a `hasRecipient: SealedMessage` assertion to the envelope. */
fun Envelope.addRecipient(
    recipient: Encrypter,
    contentKey: SymmetricKey,
): Envelope = addRecipientOpt(recipient, contentKey, null)

/** Adds a `hasRecipient` assertion with optional test nonce. */
fun Envelope.addRecipientOpt(
    recipient: Encrypter,
    contentKey: SymmetricKey,
    testNonce: Nonce? = null,
): Envelope {
    val assertion = makeHasRecipient(recipient, contentKey, testNonce)
    return addAssertionEnvelope(assertion)
}

/** Returns all SealedMessages from `hasRecipient` assertions. */
fun Envelope.recipients(): List<SealedMessage> =
    assertionsWithPredicate(HAS_RECIPIENT)
        .filter { !it.asObject()!!.isObscured() }
        .map { it.asObject()!!.extractSubject<SealedMessage>() }

/** Encrypts subject to multiple recipients. */
fun Envelope.encryptSubjectToRecipients(
    recipients: List<Encrypter>,
): Envelope = encryptSubjectToRecipientsOpt(recipients, null)

/** Encrypts subject to multiple recipients with optional test nonce. */
fun Envelope.encryptSubjectToRecipientsOpt(
    recipients: List<Encrypter>,
    testNonce: Nonce? = null,
): Envelope {
    val contentKey = SymmetricKey.create()
    var e = encryptSubject(contentKey)
    for (recipient in recipients) {
        e = e.addRecipientOpt(recipient, contentKey, testNonce)
    }
    return e
}

/** Encrypts subject to a single recipient. */
fun Envelope.encryptSubjectToRecipient(
    recipient: Encrypter,
): Envelope = encryptSubjectToRecipientOpt(recipient, null)

/** Encrypts subject to a single recipient with optional test nonce. */
fun Envelope.encryptSubjectToRecipientOpt(
    recipient: Encrypter,
    testNonce: Nonce? = null,
): Envelope = encryptSubjectToRecipientsOpt(listOf(recipient), testNonce)

/** Decrypts subject using the recipient's private key. */
fun Envelope.decryptSubjectToRecipient(
    recipient: Decrypter,
): Envelope {
    val sealedMessages = recipients()
    val contentKeyData = firstPlaintextInSealedMessages(sealedMessages, recipient)
    val contentKey = SymmetricKey.fromTaggedCborData(contentKeyData)
    return decryptSubject(contentKey)
}

/** Convenience: wrap + encrypt subject to a single recipient. */
fun Envelope.encryptToRecipient(recipient: Encrypter): Envelope =
    wrap().encryptSubjectToRecipient(recipient)

/** Convenience: decrypt subject to recipient + unwrap. */
fun Envelope.decryptToRecipient(recipient: Decrypter): Envelope =
    decryptSubjectToRecipient(recipient).unwrap()

// -- Private helpers --

private fun makeHasRecipient(
    recipient: Encrypter,
    contentKey: SymmetricKey,
    testNonce: Nonce?,
): Envelope {
    val sealedMessage = SealedMessage.create(
        contentKey.taggedCbor().toCborData(),
        recipient,
        null,
        testNonce,
    )
    return Envelope.newAssertion(HAS_RECIPIENT, sealedMessage)
}

private fun firstPlaintextInSealedMessages(
    sealedMessages: List<SealedMessage>,
    privateKey: Decrypter,
): ByteArray {
    for (sealedMessage in sealedMessages) {
        try {
            return sealedMessage.decrypt(privateKey)
        } catch (_: Exception) {
            // Try next sealed message
        }
    }
    throw EnvelopeException.UnknownRecipient()
}
