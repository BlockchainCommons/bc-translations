"""Xoshiro256** PRNG for deterministic fountain code generation.

This is a standalone implementation matching the `rand_xoshiro` crate's
Xoshiro256StarStar, seeded via SHA-256 as done in the `ur` crate.
"""

from __future__ import annotations

import hashlib

from ._crc32 import crc32

_MASK64 = (1 << 64) - 1


def _rotl(x: int, k: int) -> int:
    return ((x << k) | (x >> (64 - k))) & _MASK64


class Xoshiro256:
    """Xoshiro256** pseudo-random number generator."""

    __slots__ = ("_state",)

    def __init__(self, state: list[int]) -> None:
        self._state = list(state)

    @classmethod
    def from_bytes(cls, data: bytes) -> Xoshiro256:
        """Seed from a 32-byte array.

        Each 8-byte group is interpreted as big-endian u64 to form the
        4-element state, matching the ur crate's seeding transform.
        """
        if len(data) != 32:
            raise ValueError(f"seed must be 32 bytes, got {len(data)}")
        state = []
        for i in range(4):
            v = int.from_bytes(data[8 * i : 8 * i + 8], "big")
            state.append(v)
        return cls(state)

    @classmethod
    def from_string(cls, s: str) -> Xoshiro256:
        """Seed from a string via SHA-256."""
        hash_bytes = hashlib.sha256(s.encode()).digest()
        return cls.from_bytes(hash_bytes)

    @classmethod
    def from_data(cls, data: bytes) -> Xoshiro256:
        """Seed from arbitrary bytes via SHA-256."""
        hash_bytes = hashlib.sha256(data).digest()
        return cls.from_bytes(hash_bytes)

    @classmethod
    def from_crc(cls, data: bytes) -> Xoshiro256:
        """Seed from the CRC32 checksum of data."""
        checksum = crc32(data)
        return cls.from_data(checksum.to_bytes(4, "big"))

    def next_u64(self) -> int:
        """Generate the next 64-bit unsigned integer."""
        s = self._state
        result = (_rotl((s[1] * 5) & _MASK64, 7) * 9) & _MASK64

        t = (s[1] << 17) & _MASK64

        s[2] ^= s[0]
        s[3] ^= s[1]
        s[1] ^= s[2]
        s[0] ^= s[3]
        s[2] ^= t
        s[3] = _rotl(s[3], 45)

        return result

    def next_double(self) -> float:
        """Generate a random float in [0, 1)."""
        return self.next_u64() / (_MASK64 + 1.0)

    def next_int(self, low: int, high: int) -> int:
        """Generate a random integer in [low, high]."""
        return int(self.next_double() * (high - low + 1)) + low

    def next_byte(self) -> int:
        """Generate a random byte [0, 255]."""
        return self.next_int(0, 255)

    def next_bytes(self, n: int) -> bytes:
        """Generate n random bytes."""
        return bytes(self.next_byte() for _ in range(n))

    def shuffled(self, items: list) -> list:
        """Return a shuffled copy of the list."""
        remaining = list(items)
        result = []
        while remaining:
            index = self.next_int(0, len(remaining) - 1)
            result.append(remaining.pop(index))
        return result

    def choose_degree(self, length: int) -> int:
        """Choose a degree for fountain encoding using weighted sampling."""
        from ._weighted_sampler import WeightedSampler

        degree_weights = [1.0 / x for x in range(1, length + 1)]
        sampler = WeightedSampler(degree_weights)
        return sampler.next(self) + 1


def make_message(seed: str, size: int) -> bytes:
    """Generate a pseudo-random message for testing."""
    rng = Xoshiro256.from_string(seed)
    return rng.next_bytes(size)
