"""Secret model and validation helpers."""

from __future__ import annotations

from .constants import MAX_SECRET_LEN, MIN_SECRET_LEN
from .error import (
    SecretLengthNotEvenError,
    SecretTooLongError,
    SecretTooShortError,
)


class Secret:
    """A secret to be split into shares."""

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        self._data = data

    @classmethod
    def new(cls, data: bytes | bytearray | memoryview | str) -> "Secret":
        """Create a validated secret from bytes-like data."""
        if isinstance(data, str):
            data_bytes = data.encode("utf-8")
        else:
            data_bytes = bytes(data)
        length = len(data_bytes)
        if length < MIN_SECRET_LEN:
            raise SecretTooShortError()
        if length > MAX_SECRET_LEN:
            raise SecretTooLongError()
        if length & 1 != 0:
            raise SecretLengthNotEvenError()
        return cls(data_bytes)

    @property
    def data(self) -> bytes:
        """The underlying secret bytes."""
        return self._data

    def __bytes__(self) -> bytes:
        return self._data

    def __len__(self) -> int:
        return len(self._data)

    def __bool__(self) -> bool:
        return len(self._data) > 0

    def __hash__(self) -> int:
        return hash(self._data)

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Secret):
            return NotImplemented
        return self._data == other._data

    def __repr__(self) -> str:
        return f"Secret({self._data!r})"
