from __future__ import annotations

from collections.abc import Iterable, Iterator

from .cbor import CBOR, CBORCase
from .error import DuplicateMapKey, MisorderedMapKey
from .map import Map
from .varint import MajorType, encode_varint


class Set:
    """Ordered, deduplicated CBOR set encoded as an array sorted by deterministic CBOR key."""

    __slots__ = ("_map",)

    def __init__(self) -> None:
        self._map = Map()

    def __len__(self) -> int:
        return len(self._map)

    @property
    def is_empty(self) -> bool:
        return self._map.is_empty

    def insert(self, value: CBOR | int | str | float | bool) -> None:
        cbor_value = CBOR.from_value(value) if not isinstance(value, CBOR) else value
        self._map.insert(cbor_value, cbor_value)

    def insert_next(self, value: CBOR) -> None:
        self._map.insert_next(value, value)

    def contains(self, value: CBOR | int | str | float | bool) -> bool:
        return self._map.contains_key(value)

    def __contains__(self, value: object) -> bool:
        if not isinstance(value, (CBOR, int, str, float, bool)):
            return False
        return self.contains(value)

    def iter(self) -> Iterator[CBOR]:
        for key, _ in self._map.iter():
            yield key

    def __iter__(self) -> Iterator[CBOR]:
        return self.iter()

    def to_list(self) -> list[CBOR]:
        return list(self.iter())

    @staticmethod
    def from_list(items: Iterable[CBOR | int | str | float | bool]) -> Set:
        s = Set()
        for item in items:
            cbor_value = CBOR.from_value(item) if not isinstance(item, CBOR) else item
            s.insert(cbor_value)
        return s

    @staticmethod
    def from_sorted_list(items: Iterable[CBOR | int | str | float | bool]) -> Set:
        s = Set()
        for item in items:
            cbor_value = CBOR.from_value(item) if not isinstance(item, CBOR) else item
            s._map.insert_next(cbor_value, cbor_value)
        return s

    def cbor_data(self) -> bytes:
        items = self.to_list()
        result = encode_varint(len(items), MajorType.ARRAY)
        for item in items:
            result += item.to_cbor_data()
        return result

    def to_cbor(self) -> CBOR:
        return CBOR.from_array(self.to_list())

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Set):
            return NotImplemented
        return self._map == other._map

    def __hash__(self) -> int:
        return hash(self._map)

    def __repr__(self) -> str:
        items = ", ".join(str(item) for item in self.iter())
        return f"Set({{{items}}})"
