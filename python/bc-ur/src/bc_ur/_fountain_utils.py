"""Utility functions for fountain encoding/decoding."""

from __future__ import annotations

from ._crc32 import crc32
from ._xoshiro256 import Xoshiro256


def _div_ceil(a: int, b: int) -> int:
    """Integer division rounding towards positive infinity."""
    d, r = divmod(a, b)
    return d + (1 if r > 0 else 0)


def fragment_length(data_length: int, max_fragment_length: int) -> int:
    """Calculate the effective fragment length for fountain encoding."""
    fragment_count = _div_ceil(data_length, max_fragment_length)
    return _div_ceil(data_length, fragment_count)


def partition(data: bytes, frag_length: int) -> list[bytes]:
    """Partition data into fragments, padding the last one with zeros."""
    padding_needed = (frag_length - (len(data) % frag_length)) % frag_length
    padded = data + b"\x00" * padding_needed
    return [padded[i : i + frag_length] for i in range(0, len(padded), frag_length)]


def choose_fragments(sequence: int, fragment_count: int, checksum: int) -> list[int]:
    """Choose which fragment indexes to combine for a given sequence number."""
    if sequence <= fragment_count:
        return [sequence - 1]

    seed = sequence.to_bytes(4, "big") + checksum.to_bytes(4, "big")
    xoshiro = Xoshiro256.from_data(seed)
    degree = xoshiro.choose_degree(fragment_count)
    indexes = list(range(fragment_count))
    shuffled = xoshiro.shuffled(indexes)
    return shuffled[:degree]


def xor_bytes(v1: bytearray, v2: bytes) -> None:
    """XOR v2 into v1 in-place. Both must have the same length."""
    if len(v1) != len(v2):
        raise ValueError(
            f"XOR operands must have equal length ({len(v1)} != {len(v2)})"
        )
    for i in range(len(v1)):
        v1[i] ^= v2[i]
