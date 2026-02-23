package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.*
import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator
import com.blockchaincommons.knownvalues.SSKR_SHARE

/**
 * SSKR extension for Gordian Envelopes.
 *
 * Provides methods for splitting and combining envelopes using Sharded
 * Secret Key Reconstruction (SSKR), which is an implementation of
 * Shamir's Secret Sharing.
 */

/** Adds an `sskrShare: SSKRShare` assertion to the envelope. */
private fun Envelope.addSskrShare(share: SSKRShare): Envelope =
    addAssertion(SSKR_SHARE, share)

/** Splits the envelope into SSKR shares (grouped by SSKR groups). */
fun Envelope.sskrSplit(
    spec: SSKRSpec,
    contentKey: SymmetricKey,
): List<List<Envelope>> {
    val rng = SecureRandomNumberGenerator()
    return sskrSplitUsing(spec, contentKey, rng)
}

/** Splits the envelope into a flattened list of SSKR shares. */
fun Envelope.sskrSplitFlattened(
    spec: SSKRSpec,
    contentKey: SymmetricKey,
): List<Envelope> = sskrSplit(spec, contentKey).flatten()

/** Splits the envelope into SSKR shares using a provided RNG. */
fun Envelope.sskrSplitUsing(
    spec: SSKRSpec,
    contentKey: SymmetricKey,
    testRng: RandomNumberGenerator,
): List<List<Envelope>> {
    val masterSecret = SSKRSecret(contentKey.data())
    val shares = sskrGenerateUsing(spec, masterSecret, testRng)
    return shares.map { group ->
        group.map { share -> addSskrShare(share) }
    }
}

/** Reconstructs the original envelope from a set of SSKR share envelopes. */
fun Envelope.Companion.sskrJoin(envelopes: List<Envelope>): Envelope {
    if (envelopes.isEmpty()) throw EnvelopeException.InvalidShares()

    val grouped = sskrSharesIn(envelopes)
    for (shares in grouped.values) {
        try {
            val secret = sskrCombine(shares)
            val contentKey = SymmetricKey.fromData(secret.toByteArray())
            val envelope = envelopes.first().decryptSubject(contentKey)
            return envelope.subject()
        } catch (_: Exception) {
            // Try next group
        }
    }
    throw EnvelopeException.InvalidShares()
}

/** Extracts and groups SSKR shares from envelopes by identifier. */
private fun sskrSharesIn(
    envelopes: List<Envelope>,
): Map<Int, List<SSKRShare>> {
    val result = mutableMapOf<Int, MutableList<SSKRShare>>()
    for (envelope in envelopes) {
        for (assertion in envelope.assertionsWithPredicate(SSKR_SHARE)) {
            val share = assertion.asObject()!!.extractSubject<SSKRShare>()
            val id = share.identifier()
            result.getOrPut(id) { mutableListOf() }.add(share)
        }
    }
    return result
}
