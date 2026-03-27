package bcenvelope

import (
	"testing"
)

func TestAttachment(t *testing.T) {
	seedData := mustDecodeHex("82f32c855d3d542256180810797e0073")
	seed := NewTestSeedOpt(
		seedData,
		"Alice's Seed",
		"This is the note.",
		nil,
	)
	seedEnvelope := testSeedToEnvelope(seed)

	conformsToV1 := "https://example.com/seed-attachment/v1"
	conformsToV2 := "https://example.com/seed-attachment/v2"

	seedEnvelope = seedEnvelope.
		AddAttachment("Attachment Data V1", "com.example", &conformsToV1).
		AddAttachment("Attachment Data V2", "com.example", &conformsToV2)

	assertActualExpected(t, seedEnvelope.Format(), `Bytes(16) [
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
]`)

	attachments, err := seedEnvelope.Attachments()
	if err != nil {
		t.Fatalf("attachments failed: %v", err)
	}
	if len(attachments) != 2 {
		t.Fatalf("expected 2 attachments, got %d", len(attachments))
	}

	// No filters
	all, err := seedEnvelope.AttachmentsWithVendorAndConformsTo(nil, nil)
	if err != nil {
		t.Fatalf("filter nil/nil failed: %v", err)
	}
	if len(all) != 2 {
		t.Fatalf("expected 2, got %d", len(all))
	}

	// Vendor filter only
	vendor := "com.example"
	byVendor, err := seedEnvelope.AttachmentsWithVendorAndConformsTo(&vendor, nil)
	if err != nil {
		t.Fatalf("filter vendor failed: %v", err)
	}
	if len(byVendor) != 2 {
		t.Fatalf("expected 2, got %d", len(byVendor))
	}

	// ConformsTo filter only
	byConformsTo, err := seedEnvelope.AttachmentsWithVendorAndConformsTo(nil, &conformsToV1)
	if err != nil {
		t.Fatalf("filter conformsTo failed: %v", err)
	}
	if len(byConformsTo) != 1 {
		t.Fatalf("expected 1, got %d", len(byConformsTo))
	}

	// No matches (conformsTo)
	noMatchConformsTo := "foo"
	noMatch, err := seedEnvelope.AttachmentsWithVendorAndConformsTo(nil, &noMatchConformsTo)
	if err != nil {
		t.Fatalf("filter no-match conformsTo failed: %v", err)
	}
	if len(noMatch) != 0 {
		t.Fatalf("expected 0, got %d", len(noMatch))
	}

	// No matches (vendor)
	noMatchVendor := "bar"
	noMatch, err = seedEnvelope.AttachmentsWithVendorAndConformsTo(&noMatchVendor, nil)
	if err != nil {
		t.Fatalf("filter no-match vendor failed: %v", err)
	}
	if len(noMatch) != 0 {
		t.Fatalf("expected 0, got %d", len(noMatch))
	}

	// Get specific attachment
	v1Attachment, err := seedEnvelope.AttachmentWithVendorAndConformsTo(nil, &conformsToV1)
	if err != nil {
		t.Fatalf("get v1 attachment failed: %v", err)
	}
	payload, err := v1Attachment.AttachmentPayload()
	if err != nil {
		t.Fatalf("payload failed: %v", err)
	}
	assertActualExpected(t, payload.Format(), `"Attachment Data V1"`)

	vendorStr, err := v1Attachment.AttachmentVendor()
	if err != nil {
		t.Fatalf("vendor failed: %v", err)
	}
	if vendorStr != "com.example" {
		t.Fatalf("expected com.example, got %s", vendorStr)
	}

	conformsToStr, err := v1Attachment.AttachmentConformsTo()
	if err != nil {
		t.Fatalf("conformsTo failed: %v", err)
	}
	if conformsToStr == nil || *conformsToStr != "https://example.com/seed-attachment/v1" {
		t.Fatalf("unexpected conformsTo: %v", conformsToStr)
	}

	// Re-add attachments and verify equivalence
	seedEnvelope2 := testSeedToEnvelope(seed)
	allAttachments, err := seedEnvelope.Attachments()
	if err != nil {
		t.Fatalf("get all attachments failed: %v", err)
	}
	seedEnvelope2 = seedEnvelope2.AddAssertions(allAttachments)
	if !seedEnvelope2.IsEquivalentTo(seedEnvelope) {
		t.Fatal("re-added attachments should produce equivalent envelope")
	}
}
