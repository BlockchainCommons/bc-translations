"""UUID (Universally Unique Identifier)."""

from __future__ import annotations

from bc_rand import fill_random_data
from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_UUID,
)


UUID_SIZE: int = 16


class UUID:
    """A Universally Unique Identifier (UUID).

    UUIDs are 128-bit (16-byte) identifiers.  This implementation
    creates type 4 (random) UUIDs following the UUID specification:

    - Version field (bits 48-51) is set to 4 (random UUID)
    - Variant field (bits 64-65) is set to 2 (RFC 4122/DCE 1.1 variant)
    """

    UUID_SIZE: int = 16

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != self.UUID_SIZE:
            raise ValueError(
                f"invalid UUID size: expected {self.UUID_SIZE}, got {len(data)}"
            )
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def generate() -> UUID:
        """Create a new type 4 (random) UUID."""
        buf = bytearray(UUID_SIZE)
        fill_random_data(buf)
        buf[6] = (buf[6] & 0x0F) | 0x40  # version 4
        buf[8] = (buf[8] & 0x3F) | 0x80  # variant 2
        return UUID(bytes(buf))

    @staticmethod
    def from_data(data: bytes | bytearray | memoryview) -> UUID:
        """Restore a UUID from a byte-like object, validating length."""
        return UUID(bytes(data))

    @staticmethod
    def from_string(s: str) -> UUID:
        """Parse a UUID from the standard string format (with or without dashes)."""
        s = s.strip().replace("-", "")
        return UUID(bytes.fromhex(s))

    # --- Properties ---

    @property
    def data(self) -> bytes:
        """The raw 16-byte UUID."""
        return self._data

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_UUID])

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
    def from_untagged_cbor(cbor: CBOR) -> UUID:
        data = cbor.try_byte_string()
        if len(data) != UUID_SIZE:
            raise ValueError("invalid UUID size")
        return UUID(data)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> UUID:
        tags = UUID.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return UUID.from_untagged_cbor(item)

    # --- Display ---

    def to_string(self) -> str:
        """Return the standard UUID string format with dashes."""
        h = self._data.hex()
        return f"{h[0:8]}-{h[8:12]}-{h[12:16]}-{h[16:20]}-{h[20:32]}"

    # --- Dunder ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, UUID):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"UUID({self.to_string()})"

    def __str__(self) -> str:
        return self.to_string()

    def __bytes__(self) -> bytes:
        return self._data
