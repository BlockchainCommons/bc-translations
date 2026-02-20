"""Package-level sanity checks analogous to Rust lib.rs tests."""

from importlib.metadata import version

import bc_crypto


def test_package_version() -> None:
    assert version("bc-crypto") == "0.14.0"


def test_expected_exports_present() -> None:
    assert hasattr(bc_crypto, "sha256")
    assert hasattr(bc_crypto, "aead_chacha20_poly1305_encrypt")
    assert hasattr(bc_crypto, "ecdsa_sign")
