"""SHA-256 cryptographic digest.

A 32-byte SHA-256 hash used for data verification and content-addressable
identification.
"""

from __future__ import annotations

import functools
from typing import TYPE_CHECKING

from bc_crypto import sha256
from bc_tags import TAG_DIGEST, tags_for_values
from dcbor import CBOR, Tag

from ._error import BCComponentsError

if TYPE_CHECKING:
    from bc_ur import UR


DIGEST_SIZE: int = 32


@functools.total_ordering
class Digest:
    """A 32-byte SHA-256 digest."""

    DIGEST_SIZE: int = 32

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != self.DIGEST_SIZE:
            raise BCComponentsError.invalid_size("digest", self.DIGEST_SIZE, len(data))
        self._data = bytes(data)

    # --- Construction --------------------------------------------------

    @staticmethod
    def from_data(data: bytes | bytearray) -> Digest:
        """Create a digest from exactly 32 bytes."""
        return Digest(bytes(data))

    @staticmethod
    def from_image(image: bytes | bytearray) -> Digest:
        """Create a digest by hashing *image* with SHA-256."""
        return Digest(sha256(bytes(image)))

    @staticmethod
    def from_image_parts(image_parts: list[bytes]) -> Digest:
        """Create a digest by concatenating *image_parts* and hashing."""
        buf = b"".join(image_parts)
        return Digest.from_image(buf)

    @staticmethod
    def from_digests(digests: list[Digest]) -> Digest:
        """Create a digest by concatenating digest data and hashing."""
        buf = b"".join(d._data for d in digests)
        return Digest.from_image(buf)

    @staticmethod
    def from_hex(hex_str: str) -> Digest:
        """Create a digest from a 64-character hex string.

        Raises ``ValueError`` if the string is not valid hex of the right length.
        """
        return Digest(bytes.fromhex(hex_str))

    # --- Accessors -----------------------------------------------------

    @property
    def data(self) -> bytes:
        """The raw 32-byte digest."""
        return self._data

    def hex(self) -> str:
        """The digest as a 64-character lowercase hex string."""
        return self._data.hex()

    def short_description(self) -> str:
        """The first four bytes as an 8-character hex string."""
        return self._data[:4].hex()

    # --- Validation ----------------------------------------------------

    def validate(self, image: bytes | bytearray) -> bool:
        """Return ``True`` if this digest matches ``sha256(image)``."""
        return self == Digest.from_image(image)

    @staticmethod
    def validate_opt(image: bytes | bytearray, digest: Digest | None) -> bool:
        """Validate *image* against *digest*, returning ``True`` if *digest* is ``None``."""
        if digest is None:
            return True
        return digest.validate(image)

    # --- CBOR ----------------------------------------------------------

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_DIGEST])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_bytes(self._data)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @classmethod
    def from_untagged_cbor(cls, cbor: CBOR) -> Digest:
        data = cbor.try_byte_string()
        return cls.from_data(data)

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> Digest:
        tags = cls.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return cls.from_untagged_cbor(item)

    @classmethod
    def from_tagged_cbor_data(cls, data: bytes) -> Digest:
        cbor = CBOR.from_data(data)
        return cls.from_tagged_cbor(cbor)

    @classmethod
    def from_untagged_cbor_data(cls, data: bytes) -> Digest:
        cbor = CBOR.from_data(data)
        return cls.from_untagged_cbor(cbor)

    # --- UR ------------------------------------------------------------

    def to_ur(self) -> UR:
        from bc_ur import to_ur
        return to_ur(self)

    def ur_string(self) -> str:
        from bc_ur import to_ur_string
        return to_ur_string(self)

    @classmethod
    def from_ur(cls, ur: UR) -> Digest:
        from bc_ur import from_ur
        return from_ur(cls, ur)

    @classmethod
    def from_ur_string(cls, ur_string: str) -> Digest:
        from bc_ur import from_ur_string
        return from_ur_string(cls, ur_string)

    # --- DigestProvider ------------------------------------------------

    def digest(self) -> Digest:
        """A Digest is its own digest."""
        return self

    # --- Dunder methods -------------------------------------------------

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Digest):
            return NotImplemented
        return self._data == other._data

    def __hash__(self) -> int:
        return hash(self._data)

    def __lt__(self, other: Digest) -> bool:
        if not isinstance(other, Digest):
            return NotImplemented  # type: ignore[return-value]
        return self._data < other._data

    def __repr__(self) -> str:
        return f"Digest({self.hex()})"

    def __str__(self) -> str:
        return f"Digest({self.hex()})"

    def __bytes__(self) -> bytes:
        return self._data
