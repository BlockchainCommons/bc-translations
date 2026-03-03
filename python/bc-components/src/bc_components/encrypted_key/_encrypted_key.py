"""EncryptedKey: encrypt/decrypt a symmetric content key using key derivation."""

from __future__ import annotations

from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_ENCRYPTED_KEY,
)

from ..symmetric._encrypted_message import EncryptedMessage
from ..symmetric._symmetric_key import SymmetricKey
from ._argon2id_params import Argon2idParams
from ._hkdf_params import HKDFParams
from ._key_derivation_method import KeyDerivationMethod
from ._key_derivation_params import KeyDerivationParams
from ._pbkdf2_params import PBKDF2Params
from ._scrypt_params import ScryptParams


class EncryptedKey:
    """A symmetric content key encrypted using a derived key.

    The form is an EncryptedMessage whose AAD is the CBOR encoding of
    the key derivation parameters used.

    CDDL::

        EncryptedKey = #6.40027(EncryptedMessage)
    """

    __slots__ = ("_params", "_encrypted_message")

    def __init__(
        self,
        params: KeyDerivationParams,
        encrypted_message: EncryptedMessage,
    ) -> None:
        self._params = params
        self._encrypted_message = encrypted_message

    # --- Construction ---

    @staticmethod
    def lock_opt(
        params: KeyDerivationParams,
        secret: bytes | bytearray,
        content_key: SymmetricKey,
    ) -> EncryptedKey:
        """Lock a content key using specified derivation parameters."""
        encrypted_message = params.lock(content_key, secret)
        return EncryptedKey(params, encrypted_message)

    @staticmethod
    def lock(
        method: KeyDerivationMethod,
        secret: bytes | bytearray,
        content_key: SymmetricKey,
    ) -> EncryptedKey:
        """Lock a content key using a derivation method with default parameters."""
        if method == KeyDerivationMethod.HKDF:
            params = KeyDerivationParams.hkdf(HKDFParams.generate())
        elif method == KeyDerivationMethod.PBKDF2:
            params = KeyDerivationParams.pbkdf2(PBKDF2Params.generate())
        elif method == KeyDerivationMethod.SCRYPT:
            params = KeyDerivationParams.scrypt(ScryptParams.generate())
        elif method == KeyDerivationMethod.ARGON2ID:
            params = KeyDerivationParams.argon2id(Argon2idParams.generate())
        else:
            raise ValueError(f"Unsupported KeyDerivationMethod: {method}")
        return EncryptedKey.lock_opt(params, secret, content_key)

    # --- Properties ---

    @property
    def params(self) -> KeyDerivationParams:
        return self._params

    @property
    def encrypted_message(self) -> EncryptedMessage:
        return self._encrypted_message

    def aad_cbor(self) -> CBOR:
        """Return the AAD (key derivation params) as CBOR."""
        aad = self._encrypted_message.aad
        if not aad:
            raise ValueError("Missing AAD CBOR in EncryptedMessage")
        return CBOR.from_data(aad)

    def is_password_based(self) -> bool:
        return self._params.is_password_based()

    # --- Unlock ---

    def unlock(self, secret: bytes | bytearray) -> SymmetricKey:
        """Unlock the content key using the given secret."""
        cbor = self.aad_cbor()
        a = cbor.try_array()
        if not a:
            raise ValueError("Missing method in AAD")
        method_cbor = a[0]
        method = KeyDerivationMethod.from_cbor(method_cbor)

        if method == KeyDerivationMethod.HKDF:
            params = HKDFParams.from_cbor(cbor)
            return params.unlock(self._encrypted_message, secret)
        elif method == KeyDerivationMethod.PBKDF2:
            params = PBKDF2Params.from_cbor(cbor)
            return params.unlock(self._encrypted_message, secret)
        elif method == KeyDerivationMethod.SCRYPT:
            params = ScryptParams.from_cbor(cbor)
            return params.unlock(self._encrypted_message, secret)
        elif method == KeyDerivationMethod.ARGON2ID:
            params = Argon2idParams.from_cbor(cbor)
            return params.unlock(self._encrypted_message, secret)
        else:
            raise ValueError(f"Unsupported KeyDerivationMethod: {method}")

    # --- Display ---

    def __str__(self) -> str:
        return f"EncryptedKey({self._params})"

    def __repr__(self) -> str:
        return f"EncryptedKey(params={self._params!r})"

    def __eq__(self, other: object) -> bool:
        if isinstance(other, EncryptedKey):
            return (
                self._params == other._params
                and self._encrypted_message == other._encrypted_message
            )
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._params, self._encrypted_message))

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_ENCRYPTED_KEY])

    def untagged_cbor(self) -> CBOR:
        """The untagged CBOR is the EncryptedMessage itself."""
        return self._encrypted_message.tagged_cbor()

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
    def from_untagged_cbor(cbor: CBOR) -> EncryptedKey:
        """Decode an EncryptedKey from an untagged EncryptedMessage CBOR."""
        encrypted_message = EncryptedMessage.from_tagged_cbor(cbor)
        aad = encrypted_message.aad
        if not aad:
            raise ValueError("Missing AAD in EncryptedMessage for EncryptedKey")
        params_cbor = CBOR.from_data(aad)
        params = KeyDerivationParams.from_cbor(params_cbor)
        return EncryptedKey(params, encrypted_message)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> EncryptedKey:
        """Decode an EncryptedKey from tagged CBOR."""
        tags = EncryptedKey.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return EncryptedKey.from_untagged_cbor(item)
