"""Type tests for bc-envelope.

Translated from rust/bc-envelope/tests/type_tests.rs
"""

from bc_envelope import Envelope, extract_subject
from bc_rand import fake_random_data, make_fake_random_number_generator, rng_next_in_closed_range
from dcbor import Date
import known_values

from tests.common.check_encoding import check_encoding


def test_known_value():
    envelope = check_encoding(Envelope(known_values.SIGNED))
    assert envelope.format() == "'signed'"
    assert str(envelope.digest()) == \
        "Digest(d0e39e788c0d8f0343af4588db21d3d51381db454bdf710a9a1891aaa537693c)"


def test_date():
    date = Date.from_string("2018-01-07")
    envelope = check_encoding(Envelope(date.to_tagged_cbor()))
    assert envelope.format() == "2018-01-07"


def test_fake_random_data():
    assert fake_random_data(100) == bytes.fromhex(
        "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed"
        "518684c556472008a67932f7c682125b50cb72e8216f6906358fdaf28d354553"
        "2daee0c5bb5023f50cd8e71ec14901ac746c576c481b893be6656b80622b3a56"
        "4e59b4e2"
    )


def test_fake_numbers():
    rng = make_fake_random_number_generator()
    # Rust uses i32 range (-50..=50), so bits=32
    array = [rng_next_in_closed_range(rng, -50, 50, bits=32) for _ in range(100)]
    expected = [
        -43, -6, 43, -34, -34, 17, -9, 24, 17, -29, -32, -44, 12, -15, -46,
        20, 50, -31, -50, 36, -28, -23, 6, -27, -31, -45, -27, 26, 31, -23,
        24, 19, -32, 43, -18, -17, 6, -13, -1, -27, 4, -48, -4, -44, -6, 17,
        -15, 22, 15, 20, -25, -35, -33, -27, -17, -44, -27, 15, -14, -38,
        -29, -12, 8, 43, 49, -42, -11, -1, -42, -26, -25, 22, -13, 14, 42,
        -29, -38, 17, 2, 5, 5, -31, 27, -3, 39, -12, 42, 46, -17, -25, -46,
        -19, 16, 2, -45, 41, 12, -22, 43, -11,
    ]
    assert array == expected
