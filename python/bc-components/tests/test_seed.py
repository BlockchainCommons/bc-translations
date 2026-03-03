"""Tests for Seed, translated from Rust seed.rs and Kotlin tests.

The Rust seed.rs has no inline tests; these tests validate the Python
translation against the Rust API surface and the Seed specification.
"""

import pytest

from datetime import datetime, timezone

from bc_rand import make_fake_random_number_generator
from dcbor import Date

from bc_components import (
    PrivateKeyBase,
    Seed,
    register_tags,
)
from bc_components._error import DataTooShortError


SEED_HEX = "59f2293a5bce7d4de59e71b4207ac5d2"


def test_seed_new():
    """Test creating a new random seed."""
    seed = Seed.generate()
    assert len(seed.data) == Seed.MIN_SEED_LENGTH


def test_seed_new_with_len():
    """Test creating a new random seed with custom length."""
    seed = Seed.generate_with_len(32)
    assert len(seed.data) == 32


def test_seed_new_with_len_using():
    """Test creating a seed with a deterministic RNG."""
    rng = make_fake_random_number_generator()
    seed = Seed.generate_with_len_using(16, rng)
    assert len(seed.data) == 16


def test_seed_too_short():
    """Test that creating a seed shorter than MIN_SEED_LENGTH raises an error."""
    with pytest.raises(DataTooShortError):
        Seed(b"short")


def test_seed_from_known_hex():
    """Test creating a seed from known hex data."""
    data = bytes.fromhex(SEED_HEX)
    seed = Seed.new_opt(data)
    assert seed.data == data


def test_seed_metadata():
    """Test seed metadata (name, note, creation date)."""
    data = bytes.fromhex(SEED_HEX)
    creation_date = Date(datetime(1970, 1, 1, tzinfo=timezone.utc))  # Unix epoch
    seed = Seed.new_opt(data, name="Test Seed", note="A test note", creation_date=creation_date)

    assert seed.name == "Test Seed"
    assert seed.note == "A test note"
    assert seed.creation_date is not None


def test_seed_metadata_setters():
    """Test that seed metadata can be updated."""
    seed = Seed.generate()
    seed.name = "Updated"
    seed.note = "Updated note"
    assert seed.name == "Updated"
    assert seed.note == "Updated note"


def test_seed_cbor_roundtrip():
    """Test Seed CBOR serialization roundtrip."""
    register_tags()
    data = bytes.fromhex(SEED_HEX)
    seed = Seed.new_opt(data, name="My Seed", note="A note")

    cbor = seed.tagged_cbor()
    decoded = Seed.from_tagged_cbor(cbor)

    assert decoded.data == seed.data
    assert decoded.name == "My Seed"
    assert decoded.note == "A note"


def test_seed_cbor_roundtrip_with_date():
    """Test Seed CBOR roundtrip preserving creation date."""
    register_tags()
    data = bytes.fromhex(SEED_HEX)
    creation_date = Date(datetime(2021, 1, 1, tzinfo=timezone.utc))
    seed = Seed.new_opt(
        data, name="Dated Seed", creation_date=creation_date
    )

    cbor = seed.tagged_cbor()
    decoded = Seed.from_tagged_cbor(cbor)

    assert decoded.data == seed.data
    assert decoded.name == "Dated Seed"
    assert decoded.creation_date is not None


def test_seed_cbor_minimal():
    """Test Seed CBOR roundtrip with no metadata."""
    register_tags()
    data = bytes.fromhex(SEED_HEX)
    seed = Seed.new_opt(data)

    cbor = seed.tagged_cbor()
    decoded = Seed.from_tagged_cbor(cbor)

    assert decoded.data == data
    assert decoded.name == ""
    assert decoded.note == ""
    assert decoded.creation_date is None


def test_seed_ur_roundtrip():
    """Test Seed UR encoding roundtrip."""
    register_tags()
    data = bytes.fromhex(SEED_HEX)
    seed = Seed.new_opt(data)

    ur_string = seed.ur_string()
    decoded = Seed.from_ur_string(ur_string)

    assert decoded.data == data


def test_seed_equality():
    """Test Seed equality."""
    data = bytes.fromhex(SEED_HEX)
    seed1 = Seed.new_opt(data, name="Same")
    seed2 = Seed.new_opt(data, name="Same")
    assert seed1 == seed2


def test_seed_inequality_different_data():
    """Test Seed inequality with different data."""
    seed1 = Seed.generate()
    seed2 = Seed.generate()
    assert seed1 != seed2


def test_seed_inequality_different_name():
    """Test Seed inequality with different names."""
    data = bytes.fromhex(SEED_HEX)
    seed1 = Seed.new_opt(data, name="Name A")
    seed2 = Seed.new_opt(data, name="Name B")
    assert seed1 != seed2


def test_seed_private_key_data():
    """Test that Seed.private_key_data returns the seed bytes."""
    data = bytes.fromhex(SEED_HEX)
    seed = Seed.new_opt(data)
    assert seed.private_key_data() == data


def test_seed_as_private_key_base():
    """Test using a seed as input to PrivateKeyBase."""
    data = bytes.fromhex(SEED_HEX)
    seed = Seed.new_opt(data)
    pkb = PrivateKeyBase.new_with_provider(seed)
    assert pkb.data == data
