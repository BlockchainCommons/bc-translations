"""Fountain code part with manual CBOR serialization.

Fountain parts are encoded as CBOR arrays matching the minicbor format:
  [sequence, sequence_count, message_length, checksum, data]

We use manual CBOR construction (not dcbor) to produce byte-identical output
to minicbor, which uses standard (non-deterministic) CBOR encoding.
"""

from __future__ import annotations

from ._fountain_utils import choose_fragments
from .error import FountainError


def _encode_cbor_unsigned(value: int) -> bytes:
    """Encode an unsigned integer in CBOR (major type 0)."""
    if value < 0:
        raise FountainError("CBOR unsigned integer must be non-negative")
    if value <= 23:
        return bytes([value])
    elif value <= 0xFF:
        return bytes([0x18, value])
    elif value <= 0xFFFF:
        return bytes([0x19]) + value.to_bytes(2, "big")
    elif value <= 0xFFFFFFFF:
        return bytes([0x1A]) + value.to_bytes(4, "big")
    else:
        return bytes([0x1B]) + value.to_bytes(8, "big")


def _encode_cbor_bytes(data: bytes) -> bytes:
    """Encode a byte string in CBOR (major type 2)."""
    length = len(data)
    if length <= 23:
        return bytes([0x40 + length]) + data
    elif length <= 0xFF:
        return bytes([0x58, length]) + data
    elif length <= 0xFFFF:
        return bytes([0x59]) + length.to_bytes(2, "big") + data
    else:
        return bytes([0x5A]) + length.to_bytes(4, "big") + data


def _decode_cbor_unsigned(data: memoryview, pos: int) -> tuple[int, int]:
    """Decode a CBOR unsigned integer, returning (value, new_pos)."""
    first = data[pos]
    major = first >> 5
    info = first & 0x1F
    if major != 0:
        raise FountainError(f"expected unsigned integer at position {pos}")
    if info <= 23:
        return info, pos + 1
    elif info == 24:
        return data[pos + 1], pos + 2
    elif info == 25:
        return int.from_bytes(data[pos + 1 : pos + 3], "big"), pos + 3
    elif info == 26:
        return int.from_bytes(data[pos + 1 : pos + 5], "big"), pos + 5
    elif info == 27:
        val = int.from_bytes(data[pos + 1 : pos + 9], "big")
        if val > 0xFFFFFFFF:
            raise FountainError("value exceeds u32 range")
        return val, pos + 9
    else:
        raise FountainError(f"unsupported CBOR additional info {info}")


def _decode_cbor_bytes(data: memoryview, pos: int) -> tuple[bytes, int]:
    """Decode a CBOR byte string, returning (bytes, new_pos)."""
    first = data[pos]
    major = first >> 5
    info = first & 0x1F
    if major != 2:
        raise FountainError(f"expected byte string at position {pos}")
    if info <= 23:
        length = info
        start = pos + 1
    elif info == 24:
        length = data[pos + 1]
        start = pos + 2
    elif info == 25:
        length = int.from_bytes(data[pos + 1 : pos + 3], "big")
        start = pos + 3
    elif info == 26:
        length = int.from_bytes(data[pos + 1 : pos + 5], "big")
        start = pos + 5
    else:
        raise FountainError(f"unsupported byte string length encoding at position {pos}")
    return bytes(data[start : start + length]), start + length


class FountainPart:
    """A part emitted by a fountain encoder."""

    __slots__ = ("_sequence", "_sequence_count", "_message_length", "_checksum", "_data")

    def __init__(
        self,
        sequence: int,
        sequence_count: int,
        message_length: int,
        checksum: int,
        data: bytes,
    ) -> None:
        self._sequence = sequence
        self._sequence_count = sequence_count
        self._message_length = message_length
        self._checksum = checksum
        self._data = data

    @property
    def sequence(self) -> int:
        return self._sequence

    @property
    def sequence_count(self) -> int:
        return self._sequence_count

    @property
    def message_length(self) -> int:
        return self._message_length

    @property
    def checksum(self) -> int:
        return self._checksum

    @property
    def data(self) -> bytes:
        return self._data

    def indexes(self) -> list[int]:
        """Return the fragment indexes combined into this part."""
        return choose_fragments(self._sequence, self._sequence_count, self._checksum)

    def is_simple(self) -> bool:
        """Whether this part represents a single original fragment."""
        return len(self.indexes()) == 1

    def sequence_id(self) -> str:
        """Return the sequence identifier string (e.g., '1-9')."""
        return f"{self._sequence}-{self._sequence_count}"

    def to_cbor(self) -> bytes:
        """Encode this part as CBOR bytes (matching minicbor format)."""
        parts = [
            bytes([0x85]),  # array of 5
            _encode_cbor_unsigned(self._sequence),
            _encode_cbor_unsigned(self._sequence_count),
            _encode_cbor_unsigned(self._message_length),
            _encode_cbor_unsigned(self._checksum),
            _encode_cbor_bytes(self._data),
        ]
        return b"".join(parts)

    @classmethod
    def from_cbor(cls, cbor_data: bytes) -> FountainPart:
        """Decode a FountainPart from CBOR bytes."""
        data = memoryview(cbor_data)
        pos = 0

        # Expect array of 5 (CBOR header byte 0x85)
        if data[pos] != 0x85:
            raise FountainError(
                f"expected CBOR array of 5 elements, got 0x{data[pos]:02x}"
            )
        pos += 1

        sequence, pos = _decode_cbor_unsigned(data, pos)
        sequence_count, pos = _decode_cbor_unsigned(data, pos)
        message_length, pos = _decode_cbor_unsigned(data, pos)
        checksum, pos = _decode_cbor_unsigned(data, pos)
        part_data, pos = _decode_cbor_bytes(data, pos)

        return cls(sequence, sequence_count, message_length, checksum, part_data)

    def __eq__(self, other: object) -> bool:
        if isinstance(other, FountainPart):
            return (
                self._sequence == other._sequence
                and self._sequence_count == other._sequence_count
                and self._message_length == other._message_length
                and self._checksum == other._checksum
                and self._data == other._data
            )
        return NotImplemented

    def __repr__(self) -> str:
        return (
            f"FountainPart(seq={self._sequence}, count={self._sequence_count}, "
            f"msglen={self._message_length}, checksum=0x{self._checksum:08x}, "
            f"datalen={len(self._data)})"
        )
