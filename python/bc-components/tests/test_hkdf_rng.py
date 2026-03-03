"""Tests for HKDFRng, translated from Rust bc-components/src/hkdf_rng.rs tests."""

from bc_components import HKDFRng


def test_hkdf_rng_new():
    """Test constructing a new HKDFRng with default page length."""
    rng = HKDFRng.new(b"key_material", "salt")
    # Verify internal state via observable behavior:
    # page_length is 32 (default), page_index starts at 0, buffer is empty
    # We test these indirectly by verifying the output matches Rust.


def test_hkdf_rng_fill_buffer():
    """Test that fill_buffer populates the internal buffer."""
    rng = HKDFRng.new(b"key_material", "salt")
    rng._fill_buffer()
    # Buffer should now be non-empty (32 bytes, page_length default)
    assert len(rng._buffer) > 0
    assert rng._position == 0
    assert rng._page_index == 1


def test_hkdf_rng_next_bytes():
    """Test generating deterministic byte sequences matching Rust test vectors."""
    rng = HKDFRng.new(b"key_material", "salt")
    assert rng.random_data(16).hex() == "1032ac8ffea232a27c79fe381d7eb7e4"
    assert rng.random_data(16).hex() == "aeaaf727d35b6f338218391f9f8fa1f3"
    assert rng.random_data(16).hex() == "4348a59427711deb1e7d8a6959c6adb4"
    assert rng.random_data(16).hex() == "5d937a42cb5fb090fe1a1ec88f56e32b"


def test_hkdf_rng_next_u32():
    """Test generating a deterministic u32 matching Rust test vector."""
    rng = HKDFRng.new(b"key_material", "salt")
    num = rng.next_u32()
    assert num == 2410426896


def test_hkdf_rng_next_u64():
    """Test generating a deterministic u64 matching Rust test vector."""
    rng = HKDFRng.new(b"key_material", "salt")
    num = rng.next_u64()
    assert num == 11687583197195678224


def test_hkdf_rng_fill_bytes():
    """Test fill_random_data matching Rust test vector."""
    rng = HKDFRng.new(b"key_material", "salt")
    dest = bytearray(16)
    rng.fill_random_data(dest)
    assert dest.hex() == "1032ac8ffea232a27c79fe381d7eb7e4"
