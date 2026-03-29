"""Deterministic Xoshiro256** implementation."""

from __future__ import annotations

from bc_rand import RandomNumberGenerator

_MASK64 = 0xFFFFFFFFFFFFFFFF


def _rotate_left_64(value: int, shift: int) -> int:
    value &= _MASK64
    return ((value << shift) | (value >> (64 - shift))) & _MASK64


class Xoshiro256StarStar(RandomNumberGenerator):
    """A deterministic xoshiro256** PRNG with Rust-compatible state layout."""

    __slots__ = ("_state",)

    def __init__(self, state: tuple[int, int, int, int]) -> None:
        self._state = tuple(part & _MASK64 for part in state)

    @staticmethod
    def from_state(state: tuple[int, int, int, int] | list[int]) -> Xoshiro256StarStar:
        if len(state) != 4:
            raise ValueError("Xoshiro256StarStar state must have exactly 4 words")
        return Xoshiro256StarStar(tuple(int(part) for part in state))

    def to_state(self) -> tuple[int, int, int, int]:
        return self._state

    @staticmethod
    def from_data(data: bytes | bytearray) -> Xoshiro256StarStar:
        if len(data) != 32:
            raise ValueError("Xoshiro256StarStar data must be exactly 32 bytes")
        buffer = bytes(data)
        return Xoshiro256StarStar(
            tuple(
                int.from_bytes(buffer[index * 8 : (index + 1) * 8], "little")
                for index in range(4)
            )
        )

    def to_data(self) -> bytes:
        return b"".join(part.to_bytes(8, "little") for part in self._state)

    def next_u64(self) -> int:
        s0, s1, s2, s3 = self._state
        result = (_rotate_left_64((s1 * 5) & _MASK64, 7) * 9) & _MASK64
        temp = (s1 << 17) & _MASK64

        s2 ^= s0
        s3 ^= s1
        s1 ^= s2
        s0 ^= s3
        s2 ^= temp
        s3 = _rotate_left_64(s3, 45)

        self._state = (
            s0 & _MASK64,
            s1 & _MASK64,
            s2 & _MASK64,
            s3 & _MASK64,
        )
        return result

    def next_u32(self) -> int:
        return (self.next_u64() >> 32) & 0xFFFFFFFF

    def next_byte(self) -> int:
        return self.next_u64() & 0xFF

    def next_bytes(self, length: int) -> bytes:
        return bytes(self.next_byte() for _ in range(length))

    def clone(self) -> Xoshiro256StarStar:
        return Xoshiro256StarStar(self._state)

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Xoshiro256StarStar):
            return NotImplemented
        return self._state == other._state

    def __repr__(self) -> str:
        return f"Xoshiro256StarStar(state={self._state!r})"

