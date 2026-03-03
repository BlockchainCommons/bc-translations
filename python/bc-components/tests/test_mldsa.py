"""Tests for ML-DSA post-quantum signatures, translated from Rust mldsa/mod.rs tests."""

from bc_components import MLDSALevel

MESSAGE = (
    b"Ladies and Gentlemen of the class of '99: "
    b"If I could offer you only one tip for the future, sunscreen would be it."
)


def test_mldsa44_signing():
    """Test ML-DSA44 sign/verify roundtrip."""
    private_key, public_key = MLDSALevel.MLDSA44.keypair()
    signature = private_key.sign(MESSAGE)
    assert public_key.verify(signature, MESSAGE)
    # Truncated message should fail verification
    assert not public_key.verify(signature, MESSAGE[: len(MESSAGE) - 1])


def test_mldsa65_signing():
    """Test ML-DSA65 sign/verify roundtrip."""
    private_key, public_key = MLDSALevel.MLDSA65.keypair()
    signature = private_key.sign(MESSAGE)
    assert public_key.verify(signature, MESSAGE)
    assert not public_key.verify(signature, MESSAGE[: len(MESSAGE) - 1])


def test_mldsa87_signing():
    """Test ML-DSA87 sign/verify roundtrip."""
    private_key, public_key = MLDSALevel.MLDSA87.keypair()
    signature = private_key.sign(MESSAGE)
    assert public_key.verify(signature, MESSAGE)
    assert not public_key.verify(signature, MESSAGE[: len(MESSAGE) - 1])
