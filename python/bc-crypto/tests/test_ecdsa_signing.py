"""Tests translated from Rust ecdsa_signing.rs."""

from bc_rand import make_fake_random_number_generator

from bc_crypto import (
    ecdsa_new_private_key_using,
    ecdsa_public_key_from_private_key,
    ecdsa_sign,
    ecdsa_verify,
)

MESSAGE = (
    b"Ladies and Gentlemen of the class of '99: If I could offer you only "
    b"one tip for the future, sunscreen would be it."
)


def test_ecdsa_signing() -> None:
    rng = make_fake_random_number_generator()
    private_key = ecdsa_new_private_key_using(rng)
    public_key = ecdsa_public_key_from_private_key(private_key)
    signature = ecdsa_sign(private_key, MESSAGE)

    assert signature == bytes.fromhex(
        "e75702ed8f645ce7fe510507b2403029e461ef4570d12aa440e4f81385546a13"
        "740b7d16878ff0b46b1cbe08bc218ccb0b00937b61c4707de2ca6148508e51fb"
    )
    assert ecdsa_verify(public_key, signature, MESSAGE)
