"""ML-KEM public key."""

from __future__ import annotations

from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_MLKEM_PUBLIC_KEY,
)

from .._digest import Digest
from .._error import InvalidSizeError
from .._pq_utils import expand_bytes, random_bytes
from .._reference import Reference, ReferenceProvider
from ..symmetric._symmetric_key import SymmetricKey
from ._mlkem_ciphertext import MLKEMCiphertext
from ._mlkem_level import MLKEMLevel


def _derive_shared_secret(public_bytes: bytes, ct_bytes: bytes) -> bytes:
    """Derive a simulated shared secret from public key and ciphertext bytes."""
    digest = expand_bytes(public_bytes + ct_bytes, "mlkem:ss", 64)
    return digest[:32]


class MLKEMPublicKey(ReferenceProvider):
    """A public key for the ML-KEM post-quantum key encapsulation mechanism.

    Used to encapsulate new shared secrets.
    Uses simulated PQ crypto (SHA-256 counter-mode expansion).
    """

    __slots__ = ("_level", "_data")

    def __init__(self, level: MLKEMLevel, data: bytes) -> None:
        expected = level.public_key_size()
        if len(data) != expected:
            raise InvalidSizeError("MLKEM public key", expected, len(data))
        self._level = level
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def from_bytes(level: MLKEMLevel, data: bytes) -> MLKEMPublicKey:
        """Create a public key from raw bytes and a security level."""
        return MLKEMPublicKey(level, data)

    # --- Properties ---

    @property
    def level(self) -> MLKEMLevel:
        """The security level of this public key."""
        return self._level

    @property
    def size(self) -> int:
        """The size of this public key in bytes."""
        return self._level.public_key_size()

    @property
    def data(self) -> bytes:
        """The raw public key bytes."""
        return self._data

    # --- Encapsulation (simulated) ---

    def encapsulate_new_shared_secret(
        self,
    ) -> tuple[SymmetricKey, MLKEMCiphertext]:
        """Encapsulate a new shared secret using this public key.

        Generates a random ciphertext and derives a shared secret from it
        and this public key.

        Returns a tuple of (shared_secret, ciphertext).
        """
        ct_bytes = random_bytes(self._level.ciphertext_size())
        ciphertext = MLKEMCiphertext(self._level, ct_bytes)
        shared = _derive_shared_secret(self._data, ct_bytes)
        return SymmetricKey.from_data(shared), ciphertext

    # --- ReferenceProvider ---

    def reference(self) -> Reference:
        return Reference.from_digest(Digest.from_image(self.tagged_cbor_data()))

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_MLKEM_PUBLIC_KEY])

    def untagged_cbor(self) -> CBOR:
        return CBOR.from_array([
            self._level.to_cbor(),
            CBOR.from_bytes(self._data),
        ])

    def tagged_cbor(self) -> CBOR:
        tags = self.cbor_tags()
        return CBOR.from_tagged_value(tags[0], self.untagged_cbor())

    def tagged_cbor_data(self) -> bytes:
        return self.tagged_cbor().to_cbor_data()

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> MLKEMPublicKey:
        elements = cbor.try_array()
        if len(elements) != 2:
            raise ValueError("MLKEMPublicKey must have two elements")
        level = MLKEMLevel.from_cbor(elements[0])
        data = elements[1].try_byte_string()
        return MLKEMPublicKey(level, data)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> MLKEMPublicKey:
        tags = MLKEMPublicKey.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return MLKEMPublicKey.from_untagged_cbor(item)

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, MLKEMPublicKey):
            return self._level == other._level and self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._level, self._data))

    def __repr__(self) -> str:
        return f"{self._level.name}PublicKey"

    def __str__(self) -> str:
        return f"{self._level.name}PublicKey({self.reference().ref_hex_short()})"
