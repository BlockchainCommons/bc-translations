"""RandomNumberGenerator ABC and free functions."""

from abc import ABC, abstractmethod
from typing import Literal

from ._constants import MASK32 as _MASK32

BitWidth = Literal[8, 16, 32, 64]


class RandomNumberGenerator(ABC):
    """Abstract base for random number generators.

    Concrete subclasses must implement ``next_u32`` and ``next_u64``.
    """

    @abstractmethod
    def next_u32(self) -> int:
        """Return a random unsigned 32-bit integer."""
        ...

    @abstractmethod
    def next_u64(self) -> int:
        """Return a random unsigned 64-bit integer."""
        ...

    def random_data(self, size: int) -> bytes:
        """Return a bytes object of random bytes of the given size."""
        data = bytearray(size)
        self.fill_random_data(data)
        return bytes(data)

    def fill_random_data(self, data: bytearray) -> None:
        i = 0
        while i + 8 <= len(data):
            val = self.next_u64()
            data[i : i + 8] = val.to_bytes(8, "little")
            i += 8
        if i < len(data):
            val = self.next_u64()
            remaining = val.to_bytes(8, "little")
            data[i:] = remaining[: len(data) - i]


# ---------------------------------------------------------------------------
# Free functions
# ---------------------------------------------------------------------------


def rng_random_data(rng: RandomNumberGenerator, size: int) -> bytes:
    """Return a bytes object of random bytes of the given size."""
    data = bytearray(size)
    rng.fill_random_data(data)
    return bytes(data)


def rng_fill_random_data(rng: RandomNumberGenerator, data: bytearray) -> None:
    """Fill the given bytearray with random bytes."""
    rng.fill_random_data(data)


def rng_next_with_upper_bound(
    rng: RandomNumberGenerator,
    upper_bound: int,
    *,
    bits: BitWidth = 64,
) -> int:
    """Return a random value that is less than the given upper bound.

    Uses Lemire's "nearly divisionless" method for generating random integers
    in an interval.

    Args:
        rng: The random number generator to use.
        upper_bound: The upper bound for the randomly generated value.
            Must be non-zero.
        bits: The unsigned integer width (8, 16, 32, or 64) used for the
            algorithm.

    Returns:
        A random value in ``[0, upper_bound)``. Every value in the range
        is equally likely to be returned.
    """
    if upper_bound <= 0:
        raise ValueError(f"upper_bound must be positive, got {upper_bound}")
    mask = (1 << bits) - 1

    random = rng.next_u64() & mask
    m_full = random * upper_bound
    m_low = m_full & mask
    m_high = m_full >> bits

    if m_low < upper_bound:
        t = ((1 << bits) - upper_bound) % upper_bound
        while m_low < t:
            random = rng.next_u64() & mask
            m_full = random * upper_bound
            m_low = m_full & mask
            m_high = m_full >> bits

    return m_high


def rng_next_in_range(
    rng: RandomNumberGenerator,
    start: int,
    end: int,
    *,
    bits: BitWidth = 64,
) -> int:
    """Return a random value within the specified range.

    Use this function to generate an integer within a specific range using a
    custom random number generator.

    Args:
        rng: The random number generator to use as a source for randomness.
        start: The lower bound (inclusive) of the range.
        end: The upper bound (exclusive) of the range.
        bits: The unsigned integer width (8, 16, 32, or 64) used for the
            algorithm.

    Returns:
        A random value within ``[start, end)``.
    """
    if start >= end:
        raise ValueError(f"start must be less than end, got [{start}, {end})")
    max_val = (1 << bits) - 1
    delta = (end - start) & max_val

    if delta == max_val:
        return start + (rng.next_u64() & max_val)

    return start + rng_next_with_upper_bound(rng, delta, bits=bits)


def rng_next_in_closed_range(
    rng: RandomNumberGenerator,
    start: int,
    end: int,
    *,
    bits: BitWidth = 64,
) -> int:
    """Return a random value in the closed range ``[start, end]``."""
    if start > end:
        raise ValueError(f"start must be <= end, got [{start}, {end}]")
    max_val = (1 << bits) - 1
    delta = (end - start) & max_val

    if delta == max_val:
        return start + (rng.next_u64() & max_val)

    return start + rng_next_with_upper_bound(rng, delta + 1, bits=bits)


def rng_random_array(rng: RandomNumberGenerator, size: int) -> bytes:
    """Return a bytes object of random bytes of the given size."""
    return rng_random_data(rng, size)


def rng_random_bool(rng: RandomNumberGenerator) -> bool:
    """Return a random boolean with equal probability."""
    return rng.next_u32() % 2 == 0


def rng_random_u32(rng: RandomNumberGenerator) -> int:
    """Return a random unsigned 32-bit integer from *rng*."""
    return rng.next_u32()
