"""Argon2id key derivation helper."""

from __future__ import annotations

from argon2.low_level import Type, hash_secret_raw


def argon2id(
    password: bytes | bytearray | memoryview | str,
    salt: bytes | bytearray | memoryview | str,
    output_len: int,
) -> bytes:
    """Compute Argon2id with Rust-equivalent defaults."""
    pass_bytes = password.encode("utf-8") if isinstance(password, str) else bytes(password)
    salt_bytes = salt.encode("utf-8") if isinstance(salt, str) else bytes(salt)
    return hash_secret_raw(
        secret=pass_bytes,
        salt=salt_bytes,
        time_cost=2,
        memory_cost=19 * 1024,
        parallelism=1,
        hash_len=output_len,
        type=Type.ID,
    )


__all__ = ["argon2id"]
