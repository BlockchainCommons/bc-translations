"""Shared test fixtures for bc-rand."""

import pytest

from bc_rand import SeededRandomNumberGenerator

TEST_SEED: tuple[int, int, int, int] = (
    17295166580085024720,
    422929670265678780,
    5577237070365765850,
    7953171132032326923,
)


@pytest.fixture
def test_seed() -> tuple[int, int, int, int]:
    """The standard deterministic seed used across all bc-rand test vectors."""
    return TEST_SEED


@pytest.fixture
def fake_rng() -> SeededRandomNumberGenerator:
    """A fresh ``SeededRandomNumberGenerator`` initialised with the standard test seed."""
    return SeededRandomNumberGenerator(TEST_SEED)
