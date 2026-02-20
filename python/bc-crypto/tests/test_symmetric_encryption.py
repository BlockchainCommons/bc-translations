"""Tests translated from Rust symmetric_encryption.rs."""

from bc_rand import random_data

from bc_crypto import (
    SYMMETRIC_AUTH_SIZE,
    aead_chacha20_poly1305_decrypt_with_aad,
    aead_chacha20_poly1305_encrypt_with_aad,
)

PLAINTEXT = (
    b"Ladies and Gentlemen of the class of '99: If I could offer you only "
    b"one tip for the future, sunscreen would be it."
)
AAD = bytes.fromhex("50515253c0c1c2c3c4c5c6c7")
KEY = bytes.fromhex(
    "808182838485868788898a8b8c8d8e8f"
    "909192939495969798999a9b9c9d9e9f"
)
NONCE = bytes.fromhex("070000004041424344454647")
CIPHERTEXT = bytes.fromhex(
    "d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d6"
    "3dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b36"
    "92ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc"
    "3ff4def08e4b7a9de576d26586cec64b6116"
)
AUTH = bytes.fromhex("1ae10b594f09e26a7e902ecbd0600691")


def encrypted() -> tuple[bytes, bytes]:
    return aead_chacha20_poly1305_encrypt_with_aad(PLAINTEXT, KEY, NONCE, AAD)


def test_rfc_test_vector() -> None:
    ciphertext, auth = encrypted()
    assert ciphertext == CIPHERTEXT
    assert auth == AUTH

    decrypted = aead_chacha20_poly1305_decrypt_with_aad(
        ciphertext,
        KEY,
        NONCE,
        AAD,
        auth,
    )
    assert decrypted == PLAINTEXT


def test_random_key_and_nonce() -> None:
    key = random_data(32)
    nonce = random_data(12)
    ciphertext, auth = aead_chacha20_poly1305_encrypt_with_aad(
        PLAINTEXT,
        key,
        nonce,
        AAD,
    )
    decrypted = aead_chacha20_poly1305_decrypt_with_aad(
        ciphertext,
        key,
        nonce,
        AAD,
        auth,
    )
    assert decrypted == PLAINTEXT


def test_empty_data() -> None:
    key = random_data(32)
    nonce = random_data(12)
    ciphertext, auth = aead_chacha20_poly1305_encrypt_with_aad(b"", key, nonce, b"")
    assert len(auth) == SYMMETRIC_AUTH_SIZE
    decrypted = aead_chacha20_poly1305_decrypt_with_aad(
        ciphertext,
        key,
        nonce,
        b"",
        auth,
    )
    assert decrypted == b""
