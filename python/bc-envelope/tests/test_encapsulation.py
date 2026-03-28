"""Encapsulation (KEM) tests for Gordian Envelope.

Translated from rust/bc-envelope/tests/encapsulation_tests.rs
"""

from bc_components import EncapsulationScheme

from tests.common.check_encoding import check_encoding
from tests.common.test_data import hello_envelope


def _test_scheme(scheme: EncapsulationScheme):
    private_key, public_key = scheme.keypair()
    envelope = hello_envelope()
    encrypted_envelope = check_encoding(
        envelope.encrypt_to_recipient(public_key)
    )
    decrypted_envelope = encrypted_envelope.decrypt_to_recipient(private_key)
    assert envelope.structural_digest() == decrypted_envelope.structural_digest()


def test_encapsulation():
    """Test encrypt/decrypt round-trip for each encapsulation scheme."""
    _test_scheme(EncapsulationScheme.X25519)
    _test_scheme(EncapsulationScheme.MLKEM512)
    _test_scheme(EncapsulationScheme.MLKEM768)
    _test_scheme(EncapsulationScheme.MLKEM1024)
