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
    """A deterministic PRNG for testing.

    Seeded with four u64 values.  Uses Xoshiro256** internally.
    NOT cryptographically secure.

    The ``random_data`` and ``fill_random_data`` methods generate each
    byte individually from ``next_u64() & 0xFF`` to match the Swift
    implementation and ensure cross-platform test-vector compatibility.
    """

    __slots__ = ("_rng",)

    def __init__(self, seed: tuple[int, int, int, int]) -> None:
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
    """Return a ``SeededRandomNumberGenerator`` with the standard test seed."""
    return SeededRandomNumberGenerator(_FAKE_SEED)


def fake_random_data(size: int) -> bytes:
    """Return *size* bytes of deterministic random data from the standard test seed."""
    return make_fake_random_number_generator().random_data(size)
