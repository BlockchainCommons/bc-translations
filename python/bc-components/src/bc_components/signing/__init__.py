"""Digital signatures for various cryptographic schemes.

Provides a unified interface for creating and verifying digital signatures
using ECDSA, Schnorr, Ed25519, ML-DSA, and SSH algorithms.
"""

from ._signature import Signature
from ._signature_scheme import SignatureScheme
from ._signer import Signer, Verifier
from ._signing_private_key import (
    SchnorrSigningOptions,
    SigningOptions,
    SigningPrivateKey,
    SshSigningOptions,
)
from ._signing_public_key import SigningPublicKey

__all__ = [
    "SchnorrSigningOptions",
    "Signature",
    "SignatureScheme",
    "Signer",
    "SigningOptions",
    "SigningPrivateKey",
    "SigningPublicKey",
    "SshSigningOptions",
    "Verifier",
]
