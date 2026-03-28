"""Event class for Gordian Envelope expressions.

An ``Event`` is a standalone notification that does not expect a response.
Tagged with #6.40026 (TAG_EVENT).
"""

from __future__ import annotations

from typing import TYPE_CHECKING, Generic, Protocol, TypeVar, runtime_checkable

from bc_components import ARID
from bc_tags import TAG_EVENT
from dcbor import CBOR, Date
import known_values

if TYPE_CHECKING:
    from ._envelope import Envelope

T = TypeVar("T")


# ===========================================================================
# EventBehavior protocol
# ===========================================================================

@runtime_checkable
class EventBehavior(Protocol[T]):
    """Protocol for types that behave like events."""

    def with_note(self, note: str) -> EventBehavior[T]: ...
    def with_date(self, date: Date) -> EventBehavior[T]: ...

    @property
    def content(self) -> T: ...
    @property
    def id(self) -> ARID: ...
    @property
    def note(self) -> str: ...
    @property
    def date(self) -> Date | None: ...


# ===========================================================================
# Event class
# ===========================================================================

class Event(Generic[T]):
    """A notification event with content, ID, and optional metadata.

    The content type ``T`` must be convertible to/from an ``Envelope``.
    """

    __slots__ = ("_content", "_id", "_note", "_date")

    def __init__(self, content: T, event_id: ARID) -> None:
        self._content: T = content
        self._id = event_id
        self._note = ""
        self._date: Date | None = None

    # --- Composition ---

    def with_note(self, note: str) -> Event[T]:
        self._note = note
        return self

    def with_date(self, date: Date) -> Event[T]:
        self._date = date
        return self

    # --- Parsing ---

    @property
    def content(self) -> T:
        return self._content

    @property
    def id(self) -> ARID:
        return self._id

    @property
    def note(self) -> str:
        return self._note

    @property
    def date(self) -> Date | None:
        return self._date

    # --- Summary ---

    def summary(self) -> str:
        from ._envelope import Envelope as Env

        content_env = Env(self._content)
        return f"id: {self._id.short_description()}, content: {content_env.format_flat()}"

    # --- Envelope conversion ---

    def to_envelope(self) -> Envelope:
        """Convert this event to an Envelope."""
        from ._envelope import Envelope as Env

        e = Env(CBOR.from_tagged_value(CBOR.from_int(TAG_EVENT), self._id.to_cbor()))
        e = e.add_assertion(known_values.CONTENT, Env(self._content))
        if self._note:
            e = e.add_assertion(known_values.NOTE, self._note)
        if self._date is not None:
            e = e.add_assertion(known_values.DATE, self._date.to_tagged_cbor())
        return e

    @staticmethod
    def from_envelope(
        envelope: Envelope,
        content_extractor: type[T] | None = None,
    ) -> Event:
        """Parse an Event from an envelope.

        For simple types (str, int), the content is extracted from the
        subject of the 'content' assertion's object.
        """
        from ._error import GeneralError

        content_env = envelope.object_for_predicate(known_values.CONTENT)
        from ._envelope_encodable import _auto_extract_subject
        content = _auto_extract_subject(content_env)

        subject_cbor = envelope.subject().try_leaf()
        arid_cbor = subject_cbor.try_expected_tagged_value(CBOR.from_int(TAG_EVENT))
        arid = ARID.from_tagged_cbor(arid_cbor)

        event: Event = Event(content, arid)

        try:
            note: str = envelope.extract_object_for_predicate(known_values.NOTE, str)
            event._note = note
        except Exception:
            pass

        try:
            date: Date = envelope.extract_object_for_predicate(known_values.DATE, Date)
            event._date = date
        except Exception:
            pass

        return event

    # --- Equality ---

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Event):
            return NotImplemented
        return (
            self._content == other._content
            and self._id == other._id
            and self._note == other._note
            and self._date == other._date
        )

    def __repr__(self) -> str:
        return f"Event({self.summary()})"

    def __str__(self) -> str:
        return f"Event({self.summary()})"
