"""KeyDerivation protocol for key derivation implementations."""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, runtime_checkable

if TYPE_CHECKING:
    from ..symmetric._encrypted_message import EncryptedMessage
    from ..symmetric._symmetric_key import SymmetricKey


@runtime_checkable
class KeyDerivation(Protocol):
    """Protocol for key derivation implementations.

    Each implementation must provide an INDEX class variable identifying
    its derivation method, and implement lock/unlock operations.
    """

    INDEX: int

    def lock(
        self,
        content_key: SymmetricKey,
        secret: bytes | bytearray,
    ) -> EncryptedMessage:
        """Derive a key from *secret* and encrypt *content_key* with it."""
        ...

    def unlock(
        self,
        encrypted_message: EncryptedMessage,
        secret: bytes | bytearray,
    ) -> SymmetricKey:
        """Derive a key from *secret* and decrypt the content key from *encrypted_message*."""
        ...
