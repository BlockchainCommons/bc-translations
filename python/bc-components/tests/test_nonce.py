"""Tests for Nonce, translated from Rust bc-components/src/nonce.rs tests."""

from bc_components import NONCE_SIZE, Nonce


def test_nonce_raw():
    """Test creating a nonce from a zero-filled byte array."""
    nonce_raw = bytes(NONCE_SIZE)
    nonce = Nonce.from_data(nonce_raw)
    assert nonce.data == nonce_raw


def test_nonce_from_raw_data():
    """Test creating a nonce from raw bytes via from_data."""
    raw_data = bytes(NONCE_SIZE)
    nonce = Nonce.from_data(raw_data)
    assert nonce.data == raw_data


def test_nonce_size():
    """Test that an incorrectly sized nonce raises an error."""
    raw_data = bytes(NONCE_SIZE + 1)
    try:
        Nonce.from_data(raw_data)
        assert False, "Expected an error for wrong size"
    except Exception:
        pass


def test_nonce_new():
    """Test that two randomly generated nonces differ."""
    nonce1 = Nonce.generate()
    nonce2 = Nonce.generate()
    assert nonce1.data != nonce2.data


def test_nonce_hex_roundtrip():
    """Test hex encoding/decoding roundtrip."""
    nonce = Nonce.generate()
    hex_string = nonce.hex()
    nonce_from_hex = Nonce.from_hex(hex_string)
    assert nonce == nonce_from_hex


def test_nonce_cbor_roundtrip():
    """Test CBOR encoding/decoding roundtrip."""
    nonce = Nonce.generate()
    cbor = nonce.tagged_cbor()
    decoded_nonce = Nonce.from_tagged_cbor(cbor)
    assert nonce == decoded_nonce
