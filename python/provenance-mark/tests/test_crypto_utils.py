"""Tests for cryptographic helpers."""

from provenance_mark import extend_key, obfuscate, sha256


def test_sha256_vector() -> None:
    assert sha256(b"Hello World").hex() == (
        "a591a6d40bf420404a011733cfb7b190"
        "d62c65bf0bcda32b57b277d9ad9f146e"
    )


def test_extend_key_vector() -> None:
    assert extend_key(b"Hello World").hex() == (
        "813085a508d5fec645abe5a1fb9a23c2"
        "a6ac6bef0a99650017b3ef50538dba39"
    )


def test_obfuscate_round_trip_vector() -> None:
    obfuscated = obfuscate(b"Hello", b"World")
    assert obfuscated == bytes.fromhex("c43889aafa")
    assert obfuscate(b"Hello", obfuscated) == b"World"
