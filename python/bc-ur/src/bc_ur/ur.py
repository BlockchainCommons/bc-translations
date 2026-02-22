"""Main UR (Uniform Resource) type."""

from __future__ import annotations

from dcbor import CBOR

from ._ur_encoding import decode_ur, encode_ur
from .error import (
    InvalidSchemeError,
    NotSinglePartError,
    TypeUnspecifiedError,
    UnexpectedTypeError,
    URCborError,
    URError,
)
from .ur_type import URType


class UR:
    """A Uniform Resource (UR) is a URI-encoded CBOR object."""

    __slots__ = ("_ur_type", "_cbor")

    def __init__(self, ur_type: str | URType, cbor: CBOR) -> None:
        if isinstance(ur_type, str):
            ur_type = URType(ur_type)
        self._ur_type = ur_type
        if not isinstance(cbor, CBOR):
            cbor = CBOR.from_value(cbor)
        self._cbor = cbor

    @classmethod
    def from_ur_string(cls, ur_string: str) -> UR:
        """Parse a UR string into a UR object."""
        ur_string = ur_string.lower()
        if not ur_string.startswith("ur:"):
            raise InvalidSchemeError()

        rest = ur_string[3:]
        slash_pos = rest.find("/")
        if slash_pos < 0:
            raise TypeUnspecifiedError()

        type_str = rest[:slash_pos]
        ur_type = URType(type_str)

        kind, data = decode_ur(ur_string)
        if kind != "single":
            raise NotSinglePartError()

        try:
            cbor = CBOR.from_data(data)
        except Exception as e:
            raise URCborError(e) from e

        return cls(ur_type, cbor)

    def string(self) -> str:
        """Return the UR string representation."""
        data = self._cbor.to_cbor_data()
        return encode_ur(data, self._ur_type.value)

    def qr_string(self) -> str:
        """Return the UR string in uppercase (most efficient for QR codes)."""
        return self.string().upper()

    def qr_data(self) -> bytes:
        """Return the QR-optimized UR string as bytes."""
        return self.qr_string().encode()

    def check_type(self, expected: str | URType) -> None:
        """Verify the UR type matches the expected type."""
        if isinstance(expected, str):
            expected = URType(expected)
        if self._ur_type != expected:
            raise UnexpectedTypeError(expected.value, self._ur_type.value)

    @property
    def ur_type(self) -> URType:
        return self._ur_type

    @property
    def ur_type_str(self) -> str:
        return self._ur_type.value

    @property
    def cbor(self) -> CBOR:
        return self._cbor

    def __eq__(self, other: object) -> bool:
        if isinstance(other, UR):
            return self._ur_type == other._ur_type and self._cbor == other._cbor
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._ur_type, self._cbor.to_cbor_data()))

    def __str__(self) -> str:
        return self.string()

    def __repr__(self) -> str:
        return f"UR({self._ur_type.value!r}, {self._cbor!r})"
