package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.*
import com.blockchaincommons.dcbor.CborDate
import com.blockchaincommons.knownvalues.DATE
import kotlin.test.Test
import kotlin.test.assertEquals

class MultiPermitTest {

    @Test
    fun testMultiPermit() {
        registerTags()

        // Alice composes a poem.
        val poemText =
            "At midnight, the clocks sang lullabies to the wandering teacups."

        // Alice creates a new envelope and assigns the text as the subject.
        val originalEnvelope = Envelope.from(poemText)
            .addType("poem")
            .addAssertion("title", "A Song of Ice Cream")
            .addAssertion("author", "Plonkus the Iridescent")
            .addAssertion(DATE, CborDate.fromYmd(2025, 5, 15))

        // Alice signs the envelope with her private key.
        val (alicePrivateKeys, alicePublicKeys) = keypair()
        val signedEnvelope = originalEnvelope.sign(alicePrivateKeys)

        val expectedSignedFormat = """
            {
                "At midnight, the clocks sang lullabies to the wandering teacups." [
                    'isA': "poem"
                    "author": "Plonkus the Iridescent"
                    "title": "A Song of Ice Cream"
                    'date': 2025-05-15
                ]
            } [
                'signed': Signature
            ]
        """.trimIndent()
        assertEquals(expectedSignedFormat, signedEnvelope.format())

        // Alice picks a random content key and encrypts the signed envelope.
        val contentKey = SymmetricKey.create()
        val encryptedEnvelope = signedEnvelope.encrypt(contentKey)
        assertEquals("ENCRYPTED", encryptedEnvelope.format())

        // Alice adds the first permit using her password.
        val password = "unicorns_dance_on_mars_while_eating_pizza"
        val lockedEnvelope = encryptedEnvelope
            .addSecret(KeyDerivationMethod.Argon2id, password, contentKey)

        val expectedLockedFormat = """
            ENCRYPTED [
                'hasSecret': EncryptedKey(Argon2id)
            ]
        """.trimIndent()
        assertEquals(expectedLockedFormat, lockedEnvelope.format())

        // Alice adds recipient permits for herself and Bob.
        val (bobPrivateKeys, bobPublicKeys) = keypair()
        val lockedWithRecipients = lockedEnvelope
            .addRecipient(alicePublicKeys, contentKey)
            .addRecipient(bobPublicKeys, contentKey)

        val expectedRecipientsFormat = """
            ENCRYPTED [
                'hasRecipient': SealedMessage
                'hasRecipient': SealedMessage
                'hasSecret': EncryptedKey(Argon2id)
            ]
        """.trimIndent()
        assertEquals(expectedRecipientsFormat, lockedWithRecipients.format())

        // Alice creates a 2-of-3 SSKR split.
        val sskrGroup = SSKRGroupSpec(2, 3)
        val spec = SSKRSpec(1, listOf(sskrGroup))
        val shardedEnvelopes =
            lockedWithRecipients.sskrSplitFlattened(spec, contentKey)

        val expectedShardedFormat = """
            ENCRYPTED [
                'hasRecipient': SealedMessage
                'hasRecipient': SealedMessage
                'hasSecret': EncryptedKey(Argon2id)
                'sskrShare': SSKRShare
            ]
        """.trimIndent()
        assertEquals(expectedShardedFormat, shardedEnvelopes[0].format())

        // Five ways to unlock:

        // 1. Using the content key.
        val receivedEnvelope = shardedEnvelopes[0]
        val unlockedByKey = receivedEnvelope.decrypt(contentKey)
        assertEquals(signedEnvelope, unlockedByKey)

        // 2. Using the password.
        val unlockedByPassword = receivedEnvelope.unlock(password)
        assertEquals(signedEnvelope, unlockedByPassword)

        // 3. Using Alice's private key.
        val unlockedByAlice = receivedEnvelope.decryptToRecipient(alicePrivateKeys)
        assertEquals(signedEnvelope, unlockedByAlice)

        // 4. Using Bob's private key.
        val unlockedByBob = receivedEnvelope.decryptToRecipient(bobPrivateKeys)
        assertEquals(signedEnvelope, unlockedByBob)

        // 5. Using any two of the three SSKR shares.
        val unlockedBySskr =
            Envelope.sskrJoin(listOf(shardedEnvelopes[0], shardedEnvelopes[2]))
                .unwrap()
        assertEquals(signedEnvelope, unlockedBySskr)

        unlockedBySskr.verify(alicePublicKeys)
    }
}
