"""Ed25519 signature tests for Gordian Envelope.

Translated from rust/bc-envelope/tests/ed25519_tests.rs
"""

from bc_envelope import Envelope, extract_subject

from tests.common.check_encoding import check_encoding
from tests.common.test_data import (
    alice_private_keys,
    carol_private_keys,
    hello_envelope,
)


def test_ed25519_signed_plaintext():
    """Alice sends a signed plaintext message using Ed25519."""
    alice_private_key = alice_private_keys().ed25519_signing_private_key()
    alice_public_key = alice_private_key.public_key()

    # Alice sends a signed plaintext message to Bob
    envelope = check_encoding(
        hello_envelope().add_signature(alice_private_key)
    )
    cbor = envelope.tagged_cbor()

    expected_format = (
        '"Hello." [\n'
        "    'signed': Signature(ED25519)\n"
        "]"
    )
    assert envelope.format() == expected_format

    # Alice -> Bob
    received_envelope = check_encoding(Envelope.from_tagged_cbor(cbor))

    # Bob validates Alice's signature and reads the message
    received_plaintext = extract_subject(
        received_envelope.verify_signature_from(alice_public_key),
        str,
    )
    assert received_plaintext == "Hello."

    # Confirm it wasn't signed by Carol
    carol_public_key = (
        carol_private_keys()
        .ed25519_signing_private_key()
        .public_key()
    )
    assert not received_envelope.has_signature_from(carol_public_key)

    # Confirm it was signed by Alice OR Carol (threshold=1)
    received_envelope.verify_signatures_from_threshold(
        [alice_public_key, carol_public_key], 1
    )

    # Confirm it was not signed by Alice AND Carol (threshold=2)
    try:
        received_envelope.verify_signatures_from_threshold(
            [alice_public_key, carol_public_key], 2
        )
        assert False, "Should have raised"
    except Exception:
        pass
