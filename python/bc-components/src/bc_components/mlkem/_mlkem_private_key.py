"""ML-KEM private key."""

from __future__ import annotations

from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_MLKEM_PRIVATE_KEY,
)

from .._digest import Digest
from .._error import InvalidSizeError
from .._pq_utils import expand_bytes, random_bytes
from .._reference import Reference, ReferenceProvider
from ..symmetric._symmetric_key import SymmetricKey
from ._mlkem_ciphertext import MLKEMCiphertext
from ._mlkem_level import MLKEMLevel
from ._mlkem_public_key import MLKEMPublicKey


def _derive_public_bytes(level: MLKEMLevel, private_bytes: bytes) -> bytes:
    """Derive simulated public key bytes from private key bytes."""
    return expand_bytes(
        private_bytes,
        f"mlkem:{level.value}:public",
        level.public_key_size(),
    )


def _derive_shared_secret(public_bytes: bytes, ct_bytes: bytes) -> bytes:
    """Derive a simulated shared secret from public key and ciphertext bytes."""
    digest = expand_bytes(public_bytes + ct_bytes, "mlkem:ss", 64)
    return digest[:32]


class MLKEMPrivateKey(ReferenceProvider):
    """A private key for the ML-KEM post-quantum key encapsulation mechanism.

    Used to decapsulate shared secrets encapsulated with the corresponding
    public key.  Uses simulated PQ crypto (SHA-256 counter-mode expansion).
    """

    __slots__ = ("_level", "_data")

    def __init__(self, level: MLKEMLevel, data: bytes) -> None:
        expected = level.private_key_size()
        if len(data) != expected:
            raise InvalidSizeError("MLKEM private key", expected, len(data))
        self._level = level
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def generate(level: MLKEMLevel) -> tuple[MLKEMPrivateKey, MLKEMPublicKey]:
        """Generate a new random ML-KEM keypair at the given security level."""
        private_bytes = random_bytes(level.private_key_size())
        public_bytes = _derive_public_bytes(level, private_bytes)
        private_key = MLKEMPrivateKey(level, private_bytes)
        public_key = MLKEMPublicKey(level, public_bytes)
        return private_key, public_key

    @staticmethod
    def from_bytes(level: MLKEMLevel, data: bytes) -> MLKEMPrivateKey:
        """Create a private key from raw bytes and a security level."""
        return MLKEMPrivateKey(level, data)

    # --- Properties ---

    @property
    def level(self) -> MLKEMLevel:
        """The security level of this private key."""
        return self._level

    @property
    def size(self) -> int:
        """The size of this private key in bytes."""
        return self._level.private_key_size()

    @property
    def data(self) -> bytes:
        """The raw private key bytes."""
        return self._data

    def public_key(self) -> MLKEMPublicKey:
        """Derive the corresponding public key."""
        return MLKEMPublicKey(
            self._level,
            _derive_public_bytes(self._level, self._data),
        )

    # --- Decapsulation (simulated) ---

    def decapsulate_shared_secret(
        self,
        ciphertext: MLKEMCiphertext,
    ) -> SymmetricKey:
        """Decapsulate a shared secret from a ciphertext.

        The ciphertext level must match this private key's level.
        Raises ValueError on level mismatch.
        """
        if ciphertext.level != self._level:
            raise ValueError("MLKEM level mismatch")
        public_bytes = _derive_public_bytes(self._level, self._data)
        shared = _derive_shared_secret(public_bytes, ciphertext.data)
        return SymmetricKey.from_data(shared)

    # --- ReferenceProvider ---

    def reference(self) -> Reference:
        return Reference.from_digest(Digest.from_image(self.tagged_cbor_data()))

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_MLKEM_PRIVATE_KEY])

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
    def from_untagged_cbor(cbor: CBOR) -> MLKEMPrivateKey:
        elements = cbor.try_array()
        if len(elements) != 2:
            raise ValueError("MLKEMPrivateKey must have two elements")
        level = MLKEMLevel.from_cbor(elements[0])
        data = elements[1].try_byte_string()
        return MLKEMPrivateKey(level, data)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> MLKEMPrivateKey:
        tags = MLKEMPrivateKey.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return MLKEMPrivateKey.from_untagged_cbor(item)

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    # --- Decrypter protocol ---

    def encapsulation_private_key(self) -> EncapsulationPrivateKey:
        """Return this key wrapped as an EncapsulationPrivateKey."""
        from ..encapsulation._encapsulation_private_key import (
            EncapsulationPrivateKey,
        )

        return EncapsulationPrivateKey.from_mlkem(self)

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, MLKEMPrivateKey):
            return self._level == other._level and self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._level, self._data))

    def __repr__(self) -> str:
        return f"{self._level.name}PrivateKey"

    def __str__(self) -> str:
        return f"{self._level.name}PrivateKey({self.reference().ref_hex_short()})"


# Avoid circular import at module level
from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from ..encapsulation._encapsulation_private_key import (
        EncapsulationPrivateKey,
    )
