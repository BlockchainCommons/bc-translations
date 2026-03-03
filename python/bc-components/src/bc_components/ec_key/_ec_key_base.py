"""Base traits (protocols) for elliptic curve keys."""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, runtime_checkable

if TYPE_CHECKING:
    from ._ec_public_key import ECPublicKey


@runtime_checkable
class ECKeyBase(Protocol):
    """Protocol for all elliptic curve key types.

    All EC key types have a fixed size depending on their specific type:
    - EC private keys: 32 bytes
    - EC compressed public keys: 33 bytes
    - EC uncompressed public keys: 65 bytes
    - Schnorr public keys: 32 bytes
    """

    KEY_SIZE: int

    @staticmethod
    def from_data(data: bytes | bytearray) -> ECKeyBase:
        """Create a key from binary data, with size validation."""
        ...

    @property
    def data(self) -> bytes:
        """Return the key's binary data."""
        ...

    def hex(self) -> str:
        """Return the key as a hexadecimal string."""
        ...

    @classmethod
    def from_hex(cls, hex_str: str) -> ECKeyBase:
        """Create a key from a hexadecimal string."""
        ...


@runtime_checkable
class ECKey(ECKeyBase, Protocol):
    """Protocol for EC keys that can derive a compressed public key."""

    def public_key(self) -> ECPublicKey:
        """Return the compressed public key corresponding to this key."""
        ...
