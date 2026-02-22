"""Tests matching the example tests from the Rust bc-ur crate."""

from dcbor import CBOR

from bc_ur.multipart_decoder import MultipartDecoder
from bc_ur.multipart_encoder import MultipartEncoder
from bc_ur.ur import UR


def test_encode():
    cbor = CBOR.from_value([1, 2, 3])
    ur = UR("test", cbor)
    ur_string = ur.string()
    assert ur_string == "ur:test/lsadaoaxjygonesw"


def test_decode():
    ur_string = "ur:test/lsadaoaxjygonesw"
    ur = UR.from_ur_string(ur_string)
    assert ur.ur_type_str == "test"
    expected_cbor = CBOR.from_value([1, 2, 3])
    assert ur.cbor == expected_cbor


def _run_fountain_test(start_part: int) -> int:
    message = b"The only thing we have to fear is fear itself."
    cbor = CBOR.from_bytes(message)
    ur = UR("bytes", cbor)

    encoder = MultipartEncoder(ur, 10)
    decoder = MultipartDecoder()
    for _ in range(1000):
        part = encoder.next_part()
        if encoder.current_index >= start_part:
            decoder.receive(part)
        if decoder.is_complete:
            break
    received_ur = decoder.message()
    assert received_ur is not None
    assert received_ur == ur
    return encoder.current_index


def test_fountain():
    assert _run_fountain_test(1) == 5
    assert _run_fountain_test(51) == 61
    assert _run_fountain_test(101) == 110
    assert _run_fountain_test(501) == 507
