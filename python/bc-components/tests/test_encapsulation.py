"""Tests for encapsulation and sealed messages.

Translated from Rust encapsulation/mod.rs and encapsulation/sealed_message.rs tests.
"""

from bc_components import EncapsulationScheme, SealedMessage


def _test_encapsulation(scheme):
    """Helper: test encapsulation roundtrip for the given scheme."""
    private_key, public_key = scheme.keypair()
    secret1, ciphertext = public_key.encapsulate_new_shared_secret()
    secret2 = private_key.decapsulate_shared_secret(ciphertext)
    assert secret1 == secret2


def test_x25519():
    """Test X25519 encapsulation roundtrip."""
    _test_encapsulation(EncapsulationScheme.default())


def test_mlkem512():
    """Test ML-KEM512 encapsulation roundtrip."""
    _test_encapsulation(EncapsulationScheme.MLKEM512)


def test_mlkem768():
    """Test ML-KEM768 encapsulation roundtrip."""
    _test_encapsulation(EncapsulationScheme.MLKEM768)


def test_mlkem1024():
    """Test ML-KEM1024 encapsulation roundtrip."""
    _test_encapsulation(EncapsulationScheme.MLKEM1024)


def test_sealed_message_x25519():
    """Test SealedMessage with X25519: only the recipient can decrypt."""
    plaintext = b"Some mysteries aren't meant to be solved."

    encapsulation = EncapsulationScheme.X25519
    alice_private_key, _ = encapsulation.keypair()
    bob_private_key, bob_public_key = encapsulation.keypair()
    carol_private_key, _ = encapsulation.keypair()

    # Alice constructs a message for Bob's eyes only.
    sealed_message = SealedMessage.new(plaintext, bob_public_key)

    # Bob decrypts and reads the message.
    assert sealed_message.decrypt(bob_private_key) == plaintext

    # No one else can decrypt the message, not even the sender.
    try:
        sealed_message.decrypt(alice_private_key)
        assert False, "Alice should not be able to decrypt"
    except Exception:
        pass

    try:
        sealed_message.decrypt(carol_private_key)
        assert False, "Carol should not be able to decrypt"
    except Exception:
        pass


def test_sealed_message_mlkem512():
    """Test SealedMessage with ML-KEM512: only the recipient can decrypt."""
    plaintext = b"Some mysteries aren't meant to be solved."

    encapsulation = EncapsulationScheme.MLKEM512
    alice_private_key, _ = encapsulation.keypair()
    bob_private_key, bob_public_key = encapsulation.keypair()
    carol_private_key, _ = encapsulation.keypair()

    # Alice constructs a message for Bob's eyes only.
    sealed_message = SealedMessage.new(plaintext, bob_public_key)

    # Bob decrypts and reads the message.
    assert sealed_message.decrypt(bob_private_key) == plaintext

    # No one else can decrypt the message, not even the sender.
    try:
        sealed_message.decrypt(alice_private_key)
        assert False, "Alice should not be able to decrypt"
    except Exception:
        pass

    try:
        sealed_message.decrypt(carol_private_key)
        assert False, "Carol should not be able to decrypt"
    except Exception:
        pass
