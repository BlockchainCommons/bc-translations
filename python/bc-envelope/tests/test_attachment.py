"""Attachment tests for bc-envelope.

Translated from rust/bc-envelope/tests/attachment_tests.rs
"""

from textwrap import dedent

from bc_envelope import Envelope

from tests.common.test_seed import Seed


def test_attachment():
    seed = Seed(
        data=bytes.fromhex("82f32c855d3d542256180810797e0073"),
        name="Alice's Seed",
        note="This is the note.",
    )
    seed_envelope = (
        seed.to_envelope()
        .add_attachment(
            "Attachment Data V1",
            "com.example",
            "https://example.com/seed-attachment/v1",
        )
        .add_attachment(
            "Attachment Data V2",
            "com.example",
            "https://example.com/seed-attachment/v2",
        )
    )

    expected = dedent("""\
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
        ]""")
    assert seed_envelope.format() == expected

    assert len(seed_envelope.attachments()) == 2

    assert len(seed_envelope.attachments_with_vendor_and_conforms_to(None, None)) == 2
    assert len(seed_envelope.attachments_with_vendor_and_conforms_to("com.example", None)) == 2
    assert len(seed_envelope.attachments_with_vendor_and_conforms_to(
        None, "https://example.com/seed-attachment/v1"
    )) == 1
    assert len(seed_envelope.attachments_with_vendor_and_conforms_to(None, "foo")) == 0
    assert len(seed_envelope.attachments_with_vendor_and_conforms_to("bar", None)) == 0

    v1_attachment = seed_envelope.attachment_with_vendor_and_conforms_to(
        None, "https://example.com/seed-attachment/v1"
    )
    payload = v1_attachment.attachment_payload()
    assert payload.format() == '"Attachment Data V1"'
    assert v1_attachment.attachment_vendor() == "com.example"
    assert v1_attachment.attachment_conforms_to() == \
        "https://example.com/seed-attachment/v1"

    # Round-trip: rebuild by adding the same attachments
    seed_envelope2 = seed.to_envelope()
    attachments = seed_envelope.attachments()
    seed_envelope2 = seed_envelope2.add_assertions(attachments)
    assert seed_envelope2.is_equivalent_to(seed_envelope)
