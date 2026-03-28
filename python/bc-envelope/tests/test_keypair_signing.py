"""Keypair signing tests for Gordian Envelope.

Translated from rust/bc-envelope/tests/keypair_signing_tests.rs
"""

from bc_components import SignatureScheme

from tests.common.check_encoding import check_encoding
from tests.common.test_data import hello_envelope


def _test_scheme(scheme: SignatureScheme, options=None):
    private_key, public_key = scheme.keypair()
    envelope = check_encoding(
        hello_envelope().sign_opt(private_key, options)
    )
    envelope.verify(public_key)


def test_keypair_signing():
    """Test sign/verify for each supported signature scheme."""
    _test_scheme(SignatureScheme.SCHNORR)
    _test_scheme(SignatureScheme.ECDSA)
    _test_scheme(SignatureScheme.ED25519)
    _test_scheme(SignatureScheme.MLDSA44)
    _test_scheme(SignatureScheme.MLDSA65)
    _test_scheme(SignatureScheme.MLDSA87)


def test_keypair_signing_ssh():
    """Test sign/verify for SSH signature schemes."""
    from bc_components import SshSigningOptions

    options = SshSigningOptions(namespace="test", hash_alg="sha512")
    _test_scheme(SignatureScheme.SSH_ED25519, options)
    _test_scheme(SignatureScheme.SSH_DSA, options)
    _test_scheme(SignatureScheme.SSH_ECDSA_P256, options)
    _test_scheme(SignatureScheme.SSH_ECDSA_P384, options)
