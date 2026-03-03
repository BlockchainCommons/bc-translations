"""Union type for key derivation parameters."""

from __future__ import annotations

from bc_tags import CBOR

from ..symmetric._encrypted_message import EncryptedMessage
from ..symmetric._symmetric_key import SymmetricKey
from ._argon2id_params import Argon2idParams
from ._hkdf_params import HKDFParams
from ._key_derivation_method import KeyDerivationMethod
from ._pbkdf2_params import PBKDF2Params
from ._scrypt_params import ScryptParams


class KeyDerivationParams:
    """Union of all key derivation parameter types.

    Wraps one of HKDFParams, PBKDF2Params, ScryptParams, or Argon2idParams,
    providing a unified interface for lock/unlock operations.
    """

    __slots__ = ("_inner",)

    def __init__(
        self,
        inner: HKDFParams | PBKDF2Params | ScryptParams | Argon2idParams,
    ) -> None:
        self._inner = inner

    # --- Construction ---

    @staticmethod
    def hkdf(params: HKDFParams) -> KeyDerivationParams:
        return KeyDerivationParams(params)

    @staticmethod
    def pbkdf2(params: PBKDF2Params) -> KeyDerivationParams:
        return KeyDerivationParams(params)

    @staticmethod
    def scrypt(params: ScryptParams) -> KeyDerivationParams:
        return KeyDerivationParams(params)

    @staticmethod
    def argon2id(params: Argon2idParams) -> KeyDerivationParams:
        return KeyDerivationParams(params)

    # --- Properties ---

    @property
    def inner(self) -> HKDFParams | PBKDF2Params | ScryptParams | Argon2idParams:
        return self._inner

    def method(self) -> KeyDerivationMethod:
        """Return the key derivation method associated with these parameters."""
        if isinstance(self._inner, HKDFParams):
            return KeyDerivationMethod.HKDF
        elif isinstance(self._inner, PBKDF2Params):
            return KeyDerivationMethod.PBKDF2
        elif isinstance(self._inner, ScryptParams):
            return KeyDerivationMethod.SCRYPT
        elif isinstance(self._inner, Argon2idParams):
            return KeyDerivationMethod.ARGON2ID
        raise ValueError("Unknown inner params type")  # pragma: no cover

    def is_password_based(self) -> bool:
        """Return True if the derivation method is password-based."""
        return isinstance(
            self._inner, (PBKDF2Params, ScryptParams, Argon2idParams)
        )

    # --- Key derivation ---

    def lock(
        self,
        content_key: SymmetricKey,
        secret: bytes | bytearray,
    ) -> EncryptedMessage:
        """Derive a key from *secret* and encrypt *content_key*."""
        return self._inner.lock(content_key, secret)

    # --- Display ---

    def __str__(self) -> str:
        return str(self._inner)

    def __repr__(self) -> str:
        return f"KeyDerivationParams({self._inner!r})"

    def __eq__(self, other: object) -> bool:
        if isinstance(other, KeyDerivationParams):
            return self._inner == other._inner
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._inner)

    # --- CBOR ---

    def to_cbor(self) -> CBOR:
        """Encode the inner params as CBOR."""
        return self._inner.to_cbor()

    @staticmethod
    def from_cbor(cbor: CBOR) -> KeyDerivationParams:
        """Decode from CBOR by inspecting the first array element (method index)."""
        a = cbor.try_array()
        if not a:
            raise ValueError("KeyDerivationParams: empty array")
        index = a[0].try_int()
        method = KeyDerivationMethod.from_index(index)
        if method is None:
            raise ValueError(f"Invalid KeyDerivationMethod index: {index}")
        if method == KeyDerivationMethod.HKDF:
            return KeyDerivationParams(HKDFParams.from_cbor(cbor))
        elif method == KeyDerivationMethod.PBKDF2:
            return KeyDerivationParams(PBKDF2Params.from_cbor(cbor))
        elif method == KeyDerivationMethod.SCRYPT:
            return KeyDerivationParams(ScryptParams.from_cbor(cbor))
        elif method == KeyDerivationMethod.ARGON2ID:
            return KeyDerivationParams(Argon2idParams.from_cbor(cbor))
        raise ValueError(f"Unsupported KeyDerivationMethod: {method}")  # pragma: no cover
