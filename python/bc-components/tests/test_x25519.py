"""Tests for X25519 key agreement, translated from Rust lib.rs tests."""

from bc_rand import make_fake_random_number_generator

from bc_components import X25519PrivateKey, X25519PublicKey, register_tags


def test_x25519_keys():
    """Test X25519 key generation, UR encoding, and derivation matching Rust vectors."""
    register_tags()
    rng = make_fake_random_number_generator()

    private_key = X25519PrivateKey.generate_using(rng)
    private_key_ur = private_key.ur_string()
    assert private_key_ur == (
        "ur:agreement-private-key/hdcxkbrehkrkrsjztodseytknecfgewmgdmwfsvd"
        "vysbpmghuozsprknfwkpnehydlweynwkrtct"
    )
    assert X25519PrivateKey.from_ur_string(private_key_ur) == private_key

    public_key = private_key.public_key()
    public_key_ur = public_key.ur_string()
    assert public_key_ur == (
        "ur:agreement-public-key/hdcxwnryknkbbymnoxhswmptgydsotwswsghfmrk"
        "ksfxntbzjyrnuornkildchgswtdahehpwkrl"
    )
    assert X25519PublicKey.from_ur_string(public_key_ur) == public_key

    derived_private_key = X25519PrivateKey.derive_from_key_material(b"password")
    assert derived_private_key.ur_string() == (
        "ur:agreement-private-key/hdcxkgcfkomeeyiemywkftvabnrdolmttlrnfhjn"
        "guvaiehlrldmdpemgyjlatdthsnecytdoxat"
    )


def test_agreement():
    """Test that Alice and Bob derive the same shared key via X25519."""
    rng = make_fake_random_number_generator()

    alice_private_key = X25519PrivateKey.generate_using(rng)
    alice_public_key = alice_private_key.public_key()

    bob_private_key = X25519PrivateKey.generate_using(rng)
    bob_public_key = bob_private_key.public_key()

    alice_shared_key = alice_private_key.shared_key_with(bob_public_key)
    bob_shared_key = bob_private_key.shared_key_with(alice_public_key)
    assert alice_shared_key == bob_shared_key
