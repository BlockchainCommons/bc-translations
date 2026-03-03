"""PBKDF2 key derivation parameters."""

from __future__ import annotations

from bc_crypto import pbkdf2_hmac_sha256, pbkdf2_hmac_sha512
from bc_tags import CBOR

from .._salt import Salt
from ..symmetric._encrypted_message import EncryptedMessage
from ..symmetric._symmetric_key import SymmetricKey
from ._hash_type import HashType
from ._hkdf_params import SALT_LEN
from ._key_derivation_method import KeyDerivationMethod


class PBKDF2Params:
    """PBKDF2 key derivation parameters.

    CDDL::

        PBKDF2Params = [1, Salt, iterations: uint, HashType]
    """

    INDEX: int = KeyDerivationMethod.PBKDF2.value

    __slots__ = ("_salt", "_iterations", "_hash_type")

    def __init__(self, salt: Salt, iterations: int, hash_type: HashType) -> None:
        self._salt = salt
        self._iterations = iterations
        self._hash_type = hash_type

    # --- Construction ---

    @staticmethod
    def generate() -> PBKDF2Params:
        """Create new PBKDF2 params with defaults (100000 iterations, SHA256)."""
        return PBKDF2Params(
            Salt.generate_with_len(SALT_LEN),
            100_000,
            HashType.SHA256,
        )

    @staticmethod
    def new_opt(salt: Salt, iterations: int, hash_type: HashType) -> PBKDF2Params:
        """Create new PBKDF2 params with specific parameters."""
        return PBKDF2Params(salt, iterations, hash_type)

    # --- Properties ---

    @property
    def salt(self) -> Salt:
        return self._salt

    @property
    def iterations(self) -> int:
        return self._iterations

    @property
    def hash_type(self) -> HashType:
        return self._hash_type

    # --- Key derivation ---

    def _derive_key(self, secret: bytes | bytearray) -> SymmetricKey:
        """Derive a symmetric key using PBKDF2."""
        secret_bytes = bytes(secret)
        if self._hash_type == HashType.SHA256:
            derived = pbkdf2_hmac_sha256(
                secret_bytes, self._salt.data, self._iterations, 32
            )
        else:
            derived = pbkdf2_hmac_sha512(
                secret_bytes, self._salt.data, self._iterations, 32
            )
        return SymmetricKey.from_data(derived)

    def lock(
        self,
        content_key: SymmetricKey,
        secret: bytes | bytearray,
    ) -> EncryptedMessage:
        """Derive a key from *secret* via PBKDF2 and encrypt *content_key*."""
        derived_key = self._derive_key(secret)
        encoded_method = self.to_cbor().to_cbor_data()
        return derived_key.encrypt(content_key.data, aad=encoded_method)

    def unlock(
        self,
        encrypted_message: EncryptedMessage,
        secret: bytes | bytearray,
    ) -> SymmetricKey:
        """Derive a key from *secret* via PBKDF2 and decrypt the content key."""
        derived_key = self._derive_key(secret)
        plaintext = derived_key.decrypt(encrypted_message)
        return SymmetricKey.from_data(plaintext)

    # --- Display ---

    def __str__(self) -> str:
        return f"PBKDF2({self._hash_type})"

    def __repr__(self) -> str:
        return (
            f"PBKDF2Params(salt={self._salt!r}, "
            f"iterations={self._iterations}, "
            f"hash_type={self._hash_type!r})"
        )

    def __eq__(self, other: object) -> bool:
        if isinstance(other, PBKDF2Params):
            return (
                self._salt == other._salt
                and self._iterations == other._iterations
                and self._hash_type == other._hash_type
            )
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._salt, self._iterations, self._hash_type))

    # --- CBOR ---

    def to_cbor(self) -> CBOR:
        """Encode as CBOR array: [INDEX, salt, iterations, hash_type]."""
        return CBOR.from_array([
            CBOR.from_int(self.INDEX),
            self._salt.untagged_cbor(),
            CBOR.from_int(self._iterations),
            self._hash_type.to_cbor(),
        ])

    @staticmethod
    def from_cbor(cbor: CBOR) -> PBKDF2Params:
        """Decode from CBOR array: [INDEX, salt, iterations, hash_type]."""
        a = cbor.try_array()
        if len(a) != 4:
            raise ValueError("Invalid PBKDF2Params: expected 4 elements")
        salt = Salt.from_untagged_cbor(a[1])
        iterations = a[2].try_int()
        hash_type = HashType.from_cbor(a[3])
        return PBKDF2Params(salt, iterations, hash_type)
