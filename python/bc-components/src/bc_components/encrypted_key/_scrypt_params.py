"""Scrypt key derivation parameters."""

from __future__ import annotations

from bc_crypto import scrypt_opt
from bc_tags import CBOR

from .._salt import Salt
from ..symmetric._encrypted_message import EncryptedMessage
from ..symmetric._symmetric_key import SymmetricKey
from ._hkdf_params import SALT_LEN
from ._key_derivation_method import KeyDerivationMethod


class ScryptParams:
    """Scrypt key derivation parameters.

    CDDL::

        ScryptParams = [2, Salt, log_n: uint, r: uint, p: uint]
    """

    INDEX: int = KeyDerivationMethod.SCRYPT.value

    __slots__ = ("_salt", "_log_n", "_r", "_p")

    def __init__(self, salt: Salt, log_n: int, r: int, p: int) -> None:
        self._salt = salt
        self._log_n = log_n
        self._r = r
        self._p = p

    # --- Construction ---

    @staticmethod
    def generate() -> ScryptParams:
        """Create new Scrypt params with defaults (log_n=15, r=8, p=1)."""
        return ScryptParams(Salt.generate_with_len(SALT_LEN), 15, 8, 1)

    @staticmethod
    def new_opt(salt: Salt, log_n: int, r: int, p: int) -> ScryptParams:
        """Create new Scrypt params with specific parameters."""
        return ScryptParams(salt, log_n, r, p)

    # --- Properties ---

    @property
    def salt(self) -> Salt:
        return self._salt

    @property
    def log_n(self) -> int:
        return self._log_n

    @property
    def r(self) -> int:
        return self._r

    @property
    def p(self) -> int:
        return self._p

    # --- Key derivation ---

    def _derive_key(self, secret: bytes | bytearray) -> SymmetricKey:
        """Derive a symmetric key using scrypt."""
        derived = scrypt_opt(
            bytes(secret), self._salt.data, 32,
            self._log_n, self._r, self._p,
        )
        return SymmetricKey.from_data(derived)

    def lock(
        self,
        content_key: SymmetricKey,
        secret: bytes | bytearray,
    ) -> EncryptedMessage:
        """Derive a key from *secret* via scrypt and encrypt *content_key*."""
        derived_key = self._derive_key(secret)
        encoded_method = self.to_cbor().to_cbor_data()
        return derived_key.encrypt(content_key.data, aad=encoded_method)

    def unlock(
        self,
        encrypted_message: EncryptedMessage,
        secret: bytes | bytearray,
    ) -> SymmetricKey:
        """Derive a key from *secret* via scrypt and decrypt the content key."""
        derived_key = self._derive_key(secret)
        plaintext = derived_key.decrypt(encrypted_message)
        return SymmetricKey.from_data(plaintext)

    # --- Display ---

    def __str__(self) -> str:
        return "Scrypt"

    def __repr__(self) -> str:
        return (
            f"ScryptParams(salt={self._salt!r}, "
            f"log_n={self._log_n}, r={self._r}, p={self._p})"
        )

    def __eq__(self, other: object) -> bool:
        if isinstance(other, ScryptParams):
            return (
                self._salt == other._salt
                and self._log_n == other._log_n
                and self._r == other._r
                and self._p == other._p
            )
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._salt, self._log_n, self._r, self._p))

    # --- CBOR ---

    def to_cbor(self) -> CBOR:
        """Encode as CBOR array: [INDEX, salt, log_n, r, p]."""
        return CBOR.from_array([
            CBOR.from_int(self.INDEX),
            self._salt.untagged_cbor(),
            CBOR.from_int(self._log_n),
            CBOR.from_int(self._r),
            CBOR.from_int(self._p),
        ])

    @staticmethod
    def from_cbor(cbor: CBOR) -> ScryptParams:
        """Decode from CBOR array: [INDEX, salt, log_n, r, p]."""
        a = cbor.try_array()
        if len(a) != 5:
            raise ValueError("Invalid ScryptParams: expected 5 elements")
        salt = Salt.from_untagged_cbor(a[1])
        log_n = a[2].try_int()
        r = a[3].try_int()
        p = a[4].try_int()
        return ScryptParams(salt, log_n, r, p)
