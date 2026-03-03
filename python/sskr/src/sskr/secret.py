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

    def len(self) -> int:
        """Return the secret length in bytes."""
        return len(self._data)

    def is_empty(self) -> bool:
        """Return ``True`` if the secret is empty."""
        return self.len() == 0

    def data(self) -> bytes:
        """Return the underlying secret bytes."""
        return self._data

    def __bytes__(self) -> bytes:
        return self._data

    def __len__(self) -> int:
        return self.len()

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Secret):
            return NotImplemented
        return self._data == other._data

    def __repr__(self) -> str:
        return f"Secret({self._data!r})"
