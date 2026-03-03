"""Tests for SealedMessage, translated from Rust encapsulation/sealed_message.rs tests."""

import pytest

from bc_components import (
    EncapsulationScheme,
    SealedMessage,
    register_tags,
)


PLAINTEXT = b"Some mysteries aren't meant to be solved."


def test_sealed_message_x25519():
    """Test SealedMessage with X25519: only the recipient can decrypt."""
    register_tags()
    encapsulation = EncapsulationScheme.X25519
    alice_private_key, _ = encapsulation.keypair()
    bob_private_key, bob_public_key = encapsulation.keypair()
    carol_private_key, _ = encapsulation.keypair()

    sealed_message = SealedMessage.new(PLAINTEXT, bob_public_key)

    # Bob decrypts and reads the message.
    assert sealed_message.decrypt(bob_private_key) == PLAINTEXT

    # No one else can decrypt the message, not even the sender.
    with pytest.raises(Exception):
        sealed_message.decrypt(alice_private_key)
    with pytest.raises(Exception):
        sealed_message.decrypt(carol_private_key)


def test_sealed_message_mlkem512():
    """Test SealedMessage with ML-KEM512: only the recipient can decrypt."""
    register_tags()
    encapsulation = EncapsulationScheme.MLKEM512
    alice_private_key, _ = encapsulation.keypair()
    bob_private_key, bob_public_key = encapsulation.keypair()
    carol_private_key, _ = encapsulation.keypair()

    sealed_message = SealedMessage.new(PLAINTEXT, bob_public_key)

    assert sealed_message.decrypt(bob_private_key) == PLAINTEXT

    with pytest.raises(Exception):
        sealed_message.decrypt(alice_private_key)
    with pytest.raises(Exception):
        sealed_message.decrypt(carol_private_key)


def test_sealed_message_mlkem768():
    """Test SealedMessage with ML-KEM768: only the recipient can decrypt."""
    register_tags()
    encapsulation = EncapsulationScheme.MLKEM768
    _, bob_public_key = encapsulation.keypair()
    bob_private_key2, bob_public_key2 = encapsulation.keypair()

    # Encrypt for bob_public_key, try decrypting with a different private key.
    sealed_message = SealedMessage.new(PLAINTEXT, bob_public_key)
    with pytest.raises(Exception):
        sealed_message.decrypt(bob_private_key2)


def test_sealed_message_cbor_roundtrip():
    """Test SealedMessage CBOR serialization roundtrip."""
    register_tags()
    encapsulation = EncapsulationScheme.X25519
    bob_private_key, bob_public_key = encapsulation.keypair()

    sealed_message = SealedMessage.new(PLAINTEXT, bob_public_key)

    cbor = sealed_message.tagged_cbor()
    decoded = SealedMessage.from_tagged_cbor(cbor)

    assert sealed_message == decoded
    assert decoded.decrypt(bob_private_key) == PLAINTEXT


def test_sealed_message_mlkem_cbor_roundtrip():
    """Test SealedMessage CBOR roundtrip with ML-KEM768."""
    register_tags()
    encapsulation = EncapsulationScheme.MLKEM768
    bob_private_key, bob_public_key = encapsulation.keypair()

    sealed_message = SealedMessage.new(b"Hello, World!", bob_public_key)

    cbor = sealed_message.tagged_cbor()
    decoded = SealedMessage.from_tagged_cbor(cbor)

    assert sealed_message == decoded
    assert decoded.decrypt(bob_private_key) == b"Hello, World!"


def test_sealed_message_encapsulation_scheme():
    """Test that encapsulation_scheme returns the correct scheme."""
    register_tags()
    _, bob_public_key = EncapsulationScheme.X25519.keypair()
    sealed_message = SealedMessage.new(PLAINTEXT, bob_public_key)
    assert sealed_message.encapsulation_scheme() == EncapsulationScheme.X25519


def test_sealed_message_with_aad():
    """Test SealedMessage with additional authenticated data."""
    register_tags()
    encapsulation = EncapsulationScheme.X25519
    bob_private_key, bob_public_key = encapsulation.keypair()
    aad = b"additional authenticated data"

    sealed_message = SealedMessage.new_with_aad(PLAINTEXT, bob_public_key, aad)
    assert sealed_message.decrypt(bob_private_key) == PLAINTEXT
