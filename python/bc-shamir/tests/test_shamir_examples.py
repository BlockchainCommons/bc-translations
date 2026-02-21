"""Example-based tests for Shamir secret sharing."""

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
        bytes.fromhex("2fa566e8da63065e2706fdd70c5840206928de925dc53081"),
        bytes.fromhex("ddae74c95a63882140d73c54cf1c4a0a6ff32be03040c7ac"),
    ]

    secret = recover_secret(indexes, shares)

    assert secret == b"my secret belongs to me."
