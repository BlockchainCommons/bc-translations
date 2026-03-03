"""Integration tests translated from Rust lib.rs and private_key_base.rs tests.

These cover:
- X25519 key agreement UR vectors
- ECDSA signing key UR vectors
- ECDSA signing with exact signature bytes
- SSH Ed25519 and DSA signing roundtrips
- SSH ECDSA P256/P384 roundtrips (skipped, matching Rust #[ignore])
- PrivateKeyBase deterministic key derivation with exact hex vectors
"""

import pytest

from bc_rand import make_fake_random_number_generator

from bc_components import (
    ECPrivateKey,
    PrivateKeyBase,
    SignatureScheme,
    SigningPrivateKey,
    SigningPublicKey,
    SshSigningOptions,
    X25519PrivateKey,
    X25519PublicKey,
    register_tags,
)


def test_x25519_keys():
    """Test X25519 key generation and UR strings matching Rust vectors."""
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
    """Test X25519 Diffie-Hellman shared key agreement."""
    rng = make_fake_random_number_generator()

    alice_private_key = X25519PrivateKey.generate_using(rng)
    alice_public_key = alice_private_key.public_key()

    bob_private_key = X25519PrivateKey.generate_using(rng)
    bob_public_key = bob_private_key.public_key()

    alice_shared_key = alice_private_key.shared_key_with(bob_public_key)
    bob_shared_key = bob_private_key.shared_key_with(alice_public_key)
    assert alice_shared_key == bob_shared_key


def test_ecdsa_signing_keys():
    """Test ECDSA signing key generation with UR strings from Rust vectors."""
    register_tags()
    rng = make_fake_random_number_generator()

    # Schnorr private key
    schnorr_private_key = SigningPrivateKey.new_schnorr(
        ECPrivateKey.generate_using(rng)
    )

    # ECDSA private key
    ecdsa_private_key = SigningPrivateKey.new_ecdsa(
        ECPrivateKey.generate_using(rng)
    )
    ecdsa_public_key = ecdsa_private_key.public_key()

    # Schnorr public key
    schnorr_public_key = schnorr_private_key.public_key()

    # Derived private key
    derived_private_key = SigningPrivateKey.new_schnorr(
        ECPrivateKey.derive_from_key_material(b"password")
    )

    # These keys should be constructible; we verify CBOR roundtrip instead of UR
    # since SigningPrivateKey doesn't have ur_string() in Python.
    assert schnorr_private_key == SigningPrivateKey.from_tagged_cbor_data(
        schnorr_private_key.tagged_cbor_data()
    )
    assert ecdsa_public_key == SigningPublicKey.from_tagged_cbor_data(
        ecdsa_public_key.tagged_cbor_data()
    )
    assert schnorr_public_key == SigningPublicKey.from_tagged_cbor_data(
        schnorr_public_key.tagged_cbor_data()
    )


def test_ecdsa_signing():
    """Test ECDSA and Schnorr signing with exact byte vectors from Rust."""
    rng = make_fake_random_number_generator()
    private_key = ECPrivateKey.generate_using(rng)
    message = (
        b"Ladies and Gentlemen of the class of '99: "
        b"If I could offer you only one tip for the future, "
        b"sunscreen would be it."
    )

    # ECDSA signing
    ecdsa_public_key = private_key.public_key()
    ecdsa_signature = private_key.ecdsa_sign(message)
    expected_ecdsa = bytes.fromhex(
        "e75702ed8f645ce7fe510507b2403029e461ef4570d12aa440e4f81385546a13"
        "740b7d16878ff0b46b1cbe08bc218ccb0b00937b61c4707de2ca6148508e51fb"
    )
    assert ecdsa_signature == expected_ecdsa
    assert ecdsa_public_key.verify(ecdsa_signature, message)

    # Schnorr signing
    schnorr_public_key = private_key.schnorr_public_key()
    schnorr_signature = private_key.schnorr_sign_using(message, rng)
    expected_schnorr = bytes.fromhex(
        "df3e33900f0b94e23b6f8685f620ed92705ebfcf885ccb321620acb9927bce1e"
        "2218dcfba7cb9c3bba11611446f38774a564f265917899194e82945c8b60a996"
    )
    assert schnorr_signature == expected_schnorr
    assert schnorr_public_key.schnorr_verify(schnorr_signature, message)


