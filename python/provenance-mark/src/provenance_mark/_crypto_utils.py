"""Cryptographic helpers for provenance-mark."""

from __future__ import annotations

import hashlib
import hmac


SHA256_SIZE = 32
_MASK32 = 0xFFFFFFFF


def sha256(data: bytes | bytearray) -> bytes:
    """Return the SHA-256 digest of *data*."""
    return hashlib.sha256(bytes(data)).digest()


def sha256_prefix(data: bytes | bytearray, prefix: int) -> bytes:
    """Return the first *prefix* bytes of the SHA-256 digest of *data*."""
    return sha256(data)[:prefix]


def hkdf_hmac_sha256(
    key_material: bytes | bytearray,
    salt: bytes | bytearray,
    key_len: int,
) -> bytes:
    """Compute HKDF-HMAC-SHA256 with empty info."""
    key_material_bytes = bytes(key_material)
    salt_bytes = bytes(salt) if salt else bytes(SHA256_SIZE)
    prk = hmac.new(salt_bytes, key_material_bytes, hashlib.sha256).digest()

    blocks: list[bytes] = []
    previous = b""
    counter = 1
    while len(b"".join(blocks)) < key_len:
        previous = hmac.new(
            prk,
            previous + bytes((counter,)),
            hashlib.sha256,
        ).digest()
        blocks.append(previous)
        counter += 1
    return b"".join(blocks)[:key_len]


def extend_key(data: bytes | bytearray) -> bytes:
    """Expand *data* to 32 bytes with HKDF-HMAC-SHA256."""
    return hkdf_hmac_sha256(data, b"", 32)


def _rotl32(value: int, shift: int) -> int:
    return ((value << shift) | (value >> (32 - shift))) & _MASK32


def _quarter_round(state: list[int], a: int, b: int, c: int, d: int) -> None:
    state[a] = (state[a] + state[b]) & _MASK32
    state[d] = _rotl32(state[d] ^ state[a], 16)
    state[c] = (state[c] + state[d]) & _MASK32
    state[b] = _rotl32(state[b] ^ state[c], 12)
    state[a] = (state[a] + state[b]) & _MASK32
    state[d] = _rotl32(state[d] ^ state[a], 8)
    state[c] = (state[c] + state[d]) & _MASK32
    state[b] = _rotl32(state[b] ^ state[c], 7)


def _read_u32_le(data: bytes, offset: int) -> int:
    return int.from_bytes(data[offset : offset + 4], "little")


def _chacha20_block(state: list[int]) -> bytes:
    working = list(state)
    for _ in range(10):
        _quarter_round(working, 0, 4, 8, 12)
        _quarter_round(working, 1, 5, 9, 13)
        _quarter_round(working, 2, 6, 10, 14)
        _quarter_round(working, 3, 7, 11, 15)
        _quarter_round(working, 0, 5, 10, 15)
        _quarter_round(working, 1, 6, 11, 12)
        _quarter_round(working, 2, 7, 8, 13)
        _quarter_round(working, 3, 4, 9, 14)
    output = bytearray(64)
    for index, value in enumerate(working):
        result = (value + state[index]) & _MASK32
        output[index * 4 : (index + 1) * 4] = result.to_bytes(4, "little")
    return bytes(output)


def obfuscate(key: bytes | bytearray, message: bytes | bytearray) -> bytes:
    """XOR *message* with a deterministic ChaCha20 keystream derived from *key*."""
    message_bytes = bytes(message)
    if not message_bytes:
        return b""

    ext_key = extend_key(key)
    nonce = ext_key[-12:][::-1]

    state = [
        0x61707865,
        0x3320646E,
        0x79622D32,
        0x6B206574,
    ]
    state.extend(_read_u32_le(ext_key, i * 4) for i in range(8))
    state.append(0)
    state.extend(_read_u32_le(nonce, i * 4) for i in range(3))

    result = bytearray(message_bytes)
    offset = 0
    while offset < len(result):
        keystream = _chacha20_block(state)
        state[12] = (state[12] + 1) & _MASK32
        count = min(64, len(result) - offset)
        for index in range(count):
            result[offset + index] ^= keystream[index]
        offset += count
    return bytes(result)

