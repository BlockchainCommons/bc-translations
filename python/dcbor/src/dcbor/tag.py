from __future__ import annotations

TagValue = int


class Tag:
    """CBOR tag pairing a numeric tag value with an optional human-readable name."""

    __slots__ = ("_value", "_name")

    def __init__(self, value: TagValue, name: str | None = None) -> None:
        self._value = value
        self._name = name

    @staticmethod
    def with_value(value: TagValue) -> Tag:
        return Tag(value)

    @property
    def value(self) -> TagValue:
        return self._value

    @property
    def name(self) -> str | None:
        return self._name

    def __eq__(self, other: object) -> bool:
        if isinstance(other, Tag):
            return self._value == other._value
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._value)

    def __str__(self) -> str:
        if self._name is not None:
            return self._name
        return str(self._value)

    def __repr__(self) -> str:
        if self._name is not None:
            return f"Tag({self._value}, {self._name!r})"
        return f"Tag({self._value})"
