"""ECDSA sign/verify helpers."""

from __future__ import annotations

from btclib.ecc.libsecp256k1 import ecdsa_sign_, ecdsa_verify_
from cryptography.hazmat.primitives.asymmetric.utils import (
    decode_dss_signature,
    encode_dss_signature,
)

from .ecdsa_keys import (
    ECDSA_PRIVATE_KEY_SIZE,
    ECDSA_PUBLIC_KEY_SIZE,
    ECDSA_SIGNATURE_SIZE,
)
from .hash import double_sha256


def ecdsa_sign(private_key: bytes, message: bytes | bytearray | memoryview) -> bytes:
    """Sign message using secp256k1 ECDSA and return compact 64-byte signature."""
    msg_hash = double_sha256(bytes(message))
    der = ecdsa_sign_(msg_hash, bytes(private_key))
    r, s = decode_dss_signature(der)
    return r.to_bytes(32, "big") + s.to_bytes(32, "big")


def ecdsa_verify(
    public_key: bytes,
    signature: bytes,
    message: bytes | bytearray | memoryview,
) -> bool:
    """Verify compact 64-byte ECDSA signature."""
    if len(public_key) != ECDSA_PUBLIC_KEY_SIZE:
        raise ValueError(
            f"public_key must be {ECDSA_PUBLIC_KEY_SIZE} bytes, got {len(public_key)}"
        )
    if len(signature) != ECDSA_SIGNATURE_SIZE:
        raise ValueError(
            f"signature must be {ECDSA_SIGNATURE_SIZE} bytes, got {len(signature)}"
        )

    r = int.from_bytes(signature[:32], "big")
    s = int.from_bytes(signature[32:], "big")
    der = encode_dss_signature(r, s)
    msg_hash = double_sha256(bytes(message))
    return bool(ecdsa_verify_(msg_hash, bytes(public_key), der))


__all__ = [
    "ECDSA_PRIVATE_KEY_SIZE",
    "ECDSA_PUBLIC_KEY_SIZE",
    "ECDSA_SIGNATURE_SIZE",
    "ecdsa_sign",
    "ecdsa_verify",
]
