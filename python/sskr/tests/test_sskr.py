"""Behavioral tests translated from rust/sskr/src/lib.rs."""

from __future__ import annotations

from dataclasses import dataclass
from typing import TypeVar

from bc_rand import RandomNumberGenerator, make_fake_random_number_generator
from bc_rand import rng_next_in_closed_range

from sskr import (
    METADATA_SIZE_BYTES,
    MAX_GROUPS_COUNT,
    MAX_SECRET_LEN,
    MAX_SHARE_COUNT,
    MIN_SECRET_LEN,
    Error,
    GroupSpec,
    Secret,
    Spec,
    sskr_combine,
    sskr_generate,
    sskr_generate_using,
)


class FakeRandomNumberGenerator(RandomNumberGenerator):
    """Deterministic RNG used only in Rust's split tests."""

    def next_u64(self) -> int:
        raise NotImplementedError("next_u64 is not used by these tests")

    def next_u32(self) -> int:
        raise NotImplementedError("next_u32 is not used by these tests")

    def random_data(self, size: int) -> bytes:
        data = bytearray(size)
        self.fill_random_data(data)
        return bytes(data)

    def fill_random_data(self, data: bytearray) -> None:
        value = 0
        for index in range(len(data)):
            data[index] = value
            value = (value + 17) & 0xFF


def test_split_3_5() -> None:
    rng = FakeRandomNumberGenerator()
    secret = Secret.new(bytes.fromhex("0ff784df000c4380a5ed683f7e6e3dcf"))
    group = GroupSpec.new(3, 5)
    spec = Spec.new(1, [group])
    shares = sskr_generate_using(spec, secret, rng)

    flattened_shares = [share for group_shares in shares for share in group_shares]
    assert len(flattened_shares) == 5

    for share in flattened_shares:
        assert len(share) == METADATA_SIZE_BYTES + secret.len()

    recovered_share_indexes = [1, 2, 4]
    recovered_shares = [flattened_shares[index] for index in recovered_share_indexes]
    recovered_secret = sskr_combine(recovered_shares)
    assert recovered_secret == secret


def test_split_2_7() -> None:
    rng = FakeRandomNumberGenerator()
    secret = Secret.new(
        bytes.fromhex(
            "204188bfa6b440a1bdfd6753ff55a824"
            "1e07af5c5be943db917e3efabc184b1a"
        )
    )
    group = GroupSpec.new(2, 7)
    spec = Spec.new(1, [group])
    shares = sskr_generate_using(spec, secret, rng)

    assert len(shares) == 1
    assert len(shares[0]) == 7

    flattened_shares = [share for group_shares in shares for share in group_shares]
    assert len(flattened_shares) == 7

    for share in flattened_shares:
        assert len(share) == METADATA_SIZE_BYTES + secret.len()

    recovered_share_indexes = [3, 4]
    recovered_shares = [flattened_shares[index] for index in recovered_share_indexes]
    recovered_secret = sskr_combine(recovered_shares)
    assert recovered_secret == secret


def test_split_2_3_2_3() -> None:
    rng = FakeRandomNumberGenerator()
    secret = Secret.new(
        bytes.fromhex(
            "204188bfa6b440a1bdfd6753ff55a824"
            "1e07af5c5be943db917e3efabc184b1a"
        )
    )
    group1 = GroupSpec.new(2, 3)
    group2 = GroupSpec.new(2, 3)
    spec = Spec.new(2, [group1, group2])
    shares = sskr_generate_using(spec, secret, rng)

    assert len(shares) == 2
    assert len(shares[0]) == 3
    assert len(shares[1]) == 3

    flattened_shares = [share for group_shares in shares for share in group_shares]
    assert len(flattened_shares) == 6

    for share in flattened_shares:
        assert len(share) == METADATA_SIZE_BYTES + secret.len()

    recovered_share_indexes = [0, 1, 3, 5]
    recovered_shares = [flattened_shares[index] for index in recovered_share_indexes]
    recovered_secret = sskr_combine(recovered_shares)
    assert recovered_secret == secret


T = TypeVar("T")


def fisher_yates_shuffle(items: list[T], rng: RandomNumberGenerator) -> None:
    i = len(items)
    while i > 1:
        i -= 1
        j = rng_next_in_closed_range(rng, 0, i)
        items[i], items[j] = items[j], items[i]


def test_shuffle() -> None:
    rng = make_fake_random_number_generator()
    values = list(range(100))
    fisher_yates_shuffle(values, rng)

    assert len(values) == 100
    assert values == [
        79,
        70,
        40,
        53,
        25,
        30,
        31,
        88,
        10,
        1,
        45,
        54,
        81,
        58,
        55,
        59,
        69,
        78,
        65,
        47,
        75,
        61,
        0,
        72,
        20,
        9,
        80,
        13,
        73,
        11,
        60,
        56,
        19,
        42,
        33,
        12,
        36,
        38,
        6,
        35,
        68,
        77,
        50,
        18,
        97,
        49,
        98,
        85,
        89,
        91,
        15,
        71,
        99,
        67,
        84,
        23,
        64,
        14,
        57,
        48,
        62,
        29,
        28,
        94,
        44,
        8,
        66,
        34,
        43,
        21,
        63,
        16,
        92,
        95,
        27,
        51,
        26,
        86,
        22,
        41,
        93,
        82,
        7,
        87,
        74,
        37,
        46,
        3,
        96,
        24,
        90,
        39,
        32,
        17,
        76,
        4,
        83,
        2,
        52,
        5,
    ]


