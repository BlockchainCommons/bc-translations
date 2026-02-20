"""Tests translated from Rust argon.rs."""

from bc_crypto import argon2id


def test_argon2id_basic() -> None:
    password = b"password"
    salt = b"example salt"
    output = argon2id(password, salt, 32)
    assert len(output) == 32
    output2 = argon2id(password, salt, 32)
    assert output == output2


def test_argon2id_different_salt() -> None:
    password = b"password"
    salt1 = b"example salt"
    salt2 = b"example salt2"
    out1 = argon2id(password, salt1, 32)
    out2 = argon2id(password, salt2, 32)
    assert out1 != out2
