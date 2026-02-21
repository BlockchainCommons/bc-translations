from __future__ import annotations

from collections.abc import Callable
from enum import Enum, auto
from typing import TYPE_CHECKING, TypeVar

if TYPE_CHECKING:
    from .cbor import CBOR

S = TypeVar("S")


class WalkElement:
    __slots__ = ("_single", "_key", "_value")

    def __init__(
        self,
        single: CBOR | None = None,
        key: CBOR | None = None,
        value: CBOR | None = None,
    ) -> None:
        self._single = single
        self._key = key
        self._value = value

    @staticmethod
    def make_single(cbor: CBOR) -> WalkElement:
        return WalkElement(single=cbor)

    @staticmethod
    def make_key_value(key: CBOR, value: CBOR) -> WalkElement:
        return WalkElement(key=key, value=value)

    def as_single(self) -> CBOR | None:
        return self._single

    def as_key_value(self) -> tuple[CBOR, CBOR] | None:
        if self._key is not None and self._value is not None:
            return (self._key, self._value)
        return None

    def diagnostic_flat(self) -> str:
        if self._single is not None:
            return self._single.diagnostic_flat()
        if self._key is not None and self._value is not None:
            return f"{self._key.diagnostic_flat()}: {self._value.diagnostic_flat()}"
        return ""


class EdgeType(Enum):
    NONE = auto()
    ARRAY_ELEMENT = auto()
    MAP_KEY_VALUE = auto()
    MAP_KEY = auto()
    MAP_VALUE = auto()
    TAGGED_CONTENT = auto()

    def label(self, index: int | None = None) -> str | None:
        match self:
            case EdgeType.ARRAY_ELEMENT:
                return f"arr[{index}]"
            case EdgeType.MAP_KEY_VALUE:
                return "kv"
            case EdgeType.MAP_KEY:
                return "key"
            case EdgeType.MAP_VALUE:
                return "val"
            case EdgeType.TAGGED_CONTENT:
                return "content"
            case EdgeType.NONE:
                return None


Visitor = Callable[[WalkElement, int, EdgeType, S], tuple[S, bool]]


def walk_impl(cbor: CBOR, state: S, visitor: Visitor[S]) -> None:
    from .cbor import CBORCase

    _walk(cbor, 0, EdgeType.NONE, None, state, visitor)


def _walk(
    cbor: CBOR,
    level: int,
    edge: EdgeType,
    edge_index: int | None,
    state: S,
    visitor: Visitor[S],
) -> None:
    from .cbor import CBORCase

    element = WalkElement.make_single(cbor)
    state, stop = visitor(element, level, edge, state)
    if stop:
        return

    next_level = level + 1

    match cbor.case:
        case CBORCase.ARRAY:
            for i, child in enumerate(cbor.value):
                _walk(child, next_level, EdgeType.ARRAY_ELEMENT, i, state, visitor)

        case CBORCase.MAP:
            for key, value in cbor.value.iter():
                kv_element = WalkElement.make_key_value(key, value)
                new_state, stop = visitor(kv_element, next_level, EdgeType.MAP_KEY_VALUE, state)
                if stop:
                    continue
                _walk(key, next_level, EdgeType.MAP_KEY, None, new_state, visitor)
                _walk(value, next_level, EdgeType.MAP_VALUE, None, new_state, visitor)

        case CBORCase.TAGGED:
            _, content = cbor.value
            _walk(content, next_level, EdgeType.TAGGED_CONTENT, None, state, visitor)
