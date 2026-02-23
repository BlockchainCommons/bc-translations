@file:OptIn(ExperimentalStdlibApi::class)

package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.*
import com.blockchaincommons.dcbor.CborDate
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertTrue

class SSKRTest {

    @Test
    fun testSskr() {
        registerTags()

        // Dan has a cryptographic seed he wants to backup using a social recovery
        // scheme. The seed includes metadata he wants to back up also, making
        // it too large to fit into a basic SSKR share.
        val danSeed = TestSeed(
            data = "59f2293a5bce7d4de59e71b4207ac5d2".hexToByteArray(),
            name = "Dark Purple Aqua Love",
            note = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            creationDate = CborDate.fromString("2021-02-24"),
        )

        // Dan encrypts the seed and then splits the content key into a single group
        // 2-of-3.
        val contentKey = SymmetricKey.create()
        val seedEnvelope = danSeed.toEnvelope()
        val encryptedSeedEnvelope =
            seedEnvelope.wrap().encryptSubject(contentKey)

        val group = SSKRGroupSpec(2, 3)
        val spec = SSKRSpec(1, listOf(group))
        val envelopes = encryptedSeedEnvelope.sskrSplit(spec, contentKey)

        // Flattening gives a single list of all envelopes to distribute.
        val sentEnvelopes = envelopes.flatten()
        val sentUrs = sentEnvelopes.map { it.ur() }

        val expectedFormat = """
            ENCRYPTED [
                'sskrShare': SSKRShare
            ]
        """.trimIndent()
        assertEquals(expectedFormat, sentEnvelopes[0].format())

        // Dan sends one envelope to each of Alice, Bob, and Carol.
        // val aliceEnvelope = Envelope.fromUr(sentUrs[0])  // UNRECOVERED
        val bobEnvelope = Envelope.fromUr(sentUrs[1])
        val carolEnvelope = Envelope.fromUr(sentUrs[2])

        // At some future point, Dan retrieves two of the three envelopes
        // so he can recover his seed.
        val recoveredSeedEnvelope =
            Envelope.sskrJoin(listOf(bobEnvelope, carolEnvelope)).unwrap()

        val recoveredSeed = TestSeed.fromEnvelope(recoveredSeedEnvelope)

        // The recovered seed is correct.
        assertTrue(danSeed.data.contentEquals(recoveredSeed.data))
        assertEquals(danSeed.creationDate, recoveredSeed.creationDate)
        assertEquals(danSeed.name, recoveredSeed.name)
        assertEquals(danSeed.note, recoveredSeed.note)

        // Attempting to recover with only one envelope won't work.
        assertFailsWith<EnvelopeException> {
            Envelope.sskrJoin(listOf(bobEnvelope))
        }
    }
}
