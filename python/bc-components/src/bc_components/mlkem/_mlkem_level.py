"""ML-KEM security levels."""

from __future__ import annotations

from enum import Enum
from typing import TYPE_CHECKING

from bc_tags import CBOR

from .._error import PostQuantumError

if TYPE_CHECKING:
    from ._mlkem_private_key import MLKEMPrivateKey
    from ._mlkem_public_key import MLKEMPublicKey


class MLKEMLevel(Enum):
    """Security levels for the ML-KEM post-quantum key encapsulation mechanism.

    Each variant corresponds to a NIST security level:
    - MLKEM512: NIST level 1 (roughly AES-128)
    - MLKEM768: NIST level 3 (roughly AES-192)
    - MLKEM1024: NIST level 5 (roughly AES-256)

    The integer values (512, 768, 1024) correspond to the parameter sets
    and are used in CBOR serialization.
    """

    MLKEM512 = 512
    MLKEM768 = 768
    MLKEM1024 = 1024

    # --- Key/ciphertext sizes ---

    def private_key_size(self) -> int:
        """Return the private key size in bytes for this security level."""
        return _PRIVATE_KEY_SIZES[self]

    def public_key_size(self) -> int:
        """Return the public key size in bytes for this security level."""
        return _PUBLIC_KEY_SIZES[self]

    def shared_secret_size(self) -> int:
        """Return the shared secret size in bytes (always 32)."""
        return 32

    def ciphertext_size(self) -> int:
        """Return the ciphertext size in bytes for this security level."""
        return _CIPHERTEXT_SIZES[self]

    # --- Key generation ---

    def keypair(self) -> tuple[MLKEMPrivateKey, MLKEMPublicKey]:
        """Generate a new random ML-KEM keypair at this security level."""
        from ._mlkem_private_key import MLKEMPrivateKey

        return MLKEMPrivateKey.generate(self)

    # --- CBOR ---

    def to_cbor(self) -> CBOR:
        """Encode this level as a CBOR integer."""
        return CBOR.from_int(self.value)

    @staticmethod
    def from_cbor(cbor: CBOR) -> MLKEMLevel:
        """Decode a CBOR integer to an MLKEMLevel.

        Raises PostQuantumError if the value is not a valid level.
        """
        value = cbor.try_int()
        return MLKEMLevel.from_value(value)

    @staticmethod
    def from_value(value: int) -> MLKEMLevel:
        """Create an MLKEMLevel from a raw integer value (512, 768, or 1024).

        Raises PostQuantumError if the value is not a valid level.
        """
        try:
            return MLKEMLevel(value)
        except ValueError:
            raise PostQuantumError(f"Invalid MLKEM level: {value}")

    def __str__(self) -> str:
        return self.name


_PRIVATE_KEY_SIZES: dict[MLKEMLevel, int] = {
    MLKEMLevel.MLKEM512: 1632,
    MLKEMLevel.MLKEM768: 2400,
    MLKEMLevel.MLKEM1024: 3168,
}

_PUBLIC_KEY_SIZES: dict[MLKEMLevel, int] = {
    MLKEMLevel.MLKEM512: 800,
    MLKEMLevel.MLKEM768: 1184,
    MLKEMLevel.MLKEM1024: 1568,
}

_CIPHERTEXT_SIZES: dict[MLKEMLevel, int] = {
    MLKEMLevel.MLKEM512: 768,
    MLKEMLevel.MLKEM768: 1088,
    MLKEMLevel.MLKEM1024: 1568,
}

#: The shared secret size is 32 bytes for all ML-KEM security levels.
MLKEM_SHARED_SECRET_SIZE: int = 32
