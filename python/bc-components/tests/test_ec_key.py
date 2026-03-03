"""Tests for EC key operations, translated from Rust lib.rs tests.

Tests ECDSA signing/verification and Schnorr signing/verification
with exact byte-level test vectors from the Rust reference.
"""

from bc_rand import make_fake_random_number_generator

from bc_components import ECPrivateKey

# Exact message from Rust tests
MESSAGE = (
    b"Ladies and Gentlemen of the class of '99: "
    b"If I could offer you only one tip for the future, sunscreen would be it."
)


def test_ecdsa_signing():
    """Test ECDSA sign/verify with exact signature bytes from Rust."""
    rng = make_fake_random_number_generator()
    private_key = ECPrivateKey.generate_using(rng)

    # ECDSA
    ecdsa_public_key = private_key.public_key()
    ecdsa_signature = private_key.ecdsa_sign(MESSAGE)
    expected_ecdsa_sig = bytes.fromhex(
        "e75702ed8f645ce7fe510507b2403029e461ef4570d12aa440e4f81385546a13"
        "740b7d16878ff0b46b1cbe08bc218ccb0b00937b61c4707de2ca6148508e51fb"
    )
    assert ecdsa_signature == expected_ecdsa_sig
    assert ecdsa_public_key.verify(ecdsa_signature, MESSAGE)

    # Schnorr
    schnorr_public_key = private_key.schnorr_public_key()
    schnorr_signature = private_key.schnorr_sign_using(MESSAGE, rng)
    expected_schnorr_sig = bytes.fromhex(
        "df3e33900f0b94e23b6f8685f620ed92705ebfcf885ccb321620acb9927bce1e"
        "2218dcfba7cb9c3bba11611446f38774a564f265917899194e82945c8b60a996"
    )
    assert schnorr_signature == expected_schnorr_sig
    assert schnorr_public_key.schnorr_verify(schnorr_signature, MESSAGE)
