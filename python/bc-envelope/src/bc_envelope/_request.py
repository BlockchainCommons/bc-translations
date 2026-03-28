"""Request class for Gordian Envelope expressions.

A ``Request`` represents a message requesting execution of a function,
paired with a unique ARID identifier and optional metadata.
Tagged with #6.40004 (TAG_REQUEST).
"""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, runtime_checkable

from bc_components import ARID
from bc_tags import TAG_REQUEST
from dcbor import CBOR, Date
import known_values

from ._expression import Expression, _to_parameter
from ._function import Function
from ._parameter import Parameter

if TYPE_CHECKING:
    from ._envelope import Envelope


# ===========================================================================
# RequestBehavior protocol
# ===========================================================================

@runtime_checkable
class RequestBehavior(Protocol):
    """Protocol for types that behave like requests."""

    def with_note(self, note: str) -> RequestBehavior: ...
    def with_date(self, date: Date) -> RequestBehavior: ...

    @property
    def body(self) -> Expression: ...
    @property
    def id(self) -> ARID: ...
    @property
    def note(self) -> str: ...
    @property
    def date(self) -> Date | None: ...


# ===========================================================================
# Request class
# ===========================================================================

class Request:
    """A request message containing an expression body and metadata."""

    __slots__ = ("_body", "_id", "_note", "_date")

    def __init__(
        self,
        function_or_body: Function | str | int | Expression,
        request_id: ARID,
    ) -> None:
        if isinstance(function_or_body, Expression):
            self._body = function_or_body
        else:
            self._body = Expression(function_or_body)
        self._id = request_id
        self._note = ""
        self._date: Date | None = None

    @staticmethod
    def new_with_body(body: Expression, request_id: ARID) -> Request:
        return Request(body, request_id)

    # --- Composition (returns self for chaining) ---

    def with_parameter(
        self,
        parameter: Parameter | str | int,
        value: object,
    ) -> Request:
        self._body = self._body.with_parameter(parameter, value)
        return self

    def with_optional_parameter(
        self,
        parameter: Parameter | str | int,
        value: object | None,
    ) -> Request:
        self._body = self._body.with_optional_parameter(parameter, value)
        return self

    def with_note(self, note: str) -> Request:
        self._note = note
        return self

    def with_date(self, date: Date) -> Request:
        self._date = date
        return self

    # --- ExpressionBehavior delegation ---

    @property
    def function(self) -> Function:
        return self._body.function

    @property
    def expression_envelope(self) -> Envelope:
        return self._body.expression_envelope

    def object_for_parameter(self, param: Parameter | str | int) -> Envelope:
        return self._body.object_for_parameter(param)

    def objects_for_parameter(self, param: Parameter | str | int) -> list[Envelope]:
        return self._body.objects_for_parameter(param)

    def extract_object_for_parameter(self, param: Parameter | str | int) -> object:
        return self._body.extract_object_for_parameter(param)

    def extract_optional_object_for_parameter(self, param: Parameter | str | int) -> object | None:
        return self._body.extract_optional_object_for_parameter(param)

    def extract_objects_for_parameter(self, param: Parameter | str | int) -> list[object]:
        return self._body.extract_objects_for_parameter(param)

    # --- RequestBehavior ---

    @property
    def body(self) -> Expression:
        return self._body

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
        return f"id: {self._id.short_description()}, body: {self._body.expression_envelope.format_flat()}"

    # --- Envelope conversion ---

    def to_envelope(self) -> Envelope:
        """Convert this request to an Envelope."""
        from ._envelope import Envelope as Env

        e = Env(CBOR.from_tagged_value(CBOR.from_int(TAG_REQUEST), self._id.to_cbor()))
        e = e.add_assertion(known_values.BODY, self._body.to_envelope())
        if self._note:
            e = e.add_assertion(known_values.NOTE, self._note)
        if self._date is not None:
            e = e.add_assertion(known_values.DATE, self._date.to_tagged_cbor())
        return e

    @staticmethod
    def from_envelope(
        envelope: Envelope,
        expected_function: Function | None = None,
    ) -> Request:
        """Parse a Request from an envelope."""
        body_envelope = envelope.object_for_predicate(known_values.BODY)
        body = Expression.from_envelope(body_envelope, expected_function)

        subject_cbor = envelope.subject().try_leaf()
        arid_cbor = subject_cbor.try_expected_tagged_value(CBOR.from_int(TAG_REQUEST))
        arid = ARID.from_tagged_cbor(arid_cbor)

        req = Request(body, arid)

        try:
            note: str = envelope.extract_object_for_predicate(known_values.NOTE, str)
            req._note = note
        except Exception:
            pass

        try:
            date: Date = envelope.extract_object_for_predicate(known_values.DATE, Date)
            req._date = date
        except Exception:
            pass

        return req

    # --- Equality ---

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Request):
            return NotImplemented
        return (
            self._body == other._body
            and self._id == other._id
            and self._note == other._note
            and self._date == other._date
        )

    def __repr__(self) -> str:
        return f"Request({self.summary()})"

    def __str__(self) -> str:
        return f"Request({self.summary()})"
