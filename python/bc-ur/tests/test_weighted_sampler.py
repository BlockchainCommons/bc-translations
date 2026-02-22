"""Tests for weighted random sampling."""

from bc_ur._weighted_sampler import WeightedSampler
from bc_ur._xoshiro256 import Xoshiro256, make_message
from bc_ur._fountain_utils import fragment_length, partition

import pytest


def test_sampler_500():
    weights = [1.0, 2.0, 4.0, 8.0]
    xoshiro = Xoshiro256.from_string("Wolf")
    sampler = WeightedSampler(weights)

    expected_samples = [
        3, 3, 3, 3, 3, 3, 3, 0, 2, 3, 3, 3, 3, 1, 2, 2, 1, 3, 3, 2,
        3, 3, 1, 1, 2, 1, 1, 3, 1, 3, 1, 2, 0, 2, 1, 0, 3, 3, 3, 1,
        3, 3, 3, 3, 1, 3, 2, 3, 2, 2, 3, 3, 3, 3, 2, 3, 3, 0, 3, 3,
        3, 3, 1, 2, 3, 3, 2, 2, 2, 1, 2, 2, 1, 2, 3, 1, 3, 0, 3, 2,
        3, 3, 3, 3, 3, 3, 3, 3, 2, 3, 1, 3, 3, 2, 0, 2, 2, 3, 1, 1,
        2, 3, 2, 3, 3, 3, 3, 2, 3, 3, 3, 3, 3, 2, 3, 1, 2, 1, 1, 3,
        1, 3, 2, 2, 3, 3, 3, 1, 3, 3, 3, 3, 3, 3, 3, 3, 2, 3, 2, 3,
        3, 1, 2, 3, 3, 1, 3, 2, 3, 3, 3, 2, 3, 1, 3, 0, 3, 2, 1, 1,
        3, 1, 3, 2, 3, 3, 3, 3, 2, 0, 3, 3, 1, 3, 0, 2, 1, 3, 3, 1,
        1, 3, 1, 2, 3, 3, 3, 0, 2, 3, 2, 0, 1, 3, 3, 3, 2, 2, 2, 3,
        3, 3, 3, 3, 2, 3, 3, 3, 3, 2, 3, 3, 2, 0, 2, 3, 3, 3, 3, 2,
        1, 1, 1, 2, 1, 3, 3, 3, 2, 2, 3, 3, 1, 2, 3, 0, 3, 2, 3, 3,
        3, 3, 0, 2, 2, 3, 2, 2, 3, 3, 3, 3, 1, 3, 2, 3, 3, 3, 3, 3,
        2, 2, 3, 1, 3, 0, 2, 1, 3, 3, 3, 3, 3, 3, 3, 3, 1, 3, 3, 3,
        3, 2, 2, 2, 3, 1, 1, 3, 2, 2, 0, 3, 2, 1, 2, 1, 0, 3, 3, 3,
        2, 2, 3, 2, 1, 2, 0, 0, 3, 3, 2, 3, 3, 2, 3, 3, 3, 3, 3, 2,
        2, 2, 3, 3, 3, 3, 3, 1, 1, 3, 2, 2, 3, 1, 1, 0, 1, 3, 2, 3,
        3, 2, 3, 3, 2, 3, 3, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 1, 2, 3,
        3, 2, 2, 2, 2, 3, 3, 2, 0, 2, 1, 3, 3, 3, 3, 0, 3, 3, 3, 3,
        2, 2, 3, 1, 3, 3, 3, 2, 3, 3, 3, 2, 3, 3, 3, 3, 2, 3, 2, 1,
        3, 3, 3, 3, 2, 2, 0, 1, 2, 3, 2, 0, 3, 3, 3, 3, 3, 3, 1, 3,
        3, 2, 3, 2, 2, 3, 3, 3, 3, 3, 2, 2, 3, 3, 2, 2, 2, 1, 3, 3,
        3, 3, 1, 2, 3, 2, 3, 3, 2, 3, 2, 3, 3, 3, 2, 3, 1, 2, 3, 2,
        1, 1, 3, 3, 2, 3, 3, 2, 3, 3, 0, 0, 1, 3, 3, 2, 3, 3, 3, 3,
        1, 3, 3, 0, 3, 2, 3, 3, 1, 3, 3, 3, 3, 3, 3, 3, 0, 3, 3, 2,
    ]
    for e in expected_samples:
        assert sampler.next(xoshiro) == e


def test_choose_degree_200():
    message = make_message("Wolf", 1024)
    frag_len = fragment_length(len(message), 100)
    fragments = partition(message, frag_len)

    expected_degrees = [
        11, 3, 6, 5, 2, 1, 2, 11, 1, 3,
        9, 10, 10, 4, 2, 1, 1, 2, 1, 1,
        5, 2, 4, 10, 3, 2, 1, 1, 3, 11,
        2, 6, 2, 9, 9, 2, 6, 7, 2, 5,
        2, 4, 3, 1, 6, 11, 2, 11, 3, 1,
        6, 3, 1, 4, 5, 3, 6, 1, 1, 3,
        1, 2, 2, 1, 4, 5, 1, 1, 9, 1,
        1, 6, 4, 1, 5, 1, 2, 2, 3, 1,
        1, 5, 2, 6, 1, 7, 11, 1, 8, 1,
        5, 1, 1, 2, 2, 6, 4, 10, 1, 2,
        5, 5, 5, 1, 1, 4, 1, 1, 1, 3,
        5, 5, 5, 1, 4, 3, 3, 5, 1, 11,
        3, 2, 8, 1, 2, 1, 1, 4, 5, 2,
        1, 1, 1, 5, 6, 11, 10, 7, 4, 7,
        1, 5, 3, 1, 1, 9, 1, 2, 5, 5,
        2, 2, 3, 10, 1, 3, 2, 3, 3, 1,
        1, 2, 1, 3, 2, 2, 1, 3, 8, 4,
        1, 11, 6, 3, 1, 1, 1, 1, 1, 3,
        1, 2, 1, 10, 1, 1, 8, 2, 7, 1,
        2, 1, 9, 2, 10, 2, 1, 3, 4, 10,
    ]
    for nonce in range(1, 201):
        xoshiro = Xoshiro256.from_string(f"Wolf-{nonce}")
        assert xoshiro.choose_degree(len(fragments)) == expected_degrees[nonce - 1]


def test_negative_weights():
    with pytest.raises(ValueError, match="negative weight"):
        WeightedSampler([2.0, -1.0])


def test_zero_weights():
    with pytest.raises(ValueError, match="weights must sum"):
        WeightedSampler([0.0])
