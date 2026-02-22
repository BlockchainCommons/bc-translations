"""Low-level UR string encoding and decoding.

Implements the UR URI scheme: ur:<type>/<bytewords>
and multipart: ur:<type>/<seq>-<count>/<bytewords>
"""

from __future__ import annotations

from .bytewords import BytewordsStyle
from .bytewords import decode as bw_decode
from .bytewords import encode as bw_encode
from .error import (
    BytewordsError,
    InvalidSchemeError,
    InvalidTypeError,
    TypeUnspecifiedError,
    URDecoderError,
    URError,
)
from .ur_type import _is_ur_type_char


def encode_ur(data: bytes, ur_type: str) -> str:
    """Encode a data payload as a single-part UR string."""
    body = bw_encode(data, BytewordsStyle.MINIMAL)
    return f"ur:{ur_type}/{body}"


def decode_ur(value: str) -> tuple[str, bytes]:
    """Decode a single-part UR string.

    Returns:
        A tuple of (kind, data) where kind is "single" or "multi".
        For single-part, data is the decoded payload.
        For multi-part, data is the decoded CBOR fountain part.
    """
    if not value.startswith("ur:"):
        raise InvalidSchemeError()

    rest = value[3:]
    slash_pos = rest.find("/")
    if slash_pos < 0:
        raise TypeUnspecifiedError()

    ur_type = rest[:slash_pos]

    # Validate type characters
    if not ur_type or not all(_is_ur_type_char(c) for c in ur_type):
        raise InvalidTypeError()

    payload = rest[slash_pos + 1 :]

    # Check for multi-part (has another slash)
    second_slash = payload.rfind("/")
    if second_slash < 0:
        # Single-part
        decoded = bw_decode(payload, BytewordsStyle.MINIMAL)
        return "single", decoded
    else:
        # Multi-part: indices/payload
        indices = payload[:second_slash]
        actual_payload = payload[second_slash + 1 :]

        parts = indices.split("-")
        if len(parts) != 2:
            raise URDecoderError("Invalid indices")
        try:
            int(parts[0])
            int(parts[1])
        except ValueError:
            raise URDecoderError("Invalid indices")

        decoded = bw_decode(actual_payload, BytewordsStyle.MINIMAL)
        return "multi", decoded
