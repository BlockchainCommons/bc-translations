"""ML-KEM ciphertext."""

from __future__ import annotations

from bc_tags import (
    CBOR,
    Tag,
    tags_for_values,
    TAG_MLKEM_CIPHERTEXT,
)

from .._error import InvalidSizeError
from ._mlkem_level import MLKEMLevel


class MLKEMCiphertext:
    """A ciphertext containing an encapsulated shared secret for ML-KEM.

    Stores a security level and the raw ciphertext bytes.
    """

    __slots__ = ("_level", "_data")

    def __init__(self, level: MLKEMLevel, data: bytes) -> None:
        expected = level.ciphertext_size()
        if len(data) != expected:
            raise InvalidSizeError("MLKEM ciphertext", expected, len(data))
        self._level = level
        self._data = bytes(data)

    # --- Construction ---

    @staticmethod
    def from_bytes(level: MLKEMLevel, data: bytes) -> MLKEMCiphertext:
        """Create a ciphertext from raw bytes and a security level."""
        return MLKEMCiphertext(level, data)

    # --- Properties ---

    @property
    def level(self) -> MLKEMLevel:
        """The security level of this ciphertext."""
        return self._level

    @property
    def size(self) -> int:
        """The size of this ciphertext in bytes."""
        return self._level.ciphertext_size()

    @property
    def data(self) -> bytes:
        """The raw ciphertext bytes."""
        return self._data

    # --- CBOR ---

    @staticmethod
    def cbor_tags() -> list[Tag]:
        return tags_for_values([TAG_MLKEM_CIPHERTEXT])

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
    def from_untagged_cbor(cbor: CBOR) -> MLKEMCiphertext:
        elements = cbor.try_array()
        if len(elements) != 2:
            raise ValueError("MLKEMCiphertext must have two elements")
        level = MLKEMLevel.from_cbor(elements[0])
        data = elements[1].try_byte_string()
        return MLKEMCiphertext(level, data)

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> MLKEMCiphertext:
        tags = MLKEMCiphertext.cbor_tags()
        item = cbor.try_expected_tagged_value(tags[0])
        return MLKEMCiphertext.from_untagged_cbor(item)

    def to_cbor(self) -> CBOR:
        return self.tagged_cbor()

    def to_cbor_data(self) -> bytes:
        return self.tagged_cbor_data()

    # --- Dunder methods ---

    def __eq__(self, other: object) -> bool:
        if isinstance(other, MLKEMCiphertext):
            return self._level == other._level and self._data == other._data
        return NotImplemented

    def __hash__(self) -> int:
        return hash((self._level, self._data))

    def __repr__(self) -> str:
        return f"{self._level.name}Ciphertext"

    def __str__(self) -> str:
        return f"{self._level.name}Ciphertext"