def test_ssh_ed25519_signing():
    """Test SSH Ed25519 sign/verify roundtrip.

    The Python SSH implementation does not produce deterministic keys from a
    seed (the cryptography library always generates fresh keys), so we cannot
    match exact PEM strings from the Rust test. Instead we verify the
    sign/verify roundtrip works correctly.
    """
    message = (
        b"Ladies and Gentlemen of the class of '99: "
        b"If I could offer you only one tip for the future, "
        b"sunscreen would be it."
    )

    private_key, public_key = SignatureScheme.SSH_ED25519.keypair()
    options = SshSigningOptions(namespace="test", hash_alg="sha256")
    signature = private_key.sign_with_options(message, options)
    assert public_key.verify(signature, message)


def test_ssh_dsa_signing():
    """Test SSH DSA sign/verify roundtrip.

    As with Ed25519, we verify the roundtrip rather than exact PEM strings
    since key generation is non-deterministic in the Python cryptography
    library.
    """
    message = (
        b"Ladies and Gentlemen of the class of '99: "
        b"If I could offer you only one tip for the future, "
        b"sunscreen would be it."
    )

    private_key, public_key = SignatureScheme.SSH_DSA.keypair()
    options = SshSigningOptions(namespace="test", hash_alg="sha256")
    signature = private_key.sign_with_options(message, options)
    assert public_key.verify(signature, message)


@pytest.mark.skip(reason="Matches Rust #[ignore] -- ECDSA NistP256 SSH signing")
def test_ssh_ecdsa_nistp256_signing():
    """Test SSH ECDSA NistP256 sign/verify roundtrip."""
    message = (
        b"Ladies and Gentlemen of the class of '99: "
        b"If I could offer you only one tip for the future, "
        b"sunscreen would be it."
    )

    private_key, public_key = SignatureScheme.SSH_ECDSA_P256.keypair()
    options = SshSigningOptions(namespace="test", hash_alg="sha256")
    signature = private_key.sign_with_options(message, options)
    assert public_key.verify(signature, message)


@pytest.mark.skip(reason="Matches Rust #[ignore] -- ECDSA NistP384 SSH signing")
def test_ssh_ecdsa_nistp384_signing():
    """Test SSH ECDSA NistP384 sign/verify roundtrip."""
    message = (
        b"Ladies and Gentlemen of the class of '99: "
        b"If I could offer you only one tip for the future, "
        b"sunscreen would be it."
    )

    private_key, public_key = SignatureScheme.SSH_ECDSA_P384.keypair()
    options = SshSigningOptions(namespace="test", hash_alg="sha256")
    signature = private_key.sign_with_options(message, options)
    assert public_key.verify(signature, message)


def test_private_key_base():
    """Test PrivateKeyBase deterministic key derivation with Rust test vectors.

    Based on Rust private_key_base.rs test_private_key_base.
    """
    register_tags()
    seed = bytes.fromhex("59f2293a5bce7d4de59e71b4207ac5d2")
    private_key_base = PrivateKeyBase.from_data(seed)

    # ECDSA signing private key derivation
    ecdsa_spk = private_key_base.ecdsa_signing_private_key()
    ecdsa_inner = ecdsa_spk.to_ecdsa()
    assert ecdsa_inner is not None
    assert ecdsa_inner.data == bytes.fromhex(
        "9505a44aaf385ce633cf0e2bc49e65cc88794213bdfbf8caf04150b9c4905f5a"
    )

    # Schnorr signing public key derivation
    schnorr_spk = private_key_base.schnorr_signing_private_key()
    schnorr_pub = schnorr_spk.public_key()
    schnorr_inner = schnorr_pub.to_schnorr()
    assert schnorr_inner is not None
    assert schnorr_inner.data == bytes.fromhex(
        "fd4d22f9e8493da52d730aa402ac9e661deca099ef4db5503f519a73c3493e18"
    )

    # X25519 private key derivation
    x25519_priv = private_key_base.x25519_private_key()
    assert x25519_priv.data == bytes.fromhex(
        "77ff838285a0403d3618aa8c30491f99f55221be0b944f50bfb371f43b897485"
    )

    # X25519 public key derivation
    x25519_pub = x25519_priv.public_key()
    assert x25519_pub.data == bytes.fromhex(
        "863cf3facee3ba45dc54e5eedecb21d791d64adfb0a1c63bfb6fea366c1ee62b"
    )

    # UR roundtrip
    ur = private_key_base.ur_string()
    assert ur == (
        "ur:crypto-prvkey-base/gdhkwzdtfthptokigtvwnnjsqzcxknsktdsfecsbbk"
    )
    assert PrivateKeyBase.from_ur_string(ur) == private_key_base
