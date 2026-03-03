"""ML-DSA security levels."""

from __future__ import annotations

from enum import Enum
from typing import TYPE_CHECKING

from bc_tags import CBOR

from .._error import PostQuantumError

if TYPE_CHECKING:
    from ._mldsa_private_key import MLDSAPrivateKey
    from ._mldsa_public_key import MLDSAPublicKey


class MLDSALevel(Enum):
    """Security levels for the ML-DSA post-quantum digital signature algorithm.

    Each variant corresponds to a NIST security level:
    - MLDSA44: NIST level 2 (roughly AES-128)
    - MLDSA65: NIST level 3 (roughly AES-192)
    - MLDSA87: NIST level 5 (roughly AES-256)

    The integer values (2, 3, 5) correspond to the NIST security levels
    and are used in CBOR serialization.
    """

    MLDSA44 = 2
    MLDSA65 = 3
    MLDSA87 = 5

    # --- Key/signature sizes ---

    def private_key_size(self) -> int:
        """Return the private key size in bytes for this security level."""
        return _PRIVATE_KEY_SIZES[self]

    def public_key_size(self) -> int:
        """Return the public key size in bytes for this security level."""
        return _PUBLIC_KEY_SIZES[self]

    def signature_size(self) -> int:
        """Return the signature size in bytes for this security level."""
        return _SIGNATURE_SIZES[self]

    # --- Key generation ---

    def keypair(self) -> tuple[MLDSAPrivateKey, MLDSAPublicKey]:
        """Generate a new random ML-DSA keypair at this security level."""
        from ._mldsa_private_key import MLDSAPrivateKey

        return MLDSAPrivateKey.generate(self)

    # --- CBOR ---

    def to_cbor(self) -> CBOR:
        """Encode this level as a CBOR integer."""
        return CBOR.from_int(self.value)

    @staticmethod
    def from_cbor(cbor: CBOR) -> MLDSALevel:
        """Decode a CBOR integer to an MLDSALevel.

        Raises PostQuantumError if the value is not a valid level.
        """
        value = cbor.try_int()
        return MLDSALevel.from_value(value)

    @staticmethod
    def from_value(value: int) -> MLDSALevel:
        """Create an MLDSALevel from a raw integer value (2, 3, or 5).

        Raises PostQuantumError if the value is not a valid level.
        """
        try:
            return MLDSALevel(value)
        except ValueError:
            raise PostQuantumError(f"Invalid MLDSA level: {value}")

    def __str__(self) -> str:
        return self.name


_PRIVATE_KEY_SIZES: dict[MLDSALevel, int] = {
    MLDSALevel.MLDSA44: 2560,
    MLDSALevel.MLDSA65: 4032,
    MLDSALevel.MLDSA87: 4896,
}

_PUBLIC_KEY_SIZES: dict[MLDSALevel, int] = {
    MLDSALevel.MLDSA44: 1312,
    MLDSALevel.MLDSA65: 1952,
    MLDSALevel.MLDSA87: 2592,
}

_SIGNATURE_SIZES: dict[MLDSALevel, int] = {
    MLDSALevel.MLDSA44: 2420,
    MLDSALevel.MLDSA65: 3309,
    MLDSALevel.MLDSA87: 4627,
}
