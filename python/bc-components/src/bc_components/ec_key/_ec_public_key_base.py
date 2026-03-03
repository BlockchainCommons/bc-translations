"""Protocol for public keys that can provide their uncompressed form."""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, runtime_checkable

if TYPE_CHECKING:
    from ._ec_uncompressed_public_key import ECUncompressedPublicKey


@runtime_checkable
class ECPublicKeyBase(Protocol):
    """Protocol for EC public keys that can provide their uncompressed form.

    Elliptic curve public keys can be represented in both compressed (33 bytes)
    and uncompressed (65 bytes) formats:

    - Compressed format: Uses a single byte prefix (0x02 or 0x03) followed by
      the x-coordinate (32 bytes).
    - Uncompressed format: Uses a byte prefix (0x04) followed by both x and y
      coordinates (32 bytes each), for a total of 65 bytes.
    """

    def uncompressed_public_key(self) -> ECUncompressedPublicKey:
        """Return the uncompressed public key representation."""
        ...
