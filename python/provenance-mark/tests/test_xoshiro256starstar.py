"""Tests for the Xoshiro256** PRNG port."""

from provenance_mark import RngState, Xoshiro256StarStar, sha256


def test_rng_vector() -> None:
    rng = Xoshiro256StarStar.from_data(sha256(b"Hello World"))
    assert rng.next_bytes(32) == bytes.fromhex(
        "b18b446df414ec00714f19cb0f03e45c"
        "d3c3d5d071d2e7483ba8627c65b9926a"
    )


def test_rng_state_round_trip() -> None:
    state = (
        17295166580085024720,
        422929670265678780,
        5577237070365765850,
        7953171132032326923,
    )
    data = Xoshiro256StarStar.from_state(state).to_data()
    assert data == bytes.fromhex(
        "d0e72cf15ec604f0bcab28594b8cde05"
        "dab04ae79053664d0b9dadc201575f6e"
    )

    state2 = Xoshiro256StarStar.from_data(data).to_state()
    assert Xoshiro256StarStar.from_state(state2).to_data() == data


def test_rng_state_json_and_cbor_round_trip() -> None:
    state = RngState.from_bytes(bytes(range(32)))
    assert RngState.from_json(state.to_json()) == state
    assert RngState.from_cbor(state.to_cbor()) == state
