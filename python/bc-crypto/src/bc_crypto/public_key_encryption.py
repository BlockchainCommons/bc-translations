"""X25519 key derivation and key agreement helpers."""

from __future__ import annotations

from cryptography.hazmat.primitives import serialization
from cryptography.hazmat.primitives.asymmetric import x25519

from .hash import hkdf_hmac_sha256
from .symmetric_encryption import SYMMETRIC_KEY_SIZE

GENERIC_PRIVATE_KEY_SIZE = 32
GENERIC_PUBLIC_KEY_SIZE = 32
X25519_PRIVATE_KEY_SIZE = 32
X25519_PUBLIC_KEY_SIZE = 32


def derive_agreement_private_key(
    key_material: bytes | bytearray | memoryview | str,
) -> bytes:
    """Derive a 32-byte agreement private key from the given key material.

    May be used for key agreement or key encapsulation. Enforces domain
    separation from signing keys.
    """
    return hkdf_hmac_sha256(key_material, b"agreement", GENERIC_PRIVATE_KEY_SIZE)


def derive_signing_private_key(
    key_material: bytes | bytearray | memoryview | str,
) -> bytes:
    """Derive a 32-byte signing private key from the given key material.

    Enforces domain separation from agreement keys.
    """
    return hkdf_hmac_sha256(key_material, b"signing", GENERIC_PUBLIC_KEY_SIZE)


def x25519_new_private_key_using(rng) -> bytes:
    """Create a new X25519 private key using the given random number generator."""
    return bytes(rng.random_data(X25519_PRIVATE_KEY_SIZE))


def x25519_public_key_from_private_key(x25519_private_key: bytes) -> bytes:
    """Derive an X25519 public key from a private key."""
    sk = x25519.X25519PrivateKey.from_private_bytes(bytes(x25519_private_key))
    pk = sk.public_key()
    return pk.public_bytes(
        encoding=serialization.Encoding.Raw,
        format=serialization.PublicFormat.Raw,
    )


def x25519_shared_key(
    x25519_private_key: bytes,
    x25519_public_key: bytes,
) -> bytes:
    """Compute the shared symmetric key from the given X25519 private and public keys."""
    sk = x25519.X25519PrivateKey.from_private_bytes(bytes(x25519_private_key))
    pk = x25519.X25519PublicKey.from_public_bytes(bytes(x25519_public_key))
    shared_secret = sk.exchange(pk)
    return hkdf_hmac_sha256(shared_secret, b"agreement", SYMMETRIC_KEY_SIZE)


__all__ = [
    "GENERIC_PRIVATE_KEY_SIZE",
    "GENERIC_PUBLIC_KEY_SIZE",
    "X25519_PRIVATE_KEY_SIZE",
    "X25519_PUBLIC_KEY_SIZE",
    "derive_agreement_private_key",
    "derive_signing_private_key",
    "x25519_new_private_key_using",
    "x25519_public_key_from_private_key",
    "x25519_shared_key",
]
