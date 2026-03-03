"""Hash type enum for key derivation."""

from __future__ import annotations

from enum import IntEnum

from bc_tags import CBOR


class HashType(IntEnum):
    """Supported hash types for key derivation.

    CDDL::

        HashType = SHA256 / SHA512
        SHA256 = 0
        SHA512 = 1
    """

    SHA256 = 0
    SHA512 = 1

    def __str__(self) -> str:
        return self.name

    def to_cbor(self) -> CBOR:
        """Encode this hash type as a CBOR unsigned integer."""
        return CBOR.from_int(self.value)

    @staticmethod
    def from_cbor(cbor: CBOR) -> HashType:
        """Decode a hash type from a CBOR unsigned integer."""
        i = cbor.try_int()
        try:
            return HashType(i)
        except ValueError:
            raise ValueError(f"Invalid HashType: {i}") from None
