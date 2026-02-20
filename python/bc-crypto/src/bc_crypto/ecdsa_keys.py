"""ECDSA/secp256k1 key helpers."""

from __future__ import annotations

from btclib.ec import secp256k1
from btclib.ec.sec_point import bytes_from_point
from btclib.ecc import dsa
from btclib.ecc.libsecp256k1 import ctx, ffi, lib
from btclib.to_pub_key import point_from_pub_key

from .hash import hkdf_hmac_sha256

ECDSA_PRIVATE_KEY_SIZE = 32
ECDSA_PUBLIC_KEY_SIZE = 33
ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE = 65
ECDSA_MESSAGE_HASH_SIZE = 32
ECDSA_SIGNATURE_SIZE = 64
SCHNORR_PUBLIC_KEY_SIZE = 32


def ecdsa_new_private_key_using(rng) -> bytes:
    """Generate a new ECDSA private key using provided RNG."""
    return bytes(rng.random_data(ECDSA_PRIVATE_KEY_SIZE))


def ecdsa_public_key_from_private_key(private_key: bytes) -> bytes:
    """Derive compressed secp256k1 public key from 32-byte private key."""
    sec_bytes, _network = dsa.pub_keyinfo_from_key(
        bytes(private_key),
        compressed=True,
    )
    return sec_bytes


def ecdsa_decompress_public_key(compressed_public_key: bytes) -> bytes:
    """Convert compressed secp256k1 public key to uncompressed format."""
    point = point_from_pub_key(bytes(compressed_public_key), secp256k1)
    return bytes_from_point(point, secp256k1, compressed=False)


def ecdsa_compress_public_key(uncompressed_public_key: bytes) -> bytes:
    """Convert uncompressed secp256k1 public key to compressed format."""
    point = point_from_pub_key(bytes(uncompressed_public_key), secp256k1)
    return bytes_from_point(point, secp256k1, compressed=True)


def ecdsa_derive_private_key(
    key_material: bytes | bytearray | memoryview | str,
) -> bytes:
    """Derive ECDSA private key bytes from key material."""
    return hkdf_hmac_sha256(key_material, b"signing", 32)


def schnorr_public_key_from_private_key(private_key: bytes) -> bytes:
    """Derive x-only Schnorr public key from private key."""
    keypair = ffi.new("secp256k1_keypair *")
    if lib.secp256k1_keypair_create(ctx, keypair, bytes(private_key)) != 1:
        raise ValueError("invalid secp256k1 private key")

    xonly = ffi.new("secp256k1_xonly_pubkey *")
    if lib.secp256k1_keypair_xonly_pub(ctx, xonly, ffi.NULL, keypair) != 1:
        raise ValueError("unable to derive x-only public key")

    serialized = ffi.new("char[32]")
    if lib.secp256k1_xonly_pubkey_serialize(ctx, serialized, xonly) != 1:
        raise ValueError("unable to serialize x-only public key")

    return ffi.unpack(serialized, 32)


__all__ = [
    "ECDSA_MESSAGE_HASH_SIZE",
    "ECDSA_PRIVATE_KEY_SIZE",
    "ECDSA_PUBLIC_KEY_SIZE",
    "ECDSA_SIGNATURE_SIZE",
    "ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE",
    "SCHNORR_PUBLIC_KEY_SIZE",
    "ecdsa_compress_public_key",
    "ecdsa_decompress_public_key",
    "ecdsa_derive_private_key",
    "ecdsa_new_private_key_using",
    "ecdsa_public_key_from_private_key",
    "schnorr_public_key_from_private_key",
]
