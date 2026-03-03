"""Protocol for types that can supply private key data."""

from __future__ import annotations

from typing import Protocol, runtime_checkable


@runtime_checkable
class PrivateKeyDataProvider(Protocol):
    """A type that can provide unique data for cryptographic key derivation."""

    def private_key_data(self) -> bytes: ...
