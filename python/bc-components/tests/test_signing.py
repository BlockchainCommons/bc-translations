"""Tests for the signing module, translated from Rust signing/mod.rs tests."""

from bc_rand import make_fake_random_number_generator

from bc_components import (
    ECPrivateKey,
    Ed25519PrivateKey,
    MLDSALevel,
    Signature,
    SignatureScheme,
    SigningPrivateKey,
    SchnorrSigningOptions,
    register_tags,
)

# Signing private key data from Rust test (same hex for all three key types)
_KEY_HEX = "322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36"

ECDSA_SIGNING_PRIVATE_KEY = SigningPrivateKey.new_ecdsa(
    ECPrivateKey.from_data(bytes.fromhex(_KEY_HEX))
)
SCHNORR_SIGNING_PRIVATE_KEY = SigningPrivateKey.new_schnorr(
    ECPrivateKey.from_data(bytes.fromhex(_KEY_HEX))
)
ED25519_SIGNING_PRIVATE_KEY = SigningPrivateKey.new_ed25519(
    Ed25519PrivateKey.from_data(bytes.fromhex(_KEY_HEX))
)

MESSAGE = b"Wolf McNally"


def test_schnorr_signing():
    """Test Schnorr sign/verify roundtrip."""
    public_key = SCHNORR_SIGNING_PRIVATE_KEY.public_key()
    signature = SCHNORR_SIGNING_PRIVATE_KEY.sign(MESSAGE)

    assert public_key.verify(signature, MESSAGE)
    assert not public_key.verify(signature, b"Wolf Mcnally")

    # Schnorr signatures are non-deterministic (different each time)
    another_signature = SCHNORR_SIGNING_PRIVATE_KEY.sign(MESSAGE)
    assert signature != another_signature
    assert public_key.verify(another_signature, MESSAGE)


def test_ecdsa_signing():
    """Test ECDSA sign/verify roundtrip."""
    public_key = ECDSA_SIGNING_PRIVATE_KEY.public_key()
    signature = ECDSA_SIGNING_PRIVATE_KEY.sign(MESSAGE)

    assert public_key.verify(signature, MESSAGE)
    assert not public_key.verify(signature, b"Wolf Mcnally")

    # ECDSA signatures are deterministic
    another_signature = ECDSA_SIGNING_PRIVATE_KEY.sign(MESSAGE)
    assert signature == another_signature
    assert public_key.verify(another_signature, MESSAGE)


def test_ed25519_signing():
    """Test Ed25519 sign/verify roundtrip."""
    public_key = ED25519_SIGNING_PRIVATE_KEY.public_key()
    signature = ED25519_SIGNING_PRIVATE_KEY.sign(MESSAGE)

    assert public_key.verify(signature, MESSAGE)
    assert not public_key.verify(signature, b"Wolf Mcnally")

    # Ed25519 signatures are deterministic
    another_signature = ED25519_SIGNING_PRIVATE_KEY.sign(MESSAGE)
    assert signature == another_signature
    assert public_key.verify(another_signature, MESSAGE)


def test_mldsa_signing():
    """Test ML-DSA sign/verify roundtrip (non-deterministic)."""
    priv, pub = MLDSALevel.MLDSA65.keypair()
    signing_priv = SigningPrivateKey.new_mldsa(priv)
    signature = signing_priv.sign(MESSAGE)

    from bc_components import SigningPublicKey
    signing_pub = SigningPublicKey.from_mldsa(pub)
    assert signing_pub.verify(signature, MESSAGE)
    assert not signing_pub.verify(signature, b"Wolf Mcnally")

    another_signature = signing_priv.sign(MESSAGE)
    assert signature != another_signature


def test_schnorr_cbor():
    """Test Schnorr signature CBOR roundtrip with deterministic RNG."""
    register_tags()
    rng = make_fake_random_number_generator()
    options = SchnorrSigningOptions(rng=rng)
    signature = SCHNORR_SIGNING_PRIVATE_KEY.sign_with_options(MESSAGE, options)
    tagged_cbor_data = signature.tagged_cbor_data()
    received_signature = Signature.from_tagged_cbor_data(tagged_cbor_data)
    assert signature == received_signature


def test_ecdsa_cbor():
    """Test ECDSA signature CBOR roundtrip."""
    register_tags()
    signature = ECDSA_SIGNING_PRIVATE_KEY.sign(MESSAGE)
    tagged_cbor_data = signature.tagged_cbor_data()
    received_signature = Signature.from_tagged_cbor_data(tagged_cbor_data)
    assert signature == received_signature


def test_mldsa_cbor():
    """Test ML-DSA signature CBOR roundtrip."""
    from dcbor import CBOR as DCborCBOR
    from bc_components import MLDSASignature, SigningPublicKey

    priv, pub = MLDSALevel.MLDSA65.keypair()
    signing_priv = SigningPrivateKey.new_mldsa(priv)
    signing_pub = SigningPublicKey.from_mldsa(pub)

    signature = signing_priv.sign(MESSAGE)
    assert signing_pub.verify(signature, MESSAGE)

    # Roundtrip through CBOR
    mldsa_sig = signature.to_mldsa()
    assert mldsa_sig is not None
    tagged_cbor_data = mldsa_sig.tagged_cbor_data()
    cbor = DCborCBOR.from_data(tagged_cbor_data)
    received_signature = MLDSASignature.from_tagged_cbor(cbor)
    assert mldsa_sig == received_signature


# --- Keypair tests ---

def _test_keypair_signing(scheme, options=None):
    """Helper: generate keypair, sign, and verify."""
    private_key, public_key = scheme.keypair()
    signature = private_key.sign_with_options(MESSAGE, options)
    assert public_key.verify(signature, MESSAGE)


def test_schnorr_keypair():
    """Test Schnorr keypair generation and signing."""
    _test_keypair_signing(SignatureScheme.SCHNORR)


def test_ecdsa_keypair():
    """Test ECDSA keypair generation and signing."""
    _test_keypair_signing(SignatureScheme.ECDSA)


def test_ed25519_keypair():
    """Test Ed25519 keypair generation and signing."""
    _test_keypair_signing(SignatureScheme.ED25519)


def test_mldsa44_keypair():
    """Test ML-DSA44 keypair generation and signing."""
    _test_keypair_signing(SignatureScheme.MLDSA44)


def test_mldsa65_keypair():
    """Test ML-DSA65 keypair generation and signing."""
    _test_keypair_signing(SignatureScheme.MLDSA65)


def test_mldsa87_keypair():
    """Test ML-DSA87 keypair generation and signing."""
    _test_keypair_signing(SignatureScheme.MLDSA87)
