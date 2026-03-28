"""Secret (password / SSH agent) locking extension for Gordian Envelope.

Encrypts the subject with a derived key and attaches a ``'hasSecret'``
assertion containing an ``EncryptedKey``.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_components import EncryptedKey, SymmetricKey
from bc_components.encrypted_key import KeyDerivationMethod
import known_values

if TYPE_CHECKING:
    from ._envelope import Envelope


# ---------------------------------------------------------------------------
# lock_subject / unlock_subject
# ---------------------------------------------------------------------------

def lock_subject(
    self: Envelope,
    method: KeyDerivationMethod,
    secret: bytes | bytearray,
) -> Envelope:
    """Encrypt the subject and add a ``'hasSecret': EncryptedKey`` assertion."""
    from ._encrypt import encrypt_subject

    content_key = SymmetricKey.generate()
    encrypted_key = EncryptedKey.lock(method, secret, content_key)
    return encrypt_subject(self, content_key).add_assertion(
        known_values.HAS_SECRET, encrypted_key,
    )


def unlock_subject(self: Envelope, secret: bytes | bytearray) -> Envelope:
    """Unlock the subject by finding and decrypting the matching ``EncryptedKey``."""
    from ._encrypt import decrypt_subject
    from ._error import UnknownSecret

    for assertion in self.assertions_with_predicate(known_values.HAS_SECRET):
        obj = assertion.as_object()
        if obj.is_obscured():
            continue
        encrypted_key: EncryptedKey = obj.extract_subject()
        try:
            content_key = encrypted_key.unlock(secret)
        except Exception:
            continue
        return decrypt_subject(self, content_key)

    raise UnknownSecret()


# ---------------------------------------------------------------------------
# is_locked_with_password / is_locked_with_ssh_agent
# ---------------------------------------------------------------------------

def is_locked_with_password(self: Envelope) -> bool:
    """Return whether the envelope has a password-based ``'hasSecret'`` assertion."""
    for assertion in self.assertions_with_predicate(known_values.HAS_SECRET):
        obj = assertion.as_object()
        try:
            encrypted_key: EncryptedKey = obj.extract_subject()
            if encrypted_key.is_password_based:
                return True
        except Exception:
            continue
    return False


def is_locked_with_ssh_agent(self: Envelope) -> bool:
    """Return whether the envelope has an SSH-agent-based ``'hasSecret'`` assertion."""
    for assertion in self.assertions_with_predicate(known_values.HAS_SECRET):
        obj = assertion.as_object()
        try:
            encrypted_key: EncryptedKey = obj.extract_subject()
            if encrypted_key.is_ssh_agent:
                return True
        except Exception:
            continue
    return False


# ---------------------------------------------------------------------------
# add_secret (low-level)
# ---------------------------------------------------------------------------

def add_secret(
    self: Envelope,
    method: KeyDerivationMethod,
    secret: bytes | bytearray,
    content_key: SymmetricKey,
) -> Envelope:
    """Add a ``'hasSecret': EncryptedKey`` assertion with the given content key."""
    encrypted_key = EncryptedKey.lock(method, secret, content_key)
    return self.add_assertion(known_values.HAS_SECRET, encrypted_key)


# ---------------------------------------------------------------------------
# Convenience: lock / unlock (wrap+lock / unlock+unwrap)
# ---------------------------------------------------------------------------

def lock(
    self: Envelope,
    method: KeyDerivationMethod,
    secret: bytes | bytearray,
) -> Envelope:
    """Wrap and lock the entire envelope."""
    return lock_subject(self.wrap(), method, secret)


def unlock(self: Envelope, secret: bytes | bytearray) -> Envelope:
    """Unlock and unwrap the entire envelope."""
    return unlock_subject(self, secret).try_unwrap()
