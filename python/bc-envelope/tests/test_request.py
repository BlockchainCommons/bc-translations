"""Request tests for bc-envelope.

Translated from rust/bc-envelope/src/extension/expressions/request.rs
"""

from textwrap import dedent

from bc_components import ARID
from bc_envelope import Envelope, Request, Parameter
from dcbor import Date


def _request_id() -> ARID:
    return ARID.from_data(bytes.fromhex(
        "c66be27dbad7cd095ca77647406d07976dc0f35f0d4d654bb0e96dd227a1e9fc"
    ))


def test_basic_request():
    request = (
        Request("test", _request_id())
        .with_parameter("param1", 42)
        .with_parameter("param2", "hello")
    )

    envelope = request.to_envelope()

    expected = dedent("""\
        request(ARID(c66be27d)) [
            'body': \u00ab"test"\u00bb [
                \u2770"param1"\u2771: 42
                \u2770"param2"\u2771: "hello"
            ]
        ]""")
    assert envelope.format() == expected

    parsed_request = Request.from_envelope(envelope)
    assert parsed_request.extract_object_for_parameter("param1") == 42
    assert parsed_request.extract_object_for_parameter("param2") == "hello"
    assert parsed_request.note == ""
    assert parsed_request.date is None

    assert request == parsed_request


def test_request_with_metadata():
    request_date = Date.from_string("2024-07-04T11:11:11Z")
    request = (
        Request("test", _request_id())
        .with_parameter("param1", 42)
        .with_parameter("param2", "hello")
        .with_note("This is a test")
        .with_date(request_date)
    )

    envelope = request.to_envelope()

    expected = dedent("""\
        request(ARID(c66be27d)) [
            'body': \u00ab"test"\u00bb [
                \u2770"param1"\u2771: 42
                \u2770"param2"\u2771: "hello"
            ]
            'date': 2024-07-04T11:11:11Z
            'note': "This is a test"
        ]""")
    assert envelope.format() == expected

    parsed_request = Request.from_envelope(envelope)
    assert parsed_request.extract_object_for_parameter("param1") == 42
    assert parsed_request.extract_object_for_parameter("param2") == "hello"
    assert parsed_request.note == "This is a test"
    assert parsed_request.date == request_date

    assert request == parsed_request


def test_parameter_format():
    parameter = Parameter.new_named("testParam")
    envelope = Envelope(parameter)
    expected = '\u2770"testParam"\u2771'
    assert envelope.format() == expected
