"""Example-based tests translated from Rust `shamir.rs`."""

from bc_shamir import recover_secret, split_secret


def test_example_split(secure_rng) -> None:
    threshold = 2
    share_count = 3
    secret = b"my secret belongs to me."

    shares = split_secret(threshold, share_count, secret, secure_rng)

    assert len(shares) == share_count


def test_example_recover() -> None:
    indexes = [0, 2]
    shares = [
        bytes(
            [
                47,
                165,
                102,
                232,
                218,
                99,
                6,
                94,
                39,
                6,
                253,
                215,
                12,
                88,
                64,
                32,
                105,
                40,
                222,
                146,
                93,
                197,
                48,
                129,
            ]
        ),
        bytes(
            [
                221,
                174,
                116,
                201,
                90,
                99,
                136,
                33,
                64,
                215,
                60,
                84,
                207,
                28,
                74,
                10,
                111,
                243,
                43,
                224,
                48,
                64,
                199,
                172,
            ]
        ),
    ]

    secret = recover_secret(indexes, shares)

    assert secret == b"my secret belongs to me."
