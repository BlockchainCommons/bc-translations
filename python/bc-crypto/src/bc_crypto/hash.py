"""The hash module contains functions for hashing data."""

from __future__ import annotations

import binascii
import hashlib
import hmac

CRC32_SIZE = 4
SHA256_SIZE = 32
SHA512_SIZE = 64


def _as_bytes(data: bytes | bytearray | memoryview | str) -> bytes:
    if isinstance(data, str):
        return data.encode("utf-8")
    return bytes(data)


def crc32(data: bytes | bytearray | memoryview | str) -> int:
    """Compute the CRC-32 checksum of the given data."""
    return binascii.crc32(_as_bytes(data)) & 0xFFFFFFFF


def crc32_data_opt(
    data: bytes | bytearray | memoryview | str,
    little_endian: bool,
) -> bytes:
    """Compute the CRC-32 checksum of the given data, returning the result as a 4-byte value in the specified byte order."""
    checksum = crc32(data)
    if little_endian:
        return checksum.to_bytes(CRC32_SIZE, "little")
    return checksum.to_bytes(CRC32_SIZE, "big")


def crc32_data(data: bytes | bytearray | memoryview | str) -> bytes:
    """Compute the CRC-32 checksum of the given data, returning the result as a 4-byte value in big-endian format."""
    return crc32_data_opt(data, False)


def sha256(data: bytes | bytearray | memoryview | str) -> bytes:
    """Compute the SHA-256 digest of the input data."""
    return hashlib.sha256(_as_bytes(data)).digest()


def double_sha256(message: bytes | bytearray | memoryview | str) -> bytes:
    """Compute the double SHA-256 digest of the input data."""
    return sha256(sha256(message))


def sha512(data: bytes | bytearray | memoryview | str) -> bytes:
    """Compute the SHA-512 digest of the input data."""
    return hashlib.sha512(_as_bytes(data)).digest()


def hmac_sha256(
    key: bytes | bytearray | memoryview | str,
    message: bytes | bytearray | memoryview | str,
) -> bytes:
    """Compute the HMAC-SHA-256 for the given key and message."""
    return hmac.new(_as_bytes(key), _as_bytes(message), hashlib.sha256).digest()


def hmac_sha512(
    key: bytes | bytearray | memoryview | str,
    message: bytes | bytearray | memoryview | str,
) -> bytes:
    """Compute the HMAC-SHA-512 for the given key and message."""
    return hmac.new(_as_bytes(key), _as_bytes(message), hashlib.sha512).digest()


def pbkdf2_hmac_sha256(
    password: bytes | bytearray | memoryview | str,
    salt: bytes | bytearray | memoryview | str,
    iterations: int,
    key_len: int,
) -> bytes:
    """Compute the PBKDF2-HMAC-SHA-256 for the given password."""
    return hashlib.pbkdf2_hmac(
        "sha256",
        _as_bytes(password),
        _as_bytes(salt),
        iterations,
        key_len,
    )


def pbkdf2_hmac_sha512(
    password: bytes | bytearray | memoryview | str,
    salt: bytes | bytearray | memoryview | str,
    iterations: int,
    key_len: int,
) -> bytes:
    """Compute the PBKDF2-HMAC-SHA-512 for the given password."""
    return hashlib.pbkdf2_hmac(
        "sha512",
        _as_bytes(password),
        _as_bytes(salt),
        iterations,
        key_len,
    )


def _hkdf_extract(
    hash_fn,
    salt: bytes,
    ikm: bytes,
) -> bytes:
    return hmac.new(salt, ikm, hash_fn).digest()


def _hkdf_expand(
    hash_fn,
    prk: bytes,
    info: bytes,
    length: int,
) -> bytes:
    output = bytearray()
    prev = b""
    counter = 1
    digest_size = hash_fn().digest_size

    while len(output) < length:
        prev = hmac.new(
            prk,
            prev + info + bytes([counter]),
            hash_fn,
        ).digest()
        output.extend(prev)
        counter += 1

    return bytes(output[:length])


def hkdf_hmac_sha256(
    key_material: bytes | bytearray | memoryview | str,
    salt: bytes | bytearray | memoryview | str,
    key_len: int,
) -> bytes:
    """Compute the HKDF-HMAC-SHA-256 for the given key material."""
    ikm = _as_bytes(key_material)
    raw_salt = _as_bytes(salt)
    if len(raw_salt) == 0:
        raw_salt = b"\x00" * hashlib.sha256().digest_size
    prk = _hkdf_extract(hashlib.sha256, raw_salt, ikm)
    return _hkdf_expand(hashlib.sha256, prk, b"", key_len)


def hkdf_hmac_sha512(
    key_material: bytes | bytearray | memoryview | str,
    salt: bytes | bytearray | memoryview | str,
    key_len: int,
) -> bytes:
    """Compute the HKDF-HMAC-SHA-512 for the given key material."""
    ikm = _as_bytes(key_material)
    raw_salt = _as_bytes(salt)
    if len(raw_salt) == 0:
        raw_salt = b"\x00" * hashlib.sha512().digest_size
    prk = _hkdf_extract(hashlib.sha512, raw_salt, ikm)
    return _hkdf_expand(hashlib.sha512, prk, b"", key_len)


__all__ = [
    "CRC32_SIZE",
    "SHA256_SIZE",
    "SHA512_SIZE",
    "crc32",
    "crc32_data",
    "crc32_data_opt",
    "double_sha256",
    "hkdf_hmac_sha256",
    "hkdf_hmac_sha512",
    "hmac_sha256",
    "hmac_sha512",
    "pbkdf2_hmac_sha256",
    "pbkdf2_hmac_sha512",
    "sha256",
    "sha512",
]
