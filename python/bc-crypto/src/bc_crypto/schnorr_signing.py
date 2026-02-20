"""BIP340 Schnorr sign/verify helpers."""

from __future__ import annotations

from btclib.ecc.libsecp256k1 import ctx, ffi, lib

from .ecdsa_keys import ECDSA_PRIVATE_KEY_SIZE, SCHNORR_PUBLIC_KEY_SIZE

SCHNORR_SIGNATURE_SIZE = 64
_SCHNORRSIG_EXTRAPARAMS_MAGIC = bytes([0xDA, 0x6F, 0xB3, 0x8C])


def schnorr_sign(
    ecdsa_private_key: bytes,
    message: bytes | bytearray | memoryview,
) -> bytes:
    """Sign using secure random auxiliary data."""
    from bc_rand import SecureRandomNumberGenerator

    rng = SecureRandomNumberGenerator()
    return schnorr_sign_using(ecdsa_private_key, message, rng)


def schnorr_sign_using(
    ecdsa_private_key: bytes,
    message: bytes | bytearray | memoryview,
    rng,
) -> bytes:
    """Sign using RNG-provided 32-byte auxiliary randomness."""
    aux_rand = bytes(rng.random_data(32))
    return schnorr_sign_with_aux_rand(ecdsa_private_key, message, aux_rand)


def schnorr_sign_with_aux_rand(
    ecdsa_private_key: bytes,
    message: bytes | bytearray | memoryview,
    aux_rand: bytes,
) -> bytes:
    """Sign with explicit 32-byte auxiliary randomness."""
    if len(ecdsa_private_key) != ECDSA_PRIVATE_KEY_SIZE:
        raise ValueError(
            f"ecdsa_private_key must be {ECDSA_PRIVATE_KEY_SIZE} bytes, got {len(ecdsa_private_key)}"
        )
    if len(aux_rand) != 32:
        raise ValueError(f"aux_rand must be 32 bytes, got {len(aux_rand)}")

    keypair = ffi.new("secp256k1_keypair *")
    if lib.secp256k1_keypair_create(ctx, keypair, bytes(ecdsa_private_key)) != 1:
        raise ValueError("invalid secp256k1 private key")

    sig = ffi.new("char[64]")

    extraparams = ffi.new("secp256k1_schnorrsig_extraparams *")
    extraparams.magic = _SCHNORRSIG_EXTRAPARAMS_MAGIC
    extraparams.noncefp = ffi.NULL

    ndata = ffi.from_buffer(bytes(aux_rand))
    extraparams.ndata = ndata

    msg = bytes(message)
    ok = lib.secp256k1_schnorrsig_sign_custom(
        ctx,
        sig,
        msg,
        len(msg),
        keypair,
        extraparams,
    )
    if ok != 1:
        raise RuntimeError("schnorr signing failed")

    return ffi.unpack(sig, 64)


def schnorr_verify(
    schnorr_public_key: bytes,
    schnorr_signature: bytes,
    message: bytes | bytearray | memoryview,
) -> bool:
    """Verify BIP340 Schnorr signature."""
    if len(schnorr_signature) != SCHNORR_SIGNATURE_SIZE:
        raise ValueError(
            f"schnorr_signature must be {SCHNORR_SIGNATURE_SIZE} bytes, got {len(schnorr_signature)}"
        )
    if len(schnorr_public_key) != SCHNORR_PUBLIC_KEY_SIZE:
        raise ValueError(
            f"schnorr_public_key must be {SCHNORR_PUBLIC_KEY_SIZE} bytes, got {len(schnorr_public_key)}"
        )

    xonly = ffi.new("secp256k1_xonly_pubkey *")
    if lib.secp256k1_xonly_pubkey_parse(ctx, xonly, bytes(schnorr_public_key)) != 1:
        raise ValueError("invalid x-only public key")

    msg = bytes(message)
    return bool(
        lib.secp256k1_schnorrsig_verify(
            ctx,
            bytes(schnorr_signature),
            msg,
            len(msg),
            xonly,
        )
    )


__all__ = [
    "SCHNORR_SIGNATURE_SIZE",
    "schnorr_sign",
    "schnorr_sign_using",
    "schnorr_sign_with_aux_rand",
    "schnorr_verify",
]
