from __future__ import annotations

from typing import Callable


class BitEnumerator:
    """Reads individual bits from a byte sequence."""

    def __init__(self, data: bytes | bytearray) -> None:
        self._data = bytes(data)
        self._index = 0
        self._mask = 0x80

    def has_next(self) -> bool:
        return self._mask != 0 or self._index != len(self._data) - 1

    def next(self) -> bool:
        if not self.has_next():
            raise ValueError("BitEnumerator underflow")

        if self._mask == 0:
            self._mask = 0x80
            self._index += 1

        b = (self._data[self._index] & self._mask) != 0
        self._mask >>= 1
        return b

    def next_uint2(self) -> int:
        bit_mask = 0x02
        value = 0
        for _ in range(2):
            if self.next():
                value |= bit_mask
            bit_mask >>= 1
        return value

    def next_uint8(self) -> int:
        bit_mask = 0x80
        value = 0
        for _ in range(8):
            if self.next():
                value |= bit_mask
            bit_mask >>= 1
        return value

    def next_uint16(self) -> int:
        bit_mask = 0x8000
        value = 0
        for _ in range(16):
            if self.next():
                value |= bit_mask
            bit_mask >>= 1
        return value

    def next_frac(self) -> float:
        return self.next_uint16() / 65535.0

    def for_all(self, f: Callable[[bool], None]) -> None:
        while self.has_next():
            f(self.next())


class BitAggregator:
    """Packs individual bits into a byte sequence."""

    def __init__(self) -> None:
        self._data = bytearray()
        self._bit_mask = 0

    def append(self, bit: bool) -> None:
        if self._bit_mask == 0:
            self._bit_mask = 0x80
            self._data.append(0)

        if bit:
            self._data[-1] |= self._bit_mask

        self._bit_mask >>= 1

    def data(self) -> bytes:
        return bytes(self._data)
