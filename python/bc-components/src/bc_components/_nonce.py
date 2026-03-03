"""Cryptographic nonce (number used once).

A 12-byte random value used in authenticated encryption and other
cryptographic protocols.
"""

from __future__ import annotations

from bc_rand import fill_random_data
from bc_tags import TAG_NONCE, tags_for_values
from dcbor import CBOR, Tag

from ._error import BCComponentsError


NONCE_SIZE: int = 12


class Nonce:
    """A 12-byte cryptographic nonce."""

    NONCE_SIZE: int = 12

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != self.NONCE_SIZE:
            raise BCComponentsError.invalid_size("nonce", self.NONCE_SIZE, len(data))
        self._data = bytes(data)

    # --- Construction --------------------------------------------------

    @staticmethod
    def generate() -> Nonce:
        """Generate a new random nonce."""
        buf = bytearray(Nonce.NONCE_SIZE)
        fill_random_data(buf)
        return Nonce(bytes(buf))

    @staticmethod
    def from_data(data: bytes | bytearray) -> Nonce:
        """Create a nonce from exactly 12 bytes."""
        return Nonce(bytes(data))

    @staticmethod
    def from_hex(hex_str: str) -> Nonce:
        """Create a nonce from a 24-character hex string.

        Raises ``ValueError`` if the hex is invalid or the wrong length.
        """
        return Nonce(bytes.fromhex(hex_str))

    # --- Accessors -----------------------------------------------------

    @property
    def data(self) -> bytes:
        """The raw 12-byte nonce."""
        return self._data

    def hex(self) -> str:
        """The nonce as a 24-character lowercase hex string."""
        return self._data.hex()

    # --- CBOR ----------------------------------------------------------

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_NONCE])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_bytes(self._data)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @classmethod
    def from_untagged_cbor(cls, cbor: CBOR) -> Nonce:
        data = cbor.try_byte_string()
        return cls.from_data(data)

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> Nonce:
        tags = cls.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return cls.from_untagged_cbor(item)

    @classmethod
    def from_tagged_cbor_data(cls, data: bytes) -> Nonce:
        cbor = CBOR.from_data(data)
        return cls.from_tagged_cbor(cbor)

    @classmethod
    def from_untagged_cbor_data(cls, data: bytes) -> Nonce:
        cbor = CBOR.from_data(data)
        return cls.from_untagged_cbor(cbor)

    # --- Dunder methods ------------------------------------------------

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Nonce):
            return NotImplemented
        return self._data == other._data

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"Nonce({self.hex()})"

    def __bytes__(self) -> bytes:
        return self._data