@dataclass
class RecoverSpec:
    secret: Secret
    spec: Spec
    shares: list[list[bytes]]
    recovered_group_indexes: list[int]
    recovered_member_indexes: list[list[int]]
    recovered_shares: list[bytes]

    @classmethod
    def new(
        cls,
        secret: Secret,
        spec: Spec,
        shares: list[list[bytes]],
        rng: RandomNumberGenerator,
    ) -> "RecoverSpec":
        group_indexes = list(range(spec.group_count()))
        fisher_yates_shuffle(group_indexes, rng)
        recovered_group_indexes = group_indexes[: spec.group_threshold()]

        recovered_member_indexes: list[list[int]] = []
        for group_index in recovered_group_indexes:
            group = spec.groups()[group_index]
            member_indexes = list(range(group.member_count()))
            fisher_yates_shuffle(member_indexes, rng)
            recovered_member_indexes.append(member_indexes[: group.member_threshold()])

        recovered_shares: list[bytes] = []
        for i, recovered_group_index in enumerate(recovered_group_indexes):
            group_shares = shares[recovered_group_index]
            for recovered_member_index in recovered_member_indexes[i]:
                recovered_shares.append(group_shares[recovered_member_index])

        fisher_yates_shuffle(recovered_shares, rng)

        return cls(
            secret=secret,
            spec=spec,
            shares=shares,
            recovered_group_indexes=recovered_group_indexes,
            recovered_member_indexes=recovered_member_indexes,
            recovered_shares=recovered_shares,
        )

    def recover(self) -> None:
        success = False
        try:
            recovered_secret = sskr_combine(self.recovered_shares)
            success = recovered_secret == self.secret
        except Error:
            success = False

        if not success:
            raise AssertionError(
                "recovery failed",
                {
                    "secret": self.secret.data().hex(),
                    "spec": self.spec,
                    "shares": self.shares,
                    "recovered_group_indexes": self.recovered_group_indexes,
                    "recovered_member_indexes": self.recovered_member_indexes,
                    "recovered_shares": [share.hex() for share in self.recovered_shares],
                },
            )


def one_fuzz_test(rng: RandomNumberGenerator) -> None:
    secret_len = rng_next_in_closed_range(rng, MIN_SECRET_LEN, MAX_SECRET_LEN) & ~1
    secret = Secret.new(rng.random_data(secret_len))

    group_count = rng_next_in_closed_range(rng, 1, MAX_GROUPS_COUNT)
    group_specs = []
    for _ in range(group_count):
        member_count = rng_next_in_closed_range(rng, 1, MAX_SHARE_COUNT)
        member_threshold = rng_next_in_closed_range(rng, 1, member_count)
        group_specs.append(GroupSpec.new(member_threshold, member_count))

    group_threshold = rng_next_in_closed_range(rng, 1, group_count)
    spec = Spec.new(group_threshold, group_specs)
    shares = sskr_generate_using(spec, secret, rng)

    recover_spec = RecoverSpec.new(secret, spec, shares, rng)
    recover_spec.recover()


def test_fuzz_test() -> None:
    rng = make_fake_random_number_generator()
    for _ in range(100):
        one_fuzz_test(rng)


def test_example_encode() -> None:
    secret_string = b"my secret belongs to me."
    secret = Secret.new(secret_string)

    group1 = GroupSpec.new(2, 3)
    group2 = GroupSpec.new(3, 5)
    spec = Spec.new(2, [group1, group2])

    shares = sskr_generate(spec, secret)

    assert len(shares) == 2
    assert len(shares[0]) == 3
    assert len(shares[1]) == 5

    recovered_shares = [
        shares[0][0],
        shares[0][2],
        shares[1][0],
        shares[1][1],
        shares[1][4],
    ]

    recovered_secret = sskr_combine(recovered_shares)
    assert recovered_secret == secret


def test_example_encode_3() -> None:
    text = "my secret belongs to me."

    def roundtrip(member_threshold: int, member_count: int) -> Secret:
        secret = Secret.new(text)
        spec = Spec.new(1, [GroupSpec.new(member_threshold, member_count)])
        shares = sskr_generate(spec, secret)
        flattened_shares = [share for group_shares in shares for share in group_shares]
        return sskr_combine(flattened_shares)

    assert roundtrip(2, 3).data().decode("utf-8") == text
    assert roundtrip(1, 1).data().decode("utf-8") == text
    assert roundtrip(1, 3).data().decode("utf-8") == text


def test_example_encode_4() -> None:
    text = "my secret belongs to me."

    secret = Secret.new(text)
    spec = Spec.new(1, [GroupSpec.new(2, 3), GroupSpec.new(2, 3)])
    grouped_shares = sskr_generate(spec, secret)
    flattened_shares = [share for group_shares in grouped_shares for share in group_shares]

    recovered_share_indexes = [0, 1, 3]
    recovered_shares = [flattened_shares[index] for index in recovered_share_indexes]

    recovered_secret = sskr_combine(recovered_shares)
    assert recovered_secret.data().decode("utf-8") == text
