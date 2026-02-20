"""Deterministic seeded random number generator for testing."""

from ._constants import MASK32 as _MASK32
from ._constants import MASK64 as _MASK64
from ._xoshiro256starstar import Xoshiro256StarStar
from .random_number_generator import RandomNumberGenerator

_FAKE_SEED = (
    17295166580085024720,
    422929670265678780,
    5577237070365765850,
    7953171132032326923,
)


class SeededRandomNumberGenerator(RandomNumberGenerator):
    """A random number generator that can be used as a source of deterministic pseudo-randomness for testing purposes."""

    __slots__ = ("_rng",)

    def __init__(self, seed: tuple[int, int, int, int]) -> None:
        """Create a new seeded random number generator.

        The seed should be a 256-bit value, represented as a tuple of four
        64-bit integers. For the output distribution to look random, the seed
        should not have any obvious patterns, like all zeroes or all ones.

        This is not cryptographically secure, and should only be used for
        testing purposes.
        """
        self._rng = Xoshiro256StarStar(seed[0], seed[1], seed[2], seed[3])

    def next_u32(self) -> int:
        return self.next_u64() & _MASK32

    def next_u64(self) -> int:
        return self._rng.next_u64()

    def random_data(self, size: int) -> bytes:
        return bytes(self.next_u64() & 0xFF for _ in range(size))

    def fill_random_data(self, data: bytearray) -> None:
        for i in range(len(data)):
            data[i] = self.next_u64() & 0xFF


def make_fake_random_number_generator() -> SeededRandomNumberGenerator:
    """Create a seeded random number generator with a fixed seed."""
    return SeededRandomNumberGenerator(_FAKE_SEED)


def fake_random_data(size: int) -> bytes:
    """Create a bytes object of random data with a fixed seed."""
    return make_fake_random_number_generator().random_data(size)
