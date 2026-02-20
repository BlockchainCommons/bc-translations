"""Tests translated from Rust public_key_encryption.rs."""

from bc_rand import make_fake_random_number_generator

from bc_crypto import (
    derive_agreement_private_key,
    derive_signing_private_key,
    x25519_new_private_key_using,
    x25519_public_key_from_private_key,
    x25519_shared_key,
)


def test_x25519_keys() -> None:
    rng = make_fake_random_number_generator()
    private_key = x25519_new_private_key_using(rng)
    assert private_key == bytes.fromhex(
        "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed"
    )

    public_key = x25519_public_key_from_private_key(private_key)
    assert public_key == bytes.fromhex(
        "f1bd7a7e118ea461eba95126a3efef543ebb78439d1574bedcbe7d89174cf025"
    )

    derived_x25519_private_key = derive_agreement_private_key(b"password")
    assert derived_x25519_private_key == bytes.fromhex(
        "7b19769132648ff43ae60cbaa696d5be3f6d53e6645db72e2d37516f0729619f"
    )

    derived_signing_private_key = derive_signing_private_key(b"password")
    assert derived_signing_private_key == bytes.fromhex(
        "05cc550daa75058e613e606d9898fedf029e395911c43273a208b7e0e88e271b"
    )


def test_key_agreement() -> None:
    rng = make_fake_random_number_generator()

    alice_private_key = x25519_new_private_key_using(rng)
    alice_public_key = x25519_public_key_from_private_key(alice_private_key)

    bob_private_key = x25519_new_private_key_using(rng)
    bob_public_key = x25519_public_key_from_private_key(bob_private_key)

    alice_shared_key = x25519_shared_key(alice_private_key, bob_public_key)
    bob_shared_key = x25519_shared_key(bob_private_key, alice_public_key)

    assert alice_shared_key == bob_shared_key
    assert alice_shared_key == bytes.fromhex(
        "1e9040d1ff45df4bfca7ef2b4dd2b11101b40d91bf5bf83f8c83d53f0fbb6c23"
    )
