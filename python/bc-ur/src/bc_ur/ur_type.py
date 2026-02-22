"""Validated UR type string."""

from __future__ import annotations

from .error import InvalidTypeError


def _is_ur_type_char(c: str) -> bool:
    """Check if a character is valid in a UR type string."""
    return c.isascii() and (c.islower() or c.isdigit() or c == "-")


def _is_ur_type_string(s: str) -> bool:
    """Check if a string is a valid UR type string."""
    return len(s) > 0 and all(_is_ur_type_char(c) for c in s)


class URType:
    """A validated UR type string.

    UR types consist of lowercase ASCII letters, digits, and hyphens.
    """

    __slots__ = ("_value",)

    def __init__(self, ur_type: str) -> None:
        if not _is_ur_type_string(ur_type):
            raise InvalidTypeError()
        self._value = ur_type

    @property
    def value(self) -> str:
        return self._value

    def __eq__(self, other: object) -> bool:
        if isinstance(other, URType):
            return self._value == other._value
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._value)

    def __repr__(self) -> str:
        return f"URType({self._value!r})"

    def __str__(self) -> str:
        return self._value
