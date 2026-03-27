"""Known Value core type."""

from __future__ import annotations

from bc_components import Digest
from bc_tags import TAG_KNOWN_VALUE, tags_for_values
from dcbor import CBOR, OutOfRange, Tag

_U64_MAX = 0xFFFFFFFFFFFFFFFF


def _ensure_u64(value: int) -> int:
    if isinstance(value, bool) or not isinstance(value, int):
        raise ValueError("known value must be an integer")
    if value < 0 or value > _U64_MAX:
        raise ValueError("known value must fit in an unsigned 64-bit integer")
    return value


class KnownValue:
    """A value in a namespace of unsigned integers representing an ontological concept.

    Construct with ``KnownValue(value)`` for an unnamed value or
    ``KnownValue(value, assigned_name)`` for a named value.
    """

    __slots__ = ("_value", "_assigned_name")

    def __init__(self, value: int, assigned_name: str | None = None) -> None:
        self._value = _ensure_u64(value)
        self._assigned_name = assigned_name

    @property
    def value(self) -> int:
        """The numeric code point."""
        return self._value

    @property
    def assigned_name(self) -> str | None:
        """The assigned human-readable name, or ``None``."""
        return self._assigned_name

    @property
    def name(self) -> str:
        """The assigned name if present, otherwise the decimal string of the value."""
        if self._assigned_name is not None:
            return self._assigned_name
        return str(self._value)

    def digest(self) -> Digest:
        """Return the digest of the tagged CBOR encoding."""
        return Digest.from_image(self.tagged_cbor().to_cbor_data())

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_KNOWN_VALUE])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_int(self._value)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    @classmethod
    def from_untagged_cbor(cls, cbor: CBOR) -> KnownValue:
        value = cbor.try_int()
        if value < 0:
            raise OutOfRange()
        return cls(value)

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> KnownValue:
        tags = cls.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return cls.from_untagged_cbor(item)

    @classmethod
    def from_tagged_cbor_data(cls, data: bytes) -> KnownValue:
        return cls.from_tagged_cbor(CBOR.from_data(data))

    @classmethod
    def from_untagged_cbor_data(cls, data: bytes) -> KnownValue:
        return cls.from_untagged_cbor(CBOR.from_data(data))

    @classmethod
    def from_cbor(cls, cbor: CBOR) -> KnownValue:
        return cls.from_tagged_cbor(cbor)

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, KnownValue):
            return NotImplemented
        return self._value == other._value

    def __hash__(self) -> int:
        return hash(self._value)

    def __int__(self) -> int:
        return self._value

    def __repr__(self) -> str:
        if self._assigned_name is None:
            return f"KnownValue({self._value})"
        return f"KnownValue({self._value}, {self._assigned_name!r})"

    def __str__(self) -> str:
        return self.name
