"""scrypt key derivation helpers."""

from __future__ import annotations

import hashlib

_SCRYPT_MAXMEM = 1 << 30


def scrypt(
    password: bytes | bytearray | memoryview | str,
    salt: bytes | bytearray | memoryview | str,
    output_len: int,
) -> bytes:
    """Compute scrypt with Rust-equivalent recommended parameters."""
    pass_bytes = password.encode("utf-8") if isinstance(password, str) else bytes(password)
    salt_bytes = salt.encode("utf-8") if isinstance(salt, str) else bytes(salt)
    # Rust scrypt::Params::recommended(): log_n=15, r=8, p=1
    return hashlib.scrypt(
        pass_bytes,
        salt=salt_bytes,
        n=1 << 15,
        r=8,
        p=1,
        maxmem=_SCRYPT_MAXMEM,
        dklen=output_len,
    )


def scrypt_opt(
    password: bytes | bytearray | memoryview | str,
    salt: bytes | bytearray | memoryview | str,
    output_len: int,
    log_n: int,
    r: int,
    p: int,
) -> bytes:
    """Compute scrypt with explicit parameters."""
    pass_bytes = password.encode("utf-8") if isinstance(password, str) else bytes(password)
    salt_bytes = salt.encode("utf-8") if isinstance(salt, str) else bytes(salt)
    return hashlib.scrypt(
        pass_bytes,
        salt=salt_bytes,
        n=1 << log_n,
        r=r,
        p=p,
        maxmem=_SCRYPT_MAXMEM,
        dklen=output_len,
    )


__all__ = ["scrypt", "scrypt_opt"]
