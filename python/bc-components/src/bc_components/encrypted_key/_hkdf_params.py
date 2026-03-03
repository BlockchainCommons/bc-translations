"""HKDF key derivation parameters."""

from __future__ import annotations

from bc_crypto import hkdf_hmac_sha256, hkdf_hmac_sha512
from bc_tags import CBOR

from .._salt import Salt
from ..symmetric._encrypted_message import EncryptedMessage
from ..symmetric._symmetric_key import SymmetricKey
from ._hash_type import HashType
from ._key_derivation_method import KeyDerivationMethod

SALT_LEN = 16


class HKDFParams:
    """HKDF key derivation parameters.

    CDDL::

        HKDFParams = [0, Salt, HashType]
    """

    INDEX: int = KeyDerivationMethod.HKDF.value

    __slots__ = ("_salt", "_hash_type")

    def __init__(self, salt: Salt, hash_type: HashType) -> None:
        self._salt = salt
        self._hash_type = hash_type

    # --- Construction ---

    @staticmethod
    def generate() -> HKDFParams:
        """Create new HKDF params with a random salt and SHA256."""
        return HKDFParams(Salt.generate_with_len(SALT_LEN), HashType.SHA256)

    @staticmethod
    def new_opt(salt: Salt, hash_type: HashType) -> HKDFParams:
        """Create new HKDF params with specific salt and hash type."""
        return HKDFParams(salt, hash_type)

    # --- Properties ---

    @property
    def salt(self) -> Salt:
        return self._salt

    @property
    def hash_type(self) -> HashType:
        return self._hash_type

    # --- Key derivation ---

    def _derive_key(self, secret: bytes | bytearray) -> SymmetricKey:
        """Derive a symmetric key using HKDF."""
        secret_bytes = bytes(secret)
        if self._hash_type == HashType.SHA256:
            derived = hkdf_hmac_sha256(secret_bytes, self._salt.data, 32)
        else:
            derived = hkdf_hmac_sha512(secret_bytes, self._salt.data, 32)
        return SymmetricKey.from_data(derived)

    def lock(
        self,
        content_key: SymmetricKey,
        secret: bytes | bytearray,
    ) -> EncryptedMessage:
        """Derive a key from *secret* via HKDF and encrypt *content_key*."""
        derived_key = self._derive_key(secret)
        encoded_method = self.to_cbor().to_cbor_data()
        return derived_key.encrypt(content_key.data, aad=encoded_method)

    def unlock(
        self,
        encrypted_message: EncryptedMessage,
        secret: bytes | bytearray,
    ) -> SymmetricKey:
        """Derive a key from *secret* via HKDF and decrypt the content key."""
        derived_key = self._derive_key(secret)
        plaintext = derived_key.decrypt(encrypted_message)
        return SymmetricKey.from_data(plaintext)

    # --- Display ---

    def __str__(self) -> str:
        return f"HKDF({self._hash_type})"

    def __repr__(self) -> str:
        return f"HKDFParams(salt={self._salt!r}, hash_type={self._hash_type!r})"

    def __eq__(self, other: object) -> bool:
        if isinstance(other, HKDFParams):
            return self._salt == other._salt and self._hash_type == other._hash_type
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._salt, self._hash_type))

    # --- CBOR ---

    def to_cbor(self) -> CBOR:
        """Encode as CBOR array: [INDEX, salt, hash_type]."""
        return CBOR.from_array([
            CBOR.from_int(self.INDEX),
            self._salt.untagged_cbor(),
            self._hash_type.to_cbor(),
        ])

    @staticmethod
    def from_cbor(cbor: CBOR) -> HKDFParams:
        """Decode from CBOR array: [INDEX, salt, hash_type]."""
        a = cbor.try_array()
        if len(a) != 3:
            raise ValueError("Invalid HKDFParams: expected 3 elements")
        # a[0] is the index, already validated by caller
        salt = Salt.from_untagged_cbor(a[1])
        hash_type = HashType.from_cbor(a[2])
        return HKDFParams(salt, hash_type)
