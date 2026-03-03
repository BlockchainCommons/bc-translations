"""Tests for Digest, translated from Rust bc-components/src/digest.rs tests."""

import pytest

from bc_components import Digest, register_tags


def test_digest():
    """Test creating a digest from image data and verifying the SHA-256 output."""
    data = b"hello world"
    digest = Digest.from_image(data)
    assert len(digest.data) == Digest.DIGEST_SIZE

    expected_hex = (
        "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
    )
    assert digest.data == bytes.fromhex(expected_hex)

    # CBOR roundtrip
    cbor = digest.tagged_cbor()
    digest2 = Digest.from_tagged_cbor(cbor)
    assert digest == digest2


def test_digest_from_hex():
    """Test creating a digest from a hex string."""
    hex_str = (
        "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
    )
    digest = Digest.from_hex(hex_str)
    assert len(digest.data) == Digest.DIGEST_SIZE

    # Should match the SHA-256 of "hello world"
    expected = Digest.from_image(b"hello world")
    assert digest == expected
    assert digest.data == bytes.fromhex(hex_str)


def test_ur():
    """Test UR encoding/decoding roundtrip with exact expected UR string."""
    register_tags()
    data = b"hello world"
    digest = Digest.from_image(data)
    ur_string = digest.ur_string()
    expected_ur_string = (
        "ur:digest/hdcxrhgtdirhmugtfmayondmgmtstnkipyzssslrwsvlkngulawymhloylpsvowssnwlamnlatrs"
    )
    assert ur_string == expected_ur_string
    digest2 = Digest.from_ur_string(ur_string)
    assert digest == digest2


def test_digest_equality():
    """Test that two digests from the same hex are equal."""
    hex_str = (
        "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
    )
    digest1 = Digest.from_hex(hex_str)
    digest2 = Digest.from_hex(hex_str)
    assert digest1 == digest2


def test_digest_inequality():
    """Test that two different digests are not equal."""
    digest1 = Digest.from_hex(
        "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
    )
    digest2 = Digest.from_hex(
        "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
    )
    assert digest1 != digest2


def test_invalid_hex_string():
    """Test that an invalid hex string raises an exception."""
    with pytest.raises(ValueError):
        Digest.from_hex("invalid_hex_string")


def test_new_from_invalid_ur_string():
    """Test that an invalid UR string raises an exception."""
    register_tags()
    invalid_ur = "ur:not_digest/invalid"
    with pytest.raises(Exception):
        Digest.from_ur_string(invalid_ur)
