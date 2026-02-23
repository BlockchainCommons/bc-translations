@file:OptIn(ExperimentalStdlibApi::class)

package com.blockchaincommons.bcenvelope

// registerTags() from bc-envelope package initializes GlobalFormatContext
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

class AttachmentTest {

    @Test
    fun testAttachment() {
        registerTags()

        val seed = TestSeed(
            data = "82f32c855d3d542256180810797e0073".hexToByteArray(),
            name = "Alice's Seed",
            note = "This is the note.",
        )
        val seedEnvelope = seed
            .toEnvelope()
            .addAttachment(
                "Attachment Data V1",
                "com.example",
                "https://example.com/seed-attachment/v1",
            )
            .addAttachment(
                "Attachment Data V2",
                "com.example",
                "https://example.com/seed-attachment/v2",
            )

        val expectedFormat = """
            Bytes(16) [
                'isA': 'Seed'
                'attachment': {
                    "Attachment Data V1"
                } [
                    'conformsTo': "https://example.com/seed-attachment/v1"
                    'vendor': "com.example"
                ]
                'attachment': {
                    "Attachment Data V2"
                } [
                    'conformsTo': "https://example.com/seed-attachment/v2"
                    'vendor': "com.example"
                ]
                'name': "Alice's Seed"
                'note': "This is the note."
            ]
        """.trimIndent()
        assertEquals(expectedFormat, seedEnvelope.format())

        assertEquals(2, seedEnvelope.attachments().size)

        assertEquals(
            2,
            seedEnvelope.attachmentsWithVendorAndConformsTo(null, null).size
        )
        assertEquals(
            2,
            seedEnvelope.attachmentsWithVendorAndConformsTo("com.example", null).size
        )
        assertEquals(
            1,
            seedEnvelope.attachmentsWithVendorAndConformsTo(
                null,
                "https://example.com/seed-attachment/v1"
            ).size
        )
        assertEquals(
            0,
            seedEnvelope.attachmentsWithVendorAndConformsTo(null, "foo").size
        )
        assertEquals(
            0,
            seedEnvelope.attachmentsWithVendorAndConformsTo("bar", null).size
        )

        val v1Attachment = seedEnvelope.attachmentWithVendorAndConformsTo(
            null,
            "https://example.com/seed-attachment/v1",
        )
        val payload = v1Attachment.attachmentPayload()
        assertEquals("\"Attachment Data V1\"", payload.format())
        assertEquals("com.example", v1Attachment.attachmentVendor())
        assertEquals(
            "https://example.com/seed-attachment/v1",
            v1Attachment.attachmentConformsTo()
        )

        val seedEnvelope2 = seed.toEnvelope()
        val attachments = seedEnvelope.attachments()
        val seedEnvelope2WithAttachments = seedEnvelope2.addAssertions(attachments)
        assertTrue(seedEnvelope2WithAttachments.isEquivalentTo(seedEnvelope))
    }
}
