"""ML-DSA private key."""

from __future__ import annotations

from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_MLDSA_PRIVATE_KEY,
)

from .._digest import Digest
from .._error import InvalidSizeError
from .._pq_utils import expand_bytes, random_bytes
from .._reference import Reference, ReferenceProvider
from ._mldsa_level import MLDSALevel
from ._mldsa_public_key import MLDSAPublicKey
from ._mldsa_signature import MLDSASignature


def _derive_public_bytes(level: MLDSALevel, private_bytes: bytes) -> bytes:
    """Derive simulated public key bytes from private key bytes."""
    return expand_bytes(
        private_bytes,
        f"mldsa:{level.value}:public",
        level.public_key_size(),
    )


class MLDSAPrivateKey(ReferenceProvider):
    """A private key for the ML-DSA post-quantum digital signature algorithm.

    Supports security levels MLDSA44, MLDSA65, and MLDSA87.
    Uses simulated PQ crypto (SHA-256 counter-mode expansion).
    """

    __slots__ = ("_level", "_data")

    def __init__(self, level: MLDSALevel, data: bytes) -> None:
        expected = level.private_key_size()
        if len(data) != expected:
            raise InvalidSizeError("MLDSA private key", expected, len(data))
        self._level = level
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def generate(level: MLDSALevel) -> tuple[MLDSAPrivateKey, MLDSAPublicKey]:
        """Generate a new random ML-DSA keypair at the given security level."""
        private_bytes = random_bytes(level.private_key_size())
        public_bytes = _derive_public_bytes(level, private_bytes)
        private_key = MLDSAPrivateKey(level, private_bytes)
        public_key = MLDSAPublicKey(level, public_bytes)
        return private_key, public_key

    @staticmethod
    def from_bytes(level: MLDSALevel, data: bytes) -> MLDSAPrivateKey:
        """Create a private key from raw bytes and a security level."""
        return MLDSAPrivateKey(level, data)

    # --- Properties ---

    @property
    def level(self) -> MLDSALevel:
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

    # --- Signing (simulated) ---

    def sign(self, message: bytes) -> MLDSASignature:
        """Sign a message using this private key.

        Produces a simulated ML-DSA signature using SHA-256 expansion.
        """
        nonce = random_bytes(32)
        public_bytes = _derive_public_bytes(self._level, self._data)
        digest = expand_bytes(
            public_bytes + message + nonce,
            f"mldsa:{self._level.value}:digest",
            64,
        )
        tail = expand_bytes(
            digest,
            f"mldsa:{self._level.value}:sig",
            self._level.signature_size() - len(nonce),
        )
        return MLDSASignature(self._level, nonce + tail)

    def public_key(self) -> MLDSAPublicKey:
        """Derive the corresponding public key."""
        return MLDSAPublicKey(
            self._level,
            _derive_public_bytes(self._level, self._data),
        )

    # --- ReferenceProvider ---

    def reference(self) -> Reference:
        return Reference.from_digest(Digest.from_image(self.tagged_cbor_data()))

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_MLDSA_PRIVATE_KEY])

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
    def from_untagged_cbor(cbor: CBOR) -> MLDSAPrivateKey:
        elements = cbor.try_array()
        if len(elements) != 2:
            raise ValueError("MLDSAPrivateKey must have two elements")
        level = MLDSALevel.from_cbor(elements[0])
        data = elements[1].try_byte_string()
        return MLDSAPrivateKey(level, data)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> MLDSAPrivateKey:
        tags = MLDSAPrivateKey.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return MLDSAPrivateKey.from_untagged_cbor(item)

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, MLDSAPrivateKey):
            return self._level == other._level and self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._level, self._data))

    def __repr__(self) -> str:
        return f"{self._level.name}PrivateKey"

    def __str__(self) -> str:
        return f"{self._level.name}PrivateKey({self.reference().ref_hex_short()})"
