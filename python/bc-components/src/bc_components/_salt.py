"""Variable-length random salt.

A variable-length random value (minimum 8 bytes) used to decorrelate
other information.
"""

from __future__ import annotations

import math

from bc_rand import (
    RandomNumberGenerator,
    SecureRandomNumberGenerator,
    rng_next_in_closed_range,
    rng_random_data,
)
from bc_tags import TAG_SALT, tags_for_values
from dcbor import CBOR, Tag

from ._error import BCComponentsError


class Salt:
    """Variable-length random salt (minimum 8 bytes)."""

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        self._data = bytes(data)

    # --- Construction --------------------------------------------------

    @staticmethod
    def from_data(data: bytes | bytearray) -> Salt:
        """Create a salt from arbitrary bytes."""
        return Salt(bytes(data))

    @staticmethod
    def generate_with_len(count: int) -> Salt:
        """Create *count* bytes of random salt (minimum 8)."""
        rng = SecureRandomNumberGenerator()
        return Salt.generate_with_len_using(count, rng)

    @staticmethod
    def generate_with_len_using(
        count: int, rng: RandomNumberGenerator,
    ) -> Salt:
        """Create *count* bytes of salt using the given RNG."""
        if count < 8:
            raise BCComponentsError.data_too_short("salt", 8, count)
        return Salt(rng_random_data(rng, count))

    @staticmethod
    def generate_in_range(start: int, end: int) -> Salt:
        """Create salt with a random length in ``[start, end]`` (inclusive)."""
        if start < 8:
            raise BCComponentsError.data_too_short("salt", 8, start)
        rng = SecureRandomNumberGenerator()
        return Salt.generate_in_range_using(start, end, rng)

    @staticmethod
    def generate_in_range_using(
        start: int, end: int, rng: RandomNumberGenerator,
    ) -> Salt:
        """Create salt with random length in ``[start, end]`` using the given RNG."""
        if start < 8:
            raise BCComponentsError.data_too_short("salt", 8, start)
        count = rng_next_in_closed_range(rng, start, end)
        return Salt.generate_with_len_using(count, rng)

    @staticmethod
    def generate_for_size(size: int) -> Salt:
        """Create salt proportional to *size* bytes of data being salted."""
        rng = SecureRandomNumberGenerator()
        return Salt.generate_for_size_using(size, rng)

    @staticmethod
    def generate_for_size_using(
        size: int, rng: RandomNumberGenerator,
    ) -> Salt:
        """Create proportional salt using the given RNG."""
        count = float(size)
        min_size = max(8, math.ceil(count * 0.05))
        max_size = max(min_size + 8, math.ceil(count * 0.25))
        return Salt.generate_in_range_using(min_size, max_size, rng)

    @staticmethod
    def from_hex(hex_str: str) -> Salt:
        """Create a salt from a hex string."""
        return Salt(bytes.fromhex(hex_str))

    # --- Accessors -----------------------------------------------------

    def __len__(self) -> int:
        return len(self._data)

    def __bool__(self) -> bool:
        return len(self._data) > 0

    @property
    def data(self) -> bytes:
        """The raw salt bytes."""
        return self._data

    def hex(self) -> str:
        return self._data.hex()

    # --- CBOR ----------------------------------------------------------

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_SALT])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_bytes(self._data)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @classmethod
    def from_untagged_cbor(cls, cbor: CBOR) -> Salt:
        data = cbor.try_byte_string()
        return cls.from_data(data)

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> Salt:
        tags = cls.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return cls.from_untagged_cbor(item)

    @classmethod
    def from_tagged_cbor_data(cls, data: bytes) -> Salt:
        cbor = CBOR.from_data(data)
        return cls.from_tagged_cbor(cbor)

    @classmethod
    def from_untagged_cbor_data(cls, data: bytes) -> Salt:
        cbor = CBOR.from_data(data)
        return cls.from_untagged_cbor(cbor)

    # --- Dunder methods ------------------------------------------------

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Salt):
            return NotImplemented
        return self._data == other._data

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"Salt({len(self._data)})"

    def __bytes__(self) -> bytes:
        return self._data
