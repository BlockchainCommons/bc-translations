"""Cryptographic seed for deterministic key generation.

A source of entropy used to generate cryptographic keys deterministically,
with optional metadata (name, note, creation date).
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_rand import (
    RandomNumberGenerator,
    SecureRandomNumberGenerator,
    rng_random_data,
)
from bc_tags import TAG_SEED, TAG_SEED_V1, tags_for_values
from dcbor import CBOR, Date, Map, Tag

from ._error import BCComponentsError

if TYPE_CHECKING:
    from bc_ur import UR


class Seed:
    """A cryptographic seed with optional metadata."""

    MIN_SEED_LENGTH: int = 16

    __slots__ = ("_data", "_name", "_note", "_creation_date")

    def __init__(
        self,
        data: bytes,
        name: str = "",
        note: str = "",
        creation_date: Date | None = None,
    ) -> None:
        if len(data) < self.MIN_SEED_LENGTH:
            raise BCComponentsError.data_too_short(
                "seed", self.MIN_SEED_LENGTH, len(data),
            )
        self._data = bytes(data)
        self._name = name
        self._note = note
        self._creation_date = creation_date

    # --- Construction --------------------------------------------------

    @staticmethod
    def generate() -> Seed:
        """Create a new random seed with the minimum length (16 bytes)."""
        return Seed.generate_with_len(Seed.MIN_SEED_LENGTH)

    @staticmethod
    def generate_with_len(count: int) -> Seed:
        """Create a new random seed with *count* bytes."""
        rng = SecureRandomNumberGenerator()
        return Seed.generate_with_len_using(count, rng)

    @staticmethod
    def generate_with_len_using(count: int, rng: RandomNumberGenerator) -> Seed:
        """Create a new random seed with *count* bytes using the given RNG."""
        data = rng_random_data(rng, count)
        return Seed.new_opt(data)

    @staticmethod
    def new_opt(
        data: bytes | bytearray,
        name: str | None = None,
        note: str | None = None,
        creation_date: Date | None = None,
    ) -> Seed:
        """Create a seed from data with optional metadata."""
        return Seed(
            bytes(data),
            name=name or "",
            note=note or "",
            creation_date=creation_date,
        )

    # --- Accessors -----------------------------------------------------

    @property
    def data(self) -> bytes:
        """The raw seed bytes."""
        return self._data

    @property
    def name(self) -> str:
        return self._name

    @name.setter
    def name(self, value: str) -> None:
        self._name = value

    @property
    def note(self) -> str:
        return self._note

    @note.setter
    def note(self, value: str) -> None:
        self._note = value

    @property
    def creation_date(self) -> Date | None:
        return self._creation_date

    @creation_date.setter
    def creation_date(self, value: Date | None) -> None:
        self._creation_date = value

    # --- PrivateKeyDataProvider ----------------------------------------

    def private_key_data(self) -> bytes:
        """Return the seed data for key derivation."""
        return self._data

    # --- CBOR ----------------------------------------------------------

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_SEED, TAG_SEED_V1])

    def untagged_cbor(self) -> CBOR:
        m = Map()
        m.insert(1, CBOR.from_bytes(self._data))
        if self._creation_date is not None:
            m.insert(2, self._creation_date.to_tagged_cbor())
        if self._name:
            m.insert(3, CBOR.from_text(self._name))
        if self._note:
            m.insert(4, CBOR.from_text(self._note))
        return CBOR.from_map(m)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @classmethod
    def from_untagged_cbor(cls, cbor: CBOR) -> Seed:
        m = cbor.try_map()
        data_cbor = m.extract(1)
        data = data_cbor.try_byte_string()
        if not data:
            raise BCComponentsError.invalid_data("seed", "Seed data is empty")

        creation_date: Date | None = None
        creation_date_cbor = m.get(2)
        if creation_date_cbor is not None:
            creation_date = Date.from_tagged_cbor(creation_date_cbor)

        name: str | None = None
        name_cbor = m.get(3)
        if name_cbor is not None:
            name = name_cbor.try_text()

        note: str | None = None
        note_cbor = m.get(4)
        if note_cbor is not None:
            note = note_cbor.try_text()

        return cls.new_opt(data, name, note, creation_date)

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> Seed:
        # Try each tag in order (primary first, then legacy)
        tags = cls.cbor_tags()
        for tag in tags:
            tagged = cbor.as_tagged_value()
            if tagged is not None:
                t, item = tagged
                if t == tag:
                    return cls.from_untagged_cbor(item)
        # Fall back to trying the primary tag, which will raise on mismatch
        item = cbor.try_expected_tagged_value(tags[0])
        return cls.from_untagged_cbor(item)

    @classmethod
    def from_tagged_cbor_data(cls, data: bytes) -> Seed:
        cbor = CBOR.from_data(data)
        return cls.from_tagged_cbor(cbor)

    @classmethod
    def from_untagged_cbor_data(cls, data: bytes) -> Seed:
        cbor = CBOR.from_data(data)
        return cls.from_untagged_cbor(cbor)

    # --- UR ------------------------------------------------------------

    def to_ur(self) -> UR:
        from bc_ur import to_ur
        return to_ur(self)

    def ur_string(self) -> str:
        from bc_ur import to_ur_string
        return to_ur_string(self)

    @classmethod
    def from_ur(cls, ur: UR) -> Seed:
        from bc_ur import from_ur
        return from_ur(cls, ur)

    @classmethod
    def from_ur_string(cls, ur_string: str) -> Seed:
        from bc_ur import from_ur_string
        return from_ur_string(cls, ur_string)

    # --- Dunder methods ------------------------------------------------

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Seed):
            return NotImplemented
        return (
            self._data == other._data
            and self._name == other._name
            and self._note == other._note
            and self._creation_date == other._creation_date
        )

    def __hash__(self) -> int:
        return hash((self._data, self._name, self._note, self._creation_date))

    def __repr__(self) -> str:
        return f"Seed(len={len(self._data)}, name={self._name!r})"

    def __bytes__(self) -> bytes:
        return self._data
