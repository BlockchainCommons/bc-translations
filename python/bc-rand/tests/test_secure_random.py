"""Tests for SecureRandomNumberGenerator — translated from secure_random.rs."""

from bc_rand import random_data


def test_random_data():
    data1 = random_data(32)
    data2 = random_data(32)
    data3 = random_data(32)
    assert len(data1) == 32
    assert data1 != data2
    assert data1 != data3
