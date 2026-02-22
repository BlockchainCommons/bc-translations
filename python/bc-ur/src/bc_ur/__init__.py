"""Blockchain Commons Uniform Resources (UR) for Python.

Uniform Resources (URs) are URI-encoded CBOR structures developed by
Blockchain Commons. This package provides encoding, decoding, and
multipart (fountain code) support for URs.
"""

from . import bytewords
from .bytewords import BytewordsStyle
from .codable import (
    URDecodable,
    UREncodable,
    from_ur,
    from_ur_string,
    to_ur,
    to_ur_string,
)
from .error import (
    BytewordsError,
    FountainError,
    InvalidSchemeError,
    InvalidTypeError,
    NotSinglePartError,
    TypeUnspecifiedError,
    URCborError,
    URDecoderError,
    URError,
    UnexpectedTypeError,
)
from .multipart_decoder import MultipartDecoder
from .multipart_encoder import MultipartEncoder
from .ur import UR
from .ur_type import URType

__all__ = [
    "UR",
    "URType",
    "URError",
    "URDecoderError",
    "BytewordsError",
    "URCborError",
    "InvalidSchemeError",
    "TypeUnspecifiedError",
    "InvalidTypeError",
    "NotSinglePartError",
    "UnexpectedTypeError",
    "FountainError",
    "UREncodable",
    "URDecodable",
    "to_ur",
    "to_ur_string",
    "from_ur",
    "from_ur_string",
    "MultipartEncoder",
    "MultipartDecoder",
    "BytewordsStyle",
    "bytewords",
]
