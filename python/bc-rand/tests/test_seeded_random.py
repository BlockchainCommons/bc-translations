"""Tests for SeededRandomNumberGenerator -- translated from seeded_random.rs."""

from bc_rand import (
    SeededRandomNumberGenerator,
    fake_random_data,
    rng_next_in_range,
    rng_next_with_upper_bound,
)


def test_next_u64(test_seed: tuple[int, int, int, int]) -> None:
    rng = SeededRandomNumberGenerator(test_seed)
    assert rng.next_u64() == 1104683000648959614


def test_next_50(test_seed: tuple[int, int, int, int]) -> None:
    rng = SeededRandomNumberGenerator(test_seed)
    expected = [
        1104683000648959614,
        9817345228149227957,
        546276821344993881,
        15870950426333349563,
        830653509032165567,
        14772257893953840492,
        3512633850838187726,
        6358411077290857510,
        7897285047238174514,
        18314839336815726031,
        4978716052961022367,
        17373022694051233817,
        663115362299242570,
        9811238046242345451,
        8113787839071393872,
        16155047452816275860,
        673245095821315645,
        1610087492396736743,
        1749670338128618977,
        3927771759340679115,
        9610589375631783853,
        5311608497352460372,
        11014490817524419548,
        6320099928172676090,
        12513554919020212402,
        6823504187935853178,
        1215405011954300226,
        8109228150255944821,
        4122548551796094879,
        16544885818373129566,
        5597102191057004591,
        11690994260783567085,
        9374498734039011409,
        18246806104446739078,
        2337407889179712900,
        12608919248151905477,
        7641631838640172886,
        8421574250687361351,
        8697189342072434208,
        8766286633078002696,
        14800090277885439654,
        17865860059234099833,
        4673315107448681522,
        14288183874156623863,
        7587575203648284614,
        9109213819045273474,
        11817665411945280786,
        1745089530919138651,
        5730370365819793488,
        5496865518262805451,
    ]
    actual = [rng.next_u64() for _ in range(len(expected))]
    assert actual == expected


def test_fake_random_data() -> None:
    expected = bytes.fromhex(
        "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed"
        "518684c556472008a67932f7c682125b50cb72e8216f6906358fdaf28d354553"
        "2daee0c5bb5023f50cd8e71ec14901ac746c576c481b893be6656b80622b3a56"
        "4e59b4e2"
    )
    assert fake_random_data(100) == expected


def test_next_with_upper_bound(
    test_seed: tuple[int, int, int, int],
) -> None:
    rng = SeededRandomNumberGenerator(test_seed)
    assert rng_next_with_upper_bound(rng, 10000, bits=32) == 745


def test_in_range(test_seed: tuple[int, int, int, int]) -> None:
    rng = SeededRandomNumberGenerator(test_seed)
    v = [rng_next_in_range(rng, 0, 100, bits=32) for _ in range(100)]
    expected = [
        7, 44, 92, 16, 16, 67, 41, 74, 66, 20, 18, 6, 62, 34, 4, 69, 99,
        19, 0, 85, 22, 27, 56, 23, 19, 5, 23, 76, 80, 27, 74, 69, 17, 92,
        31, 32, 55, 36, 49, 23, 53, 2, 46, 6, 43, 66, 34, 71, 64, 69, 25,
        14, 17, 23, 32, 6, 23, 65, 35, 11, 21, 37, 58, 92, 98, 8, 38, 49,
        7, 24, 24, 71, 37, 63, 91, 21, 11, 66, 52, 54, 55, 19, 76, 46, 89,
        38, 91, 95, 33, 25, 4, 30, 66, 51, 5, 91, 62, 27, 92, 39,
    ]
    assert v == expected


def test_fill_random_data(test_seed: tuple[int, int, int, int]) -> None:
    rng1 = SeededRandomNumberGenerator(test_seed)
    v1 = rng1.random_data(100)
    rng2 = SeededRandomNumberGenerator(test_seed)
    v2 = bytearray(100)
    rng2.fill_random_data(v2)
    assert v1 == bytes(v2)
