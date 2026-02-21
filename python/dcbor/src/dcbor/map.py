from __future__ import annotations

from collections.abc import Iterator

from .cbor import CBOR, CBORCase
from .error import DuplicateMapKey, MisorderedMapKey, MissingMapKey, WrongType
from .varint import MajorType, encode_varint

class _MapEntry:
    __slots__ = ("key", "value")

    def __init__(self, key: CBOR, value: CBOR) -> None:
        self.key = key
        self.value = value


class Map:
    """Deterministic CBOR map that stores key-value pairs sorted by encoded key."""

    __slots__ = ("_entries", "_last_key")

    def __init__(self) -> None:
        self._entries: dict[bytes, _MapEntry] = {}
        self._last_key: bytes | None = None

    def __len__(self) -> int:
        return len(self._entries)

    @property
    def is_empty(self) -> bool:
        return len(self._entries) == 0

    def insert(self, key: CBOR | int | str | float | bool, value: CBOR | int | str | float | bool) -> None:
        """Insert or overwrite a key-value pair; accepts native Python scalars as well as CBOR."""
        cbor_key = CBOR.from_value(key) if not isinstance(key, CBOR) else key
        cbor_value = CBOR.from_value(value) if not isinstance(value, CBOR) else value
        encoded_key = cbor_key.to_cbor_data()
        self._entries[encoded_key] = _MapEntry(cbor_key, cbor_value)

    def insert_next(self, key: CBOR, value: CBOR) -> None:
        encoded_key = key.to_cbor_data()
        if encoded_key in self._entries:
            raise DuplicateMapKey()
        if self._last_key is not None and encoded_key <= self._last_key:
            raise MisorderedMapKey()
        self._entries[encoded_key] = _MapEntry(key, value)
        self._last_key = encoded_key

    def get(self, key: CBOR | int | str | float | bool) -> CBOR | None:
        """Return the value for key, or None if not present."""
        cbor_key = CBOR.from_value(key) if not isinstance(key, CBOR) else key
        encoded_key = cbor_key.to_cbor_data()
        entry = self._entries.get(encoded_key)
        if entry is not None:
            return entry.value
        return None

    def extract(self, key: CBOR | int | str | float | bool) -> CBOR:
        """Return the value for key, raising MissingMapKey if not present."""
        result = self.get(key)
        if result is None:
            raise MissingMapKey()
        return result

    def contains_key(self, key: CBOR | int | str | float | bool) -> bool:
        cbor_key = CBOR.from_value(key) if not isinstance(key, CBOR) else key
        encoded_key = cbor_key.to_cbor_data()
        return encoded_key in self._entries

    def __contains__(self, key: object) -> bool:
        if not isinstance(key, (CBOR, int, str, float, bool)):
            return False
        return self.contains_key(key)

    def __getitem__(self, key: CBOR | int | str | float | bool) -> CBOR:
        return self.extract(key)

    def iter(self) -> Iterator[tuple[CBOR, CBOR]]:
        for encoded_key in sorted(self._entries.keys()):
            entry = self._entries[encoded_key]
            yield (entry.key, entry.value)

    def __iter__(self) -> Iterator[tuple[CBOR, CBOR]]:
        return self.iter()

    def keys(self) -> list[CBOR]:
        return [entry.key for _, entry in sorted(self._entries.items())]

    def values(self) -> list[CBOR]:
        return [entry.value for _, entry in sorted(self._entries.items())]

    def cbor_data(self) -> bytes:
        sorted_keys = sorted(self._entries.keys())
        result = encode_varint(len(sorted_keys), MajorType.MAP)
        for encoded_key in sorted_keys:
            entry = self._entries[encoded_key]
            result += encoded_key + entry.value.to_cbor_data()
        return result

    def to_cbor(self) -> CBOR:
        return CBOR(CBORCase.MAP, self)

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Map):
            return NotImplemented
        if len(self._entries) != len(other._entries):
            return False
        for (k1, e1), (k2, e2) in zip(
            sorted(self._entries.items()),
            sorted(other._entries.items()),
        ):
            if k1 != k2 or e1.key != e2.key or e1.value != e2.value:
                return False
        return True

    def __hash__(self) -> int:
        items = tuple(
            (entry.key, entry.value)
            for _, entry in sorted(self._entries.items())
        )
        return hash(items)

    def __repr__(self) -> str:
        pairs = ", ".join(
            f"{entry.key}: {entry.value}"
            for _, entry in sorted(self._entries.items())
        )
        return f"Map({{{pairs}}})"

    @staticmethod
    def from_dict(d: dict[CBOR | int | str | float | bool, CBOR | int | str | float | bool]) -> Map:
        """Build a Map from a plain Python dict, converting keys and values automatically."""
        m = Map()
        for k, v in d.items():
            cbor_key = CBOR.from_value(k) if not isinstance(k, CBOR) else k
            cbor_value = CBOR.from_value(v) if not isinstance(v, CBOR) else v
            m.insert(cbor_key, cbor_value)
        return m
