"""CBOR-tagged container for UTF-8 JSON text.

Wraps JSON text as a CBOR byte string with tag 262 (``TAG_JSON``).
"""

from __future__ import annotations

from bc_tags import TAG_JSON, tags_for_values
from dcbor import CBOR, Tag


class JSON:
    """A CBOR-tagged container for UTF-8 JSON text."""

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        self._data = bytes(data)

    # --- Construction --------------------------------------------------

    @staticmethod
    def from_data(data: bytes | bytearray) -> JSON:
        """Create a JSON instance from raw bytes."""
        return JSON(bytes(data))

    @staticmethod
    def from_string(s: str) -> JSON:
        """Create a JSON instance from a string."""
        return JSON(s.encode("utf-8"))

    @staticmethod
    def from_hex(hex_str: str) -> JSON:
        """Create a JSON instance from a hex string."""
        return JSON(bytes.fromhex(hex_str))

    # --- Accessors -----------------------------------------------------

    def __len__(self) -> int:
        return len(self._data)

    def __bool__(self) -> bool:
        return len(self._data) > 0

    @property
    def data(self) -> bytes:
        """The raw byte data."""
        return self._data

    def as_str(self) -> str:
        """Return the data as a UTF-8 string.

        Raises ``UnicodeDecodeError`` if the data is not valid UTF-8.
        """
        return self._data.decode("utf-8")

    def hex(self) -> str:
        """Return the data as a hex string."""
        return self._data.hex()

    # --- CBOR ----------------------------------------------------------

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_JSON])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_bytes(self._data)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @classmethod
    def from_untagged_cbor(cls, cbor: CBOR) -> JSON:
        data = cbor.try_byte_string()
        return cls.from_data(data)

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> JSON:
        tags = cls.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return cls.from_untagged_cbor(item)

    @classmethod
    def from_tagged_cbor_data(cls, data: bytes) -> JSON:
        cbor = CBOR.from_data(data)
        return cls.from_tagged_cbor(cbor)

    @classmethod
    def from_untagged_cbor_data(cls, data: bytes) -> JSON:
        cbor = CBOR.from_data(data)
        return cls.from_untagged_cbor(cbor)

    # --- Dunder methods ------------------------------------------------

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, JSON):
            return NotImplemented
        return self._data == other._data

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"JSON({self.as_str()})"

    def __bytes__(self) -> bytes:
        return self._data
