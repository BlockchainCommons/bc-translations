"""Tests translated from Rust scrypt.rs."""

from bc_crypto import scrypt, scrypt_opt


def test_scrypt_basic() -> None:
    password = b"password"
    salt = b"salt"
    output = scrypt(password, salt, 32)
    assert len(output) == 32
    output2 = scrypt(password, salt, 32)
    assert output == output2


def test_scrypt_different_salt() -> None:
    password = b"password"
    salt1 = b"salt1"
    salt2 = b"salt2"
    out1 = scrypt(password, salt1, 32)
    out2 = scrypt(password, salt2, 32)
    assert out1 != out2


def test_scrypt_opt_basic() -> None:
    password = b"password"
    salt = b"salt"
    output = scrypt_opt(password, salt, 32, 15, 8, 1)
    assert len(output) == 32


def test_scrypt_output_length() -> None:
    password = b"password"
    salt = b"salt"
    for length in [16, 24, 32, 64]:
        output = scrypt(password, salt, length)
        assert len(output) == length
