"""Globally unique reference type.

A ``Reference`` is a 32-byte identifier derived from a cryptographic
digest of an object's serialized form.  ``ReferenceProvider`` is a
protocol for objects that can produce a reference.
"""

from __future__ import annotations

import functools
from typing import Protocol, runtime_checkable

from bc_tags import TAG_REFERENCE, tags_for_values
from bc_ur import bytewords
from dcbor import CBOR, Tag

from ._digest import Digest
from ._error import BCComponentsError


@runtime_checkable
class ReferenceProvider(Protocol):
    """A type that can provide a globally unique reference to itself."""

    def reference(self) -> Reference: ...


@functools.total_ordering
class Reference:
    """A 32-byte globally unique reference."""

    REFERENCE_SIZE: int = 32

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != self.REFERENCE_SIZE:
            raise BCComponentsError.invalid_size(
                "reference", self.REFERENCE_SIZE, len(data),
            )
        self._data = bytes(data)

    # --- Construction --------------------------------------------------

    @staticmethod
    def from_data(data: bytes | bytearray) -> Reference:
        """Create a reference from exactly 32 bytes."""
        return Reference(bytes(data))

    @staticmethod
    def from_digest(digest: Digest) -> Reference:
        """Create a reference from a Digest."""
        return Reference(digest.data)

    @staticmethod
    def from_hex(hex_str: str) -> Reference:
        """Create a reference from a 64-character hex string."""
        return Reference(bytes.fromhex(hex_str))

    # --- Accessors -----------------------------------------------------

    @property
    def data(self) -> bytes:
        """The raw 32-byte reference."""
        return self._data

    def ref_hex(self) -> str:
        """The full 64-character hex representation."""
        return self._data.hex()

    def ref_data_short(self) -> bytes:
        """The first four bytes of the reference."""
        return self._data[:4]

    def ref_hex_short(self) -> str:
        """The first four bytes as an 8-character hex string."""
        return self._data[:4].hex()

    def bytewords_identifier(self, prefix: str | None = None) -> str:
        """The first four bytes as upper-case ByteWords."""
        s = bytewords.identifier(self._data[:4]).upper()
        if prefix is not None:
            return f"{prefix} {s}"
        return s

    def bytemoji_identifier(self, prefix: str | None = None) -> str:
        """The first four bytes as upper-case Bytemoji."""
        s = bytewords.bytemoji_identifier(self._data[:4]).upper()
        if prefix is not None:
            return f"{prefix} {s}"
        return s

    # --- DigestProvider ------------------------------------------------

    def digest(self) -> Digest:
        """Return the digest of this reference's tagged CBOR encoding."""
        return Digest.from_image(self.tagged_cbor().to_cbor_data())

    # --- ReferenceProvider ---------------------------------------------

    def reference(self) -> Reference:
        """Return a Reference to this Reference (via digest)."""
        return Reference.from_digest(self.digest())

    # --- CBOR ----------------------------------------------------------

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_REFERENCE])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_bytes(self._data)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @classmethod
    def from_untagged_cbor(cls, cbor: CBOR) -> Reference:
        data = cbor.try_byte_string()
        return cls.from_data(data)

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> Reference:
        tags = cls.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return cls.from_untagged_cbor(item)

    @classmethod
    def from_tagged_cbor_data(cls, data: bytes) -> Reference:
        cbor = CBOR.from_data(data)
        return cls.from_tagged_cbor(cbor)

    @classmethod
    def from_untagged_cbor_data(cls, data: bytes) -> Reference:
        cbor = CBOR.from_data(data)
        return cls.from_untagged_cbor(cbor)

    # --- Dunder methods ------------------------------------------------

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Reference):
            return NotImplemented
        return self._data == other._data

    def __hash__(self) -> int:
        return hash(self._data)

    def __lt__(self, other: Reference) -> bool:
        if not isinstance(other, Reference):
            return NotImplemented  # type: ignore[return-value]
        return self._data < other._data

    def __repr__(self) -> str:
        return f"Reference({self.ref_hex()})"

    def __str__(self) -> str:
        return f"Reference({self.ref_hex_short()})"

    def __bytes__(self) -> bytes:
        return self._data
