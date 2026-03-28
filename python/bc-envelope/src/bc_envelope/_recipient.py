"""Public-key recipient encryption extension for Gordian Envelope.

Encrypts content to one or more recipients using public-key cryptography.
Each recipient's content key is distributed via ``SealedMessage``.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_components import Nonce, SealedMessage, SymmetricKey
from bc_components._encrypter import Decrypter, Encrypter
import known_values

if TYPE_CHECKING:
    from ._envelope import Envelope


# ---------------------------------------------------------------------------
# Low-level: add_recipient / recipients
# ---------------------------------------------------------------------------

def add_recipient(
    self: Envelope,
    recipient: Encrypter,
    content_key: SymmetricKey,
) -> Envelope:
    """Add a ``'hasRecipient': SealedMessage`` assertion."""
    return add_recipient_opt(self, recipient, content_key, None)


def add_recipient_opt(
    self: Envelope,
    recipient: Encrypter,
    content_key: SymmetricKey,
    test_nonce: Nonce | None = None,
) -> Envelope:
    """Add a recipient assertion with an optional test nonce."""
    assertion = _make_has_recipient(recipient, content_key, test_nonce)
    return self.add_assertion_envelope(assertion)


def recipients(self: Envelope) -> list[SealedMessage]:
    """Extract all ``SealedMessage`` objects from ``'hasRecipient'`` assertions."""
    result: list[SealedMessage] = []
    for assertion in self.assertions_with_predicate(known_values.HAS_RECIPIENT):
        obj = assertion.as_object()
        if obj.is_obscured():
            continue
        result.append(obj.extract_subject())
    return result


# ---------------------------------------------------------------------------
# encrypt_subject_to_recipients / encrypt_subject_to_recipient
# ---------------------------------------------------------------------------

def encrypt_subject_to_recipients(
    self: Envelope,
    recipient_list: list[Encrypter],
) -> Envelope:
    """Encrypt the subject to multiple recipients."""
    return encrypt_subject_to_recipients_opt(self, recipient_list, None)


def encrypt_subject_to_recipients_opt(
    self: Envelope,
    recipient_list: list[Encrypter],
    test_nonce: Nonce | None = None,
) -> Envelope:
    """Encrypt to multiple recipients with an optional test nonce."""
    from ._encrypt import encrypt_subject

    content_key = SymmetricKey.generate()
    e = encrypt_subject(self, content_key)
    for r in recipient_list:
        e = add_recipient_opt(e, r, content_key, test_nonce)
    return e


def encrypt_subject_to_recipient(
    self: Envelope,
    recipient: Encrypter,
) -> Envelope:
    """Encrypt the subject to a single recipient."""
    return encrypt_subject_to_recipients_opt(self, [recipient], None)


def encrypt_subject_to_recipient_opt(
    self: Envelope,
    recipient: Encrypter,
    test_nonce: Nonce | None = None,
) -> Envelope:
    """Encrypt to a single recipient with an optional test nonce."""
    return encrypt_subject_to_recipients_opt(self, [recipient], test_nonce)


# ---------------------------------------------------------------------------
# decrypt_subject_to_recipient
# ---------------------------------------------------------------------------

def decrypt_subject_to_recipient(
    self: Envelope,
    recipient: Decrypter,
) -> Envelope:
    """Decrypt the envelope's subject using the recipient's private key."""
    from ._encrypt import decrypt_subject
    from ._error import UnknownRecipient

    sealed_messages = recipients(self)
    content_key_data: bytes | None = None
    for sm in sealed_messages:
        try:
            content_key_data = sm.decrypt(recipient)
            break
        except Exception:
            continue

    if content_key_data is None:
        raise UnknownRecipient()

    content_key = SymmetricKey.from_tagged_cbor_data(content_key_data)
    return decrypt_subject(self, content_key)


# ---------------------------------------------------------------------------
# Convenience: encrypt_to_recipient / decrypt_to_recipient
# ---------------------------------------------------------------------------

def encrypt_to_recipient(self: Envelope, recipient: Encrypter) -> Envelope:
    """Wrap and encrypt to a single recipient."""
    return encrypt_subject_to_recipient(self.wrap(), recipient)


def decrypt_to_recipient(self: Envelope, recipient: Decrypter) -> Envelope:
    """Decrypt and unwrap from a single recipient."""
    return decrypt_subject_to_recipient(self, recipient).try_unwrap()


# ---------------------------------------------------------------------------
# Internal helpers
# ---------------------------------------------------------------------------

def _make_has_recipient(
    recipient: Encrypter,
    content_key: SymmetricKey,
    test_nonce: Nonce | None = None,
) -> Envelope:
    """Create a ``'hasRecipient': SealedMessage`` assertion envelope."""
    from ._envelope import Envelope as Env

    sealed_message = SealedMessage.new_opt(
        content_key.to_cbor_data(),
        recipient,
        None,
        test_nonce,
    )
    return Env.new_assertion(known_values.HAS_RECIPIENT, sealed_message)
