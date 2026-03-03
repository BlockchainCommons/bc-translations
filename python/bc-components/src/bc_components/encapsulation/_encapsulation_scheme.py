"""Supported key encapsulation mechanism schemes."""

from __future__ import annotations

from enum import Enum, auto
from typing import TYPE_CHECKING

from bc_rand import RandomNumberGenerator

from .._error import GeneralError

if TYPE_CHECKING:
    from ._encapsulation_private_key import EncapsulationPrivateKey
    from ._encapsulation_public_key import EncapsulationPublicKey


class EncapsulationScheme(Enum):
    """Supported key encapsulation mechanisms.

    - X25519: Curve25519-based Diffie-Hellman key exchange (default)
    - MLKEM512: ML-KEM post-quantum KEM at NIST level 1
    - MLKEM768: ML-KEM post-quantum KEM at NIST level 3
    - MLKEM1024: ML-KEM post-quantum KEM at NIST level 5
    """

    X25519 = auto()
    MLKEM512 = auto()
    MLKEM768 = auto()
    MLKEM1024 = auto()

    def keypair(
        self,
    ) -> tuple[EncapsulationPrivateKey, EncapsulationPublicKey]:
        """Generate a new random keypair for this encapsulation scheme."""
        from ..mlkem._mlkem_level import MLKEMLevel
        from ..x25519._x25519_private_key import X25519PrivateKey
        from ._encapsulation_private_key import EncapsulationPrivateKey
        from ._encapsulation_public_key import EncapsulationPublicKey

        if self == EncapsulationScheme.X25519:
            priv, pub = X25519PrivateKey.keypair()
            return (
                EncapsulationPrivateKey.from_x25519(priv),
                EncapsulationPublicKey.from_x25519(pub),
            )

        level_map = {
            EncapsulationScheme.MLKEM512: MLKEMLevel.MLKEM512,
            EncapsulationScheme.MLKEM768: MLKEMLevel.MLKEM768,
            EncapsulationScheme.MLKEM1024: MLKEMLevel.MLKEM1024,
        }
        level = level_map[self]
        priv_mlkem, pub_mlkem = level.keypair()
        return (
            EncapsulationPrivateKey.from_mlkem(priv_mlkem),
            EncapsulationPublicKey.from_mlkem(pub_mlkem),
        )

    def keypair_using(
        self,
        rng: RandomNumberGenerator,
    ) -> tuple[EncapsulationPrivateKey, EncapsulationPublicKey]:
        """Generate a deterministic keypair using the provided RNG.

        Only X25519 supports deterministic key generation.
        Raises GeneralError for ML-KEM schemes.
        """
        from ..x25519._x25519_private_key import X25519PrivateKey
        from ._encapsulation_private_key import EncapsulationPrivateKey
        from ._encapsulation_public_key import EncapsulationPublicKey

        if self != EncapsulationScheme.X25519:
            raise GeneralError(
                "Deterministic keypair generation not supported "
                "for this encapsulation scheme"
            )

        priv, pub = X25519PrivateKey.keypair_using(rng)
        return (
            EncapsulationPrivateKey.from_x25519(priv),
            EncapsulationPublicKey.from_x25519(pub),
        )

    @staticmethod
    def default() -> EncapsulationScheme:
        """Return the default encapsulation scheme (X25519)."""
        return EncapsulationScheme.X25519

    def __str__(self) -> str:
        return self.name
