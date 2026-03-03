"""Supported digital signature schemes."""

from __future__ import annotations

from enum import Enum, unique
from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from ._signing_private_key import SigningPrivateKey
    from ._signing_public_key import SigningPublicKey


@unique
class SignatureScheme(Enum):
    """Enum of supported digital signature schemes.

    Includes elliptic curve schemes (ECDSA, Schnorr), Edwards curve
    schemes (Ed25519), post-quantum schemes (ML-DSA), and SSH-specific
    algorithms.
    """

    SCHNORR = "schnorr"
    ECDSA = "ecdsa"
    ED25519 = "ed25519"
    MLDSA44 = "mldsa44"
    MLDSA65 = "mldsa65"
    MLDSA87 = "mldsa87"
    SSH_ED25519 = "ssh_ed25519"
    SSH_DSA = "ssh_dsa"
    SSH_ECDSA_P256 = "ssh_ecdsa_p256"
    SSH_ECDSA_P384 = "ssh_ecdsa_p384"

    def keypair(self) -> tuple[SigningPrivateKey, SigningPublicKey]:
        """Create a new key pair for this scheme.

        Returns a tuple of (private_key, public_key).
        """
        return self.keypair_opt("")

    def keypair_opt(
        self,
        comment: str = "",
    ) -> tuple[SigningPrivateKey, SigningPublicKey]:
        """Create a new key pair with an optional comment (used for SSH keys).

        Returns a tuple of (private_key, public_key).
        """
        from ..ec_key import ECPrivateKey
        from ..ed25519._ed25519_private_key import Ed25519PrivateKey
        from ._signing_private_key import SigningPrivateKey
        from ._signing_public_key import SigningPublicKey

        if self == SignatureScheme.SCHNORR:
            priv = SigningPrivateKey.new_schnorr(ECPrivateKey.generate())
            pub = priv.public_key()
            return (priv, pub)
        elif self == SignatureScheme.ECDSA:
            priv = SigningPrivateKey.new_ecdsa(ECPrivateKey.generate())
            pub = priv.public_key()
            return (priv, pub)
        elif self == SignatureScheme.ED25519:
            priv = SigningPrivateKey.new_ed25519(Ed25519PrivateKey.generate())
            pub = priv.public_key()
            return (priv, pub)
        elif self in (
            SignatureScheme.MLDSA44,
            SignatureScheme.MLDSA65,
            SignatureScheme.MLDSA87,
        ):
            from ..mldsa._mldsa_level import MLDSALevel

            level_map = {
                SignatureScheme.MLDSA44: MLDSALevel.MLDSA44,
                SignatureScheme.MLDSA65: MLDSALevel.MLDSA65,
                SignatureScheme.MLDSA87: MLDSALevel.MLDSA87,
            }
            level = level_map[self]
            mldsa_priv, mldsa_pub = level.keypair()
            priv = SigningPrivateKey.new_mldsa(mldsa_priv)
            pub = SigningPublicKey.from_mldsa(mldsa_pub)
            return (priv, pub)
        elif self in (
            SignatureScheme.SSH_ED25519,
            SignatureScheme.SSH_DSA,
            SignatureScheme.SSH_ECDSA_P256,
            SignatureScheme.SSH_ECDSA_P384,
        ):
            ssh_priv_key = _generate_ssh_private_key(self)
            priv = SigningPrivateKey.new_ssh(ssh_priv_key)
            pub = priv.public_key()
            return (priv, pub)
        else:
            raise ValueError(f"Unsupported signature scheme: {self}")

    @staticmethod
    def default() -> SignatureScheme:
        """Return the default signature scheme (Schnorr)."""
        return SignatureScheme.SCHNORR

    def keypair_using(
        self,
        rng: object,
        comment: str = "",
    ) -> tuple[SigningPrivateKey, SigningPublicKey]:
        """Create a key pair using a provided RNG.

        Not all schemes support deterministic generation.
        """
        from ..ec_key import ECPrivateKey
        from ..ed25519._ed25519_private_key import Ed25519PrivateKey
        from ._signing_private_key import SigningPrivateKey

        if self == SignatureScheme.SCHNORR:
            priv = SigningPrivateKey.new_schnorr(ECPrivateKey.generate_using(rng))
            pub = priv.public_key()
            return (priv, pub)
        elif self == SignatureScheme.ECDSA:
            priv = SigningPrivateKey.new_ecdsa(ECPrivateKey.generate_using(rng))
            pub = priv.public_key()
            return (priv, pub)
        elif self == SignatureScheme.ED25519:
            priv = SigningPrivateKey.new_ed25519(Ed25519PrivateKey.generate_using(rng))
            pub = priv.public_key()
            return (priv, pub)
        elif self in (
            SignatureScheme.SSH_ED25519,
            SignatureScheme.SSH_DSA,
            SignatureScheme.SSH_ECDSA_P256,
            SignatureScheme.SSH_ECDSA_P384,
        ):
            # Note: SSH key generation via cryptography library does not
            # support seeded/deterministic generation, so rng is ignored.
            ssh_priv_key = _generate_ssh_private_key(self)
            priv = SigningPrivateKey.new_ssh(ssh_priv_key)
            pub = priv.public_key()
            return (priv, pub)
        else:
            raise ValueError(
                "Deterministic keypair generation not supported for "
                f"this signature scheme: {self}"
            )


def _generate_ssh_private_key(scheme: SignatureScheme) -> object:
    """Generate an SSH private key for the given scheme.

    Uses the `cryptography` library to generate the appropriate key type.
    """
    from cryptography.hazmat.primitives.asymmetric import (
        dsa,
        ec,
        ed25519,
    )

    if scheme == SignatureScheme.SSH_ED25519:
        return ed25519.Ed25519PrivateKey.generate()
    elif scheme == SignatureScheme.SSH_DSA:
        return dsa.generate_private_key(key_size=1024)
    elif scheme == SignatureScheme.SSH_ECDSA_P256:
        return ec.generate_private_key(ec.SECP256R1())
    elif scheme == SignatureScheme.SSH_ECDSA_P384:
        return ec.generate_private_key(ec.SECP384R1())
    else:
        raise ValueError(f"Unsupported SSH scheme: {scheme}")
