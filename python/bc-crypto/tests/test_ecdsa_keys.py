"""Tests translated from Rust ecdsa_keys.rs."""

from bc_rand import make_fake_random_number_generator

from bc_crypto import (
    ecdsa_compress_public_key,
    ecdsa_decompress_public_key,
    ecdsa_derive_private_key,
    ecdsa_new_private_key_using,
    ecdsa_public_key_from_private_key,
    schnorr_public_key_from_private_key,
)


def test_ecdsa_keys() -> None:
    rng = make_fake_random_number_generator()
    private_key = ecdsa_new_private_key_using(rng)
    assert private_key == bytes.fromhex(
        "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed"
    )

    public_key = ecdsa_public_key_from_private_key(private_key)
    assert public_key == bytes.fromhex(
        "0271b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b"
    )

    decompressed = ecdsa_decompress_public_key(public_key)
    assert decompressed == bytes.fromhex(
        "0471b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b"
        "72325f1f3bb69a44d3f1cb6d1fd488220dd502f49c0b1a46cb91ce3718d8334a"
    )

    compressed = ecdsa_compress_public_key(decompressed)
    assert compressed == public_key

    x_only_public_key = schnorr_public_key_from_private_key(private_key)
    assert x_only_public_key == bytes.fromhex(
        "71b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b"
    )

    derived_private_key = ecdsa_derive_private_key(b"password")
    assert derived_private_key == bytes.fromhex(
        "05cc550daa75058e613e606d9898fedf029e395911c43273a208b7e0e88e271b"
    )
