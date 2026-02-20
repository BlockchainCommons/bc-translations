"""Ed25519 sign/verify helpers."""

from __future__ import annotations

from cryptography.exceptions import InvalidSignature
from cryptography.hazmat.primitives import serialization
from cryptography.hazmat.primitives.asymmetric.ed25519 import (
    Ed25519PrivateKey,
    Ed25519PublicKey,
)

ED25519_PUBLIC_KEY_SIZE = 32
ED25519_PRIVATE_KEY_SIZE = 32
ED25519_SIGNATURE_SIZE = 64


def ed25519_new_private_key_using(rng) -> bytes:
    """Generate Ed25519 private key bytes using provided RNG."""
    return bytes(rng.random_data(ED25519_PRIVATE_KEY_SIZE))


def ed25519_public_key_from_private_key(private_key: bytes) -> bytes:
    """Derive Ed25519 public key from 32-byte private key."""
    sk = Ed25519PrivateKey.from_private_bytes(bytes(private_key))
    vk = sk.public_key()
    return vk.public_bytes(
        encoding=serialization.Encoding.Raw,
        format=serialization.PublicFormat.Raw,
    )


def ed25519_sign(
    private_key: bytes,
    message: bytes | bytearray | memoryview,
) -> bytes:
    """Sign message with Ed25519."""
    sk = Ed25519PrivateKey.from_private_bytes(bytes(private_key))
    return sk.sign(bytes(message))


def ed25519_verify(
    public_key: bytes,
    message: bytes | bytearray | memoryview,
    signature: bytes,
) -> bool:
    """Verify Ed25519 signature."""
    try:
        vk = Ed25519PublicKey.from_public_bytes(bytes(public_key))
        vk.verify(bytes(signature), bytes(message))
        return True
    except InvalidSignature:
        return False


__all__ = [
    "ED25519_PRIVATE_KEY_SIZE",
    "ED25519_PUBLIC_KEY_SIZE",
    "ED25519_SIGNATURE_SIZE",
    "ed25519_new_private_key_using",
    "ed25519_public_key_from_private_key",
    "ed25519_sign",
    "ed25519_verify",
]
