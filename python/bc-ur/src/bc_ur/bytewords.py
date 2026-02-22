"""Bytewords encoding and decoding with CRC32 checksums.

Bytewords encodes byte data using a dictionary of 256 four-letter words.
Three encoding styles are supported:
  - STANDARD: words separated by spaces
  - URI: words separated by dashes
  - MINIMAL: first and last letter of each word, concatenated
"""

from __future__ import annotations

import enum

from ._bytewords_constants import (
    BYTEMOJIS,
    BYTEWORDS,
    MINIMAL_TO_INDEX,
    MINIMALS,
    WORD_TO_INDEX,
)
from ._crc32 import crc32
from .error import BytewordsError


class BytewordsStyle(enum.Enum):
    """Encoding style for bytewords."""

    STANDARD = "standard"
    URI = "uri"
    MINIMAL = "minimal"


def encode(data: bytes, style: BytewordsStyle) -> str:
    """Encode bytes as a bytewords string with CRC32 checksum."""
    checksum = crc32(data).to_bytes(4, "big")
    all_bytes = bytes(data) + checksum

    if style == BytewordsStyle.MINIMAL:
        words = [MINIMALS[b] for b in all_bytes]
        return "".join(words)

    words = [BYTEWORDS[b] for b in all_bytes]
    separator = " " if style == BytewordsStyle.STANDARD else "-"
    return separator.join(words)


def decode(encoded: str, style: BytewordsStyle) -> bytes:
    """Decode a bytewords string back to bytes, verifying CRC32 checksum."""
    if not encoded.isascii():
        raise BytewordsError("bytewords string contains non-ASCII characters")

    if style == BytewordsStyle.MINIMAL:
        return _decode_minimal(encoded)

    separator = " " if style == BytewordsStyle.STANDARD else "-"
    parts = encoded.split(separator)
    data = _decode_words(parts, WORD_TO_INDEX)
    return _strip_checksum(data)


def _decode_minimal(encoded: str) -> bytes:
    if len(encoded) % 2 != 0:
        raise BytewordsError("invalid length")

    parts = [encoded[i : i + 2] for i in range(0, len(encoded), 2)]
    data = _decode_words(parts, MINIMAL_TO_INDEX)
    return _strip_checksum(data)


def _decode_words(words: list[str], index: dict[str, int]) -> bytes:
    result = []
    for word in words:
        if word not in index:
            raise BytewordsError("invalid word")
        result.append(index[word])
    return bytes(result)


def _strip_checksum(data: bytes) -> bytes:
    if len(data) < 4:
        raise BytewordsError("invalid checksum")
    payload = data[:-4]
    checksum = data[-4:]
    expected = crc32(payload).to_bytes(4, "big")
    if checksum != expected:
        raise BytewordsError("invalid checksum")
    return payload


def identifier(data: bytes) -> str:
    """Encode a 4-byte slice as space-separated bytewords for identification."""
    if len(data) != 4:
        raise ValueError("identifier requires exactly 4 bytes")
    words = [BYTEWORDS[b] for b in data]
    return " ".join(words)


def bytemoji_identifier(data: bytes) -> str:
    """Encode a 4-byte slice as space-separated bytemojis for identification."""
    if len(data) != 4:
        raise ValueError("bytemoji_identifier requires exactly 4 bytes")
    emojis = [BYTEMOJIS[b] for b in data]
    return " ".join(emojis)
