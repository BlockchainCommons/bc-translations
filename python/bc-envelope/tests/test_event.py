"""Event tests for bc-envelope.

Translated from rust/bc-envelope/src/extension/expressions/event.rs
"""

from textwrap import dedent

from bc_components import ARID
from bc_envelope import Event
from dcbor import Date


def _event_id() -> ARID:
    return ARID.from_data(bytes.fromhex(
        "c66be27dbad7cd095ca77647406d07976dc0f35f0d4d654bb0e96dd227a1e9fc"
    ))


def test_event():
    event_date = Date.from_string("2024-07-04T11:11:11Z")
    event = (
        Event("test", _event_id())
        .with_note("This is a test")
        .with_date(event_date)
    )

    envelope = event.to_envelope()

    expected = dedent("""\
        event(ARID(c66be27d)) [
            'content': "test"
            'date': 2024-07-04T11:11:11Z
            'note': "This is a test"
        ]""")
    assert envelope.format() == expected

    parsed_event = Event.from_envelope(envelope)
    assert parsed_event.content == "test"
    assert parsed_event.note == "This is a test"
    assert parsed_event.date == event_date
