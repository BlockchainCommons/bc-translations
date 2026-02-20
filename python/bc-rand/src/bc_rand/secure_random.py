"""Cryptographically secure random number generator."""

import os
import struct

from ._constants import MASK32 as _MASK32
from .random_number_generator import RandomNumberGenerator


def random_data(size: int) -> bytes:
    """Return *size* cryptographically strong random bytes."""
    return os.urandom(size)


def fill_random_data(data: bytearray) -> None:
    """Fill *data* with cryptographically strong random bytes."""
    data[:] = os.urandom(len(data))


def next_u64() -> int:
    """Return a cryptographically strong random u64."""
    return struct.unpack("<Q", os.urandom(8))[0]


class SecureRandomNumberGenerator(RandomNumberGenerator):
    """A cryptographically secure random number generator.

    Backed by ``os.urandom`` (the OS CSPRNG).
    """

    __slots__ = ()

    def next_u32(self) -> int:
        return next_u64() & _MASK32

    def next_u64(self) -> int:
        # Calls the module-level next_u64(), not self.next_u64()
        return next_u64()

    def random_data(self, size: int) -> bytes:
        return random_data(size)

    def fill_random_data(self, data: bytearray) -> None:
        fill_random_data(data)
