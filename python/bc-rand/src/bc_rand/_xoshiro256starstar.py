"""Xoshiro256** PRNG implementation.

Internal module -- not part of the public API.
Reference: https://prng.di.unimi.it/xoshiro256starstar.c
"""

from ._constants import MASK64 as _MASK64


def _rotl64(x: int, k: int) -> int:
    return ((x << k) | (x >> (64 - k))) & _MASK64


class Xoshiro256StarStar:
    __slots__ = ("_s",)

    def __init__(self, s0: int, s1: int, s2: int, s3: int) -> None:
        self._s = [s0 & _MASK64, s1 & _MASK64, s2 & _MASK64, s3 & _MASK64]

    def next_u64(self) -> int:
        s = self._s
        result = (_rotl64((s[1] * 5) & _MASK64, 7) * 9) & _MASK64
        t = (s[1] << 17) & _MASK64

        s[2] ^= s[0]
        s[3] ^= s[1]
        s[1] ^= s[2]
        s[0] ^= s[3]
        s[2] ^= t
        s[3] = _rotl64(s[3], 45)

        return result
