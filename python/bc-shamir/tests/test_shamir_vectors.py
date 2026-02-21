"""Deterministic vector tests for Shamir secret sharing."""

from bc_shamir import recover_secret, split_secret


def test_split_secret_3_5(fake_rng) -> None:
    secret = bytes.fromhex("0ff784df000c4380a5ed683f7e6e3dcf")
    shares = split_secret(3, 5, secret, fake_rng)

    assert len(shares) == 5
    assert shares[0] == bytes.fromhex("00112233445566778899aabbccddeeff")
    assert shares[1] == bytes.fromhex("d43099fe444807c46921a4f33a2a798b")
    assert shares[2] == bytes.fromhex("d9ad4e3bec2e1a7485698823abf05d36")
    assert shares[3] == bytes.fromhex("0d8cf5f6ec337bc764d1866b5d07ca42")
    assert shares[4] == bytes.fromhex("1aa7fe3199bc5092ef3816b074cabdf2")

    recovered_share_indexes = [1, 2, 4]
    recovered_shares = [shares[index] for index in recovered_share_indexes]
    recovered = recover_secret(recovered_share_indexes, recovered_shares)

    assert recovered == secret


def test_split_secret_2_7(fake_rng) -> None:
    secret = bytes.fromhex(
        "204188bfa6b440a1bdfd6753ff55a824"
        "1e07af5c5be943db917e3efabc184b1a"
    )
    shares = split_secret(2, 7, secret, fake_rng)

    assert len(shares) == 7
    assert shares[0] == bytes.fromhex(
        "2dcd14c2252dc8489af3985030e74d5a"
        "48e8eff1478ab86e65b43869bf39d556"
    )
    assert shares[1] == bytes.fromhex(
        "a1dfdd798388aada635b9974472b4fc5"
        "9a32ae520c42c9f6a0af70149b882487"
    )
    assert shares[2] == bytes.fromhex(
        "2ee99daf727c0c7773b89a18de64497f"
        "f7476dacd1015a45f482a893f7402cef"
    )
    assert shares[3] == bytes.fromhex(
        "a2fb5414d4d96ee58a109b3ca9a84be0"
        "259d2c0f9ac92bdd3199e0eed3f1dd3e"
    )
    assert shares[4] == bytes.fromhex(
        "2b851d188b8f5b3653659cc0f7fa4510"
        "2dadf04b708767385cd803862fcb3c3f"
    )
    assert shares[5] == bytes.fromhex(
        "a797d4a32d2a39a4aacd9de48036478f"
        "ff77b1e83b4f16a099c34bfb0b7acdee"
    )
    assert shares[6] == bytes.fromhex(
        "28a19475dcde9f09ba2e9e8819794135"
        "92027216e60c8513cdee937c67b2c586"
    )

    recovered_share_indexes = [3, 4]
    recovered_shares = [shares[index] for index in recovered_share_indexes]
    recovered = recover_secret(recovered_share_indexes, recovered_shares)

    assert recovered == secret
