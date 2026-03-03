"""Tests for ML-KEM post-quantum key encapsulation, translated from Rust mlkem/mod.rs tests."""

from bc_components import MLKEMLevel


def test_mlkem512():
    """Test ML-KEM512 keypair and encapsulation with exact size checks."""
    private_key, public_key = MLKEMLevel.MLKEM512.keypair()
    shared_secret_1, ciphertext = public_key.encapsulate_new_shared_secret()
    assert private_key.size == 1632
    assert public_key.size == 800
    assert ciphertext.size == 768
    shared_secret_2 = private_key.decapsulate_shared_secret(ciphertext)
    assert shared_secret_1 == shared_secret_2


def test_mlkem768():
    """Test ML-KEM768 keypair and encapsulation with exact size checks."""
    private_key, public_key = MLKEMLevel.MLKEM768.keypair()
    shared_secret_1, ciphertext = public_key.encapsulate_new_shared_secret()
    assert private_key.size == 2400
    assert public_key.size == 1184
    assert ciphertext.size == 1088
    shared_secret_2 = private_key.decapsulate_shared_secret(ciphertext)
    assert shared_secret_1 == shared_secret_2


def test_mlkem1024():
    """Test ML-KEM1024 keypair and encapsulation with exact size checks."""
    private_key, public_key = MLKEMLevel.MLKEM1024.keypair()
    shared_secret_1, ciphertext = public_key.encapsulate_new_shared_secret()
    assert private_key.size == 3168
    assert public_key.size == 1568
    assert ciphertext.size == 1568
    shared_secret_2 = private_key.decapsulate_shared_secret(ciphertext)
    assert shared_secret_1 == shared_secret_2
