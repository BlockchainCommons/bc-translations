"""ChaCha20-Poly1305 symmetric encryption helpers."""

from __future__ import annotations

from cryptography.exceptions import InvalidTag
from cryptography.hazmat.primitives.ciphers.aead import ChaCha20Poly1305

from .error import AeadError

SYMMETRIC_KEY_SIZE = 32
SYMMETRIC_NONCE_SIZE = 12
SYMMETRIC_AUTH_SIZE = 16


def aead_chacha20_poly1305_encrypt_with_aad(
    plaintext: bytes | bytearray | memoryview,
    key: bytes,
    nonce: bytes,
    aad: bytes | bytearray | memoryview,
) -> tuple[bytes, bytes]:
    """Encrypt data and return `(ciphertext, auth_tag)`."""
    cipher = ChaCha20Poly1305(key)
    encrypted = cipher.encrypt(bytes(nonce), bytes(plaintext), bytes(aad))
    return encrypted[:-SYMMETRIC_AUTH_SIZE], encrypted[-SYMMETRIC_AUTH_SIZE:]


def aead_chacha20_poly1305_encrypt(
    plaintext: bytes | bytearray | memoryview,
    key: bytes,
    nonce: bytes,
) -> tuple[bytes, bytes]:
    """Encrypt data without AAD and return `(ciphertext, auth_tag)`."""
    return aead_chacha20_poly1305_encrypt_with_aad(plaintext, key, nonce, b"")


def aead_chacha20_poly1305_decrypt_with_aad(
    ciphertext: bytes | bytearray | memoryview,
    key: bytes,
    nonce: bytes,
    aad: bytes | bytearray | memoryview,
    auth: bytes,
) -> bytes:
    """Decrypt data and verify auth tag; raise ``AeadError`` on failure."""
    cipher = ChaCha20Poly1305(key)
    payload = bytes(ciphertext) + bytes(auth)
    try:
        return cipher.decrypt(bytes(nonce), payload, bytes(aad))
    except InvalidTag as exc:
        raise AeadError("AEAD error") from exc


def aead_chacha20_poly1305_decrypt(
    ciphertext: bytes | bytearray | memoryview,
    key: bytes,
    nonce: bytes,
    auth: bytes,
) -> bytes:
    """Decrypt data without AAD and verify auth tag."""
    return aead_chacha20_poly1305_decrypt_with_aad(
        ciphertext,
        key,
        nonce,
        b"",
        auth,
    )


__all__ = [
    "SYMMETRIC_AUTH_SIZE",
    "SYMMETRIC_KEY_SIZE",
    "SYMMETRIC_NONCE_SIZE",
    "aead_chacha20_poly1305_decrypt",
    "aead_chacha20_poly1305_decrypt_with_aad",
    "aead_chacha20_poly1305_encrypt",
    "aead_chacha20_poly1305_encrypt_with_aad",
]
