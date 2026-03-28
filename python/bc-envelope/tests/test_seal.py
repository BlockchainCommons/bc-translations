"""Seal/unseal tests for bc-envelope.

Translated from rust/bc-envelope/src/seal.rs
"""

from bc_components import EncapsulationScheme, SignatureScheme
from bc_components.signing._signing_private_key import SshSigningOptions
from bc_envelope import Envelope, extract_subject


def test_seal_and_unseal():
    message = "Top secret message"
    original_envelope = Envelope(message)

    sender_private, sender_public = SignatureScheme.ED25519.keypair()
    recipient_private, recipient_public = EncapsulationScheme.X25519.keypair()

    # Seal the envelope
    sealed_envelope = original_envelope.seal(sender_private, recipient_public)

    # Verify the envelope is encrypted
    assert sealed_envelope.is_subject_encrypted()

    # Unseal the envelope
    unsealed_envelope = sealed_envelope.unseal(sender_public, recipient_private)

    # Verify we got back the original message
    extracted_message = extract_subject(unsealed_envelope, str)
    assert extracted_message == message


def test_seal_opt_with_options():
    message = "Confidential data"
    original_envelope = Envelope(message)

    sender_private, sender_public = SignatureScheme.ED25519.keypair()
    recipient_private, recipient_public = EncapsulationScheme.X25519.keypair()

    # Create signing options
    options = SshSigningOptions(namespace="test", hash_alg="sha512")

    # Seal the envelope with options
    sealed_envelope = original_envelope.seal_opt(
        sender_private, recipient_public, options
    )

    # Verify the envelope is encrypted
    assert sealed_envelope.is_subject_encrypted()

    # Unseal the envelope
    unsealed_envelope = sealed_envelope.unseal(sender_public, recipient_private)

    # Verify we got back the original message
    extracted_message = extract_subject(unsealed_envelope, str)
    assert extracted_message == message
