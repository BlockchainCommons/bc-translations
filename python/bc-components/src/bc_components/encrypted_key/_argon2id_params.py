"""Argon2id key derivation parameters."""

from __future__ import annotations

from bc_crypto import argon2id
from bc_tags import CBOR

from .._salt import Salt
from ..symmetric._encrypted_message import EncryptedMessage
from ..symmetric._symmetric_key import SymmetricKey
from ._hkdf_params import SALT_LEN
from ._key_derivation_method import KeyDerivationMethod


class Argon2idParams:
    """Argon2id key derivation parameters.

    CDDL::

        Argon2idParams = [3, Salt]
    """

    INDEX: int = KeyDerivationMethod.ARGON2ID.value

    __slots__ = ("_salt",)

    def __init__(self, salt: Salt) -> None:
        self._salt = salt

    # --- Construction ---

    @staticmethod
    def generate() -> Argon2idParams:
        """Create new Argon2id params with a random salt."""
        return Argon2idParams(Salt.generate_with_len(SALT_LEN))

    @staticmethod
    def new_opt(salt: Salt) -> Argon2idParams:
        """Create new Argon2id params with a specific salt."""
        return Argon2idParams(salt)

    # --- Properties ---

    @property
    def salt(self) -> Salt:
        return self._salt

    # --- Key derivation ---

    def _derive_key(self, secret: bytes | bytearray) -> SymmetricKey:
        """Derive a symmetric key using Argon2id."""
        derived = argon2id(bytes(secret), self._salt.data, 32)
        return SymmetricKey.from_data(derived)

    def lock(
        self,
        content_key: SymmetricKey,
        secret: bytes | bytearray,
    ) -> EncryptedMessage:
        """Derive a key from *secret* via Argon2id and encrypt *content_key*."""
        derived_key = self._derive_key(secret)
        encoded_method = self.to_cbor().to_cbor_data()
        return derived_key.encrypt(content_key.data, aad=encoded_method)

    def unlock(
        self,
        encrypted_message: EncryptedMessage,
        secret: bytes | bytearray,
    ) -> SymmetricKey:
        """Derive a key from *secret* via Argon2id and decrypt the content key."""
        derived_key = self._derive_key(secret)
        plaintext = derived_key.decrypt(encrypted_message)
        return SymmetricKey.from_data(plaintext)

    # --- Display ---

    def __str__(self) -> str:
        return "Argon2id"

    def __repr__(self) -> str:
        return f"Argon2idParams(salt={self._salt!r})"

    def __eq__(self, other: object) -> bool:
        if isinstance(other, Argon2idParams):
            return self._salt == other._salt
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._salt)

    # --- CBOR ---

    def to_cbor(self) -> CBOR:
        """Encode as CBOR array: [INDEX, salt]."""
        return CBOR.from_array([
            CBOR.from_int(self.INDEX),
            self._salt.untagged_cbor(),
        ])

    @staticmethod
    def from_cbor(cbor: CBOR) -> Argon2idParams:
        """Decode from CBOR array: [INDEX, salt]."""
        a = cbor.try_array()
        if len(a) != 2:
            raise ValueError("Invalid Argon2idParams: expected 2 elements")
        salt = Salt.from_untagged_cbor(a[1])
        return Argon2idParams(salt)
