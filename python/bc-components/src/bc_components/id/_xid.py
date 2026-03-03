"""XID (eXtensible IDentifier)."""

from __future__ import annotations

import functools
from typing import TYPE_CHECKING, Protocol, runtime_checkable

from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_XID,
)
from bc_ur import bytewords

from .._digest import Digest
from .._error import InvalidSizeError
from .._reference import Reference, ReferenceProvider

if TYPE_CHECKING:
    from ..signing._signing_public_key import SigningPublicKey


XID_SIZE: int = 32


@runtime_checkable
class XIDProvider(Protocol):
    """A provider trait for obtaining XIDs from various objects."""

    def xid(self) -> XID: ...


@functools.total_ordering
class XID(ReferenceProvider):
    """An eXtensible IDentifier (XID).

    A XID is a unique 32-byte identifier for a subject entity (person,
    organization, device, or any other entity).

    - Cryptographically tied to a public key at inception (the "genesis key")
    - Remains stable throughout its lifecycle
    - Created by taking the SHA-256 hash of the CBOR encoding of a
      public signing key

    As defined in BCR-2024-010.
    """

    XID_SIZE: int = 32

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != self.XID_SIZE:
            raise InvalidSizeError("XID", self.XID_SIZE, len(data))
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def from_data(data: bytes | bytearray | memoryview) -> XID:
        """Create a new XID from a byte-like object, validating length."""
        return XID(bytes(data))

    @staticmethod
    def new(genesis_key: SigningPublicKey) -> XID:
        """Create a new XID from the given public key (the "genesis key").

        The XID is the SHA-256 digest of the CBOR encoding of the public key.
        """
        key_cbor_data = genesis_key.tagged_cbor_data()
        digest = Digest.from_image(key_cbor_data)
        return XID(digest.data)

    @staticmethod
    def from_hex(hex_str: str) -> XID:
        """Create a XID from a hexadecimal string.

        Raises ValueError if the string is not exactly 64 hex digits.
        """
        return XID(bytes.fromhex(hex_str))

    # --- Properties ---

    @property
    def data(self) -> bytes:
        """The raw 32-byte XID."""
        return self._data

    def hex(self) -> str:
        """Return the XID as a hexadecimal string."""
        return self._data.hex()

    def short_description(self) -> str:
        """Return the first four bytes as a hex string."""
        return self.ref_hex_short()

    # --- Validation ---

    def validate(self, key: SigningPublicKey) -> bool:
        """Validate the XID against the given public key."""
        key_data = key.tagged_cbor_data()
        digest = Digest.from_image(key_data)
        return digest.data == self._data

    # --- Identifier display ---

    def ref_hex_short(self) -> str:
        """Return the first four bytes as an 8-character hex string."""
        return self._data[:4].hex()

    def bytewords_identifier(self, prefix: bool = False) -> str:
        """Return the first four bytes as upper-case ByteWords."""
        s = bytewords.identifier(self._data[:4]).upper()
        if prefix:
            return f"\U0001F167 {s}"
        return s

    def bytemoji_identifier(self, prefix: bool = False) -> str:
        """Return the first four bytes as Bytemoji."""
        s = bytewords.bytemoji_identifier(self._data[:4])
        if prefix:
            return f"\U0001F167 {s}"
        return s

    # --- XIDProvider ---

    def xid(self) -> XID:
        return self

    # --- ReferenceProvider ---

    def reference(self) -> Reference:
        return Reference.from_data(self._data)

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_XID])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_bytes(self._data)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> XID:
        data = cbor.try_byte_string()
        return XID.from_data(data)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> XID:
        tags = XID.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return XID.from_untagged_cbor(item)

    # --- UR ---

    def ur_string(self) -> str:
        from bc_ur import to_ur_string
        return to_ur_string(self)

    @staticmethod
    def from_ur_string(ur_string: str) -> XID:
        from bc_ur import from_ur_string
        return from_ur_string(XID, ur_string)

    # --- Dunder ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, XID):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __lt__(self, other: XID) -> bool:
        if not isinstance(other, XID):
            return NotImplemented  # type: ignore[return-value]
        return self._data < other._data

    def __repr__(self) -> str:
        return f"XID({self.hex()})"

    def __str__(self) -> str:
        return f"XID({self.short_description()})"

    def __bytes__(self) -> bytes:
        return self._data
