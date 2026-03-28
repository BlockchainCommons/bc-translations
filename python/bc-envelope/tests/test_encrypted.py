"""Encrypted envelope tests for Gordian Envelope.

Translated from rust/bc-envelope/tests/encrypted_tests.rs
"""

from bc_components import EncryptedMessage, Nonce, SymmetricKey
from bc_envelope import Envelope, extract_subject

from tests.common.check_encoding import check_encoding
from tests.common.test_data import (
    assertion_envelope,
    double_assertion_envelope,
    double_wrapped_envelope,
    hello_envelope,
    known_value_envelope,
    single_assertion_envelope,
    wrapped_envelope,
)


def _symmetric_key() -> SymmetricKey:
    return SymmetricKey(bytes.fromhex(
        "38900719dea655e9a1bc1682aaccf0bfcd79a7239db672d39216e4acdd660dc0"
    ))


def _fake_nonce() -> Nonce:
    return Nonce(bytes.fromhex("4d785658f36c22fb5aed3ac0"))


def _encrypted_test(e1: Envelope):
    e2 = check_encoding(
        e1.encrypt_subject_opt(_symmetric_key(), _fake_nonce())
    )

    assert e1.is_equivalent_to(e2)
    assert e1.subject().is_equivalent_to(e2.subject())

    encrypted_message: EncryptedMessage = extract_subject(e2, EncryptedMessage)
    assert encrypted_message.aad_digest() == e1.subject().digest()

    e3 = e2.decrypt_subject(_symmetric_key())
    assert e1.is_equivalent_to(e3)


def test_encrypted():
    """Encrypt/decrypt round-trip for various envelope types."""
    _encrypted_test(hello_envelope())
    _encrypted_test(wrapped_envelope())
    _encrypted_test(double_wrapped_envelope())
    _encrypted_test(known_value_envelope())
    _encrypted_test(assertion_envelope())
    _encrypted_test(single_assertion_envelope())
    _encrypted_test(double_assertion_envelope())
