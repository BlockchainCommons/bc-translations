"""Seed domain object example for Envelope tests.

Translated from rust/bc-envelope/tests/common/test_seed.rs

Demonstrates how a domain object (Seed) can be converted to/from Envelope.
"""

from __future__ import annotations

from typing import Optional

from dcbor import CBOR, Date, Map
from bc_envelope import Envelope, extract_subject
from known_values import KnownValue
import known_values


class Seed:
    """A seed with optional name, note, and creation date."""

    def __init__(
        self,
        data: bytes,
        name: str = "",
        note: str = "",
        creation_date: Optional[Date] = None,
    ) -> None:
        if not data:
            raise ValueError("invalid seed data")
        self._data = bytes(data)
        self._name = name
        self._note = note
        self._creation_date = creation_date

    @property
    def data(self) -> bytes:
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
    def creation_date(self) -> Optional[Date]:
        return self._creation_date

    @creation_date.setter
    def creation_date(self, value: Optional[Date]) -> None:
        self._creation_date = value

    # --- CBOR encoding (tagged map) ---

    def tagged_cbor(self) -> CBOR:
        from bc_tags import TAG_SEED, tags_for_values
        m = Map()
        m.insert(1, CBOR.from_bytes(self._data))
        if self._creation_date is not None:
            m.insert(2, self._creation_date.tagged_cbor())
        if self._name:
            m.insert(3, CBOR.from_value(self._name))
        if self._note:
            m.insert(4, CBOR.from_value(self._note))
        tags = tags_for_values([TAG_SEED])
        return CBOR.from_tagged_value(tags[0], m.cbor)

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> Seed:
        from bc_tags import TAG_SEED, tags_for_values
        tags = tags_for_values([TAG_SEED])
        inner = cbor.try_expected_tagged_value(tags[0])
        m = inner.try_map()
        data = m.extract(1).try_byte_string()
        if not data:
            raise ValueError("invalid seed data")
        creation_date: Optional[Date] = None
        cd_cbor = m.get_optional(2)
        if cd_cbor is not None:
            creation_date = Date.from_tagged_cbor(cd_cbor)
        name = ""
        name_cbor = m.get_optional(3)
        if name_cbor is not None:
            name = name_cbor.try_text()
        note = ""
        note_cbor = m.get_optional(4)
        if note_cbor is not None:
            note = note_cbor.try_text()
        return cls(data, name, note, creation_date)

    # --- Envelope encoding ---

    def to_envelope(self) -> Envelope:
        e = Envelope(CBOR.from_bytes(self._data))
        e = e.add_type(known_values.SEED_TYPE)
        if self._creation_date is not None:
            e = e.add_assertion(known_values.DATE, self._creation_date)
        if self._name:
            e = e.add_assertion(known_values.NAME, self._name)
        if self._note:
            e = e.add_assertion(known_values.NOTE, self._note)
        return e

    @classmethod
    def from_envelope(cls, envelope: Envelope) -> Seed:
        envelope.check_type_value(known_values.SEED_TYPE)
        data = envelope.subject().try_leaf().try_byte_string()

        name_obj = envelope.extract_optional_object_for_predicate(
            known_values.NAME, str
        )
        name = name_obj if name_obj is not None else ""

        note_obj = envelope.extract_optional_object_for_predicate(
            known_values.NOTE, str
        )
        note = note_obj if note_obj is not None else ""

        creation_date = envelope.extract_optional_object_for_predicate(
            known_values.DATE, Date
        )

        return cls(data, name, note, creation_date)

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
        return hash(self._data)

    def __repr__(self) -> str:
        return f"Seed(data={self._data.hex()}, name={self._name!r})"
