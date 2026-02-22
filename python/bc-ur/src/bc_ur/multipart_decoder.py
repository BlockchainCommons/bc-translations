"""Multipart UR decoder using fountain codes."""

from __future__ import annotations

from dcbor import CBOR

from ._fountain_decoder import FountainDecoder
from ._fountain_part import FountainPart
from ._ur_encoding import decode_ur
from .error import (
    InvalidSchemeError,
    InvalidTypeError,
    URCborError,
    URDecoderError,
    UnexpectedTypeError,
)
from .ur import UR
from .ur_type import URType


class MultipartDecoder:
    """Decodes multipart UR strings back into a UR using fountain codes."""

    __slots__ = ("_ur_type", "_decoder")

    def __init__(self) -> None:
        self._ur_type: URType | None = None
        self._decoder = FountainDecoder()

    def receive(self, value: str) -> None:
        """Receive a multipart UR string."""
        # Normalize to lowercase for case-insensitive handling
        value = value.lower()
        decoded_type = self._decode_type(value)

        if self._ur_type is not None:
            if self._ur_type != decoded_type:
                raise UnexpectedTypeError(
                    self._ur_type.value, decoded_type.value
                )
        else:
            self._ur_type = decoded_type

        kind, data = decode_ur(value)
        if kind != "multi":
            raise URDecoderError("Can't decode single-part UR as multi-part")

        part = FountainPart.from_cbor(data)
        self._decoder.receive(part)

    @property
    def is_complete(self) -> bool:
        return self._decoder.is_complete

    def message(self) -> UR | None:
        """Return the decoded UR if complete, else None."""
        data = self._decoder.message()
        if data is None:
            return None

        try:
            cbor = CBOR.from_data(data)
        except Exception as e:
            raise URCborError(e) from e

        if self._ur_type is None:
            raise URDecoderError("UR type not set")
        return UR(self._ur_type, cbor)

    @staticmethod
    def _decode_type(ur_string: str) -> URType:
        if not ur_string.startswith("ur:"):
            raise InvalidSchemeError()
        rest = ur_string[3:]
        slash_pos = rest.find("/")
        if slash_pos < 0:
            raise InvalidTypeError()
        type_str = rest[:slash_pos]
        return URType(type_str)
