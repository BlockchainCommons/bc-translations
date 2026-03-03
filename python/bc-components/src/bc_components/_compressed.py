"""Compressed binary object with integrity verification.

Uses raw DEFLATE (RFC 1951) compression via the ``zlib`` stdlib module,
with CRC-32 checksum and optional cryptographic digest.
"""

from __future__ import annotations

import zlib

from bc_crypto import crc32
from bc_tags import TAG_COMPRESSED, tags_for_values
from dcbor import CBOR, Tag

from ._digest import Digest
from ._error import BCComponentsError


class Compressed:
    """A compressed binary object with CRC-32 checksum and optional digest."""

    __slots__ = ("_checksum", "_decompressed_size", "_compressed_data", "_digest")

    def __init__(
        self,
        checksum: int,
        decompressed_size: int,
        compressed_data: bytes,
        digest: Digest | None = None,
    ) -> None:
        if len(compressed_data) > decompressed_size:
            raise BCComponentsError.compression(
                "compressed data is larger than decompressed size"
            )
        self._checksum = checksum
        self._decompressed_size = decompressed_size
        self._compressed_data = bytes(compressed_data)
        self._digest = digest

    # --- Construction --------------------------------------------------

    @staticmethod
    def new(
        checksum: int,
        decompressed_size: int,
        compressed_data: bytes,
        digest: Digest | None = None,
    ) -> Compressed:
        """Low-level constructor for pre-compressed data or deserialization."""
        return Compressed(checksum, decompressed_size, compressed_data, digest)

    @staticmethod
    def from_decompressed_data(
        decompressed_data: bytes | bytearray,
        digest: Digest | None = None,
    ) -> Compressed:
        """Compress *decompressed_data* using raw DEFLATE (level 6).

        If compression would increase the size, the original data is stored.
        """
        decompressed_data = bytes(decompressed_data)
        # Raw DEFLATE with wbits=-15, level 6 to match Rust miniz_oxide level 6
        compressed_data = zlib.compress(decompressed_data, level=6, wbits=-15)
        checksum = crc32(decompressed_data)
        decompressed_size = len(decompressed_data)
        compressed_size = len(compressed_data)
        if compressed_size != 0 and compressed_size < decompressed_size:
            return Compressed(checksum, decompressed_size, compressed_data, digest)
        return Compressed(
            checksum, decompressed_size, decompressed_data, digest,
        )

    # --- Decompression -------------------------------------------------

    def decompress(self) -> bytes:
        """Decompress and return the original data, verifying the CRC-32 checksum."""
        compressed_size = len(self._compressed_data)
        if compressed_size >= self._decompressed_size:
            # Data was stored uncompressed
            return self._compressed_data

        try:
            decompressed = zlib.decompress(self._compressed_data, wbits=-15)
        except zlib.error:
            raise BCComponentsError.compression("corrupt compressed data")

        if crc32(decompressed) != self._checksum:
            raise BCComponentsError.compression(
                "compressed data checksum mismatch"
            )
        return decompressed

    # --- Accessors -----------------------------------------------------

    @property
    def checksum(self) -> int:
        return self._checksum

    @property
    def decompressed_size(self) -> int:
        return self._decompressed_size

    @property
    def compressed_size(self) -> int:
        return len(self._compressed_data)

    @property
    def compression_ratio(self) -> float:
        if self._decompressed_size == 0:
            return float("nan")
        return self.compressed_size / self._decompressed_size

    @property
    def digest_opt(self) -> Digest | None:
        return self._digest

    @property
    def has_digest(self) -> bool:
        return self._digest is not None

    def digest(self) -> Digest:
        """Return the digest, raising if none is present."""
        if self._digest is None:
            raise ValueError("Compressed has no digest")
        return self._digest

    # --- CBOR ----------------------------------------------------------

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_COMPRESSED])

    def untagged_cbor(self) -> CBOR:
        elements: list[CBOR] = [
            CBOR.from_int(self._checksum),
            CBOR.from_int(self._decompressed_size),
            CBOR.from_bytes(self._compressed_data),
        ]
        if self._digest is not None:
            elements.append(self._digest.tagged_cbor())
        return CBOR.from_array(elements)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @classmethod
    def from_untagged_cbor(cls, cbor: CBOR) -> Compressed:
        elements = cbor.try_array()
        if len(elements) < 3 or len(elements) > 4:
            raise BCComponentsError.invalid_data(
                "compressed", "invalid number of elements"
            )
        checksum = elements[0].try_int()
        decompressed_size = elements[1].try_int()
        compressed_data = elements[2].try_byte_string()
        digest: Digest | None = None
        if len(elements) == 4:
            digest = Digest.from_tagged_cbor(elements[3])
        return cls.new(checksum, decompressed_size, compressed_data, digest)

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> Compressed:
        tags = cls.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return cls.from_untagged_cbor(item)

    @classmethod
    def from_tagged_cbor_data(cls, data: bytes) -> Compressed:
        cbor = CBOR.from_data(data)
        return cls.from_tagged_cbor(cbor)

    # --- Dunder methods ------------------------------------------------

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Compressed):
            return NotImplemented
        return (
            self._checksum == other._checksum
            and self._decompressed_size == other._decompressed_size
            and self._compressed_data == other._compressed_data
            and self._digest == other._digest
        )

    def __hash__(self) -> int:
        return hash(
            (self._checksum, self._decompressed_size, self._compressed_data)
        )

    def __repr__(self) -> str:
        digest_str = (
            self._digest.short_description() if self._digest is not None else "None"
        )
        return (
            f"Compressed(checksum: {self._checksum:08x}, "
            f"size: {self.compressed_size}/{self._decompressed_size}, "
            f"ratio: {self.compression_ratio:.2f}, "
            f"digest: {digest_str})"
        )
