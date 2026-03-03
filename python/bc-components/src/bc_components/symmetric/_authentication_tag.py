"""Authentication tag for ChaCha20-Poly1305 AEAD.

A 16-byte Poly1305 MAC value that verifies both the authenticity and
integrity of an encrypted message.
"""

from __future__ import annotations

from dcbor import CBOR

from .._error import BCComponentsError

AUTHENTICATION_TAG_SIZE: int = 16


class AuthenticationTag:
    """A 16-byte authentication tag produced by ChaCha20-Poly1305 encryption."""

    AUTHENTICATION_TAG_SIZE: int = 16

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != self.AUTHENTICATION_TAG_SIZE:
            raise BCComponentsError.invalid_size(
                "authentication tag", self.AUTHENTICATION_TAG_SIZE, len(data)
            )
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def from_data(data: bytes | bytearray | memoryview) -> AuthenticationTag:
        """Restore an AuthenticationTag from a byte-like object, validating length."""
        return AuthenticationTag(bytes(data))

    # --- Accessors ---

    @property
    def data(self) -> bytes:
        """The raw 16-byte authentication tag."""
        return self._data

    # --- CBOR (untagged, used inside EncryptedMessage) ---

    def to_cbor(self) -> CBOR:
        """Encode as a CBOR byte string (no outer tag)."""
        return CBOR.from_bytes(self._data)

    @staticmethod
    def from_cbor(cbor: CBOR) -> AuthenticationTag:
        """Decode from a CBOR byte string."""
        data = cbor.try_byte_string()
        return AuthenticationTag.from_data(data)

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, AuthenticationTag):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"AuthenticationTag({self._data.hex()})"

    def __str__(self) -> str:
        return f"AuthenticationTag({self._data.hex()})"

    def __bytes__(self) -> bytes:
        return self._data
