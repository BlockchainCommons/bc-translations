"""ML-DSA public key."""

from __future__ import annotations

from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_MLDSA_PUBLIC_KEY,
)

from .._digest import Digest
from .._error import InvalidSizeError, LevelMismatchError
from .._pq_utils import expand_bytes
from .._reference import Reference, ReferenceProvider
from ._mldsa_level import MLDSALevel
from ._mldsa_signature import MLDSASignature


class MLDSAPublicKey(ReferenceProvider):
    """A public key for the ML-DSA post-quantum digital signature algorithm.

    Used to verify signatures created by the corresponding private key.
    """

    __slots__ = ("_level", "_data")

    def __init__(self, level: MLDSALevel, data: bytes) -> None:
        expected = level.public_key_size()
        if len(data) != expected:
            raise InvalidSizeError("MLDSA public key", expected, len(data))
        self._level = level
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def from_bytes(level: MLDSALevel, data: bytes) -> MLDSAPublicKey:
        """Create a public key from raw bytes and a security level."""
        return MLDSAPublicKey(level, data)

    # --- Properties ---

    @property
    def level(self) -> MLDSALevel:
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

    # --- Verification (simulated) ---

    def verify(self, signature: MLDSASignature, message: bytes) -> bool:
        """Verify an ML-DSA signature for a message.

        Returns True if the signature is valid, False otherwise.
        Raises LevelMismatchError if the signature level does not match.
        """
        if signature.level != self._level:
            raise LevelMismatchError()

        sig_bytes = signature.data
        nonce = sig_bytes[:32]
        tail = sig_bytes[32:]

        # Re-derive the signature using the same simulated scheme
        digest = expand_bytes(
            self._data + message + nonce,
            f"mldsa:{self._level.value}:digest",
            64,
        )
        expected_tail = expand_bytes(
            digest,
            f"mldsa:{self._level.value}:sig",
            self._level.signature_size() - len(nonce),
        )
        return tail == expected_tail

    # --- ReferenceProvider ---

    def reference(self) -> Reference:
        return Reference.from_digest(Digest.from_image(self.tagged_cbor_data()))

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_MLDSA_PUBLIC_KEY])

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
    def from_untagged_cbor(cbor: CBOR) -> MLDSAPublicKey:
        elements = cbor.try_array()
        if len(elements) != 2:
            raise ValueError("MLDSAPublicKey must have two elements")
        level = MLDSALevel.from_cbor(elements[0])
        data = elements[1].try_byte_string()
        return MLDSAPublicKey(level, data)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> MLDSAPublicKey:
        tags = MLDSAPublicKey.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return MLDSAPublicKey.from_untagged_cbor(item)

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, MLDSAPublicKey):
            return self._level == other._level and self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._level, self._data))

    def __repr__(self) -> str:
        return f"{self._level.name}PublicKey"

    def __str__(self) -> str:
        return f"{self._level.name}PublicKey({self.reference().ref_hex_short()})"
