"""Expression class and ExpressionBehavior protocol for Gordian Envelope.

An expression consists of a function (the envelope subject) and zero or
more parameters (as assertions). It represents a computation or function
call that can be evaluated.
"""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, TypeVar, runtime_checkable

from dcbor import CBOR

from ._function import Function
from ._parameter import Parameter

if TYPE_CHECKING:
    from ._envelope import Envelope

T = TypeVar("T")


# ===========================================================================
# ExpressionBehavior protocol
# ===========================================================================

@runtime_checkable
class ExpressionBehavior(Protocol):
    """Protocol for types that behave like expressions."""

    def with_parameter(self, parameter: Parameter | str | int, value: object) -> ExpressionBehavior: ...
    def with_optional_parameter(self, parameter: Parameter | str | int, value: object | None) -> ExpressionBehavior: ...

    @property
    def function(self) -> Function: ...

    @property
    def expression_envelope(self) -> Envelope: ...

    def object_for_parameter(self, param: Parameter | str | int) -> Envelope: ...
    def objects_for_parameter(self, param: Parameter | str | int) -> list[Envelope]: ...
    def extract_object_for_parameter(self, param: Parameter | str | int, type_class: type[T] | None = None) -> object: ...
    def extract_optional_object_for_parameter(self, param: Parameter | str | int, type_class: type[T] | None = None) -> object | None: ...
    def extract_objects_for_parameter(self, param: Parameter | str | int, type_class: type[T] | None = None) -> list[object]: ...


# ===========================================================================
# Expression class
# ===========================================================================

def _to_parameter(p: Parameter | str | int) -> Parameter:
    if isinstance(p, Parameter):
        return p
    if isinstance(p, int):
        return Parameter.new_known(p)
    return Parameter.new_named(p)


def _to_function(f: Function | str | int) -> Function:
    if isinstance(f, Function):
        return f
    if isinstance(f, int):
        return Function.new_known(f)
    return Function.new_named(f)


class Expression:
    """An expression in a Gordian Envelope.

    An expression consists of a function (the subject) and zero or more
    parameters (as assertions on the envelope).
    """

    __slots__ = ("_function", "_envelope")

    def __init__(self, func: Function | str | int) -> None:
        from ._envelope import Envelope as Env

        self._function = _to_function(func)
        self._envelope: Envelope = Env(self._function)

    # --- Composition ---

    def with_parameter(
        self,
        parameter: Parameter | str | int,
        value: object,
    ) -> Expression:
        """Add a parameter assertion."""
        from ._envelope import Envelope as Env

        p = _to_parameter(parameter)
        assertion = Env.new_assertion(p, value)
        self._envelope = self._envelope.add_assertion_envelope(assertion)
        return self

    def with_optional_parameter(
        self,
        parameter: Parameter | str | int,
        value: object | None,
    ) -> Expression:
        """Add a parameter assertion if value is not None."""
        if value is not None:
            return self.with_parameter(parameter, value)
        return self

    # --- Parsing ---

    @property
    def function(self) -> Function:
        return self._function

    @property
    def expression_envelope(self) -> Envelope:
        return self._envelope

    def object_for_parameter(
        self,
        param: Parameter | str | int,
    ) -> Envelope:
        return self._envelope.object_for_predicate(_to_parameter(param))

    def objects_for_parameter(
        self,
        param: Parameter | str | int,
    ) -> list[Envelope]:
        return self._envelope.objects_for_predicate(_to_parameter(param))

    def extract_object_for_parameter(
        self,
        param: Parameter | str | int,
    ) -> object:
        from ._envelope_encodable import _auto_extract_subject
        obj_env = self._envelope.object_for_predicate(_to_parameter(param))
        return _auto_extract_subject(obj_env)

    def extract_optional_object_for_parameter(
        self,
        param: Parameter | str | int,
    ) -> object | None:
        from ._error import NonexistentPredicate
        from ._envelope_encodable import _auto_extract_subject
        try:
            obj_env = self._envelope.object_for_predicate(_to_parameter(param))
            return _auto_extract_subject(obj_env)
        except NonexistentPredicate:
            return None

    def extract_objects_for_parameter(
        self,
        param: Parameter | str | int,
    ) -> list[object]:
        from ._envelope_encodable import _auto_extract_subject
        objs = self._envelope.objects_for_predicate(_to_parameter(param))
        return [_auto_extract_subject(o) for o in objs]

    # --- Conversions ---

    def to_envelope(self) -> Envelope:
        """Convert this expression to an Envelope."""
        return self._envelope

    @staticmethod
    def from_envelope(
        envelope: Envelope,
        expected_function: Function | None = None,
    ) -> Expression:
        """Parse an expression from an envelope."""
        # Extract the Function from the subject's CBOR (tagged with TAG_FUNCTION)
        leaf_cbor = envelope.subject().try_leaf()
        func = Function.from_tagged_cbor(leaf_cbor)
        if expected_function is not None and func != expected_function:
            raise ValueError(
                f"Expected function {expected_function!r}, but found {func!r}"
            )
        expr = Expression.__new__(Expression)
        expr._function = func
        expr._envelope = envelope
        return expr

    # --- Equality ---

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Expression):
            return NotImplemented
        return (
            self._function == other._function
            and self._envelope.digest() == other._envelope.digest()
        )

    def __repr__(self) -> str:
        return f"Expression({self._envelope.format_flat()!r})"

    def __str__(self) -> str:
        return self._envelope.format_flat()
