"""Package-level metadata tests for bc-shamir."""

from importlib.metadata import version

import bc_shamir


def test_package_version() -> None:
    assert version("bc-shamir") == "0.13.0"


def test_expected_exports_present() -> None:
    assert hasattr(bc_shamir, "split_secret")
    assert hasattr(bc_shamir, "recover_secret")
    assert hasattr(bc_shamir, "MAX_SECRET_LEN")
