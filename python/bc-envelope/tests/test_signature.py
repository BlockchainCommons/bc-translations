"""Signature tests for Gordian Envelope.

Translated from rust/bc-envelope/tests/signature_tests.rs
"""

import known_values
from bc_envelope import Envelope, SignatureMetadata, extract_subject

from tests.common.check_encoding import check_encoding
from tests.common.test_data import (
    PLAINTEXT_HELLO,
    alice_private_keys,
    alice_public_keys,
    carol_private_keys,
    carol_public_keys,
    hello_envelope,
)


def test_signed_plaintext():
    """Alice sends a signed plaintext message to Bob."""
    envelope = check_encoding(
        hello_envelope().add_signature(alice_private_keys())
    )
    cbor = envelope.tagged_cbor()

    expected_format = (
        '"Hello." [\n'
        "    'signed': Signature\n"
        "]"
    )
    assert envelope.format() == expected_format

    # Alice -> Bob
    received_envelope = check_encoding(Envelope.from_tagged_cbor(cbor))

    # Bob validates Alice's signature and reads the message
    received_plaintext = extract_subject(
        received_envelope.verify_signature_from(alice_public_keys()),
        str,
    )
    assert received_plaintext == "Hello."

    # Confirm it wasn't signed by Carol
    assert not received_envelope.has_signature_from(carol_public_keys())

    # Confirm it was signed by Alice OR Carol (threshold=1)
    received_envelope.verify_signatures_from_threshold(
        [alice_public_keys(), carol_public_keys()], 1
    )

    # Confirm it was not signed by Alice AND Carol (threshold=2)
    try:
        received_envelope.verify_signatures_from_threshold(
            [alice_public_keys(), carol_public_keys()], 2
        )
        assert False, "Should have raised"
    except Exception:
        pass


def test_multisigned_plaintext():
    """Alice and Carol jointly sign a plaintext message to Bob."""
    envelope = check_encoding(
        hello_envelope()
        .add_signatures([alice_private_keys(), carol_private_keys()])
    )

    expected_format = (
        '"Hello." [\n'
        "    'signed': Signature\n"
        "    'signed': Signature\n"
        "]"
    )
    assert envelope.format() == expected_format

    # Alice & Carol -> Bob
    cbor = envelope.tagged_cbor()

    # Bob verifies both signatures
    received_plaintext = extract_subject(
        check_encoding(Envelope.from_tagged_cbor(cbor))
        .verify_signatures_from([alice_public_keys(), carol_public_keys()]),
        str,
    )
    assert received_plaintext == PLAINTEXT_HELLO


def test_signed_with_metadata():
    """Alice signs a message with metadata."""
    envelope = hello_envelope()

    metadata = SignatureMetadata().with_assertion(
        known_values.NOTE, "Alice signed this."
    )

    envelope = check_encoding(
        envelope
        .wrap()
        .add_signature_opt(alice_private_keys(), None, metadata)
    )

    expected_format = (
        "{\n"
        '    "Hello."\n'
        "} [\n"
        "    'signed': {\n"
        "        Signature [\n"
        "            'note': \"Alice signed this.\"\n"
        "        ]\n"
        "    } [\n"
        "        'signed': Signature\n"
        "    ]\n"
        "]"
    )
    assert envelope.format() == expected_format

    # Alice -> Bob
    cbor = envelope.tagged_cbor()

    # Bob verifies and gets metadata
    received_envelope = check_encoding(Envelope.from_tagged_cbor(cbor))
    received_plaintext, metadata_envelope = received_envelope.verify_returning_metadata(
        alice_public_keys()
    )

    expected_meta_format = (
        "Signature [\n"
        "    'note': \"Alice signed this.\"\n"
        "]"
    )
    assert metadata_envelope.format() == expected_meta_format

    note = extract_subject(
        metadata_envelope.object_for_predicate(known_values.NOTE),
        str,
    )
    assert note == "Alice signed this."

    # Bob reads the message
    received_text = extract_subject(received_plaintext, str)
    assert received_text == PLAINTEXT_HELLO
