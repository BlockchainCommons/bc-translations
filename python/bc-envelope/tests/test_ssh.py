"""SSH signing tests for bc-envelope.

Translated from rust/bc-envelope/tests/ssh_tests.rs
"""

from textwrap import dedent

from bc_components import SignatureScheme
from bc_components.signing._signing_private_key import SshSigningOptions
from bc_envelope import Envelope, extract_subject

from tests.common.check_encoding import check_encoding
from tests.common.test_data import (
    alice_private_keys,
    carol_private_keys,
    hello_envelope,
)


def test_ssh_signed_plaintext():
    alice_ssh_private_key = alice_private_keys().ssh_signing_private_key(
        SignatureScheme.SSH_ED25519, "alice@example.com"
    )
    alice_ssh_public_key = alice_ssh_private_key.public_key()

    # Alice sends a signed plaintext message to Bob.
    options = SshSigningOptions(namespace="test", hash_alg="sha256")
    envelope = check_encoding(
        hello_envelope().add_signature_opt(alice_ssh_private_key, options)
    )

    formatted = envelope.format()
    assert formatted.startswith('"Hello." [\n')
    assert "'signed': Signature" in formatted

    # Round-trip through CBOR
    cbor = envelope.tagged_cbor()
    received_envelope = check_encoding(Envelope.from_tagged_cbor(cbor))

    # Bob verifies Alice's signature and reads the message.
    verified = received_envelope.verify_signature_from(alice_ssh_public_key)
    received_plaintext = extract_subject(verified, str)
    assert received_plaintext == "Hello."

    # Confirm it wasn't signed by Carol.
    carol_ssh_public_key = carol_private_keys().ssh_signing_private_key(
        SignatureScheme.SSH_ED25519, "carol@example.com"
    ).public_key()
    try:
        received_envelope.verify_signature_from(carol_ssh_public_key)
        assert False, "Expected signature verification failure"
    except Exception:
        pass

    # Confirm it was signed by Alice OR Carol (threshold 1).
    received_envelope.verify_signatures_from_threshold(
        [alice_ssh_public_key, carol_ssh_public_key], 1
    )

    # Confirm it was NOT signed by Alice AND Carol (threshold 2).
    try:
        received_envelope.verify_signatures_from_threshold(
            [alice_ssh_public_key, carol_ssh_public_key], 2
        )
        assert False, "Expected threshold verification failure"
    except Exception:
        pass
