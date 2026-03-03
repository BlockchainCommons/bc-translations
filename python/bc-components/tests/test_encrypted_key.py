"""Tests for EncryptedKey, translated from Rust encrypted_key/encrypted_key_impl.rs tests."""

import pytest

from bc_components import (
    BCComponentsError,
    EncryptedKey,
    KeyDerivationMethod,
    SymmetricKey,
    register_tags,
)


TEST_SECRET = b"correct horse battery staple"
WRONG_SECRET = b"wrong secret"


def _test_content_key() -> SymmetricKey:
    return SymmetricKey.generate()


# --- Roundtrip tests ---


def test_encrypted_key_hkdf_roundtrip():
    """Test HKDF lock/unlock roundtrip with CBOR serialization."""
    register_tags()
    content_key = _test_content_key()

    encrypted = EncryptedKey.lock(KeyDerivationMethod.HKDF, TEST_SECRET, content_key)
    assert "HKDF" in str(encrypted)
    assert "SHA256" in str(encrypted)

    cbor = encrypted.tagged_cbor()
    encrypted2 = EncryptedKey.from_tagged_cbor(cbor)
    decrypted = encrypted2.unlock(TEST_SECRET)

    assert content_key == decrypted


def test_encrypted_key_pbkdf2_roundtrip():
    """Test PBKDF2 lock/unlock roundtrip with CBOR serialization."""
    register_tags()
    content_key = _test_content_key()

    encrypted = EncryptedKey.lock(KeyDerivationMethod.PBKDF2, TEST_SECRET, content_key)
    assert "PBKDF2" in str(encrypted)
    assert "SHA256" in str(encrypted)

    cbor = encrypted.tagged_cbor()
    encrypted2 = EncryptedKey.from_tagged_cbor(cbor)
    decrypted = encrypted2.unlock(TEST_SECRET)

    assert content_key == decrypted


def test_encrypted_key_scrypt_roundtrip():
    """Test Scrypt lock/unlock roundtrip with CBOR serialization."""
    register_tags()
    content_key = _test_content_key()

    encrypted = EncryptedKey.lock(KeyDerivationMethod.SCRYPT, TEST_SECRET, content_key)
    assert "Scrypt" in str(encrypted)

    cbor = encrypted.tagged_cbor()
    encrypted2 = EncryptedKey.from_tagged_cbor(cbor)
    decrypted = encrypted2.unlock(TEST_SECRET)

    assert content_key == decrypted


def test_encrypted_key_argon2id_roundtrip():
    """Test Argon2id lock/unlock roundtrip with CBOR serialization."""
    register_tags()
    content_key = _test_content_key()

    encrypted = EncryptedKey.lock(
        KeyDerivationMethod.ARGON2ID, TEST_SECRET, content_key
    )
    assert "Argon2id" in str(encrypted)

    cbor = encrypted.tagged_cbor()
    encrypted2 = EncryptedKey.from_tagged_cbor(cbor)
    decrypted = encrypted2.unlock(TEST_SECRET)

    assert content_key == decrypted


# --- Wrong secret tests ---


def test_encrypted_key_wrong_secret_fails_hkdf():
    """Test that HKDF decryption with wrong secret raises an error."""
    content_key = _test_content_key()
    encrypted = EncryptedKey.lock(KeyDerivationMethod.HKDF, TEST_SECRET, content_key)
    with pytest.raises(Exception):
        encrypted.unlock(WRONG_SECRET)


def test_encrypted_key_wrong_secret_fails_pbkdf2():
    """Test that PBKDF2 decryption with wrong secret raises an error."""
    content_key = _test_content_key()
    encrypted = EncryptedKey.lock(KeyDerivationMethod.PBKDF2, TEST_SECRET, content_key)
    with pytest.raises(Exception):
        encrypted.unlock(WRONG_SECRET)


def test_encrypted_key_wrong_secret_fails_scrypt():
    """Test that Scrypt decryption with wrong secret raises an error."""
    content_key = _test_content_key()
    encrypted = EncryptedKey.lock(KeyDerivationMethod.SCRYPT, TEST_SECRET, content_key)
    with pytest.raises(Exception):
        encrypted.unlock(WRONG_SECRET)


def test_encrypted_key_wrong_secret_fails_argon2id():
    """Test that Argon2id decryption with wrong secret raises an error."""
    content_key = _test_content_key()
    encrypted = EncryptedKey.lock(
        KeyDerivationMethod.ARGON2ID, TEST_SECRET, content_key
    )
    with pytest.raises(Exception):
        encrypted.unlock(WRONG_SECRET)


# --- Params variant test ---


def test_encrypted_key_params_variant():
    """Test that each method produces the correct params variant."""
    content_key = _test_content_key()

    hkdf = EncryptedKey.lock(KeyDerivationMethod.HKDF, TEST_SECRET, content_key)
    assert not hkdf.is_password_based()

    pbkdf2 = EncryptedKey.lock(KeyDerivationMethod.PBKDF2, TEST_SECRET, content_key)
    assert pbkdf2.is_password_based()

    scrypt = EncryptedKey.lock(KeyDerivationMethod.SCRYPT, TEST_SECRET, content_key)
    assert scrypt.is_password_based()

    argon2id = EncryptedKey.lock(
        KeyDerivationMethod.ARGON2ID, TEST_SECRET, content_key
    )
    assert argon2id.is_password_based()
