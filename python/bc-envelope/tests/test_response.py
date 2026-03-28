"""Response tests for bc-envelope.

Translated from rust/bc-envelope/src/extension/expressions/response.rs
"""

from textwrap import dedent

from bc_components import ARID
from bc_envelope import Response
from known_values import KnownValue
import known_values


def _request_id() -> ARID:
    return ARID.from_data(bytes.fromhex(
        "c66be27dbad7cd095ca77647406d07976dc0f35f0d4d654bb0e96dd227a1e9fc"
    ))


def test_success_ok():
    response = Response.new_success(_request_id())
    envelope = response.to_envelope()

    expected = dedent("""\
        response(ARID(c66be27d)) [
            'result': 'OK'
        ]""")
    assert envelope.format() == expected

    parsed_response = Response.from_envelope(envelope)
    assert parsed_response.is_success
    assert parsed_response.expect_id() == _request_id()
    result_kv = parsed_response.extract_result()
    assert result_kv == known_values.OK_VALUE
    assert response == parsed_response


def test_success_result():
    response = Response.new_success(_request_id()).with_result("It works!")
    envelope = response.to_envelope()

    expected = dedent("""\
        response(ARID(c66be27d)) [
            'result': "It works!"
        ]""")
    assert envelope.format() == expected

    parsed_response = Response.from_envelope(envelope)
    assert parsed_response.is_success
    assert parsed_response.expect_id() == _request_id()
    assert parsed_response.extract_result() == "It works!"
    assert response == parsed_response


def test_early_failure():
    response = Response.new_early_failure()
    envelope = response.to_envelope()

    expected = dedent("""\
        response('Unknown') [
            'error': 'Unknown'
        ]""")
    assert envelope.format() == expected

    parsed_response = Response.from_envelope(envelope)
    assert parsed_response.is_failure
    assert parsed_response.id is None
    assert parsed_response.extract_error() == known_values.UNKNOWN_VALUE
    assert response == parsed_response


def test_failure():
    response = Response.new_failure(_request_id()).with_error("It doesn't work!")
    envelope = response.to_envelope()

    expected = dedent("""\
        response(ARID(c66be27d)) [
            'error': "It doesn't work!"
        ]""")
    assert envelope.format() == expected

    parsed_response = Response.from_envelope(envelope)
    assert parsed_response.is_failure
    assert parsed_response.id == _request_id()
    assert parsed_response.extract_error() == "It doesn't work!"
    assert response == parsed_response
