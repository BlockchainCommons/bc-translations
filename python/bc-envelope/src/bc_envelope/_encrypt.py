"""Symmetric encryption and decryption extension for Gordian Envelope.

Encrypts/decrypts envelope subjects using ChaCha20-Poly1305, preserving
the envelope's digest tree structure so signatures and proofs remain valid.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_components import Nonce, SymmetricKey
from bc_tags import TAG_ENVELOPE, TAG_LEAF
from dcbor import CBOR

from ._envelope_case import CaseType

if TYPE_CHECKING:
    from ._envelope import Envelope


# ---------------------------------------------------------------------------
# encrypt_subject / decrypt_subject
# ---------------------------------------------------------------------------

def encrypt_subject(self: Envelope, key: SymmetricKey) -> Envelope:
    """Return a new envelope with its subject encrypted."""
    return encrypt_subject_opt(self, key, None)


def encrypt_subject_opt(
    self: Envelope,
    key: SymmetricKey,
    test_nonce: Nonce | None = None,
) -> Envelope:
    """Encrypt the subject, optionally with a deterministic test nonce."""
    from ._envelope import Envelope as Env
    from ._error import AlreadyElided, AlreadyEncrypted

    case = self.case
    ct = case.case_type

    if ct == CaseType.NODE:
        subject = case.subject
        assertions = case.assertions
        envelope_digest = case.digest

        if subject.is_encrypted():
            raise AlreadyEncrypted()

        encoded_cbor = subject.tagged_cbor().to_cbor_data()
        digest = subject.digest()
        encrypted_message = key.encrypt_with_digest(encoded_cbor, digest, test_nonce)
        encrypted_subject = Env.new_with_encrypted(encrypted_message)
        result = Env.new_with_unchecked_assertions(encrypted_subject, list(assertions))
        original_digest = envelope_digest

    elif ct == CaseType.LEAF:
        cbor_val = case.cbor
        digest = case.digest
        encoded_cbor = CBOR.from_tagged_value(
            CBOR.from_int(TAG_ENVELOPE),
            CBOR.from_tagged_value(CBOR.from_int(TAG_LEAF), cbor_val),
        ).to_cbor_data()
        encrypted_message = key.encrypt_with_digest(encoded_cbor, digest, test_nonce)
        result = Env.new_with_encrypted(encrypted_message)
        original_digest = digest

    elif ct == CaseType.WRAPPED:
        digest = case.digest
        encoded_cbor = self.tagged_cbor().to_cbor_data()
        encrypted_message = key.encrypt_with_digest(encoded_cbor, digest, test_nonce)
        result = Env.new_with_encrypted(encrypted_message)
        original_digest = digest

    elif ct == CaseType.KNOWN_VALUE:
        value = case.value
        digest = case.digest
        encoded_cbor = CBOR.from_tagged_value(
            CBOR.from_int(TAG_ENVELOPE),
            value.untagged_cbor(),
        ).to_cbor_data()
        encrypted_message = key.encrypt_with_digest(encoded_cbor, digest, test_nonce)
        result = Env.new_with_encrypted(encrypted_message)
        original_digest = digest

    elif ct == CaseType.ASSERTION:
        assertion = case.assertion
        digest = assertion.digest()
        encoded_cbor = CBOR.from_tagged_value(
            CBOR.from_int(TAG_ENVELOPE),
            assertion.to_cbor(),
        ).to_cbor_data()
        encrypted_message = key.encrypt_with_digest(encoded_cbor, digest, test_nonce)
        result = Env.new_with_encrypted(encrypted_message)
        original_digest = digest

    elif ct == CaseType.ENCRYPTED:
        raise AlreadyEncrypted()

    elif ct == CaseType.COMPRESSED:
        compressed = case.compressed
        digest = compressed.digest()
        encoded_cbor = CBOR.from_tagged_value(
            CBOR.from_int(TAG_ENVELOPE),
            compressed.tagged_cbor(),
        ).to_cbor_data()
        encrypted_message = key.encrypt_with_digest(encoded_cbor, digest, test_nonce)
        result = Env.new_with_encrypted(encrypted_message)
        original_digest = digest

    elif ct == CaseType.ELIDED:
        raise AlreadyElided()

    else:
        raise AlreadyElided()

    assert result.digest() == original_digest
    return result


def decrypt_subject(self: Envelope, key: SymmetricKey) -> Envelope:
    """Return a new envelope with its subject decrypted."""
    from ._envelope import Envelope as Env
    from ._error import InvalidDigest, MissingDigest, NotEncrypted

    subject_case = self.subject().case
    if subject_case.case_type != CaseType.ENCRYPTED:
        raise NotEncrypted()

    message = subject_case.encrypted_message
    encoded_cbor = key.decrypt(message)
    subject_digest = message.aad_digest()
    if subject_digest is None:
        raise MissingDigest()

    cbor = CBOR.from_data(encoded_cbor)
    result_subject = Env.from_tagged_cbor(cbor)

    if result_subject.digest() != subject_digest:
        raise InvalidDigest()

    case = self.case
    if case.case_type == CaseType.NODE:
        assertions = case.assertions
        node_digest = case.digest
        result = Env.new_with_unchecked_assertions(result_subject, list(assertions))
        if result.digest() != node_digest:
            raise InvalidDigest()
        return result
    else:
        return result_subject


# ---------------------------------------------------------------------------
# encrypt / decrypt (wrap+encrypt / decrypt+unwrap convenience)
# ---------------------------------------------------------------------------

def encrypt(self: Envelope, key: SymmetricKey) -> Envelope:
    """Wrap and encrypt the entire envelope."""
    return encrypt_subject(self.wrap(), key)


def decrypt(self: Envelope, key: SymmetricKey) -> Envelope:
    """Decrypt and unwrap the entire envelope."""
    return decrypt_subject(self, key).try_unwrap()
