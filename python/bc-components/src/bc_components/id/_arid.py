"""ARID (Apparently Random Identifier)."""

from __future__ import annotations

import functools

from bc_rand import random_data
from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_ARID,
)

from .._error import InvalidSizeError


ARID_SIZE: int = 32


@functools.total_ordering
class ARID:
    """An Apparently Random Identifier (ARID).

    An ARID is a cryptographically strong, universally unique 256-bit
    (32-byte) identifier with the following properties:

    - Non-correlatability: cannot be correlated with its referent
    - Neutral semantics: contains no inherent type information
    - Open generation: any method producing statistically random bits
    - Minimum strength: 256 bits (32 bytes)
    - Cryptographic suitability: suitable as inputs to crypto constructs

    As defined in BCR-2022-002.
    """

    ARID_SIZE: int = 32

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != self.ARID_SIZE:
            raise InvalidSizeError("ARID", self.ARID_SIZE, len(data))
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def generate() -> ARID:
        """Create a new random ARID."""
        return ARID(random_data(ARID_SIZE))

    @staticmethod
    def from_data(data: bytes | bytearray | memoryview) -> ARID:
        """Create an ARID from a byte-like object, validating length."""
        return ARID(bytes(data))

    @staticmethod
    def from_hex(hex_str: str) -> ARID:
        """Create an ARID from a hexadecimal string.

        Raises ValueError if the string is not exactly 64 hex digits.
        """
        return ARID(bytes.fromhex(hex_str))

    # --- Properties ---

    @property
    def data(self) -> bytes:
        """The raw 32-byte ARID."""
        return self._data

    def hex(self) -> str:
        """Return the ARID as a hexadecimal string."""
        return self._data.hex()

    def short_description(self) -> str:
        """Return the first four bytes as a hex string."""
        return self._data[:4].hex()

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_ARID])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_bytes(self._data)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> ARID:
        data = cbor.try_byte_string()
        return ARID.from_data(data)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> ARID:
        tags = ARID.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return ARID.from_untagged_cbor(item)

    # --- UR ---

    def ur_string(self) -> str:
        from bc_ur import to_ur_string
        return to_ur_string(self)

    @staticmethod
    def from_ur_string(ur_string: str) -> ARID:
        from bc_ur import from_ur_string
        return from_ur_string(ARID, ur_string)

    # --- Dunder ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, ARID):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __lt__(self, other: ARID) -> bool:
        if not isinstance(other, ARID):
            return NotImplemented  # type: ignore[return-value]
        return self._data < other._data

    def __repr__(self) -> str:
        return f"ARID({self.hex()})"

    def __str__(self) -> str:
        return f"ARID({self.hex()})"

    def __bytes__(self) -> bytes:
        return self._data
