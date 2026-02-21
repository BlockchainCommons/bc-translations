"""Shared fixtures for Shamir secret sharing tests."""

import pytest

from bc_rand import RandomNumberGenerator, SecureRandomNumberGenerator


class FakeRandomNumberGenerator(RandomNumberGenerator):
    """Deterministic test RNG that produces a repeating byte sequence."""

    def next_u32(self) -> int:  # pragma: no cover - intentionally unimplemented
        raise NotImplementedError

    def next_u64(self) -> int:  # pragma: no cover - intentionally unimplemented
        raise NotImplementedError

    def random_data(self, size: int) -> bytes:
        data = bytearray(size)
        self.fill_random_data(data)
        return bytes(data)

    def fill_random_data(self, data: bytearray) -> None:
        value = 0
        for idx in range(len(data)):
            data[idx] = value
            value = (value + 17) & 0xFF


@pytest.fixture
def fake_rng() -> FakeRandomNumberGenerator:
    return FakeRandomNumberGenerator()


@pytest.fixture
def secure_rng() -> SecureRandomNumberGenerator:
    return SecureRandomNumberGenerator()
