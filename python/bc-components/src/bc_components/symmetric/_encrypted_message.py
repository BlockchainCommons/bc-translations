"""Encrypted message using ChaCha20-Poly1305 AEAD.

Contains ciphertext, nonce, authentication tag, and optional additional
authenticated data (AAD).
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_tags import TAG_ENCRYPTED, tags_for_values
from dcbor import CBOR, Tag

from .._digest import Digest
from .._error import BCComponentsError
from .._nonce import Nonce
from ._authentication_tag import AuthenticationTag

if TYPE_CHECKING:
    from bc_ur import UR


class EncryptedMessage:
    """A ChaCha20-Poly1305 encrypted message with optional AAD.

    CDDL::

        EncryptedMessage =
            #6.40002([ ciphertext: bstr, nonce: bstr, auth: bstr, ? aad: bstr ])
    """

    __slots__ = ("_ciphertext", "_aad", "_nonce", "_auth")

    def __init__(
        self,
        ciphertext: bytes | bytearray,
        aad: bytes | bytearray,
        nonce: Nonce,
        auth: AuthenticationTag,
    ) -> None:
        self._ciphertext = bytes(ciphertext)
        self._aad = bytes(aad)
        self._nonce = nonce
        self._auth = auth

    # --- Construction ---

    @staticmethod
    def new(
        ciphertext: bytes | bytearray,
        aad: bytes | bytearray,
        nonce: Nonce,
        auth: AuthenticationTag,
    ) -> EncryptedMessage:
        """Restore an EncryptedMessage from its component parts."""
        return EncryptedMessage(ciphertext, aad, nonce, auth)

    # --- Accessors ---

    @property
    def ciphertext(self) -> bytes:
        """The encrypted data."""
        return self._ciphertext

    @property
    def aad(self) -> bytes:
        """Additional authenticated data (may be empty)."""
        return self._aad

    @property
    def nonce(self) -> Nonce:
        """The 12-byte nonce used for encryption."""
        return self._nonce

    @property
    def authentication_tag(self) -> AuthenticationTag:
        """The 16-byte Poly1305 authentication tag."""
        return self._auth

    # --- AAD helpers ---

    def aad_cbor(self) -> CBOR | None:
        """Parse the AAD as CBOR, or return ``None``."""
        if not self._aad:
            return None
        try:
            return CBOR.from_data(self._aad)
        except Exception:
            return None

    def aad_digest(self) -> Digest | None:
        """Parse the AAD as a tagged CBOR ``Digest``, or return ``None``."""
        cbor = self.aad_cbor()
        if cbor is None:
            return None
        try:
            return Digest.from_tagged_cbor(cbor)
        except Exception:
            return None

    @property
    def has_digest(self) -> bool:
        """``True`` if the AAD contains a valid tagged Digest."""
        return self.aad_digest() is not None

    # --- DigestProvider ---

    def digest(self) -> Digest:
        """Return the digest stored in the AAD (raises if absent)."""
        d = self.aad_digest()
        if d is not None:
            return d
        raise BCComponentsError.invalid_data(
            "EncryptedMessage", "AAD does not contain a valid digest"
        )

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_ENCRYPTED])

    def untagged_cbor(self) -> CBOR:
        items: list[CBOR] = [
            CBOR.from_bytes(self._ciphertext),
            CBOR.from_bytes(self._nonce.data),
            CBOR.from_bytes(self._auth.data),
        ]
        if self._aad:
            items.append(CBOR.from_bytes(self._aad))
        return CBOR.from_array(items)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @classmethod
    def from_untagged_cbor(cls, cbor: CBOR) -> EncryptedMessage:
        elements = cbor.try_array()
        if len(elements) < 3:
            raise BCComponentsError.invalid_data(
                "EncryptedMessage", "must have at least 3 elements"
            )
        ciphertext = elements[0].try_byte_string()
        nonce_data = elements[1].try_byte_string()
        nonce = Nonce.from_data(nonce_data)
        auth_data = elements[2].try_byte_string()
        auth = AuthenticationTag.from_data(auth_data)
        aad = elements[3].try_byte_string() if len(elements) > 3 else b""
        return cls(ciphertext, aad, nonce, auth)

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> EncryptedMessage:
        tags = cls.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return cls.from_untagged_cbor(item)

    @classmethod
    def from_tagged_cbor_data(cls, data: bytes) -> EncryptedMessage:
        cbor = CBOR.from_data(data)
        return cls.from_tagged_cbor(cbor)

    # --- UR ---

    def to_ur(self) -> UR:
        from bc_ur import to_ur
        return to_ur(self)

    def ur_string(self) -> str:
        from bc_ur import to_ur_string
        return to_ur_string(self)

    @classmethod
    def from_ur(cls, ur: UR) -> EncryptedMessage:
        from bc_ur import from_ur
        return from_ur(cls, ur)

    @classmethod
    def from_ur_string(cls, ur_string: str) -> EncryptedMessage:
        from bc_ur import from_ur_string
        return from_ur_string(cls, ur_string)

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, EncryptedMessage):
            return (
                self._ciphertext == other._ciphertext
                and self._aad == other._aad
                and self._nonce == other._nonce
                and self._auth == other._auth
            )
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._ciphertext, self._aad, self._nonce, self._auth))

    def __repr__(self) -> str:
        return (
            f"EncryptedMessage("
            f"ciphertext={self._ciphertext.hex()}, "
            f"aad={self._aad.hex()}, "
            f"nonce={self._nonce!r}, "
            f"auth={self._auth!r})"
        )

    def __str__(self) -> str:
        return f"EncryptedMessage(ciphertext={self._ciphertext.hex()})"
