"""Tests for Ed25519 private and public key operations.

Translated from the Rust Ed25519 module and Kotlin PrivateKeyBaseTest.
Covers key creation, derivation, signing, verification, and roundtrips.
"""

from bc_rand import make_fake_random_number_generator

from bc_components import (
    Ed25519PrivateKey,
    Ed25519PublicKey,
    PrivateKeyBase,
    register_tags,
)


SEED_HEX = "59f2293a5bce7d4de59e71b4207ac5d2"
MESSAGE = b"Wolf McNally"


def test_ed25519_key_creation():
    """Test creating a new random Ed25519 key pair."""
    private_key = Ed25519PrivateKey.generate()
    assert len(private_key.data) == 32

    public_key = private_key.public_key()
    assert len(public_key.data) == 32


def test_ed25519_key_creation_using_rng():
    """Test creating an Ed25519 key with a fake RNG for determinism."""
    rng = make_fake_random_number_generator()
    private_key = Ed25519PrivateKey.generate_using(rng)
    assert len(private_key.data) == 32


def test_ed25519_from_data():
    """Test restoring an Ed25519 key from raw data."""
    data = bytes.fromhex(
        "322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36"
    )
    private_key = Ed25519PrivateKey.from_data(data)
    assert private_key.data == data
    assert private_key.hex() == data.hex()


def test_ed25519_from_hex():
    """Test restoring an Ed25519 key from a hex string."""
    hex_str = "322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36"
    private_key = Ed25519PrivateKey.from_hex(hex_str)
    assert private_key.hex() == hex_str


def test_ed25519_derive_from_key_material():
    """Test deterministic derivation from key material."""
    key1 = Ed25519PrivateKey.derive_from_key_material(b"password")
    key2 = Ed25519PrivateKey.derive_from_key_material(b"password")
    assert key1 == key2

    key3 = Ed25519PrivateKey.derive_from_key_material(b"other")
    assert key1 != key3


def test_ed25519_sign_verify():
    """Test Ed25519 sign/verify roundtrip."""
    private_key = Ed25519PrivateKey.from_data(bytes.fromhex(
        "322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36"
    ))
    public_key = private_key.public_key()

    signature = private_key.sign(MESSAGE)
    assert len(signature) == 64

    assert public_key.verify(signature, MESSAGE)
    assert not public_key.verify(signature, b"Wrong message")


def test_ed25519_deterministic_signatures():
    """Test that Ed25519 signatures are deterministic."""
    private_key = Ed25519PrivateKey.from_data(bytes.fromhex(
        "322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36"
    ))

    sig1 = private_key.sign(MESSAGE)
    sig2 = private_key.sign(MESSAGE)
    assert sig1 == sig2


def test_ed25519_public_key_from_data():
    """Test restoring an Ed25519 public key from raw data."""
    private_key = Ed25519PrivateKey.generate()
    public_key = private_key.public_key()
    restored = Ed25519PublicKey.from_data(public_key.data)
    assert public_key == restored


def test_ed25519_via_private_key_base():
    """Test Ed25519 key derivation via PrivateKeyBase."""
    seed_data = bytes.fromhex(SEED_HEX)
    pkb = PrivateKeyBase.from_data(seed_data)
    signing_priv = pkb.ed25519_signing_private_key()

    assert signing_priv is not None
    # Verify the derived signing key can sign and verify
    public_key = signing_priv.public_key()
    signature = signing_priv.sign(MESSAGE)
    assert public_key.verify(signature, MESSAGE)


def test_ed25519_deterministic_derivation():
    """Test that the same seed produces the same Ed25519 key."""
    seed_data = bytes.fromhex(SEED_HEX)
    pkb1 = PrivateKeyBase.from_data(seed_data)
    pkb2 = PrivateKeyBase.from_data(seed_data)

    key1 = pkb1.ed25519_signing_private_key()
    key2 = pkb2.ed25519_signing_private_key()
    assert key1 == key2


def test_ed25519_reference():
    """Test Ed25519 reference/short hex generation."""
    private_key = Ed25519PrivateKey.from_data(bytes.fromhex(
        "322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36"
    ))
    ref_hex_short = private_key.ref_hex_short()
    assert len(ref_hex_short) == 8  # 4 bytes = 8 hex chars


def test_ed25519_invalid_size():
    """Test that wrong-size data raises an error."""
    import pytest
    with pytest.raises(Exception):
        Ed25519PrivateKey.from_data(b"short")
    with pytest.raises(Exception):
        Ed25519PublicKey.from_data(b"short")
