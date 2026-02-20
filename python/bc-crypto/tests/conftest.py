"""Shared fixtures for bc-crypto tests."""

import pytest

from bc_rand import make_fake_random_number_generator


@pytest.fixture
def fake_rng():
    return make_fake_random_number_generator()
