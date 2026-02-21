from __future__ import annotations

from collections.abc import Iterator


class ByteString:
    """Immutable byte sequence used as a CBOR byte-string value."""

    __slots__ = ("_data",)

    def __init__(self, data: bytes | bytearray | list[int] = b"") -> None:
        if isinstance(data, (bytes, bytearray)):
            self._data = bytes(data)
        else:
            self._data = bytes(data)

    @property
    def data(self) -> bytes:
        return self._data

    def __len__(self) -> int:
        return len(self._data)

    @property
    def is_empty(self) -> bool:
        return len(self._data) == 0

    def extend(self, other: bytes | bytearray) -> None:
        self._data = self._data + bytes(other)

    def to_bytes(self) -> bytes:
        return self._data

    def __iter__(self) -> Iterator[int]:
        return iter(self._data)

    def __getitem__(self, index: int | slice) -> int | bytes:
        return self._data[index]

    def __eq__(self, other: object) -> bool:
        if isinstance(other, ByteString):
            return self._data == other._data
        if isinstance(other, (bytes, bytearray)):
            return self._data == bytes(other)
        return NotImplemented

    def __lt__(self, other: ByteString) -> bool:
        return self._data < other._data

    def __le__(self, other: ByteString) -> bool:
        return self._data <= other._data

    def __gt__(self, other: ByteString) -> bool:
        return self._data > other._data

    def __ge__(self, other: ByteString) -> bool:
        return self._data >= other._data

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"ByteString({self._data!r})"

    def __str__(self) -> str:
        return self._data.hex()

    def __bytes__(self) -> bytes:
        return self._data
