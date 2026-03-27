using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class AttachmentTests
{
    [Fact]
    public void TestAttachment()
    {
        var seed = new Seed(
            Convert.FromHexString("82f32c855d3d542256180810797e0073"),
            "Alice's Seed",
            "This is the note.",
            null);
        var seedEnvelope = seed.ToEnvelope()
            .AddAttachment(
                "Attachment Data V1",
                "com.example",
                "https://example.com/seed-attachment/v1")
            .AddAttachment(
                "Attachment Data V2",
                "com.example",
                "https://example.com/seed-attachment/v2");

        var expectedFormat =
            "Bytes(16) [\n" +
            "    'isA': 'Seed'\n" +
            "    'attachment': {\n" +
            "        \"Attachment Data V1\"\n" +
            "    } [\n" +
            "        'conformsTo': \"https://example.com/seed-attachment/v1\"\n" +
            "        'vendor': \"com.example\"\n" +
            "    ]\n" +
            "    'attachment': {\n" +
            "        \"Attachment Data V2\"\n" +
            "    } [\n" +
            "        'conformsTo': \"https://example.com/seed-attachment/v2\"\n" +
            "        'vendor': \"com.example\"\n" +
            "    ]\n" +
            "    'name': \"Alice's Seed\"\n" +
            "    'note': \"This is the note.\"\n" +
            "]";
        Assert.Equal(expectedFormat, seedEnvelope.Format());

        Assert.Equal(2, seedEnvelope.Attachments().Count);

        Assert.Equal(2,
            seedEnvelope.AttachmentsWithVendorAndConformsTo(null, null).Count);
        Assert.Equal(2,
            seedEnvelope.AttachmentsWithVendorAndConformsTo("com.example", null).Count);
        Assert.Single(
            seedEnvelope.AttachmentsWithVendorAndConformsTo(
                null, "https://example.com/seed-attachment/v1"));

        Assert.Empty(
            seedEnvelope.AttachmentsWithVendorAndConformsTo(null, "foo"));
        Assert.Empty(
            seedEnvelope.AttachmentsWithVendorAndConformsTo("bar", null));

        var v1Attachment = seedEnvelope.AttachmentWithVendorAndConformsTo(
            null, "https://example.com/seed-attachment/v1");
        var payload = v1Attachment.AttachmentPayload();
        Assert.Equal("\"Attachment Data V1\"", payload.Format());
        Assert.Equal("com.example", v1Attachment.AttachmentVendor());
        Assert.Equal("https://example.com/seed-attachment/v1",
            v1Attachment.AttachmentConformsTo());

        var seedEnvelope2 = seed.ToEnvelope();
        var attachments = seedEnvelope.Attachments();
        seedEnvelope2 = seedEnvelope2.AddAssertions(attachments);
        Assert.True(seedEnvelope2.IsEquivalentTo(seedEnvelope));
    }
}
