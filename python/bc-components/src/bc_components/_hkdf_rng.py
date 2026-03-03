"""A deterministic RNG based on HKDF-HMAC-SHA256."""

from __future__ import annotations

import struct

from bc_crypto import hkdf_hmac_sha256
from bc_rand import RandomNumberGenerator


class HKDFRng(RandomNumberGenerator):
    """A deterministic random number generator based on HKDF-HMAC-SHA256.

    HKDFRng uses the HMAC-based Key Derivation Function (HKDF) to
    generate deterministic random numbers from a combination of key
    material and salt.  It serves as a key-stretching mechanism that can
    produce an arbitrary amount of random-looking bytes from a single
    seed.

    The same key material and salt will always produce the same sequence,
    making it useful for deterministic key derivation and testing.
    """

    __slots__ = (
        "_buffer",
        "_position",
        "_key_material",
        "_salt",
        "_page_length",
        "_page_index",
    )

    def __init__(
        self,
        key_material: bytes | bytearray,
        salt: str,
        page_length: int = 32,
    ) -> None:
        self._buffer = b""
        self._position = 0
        self._key_material = bytes(key_material)
        self._salt = salt
        self._page_length = page_length
        self._page_index = 0

    # --- Construction ---

    @staticmethod
    def new(key_material: bytes | bytearray, salt: str) -> HKDFRng:
        """Create a new HKDFRng with the default page length of 32 bytes."""
        return HKDFRng(key_material, salt, 32)

    @staticmethod
    def new_with_page_length(
        key_material: bytes | bytearray,
        salt: str,
        page_length: int,
    ) -> HKDFRng:
        """Create a new HKDFRng with a custom page length."""
        return HKDFRng(key_material, salt, page_length)

    # --- Internal ---

    def _fill_buffer(self) -> None:
        """Refill the internal buffer using HKDF."""
        salt_string = f"{self._salt}-{self._page_index}"
        self._buffer = hkdf_hmac_sha256(
            self._key_material,
            salt_string,
            self._page_length,
        )
        self._position = 0
        self._page_index += 1

    def _next_bytes(self, length: int) -> bytes:
        """Generate the specified number of deterministic random bytes."""
        result = bytearray()
        while len(result) < length:
            if self._position >= len(self._buffer):
                self._fill_buffer()
            remaining = length - len(result)
            available = len(self._buffer) - self._position
            take = min(remaining, available)
            result.extend(self._buffer[self._position : self._position + take])
            self._position += take
        return bytes(result)

    # --- RandomNumberGenerator protocol ---

    def next_u32(self) -> int:
        """Generate a deterministic random u32 value (little-endian)."""
        data = self._next_bytes(4)
        return struct.unpack("<I", data)[0]

    def next_u64(self) -> int:
        """Generate a deterministic random u64 value (little-endian)."""
        data = self._next_bytes(8)
        return struct.unpack("<Q", data)[0]

    def random_data(self, size: int) -> bytes:
        """Generate the specified number of deterministic random bytes."""
        return self._next_bytes(size)

    def fill_random_data(self, data: bytearray) -> None:
        """Fill the provided buffer with deterministic random bytes."""
        result = self._next_bytes(len(data))
        data[:] = result
