"""Ed25519 digital signature types.

Re-exports ``Ed25519PrivateKey`` and ``Ed25519PublicKey``.
"""

from ._ed25519_private_key import Ed25519PrivateKey
from ._ed25519_public_key import Ed25519PublicKey

__all__ = [
    "Ed25519PrivateKey",
    "Ed25519PublicKey",
]
