"""Symmetric encryption key for ChaCha20-Poly1305 AEAD.

A 32-byte key used for both encryption and decryption following the IETF
ChaCha20-Poly1305 specification (RFC 8439).
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_crypto import (
    aead_chacha20_poly1305_decrypt_with_aad,
    aead_chacha20_poly1305_encrypt_with_aad,
)
from bc_rand import RandomNumberGenerator, SecureRandomNumberGenerator
from bc_tags import TAG_SYMMETRIC_KEY, tags_for_values
from dcbor import CBOR, Tag

from .._digest import Digest
from .._error import BCComponentsError
from .._nonce import Nonce
from .._reference import Reference
from ._authentication_tag import AuthenticationTag
from ._encrypted_message import EncryptedMessage

if TYPE_CHECKING:
    from bc_ur import UR


SYMMETRIC_KEY_SIZE: int = 32


class SymmetricKey:
    """A 32-byte symmetric encryption key for ChaCha20-Poly1305 AEAD."""

    SYMMETRIC_KEY_SIZE: int = 32

    __slots__ = ("_data",)

    def __init__(self, data: bytes) -> None:
        if len(data) != self.SYMMETRIC_KEY_SIZE:
            raise BCComponentsError.invalid_size(
                "symmetric key", self.SYMMETRIC_KEY_SIZE, len(data)
            )
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def generate() -> SymmetricKey:
        """Create a new random symmetric key."""
        rng = SecureRandomNumberGenerator()
        return SymmetricKey.generate_using(rng)

    @staticmethod
    def generate_using(rng: RandomNumberGenerator) -> SymmetricKey:
        """Create a new random symmetric key using the given RNG."""
        data = rng.random_data(SymmetricKey.SYMMETRIC_KEY_SIZE)
        return SymmetricKey.from_data(bytes(data))

    @staticmethod
    def from_data(data: bytes | bytearray) -> SymmetricKey:
        """Create a symmetric key from exactly 32 bytes."""
        return SymmetricKey(bytes(data))

    @staticmethod
    def from_hex(hex_str: str) -> SymmetricKey:
        """Create a symmetric key from a 64-character hex string."""
        return SymmetricKey.from_data(bytes.fromhex(hex_str))

    # --- Accessors ---

    @property
    def data(self) -> bytes:
        """The raw 32-byte key."""
        return self._data

    def hex(self) -> str:
        """The key as a 64-character lowercase hex string."""
        return self._data.hex()

    # --- Encryption / Decryption ---

    def encrypt(
        self,
        plaintext: bytes | bytearray,
        aad: bytes | bytearray | None = None,
        nonce: Nonce | None = None,
    ) -> EncryptedMessage:
        """Encrypt *plaintext* with this key.

        Args:
            plaintext: The data to encrypt.
            aad: Optional additional authenticated data.
            nonce: Optional nonce; a random one is generated if omitted.

        Returns:
            An ``EncryptedMessage`` containing the ciphertext, nonce,
            authentication tag, and AAD.
        """
        used_aad = bytes(aad) if aad is not None else b""
        used_nonce = nonce if nonce is not None else Nonce.generate()
        ciphertext, auth = aead_chacha20_poly1305_encrypt_with_aad(
            bytes(plaintext),
            self._data,
            used_nonce.data,
            used_aad,
        )
        return EncryptedMessage(
            ciphertext,
            used_aad,
            used_nonce,
            AuthenticationTag.from_data(auth),
        )

    def encrypt_with_digest(
        self,
        plaintext: bytes | bytearray,
        digest: Digest,
        nonce: Nonce | None = None,
    ) -> EncryptedMessage:
        """Encrypt *plaintext*, embedding the given *digest* in the AAD."""
        return self.encrypt(plaintext, digest.tagged_cbor_data(), nonce)

    def decrypt(self, message: EncryptedMessage) -> bytes:
        """Decrypt an ``EncryptedMessage`` with this key.

        Raises ``bc_crypto.AeadError`` if decryption fails.
        """
        return aead_chacha20_poly1305_decrypt_with_aad(
            message.ciphertext,
            self._data,
            message.nonce.data,
            message.aad,
            message.authentication_tag.data,
        )

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_SYMMETRIC_KEY])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_bytes(self._data)

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @classmethod
    def from_untagged_cbor(cls, cbor: CBOR) -> SymmetricKey:
        data = cbor.try_byte_string()
        return cls.from_data(data)

    @classmethod
    def from_tagged_cbor(cls, cbor: CBOR) -> SymmetricKey:
        tags = cls.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return cls.from_untagged_cbor(item)

    @classmethod
    def from_tagged_cbor_data(cls, data: bytes) -> SymmetricKey:
        cbor = CBOR.from_data(data)
        return cls.from_tagged_cbor(cbor)

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    # --- ReferenceProvider ---

    def reference(self) -> Reference:
        return Reference.from_digest(
            Digest.from_image(self.tagged_cbor_data())
        )

    def ref_hex(self) -> str:
        return self.reference().ref_hex()

    def ref_hex_short(self) -> str:
        return self.reference().ref_hex_short()

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, SymmetricKey):
            return self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._data)

    def __repr__(self) -> str:
        return f"SymmetricKey({self.hex()})"

    def __str__(self) -> str:
        return f"SymmetricKey({self.ref_hex_short()})"

    def __bytes__(self) -> bytes:
        return self._data
