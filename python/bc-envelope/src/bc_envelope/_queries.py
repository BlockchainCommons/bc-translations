"""Query methods for extracting typed data from envelopes.

Provides assertion-finding, predicate-matching, and typed extraction
methods that get monkey-patched onto the Envelope class.
"""

from __future__ import annotations

from typing import Any, TypeVar

T = TypeVar("T")


def assertions_with_predicate(self: Any, predicate: Any) -> list[Any]:
    """Return all assertions whose predicate matches *predicate*.

    Matching is by digest equivalence.
    """
    from ._envelope import Envelope, envelope_encodable

    pred_env = envelope_encodable(predicate)
    result: list[Envelope] = []
    for a in self.assertions():
        p = a.as_predicate()
        if p is not None and p.digest() == pred_env.digest():
            result.append(a)
    return result


def assertion_with_predicate(self: Any, predicate: Any) -> Any:
    """Return the single assertion whose predicate matches *predicate*.

    Raises ``NonexistentPredicate`` if no match, ``AmbiguousPredicate``
    if more than one.
    """
    from ._error import AmbiguousPredicate, NonexistentPredicate

    matches = assertions_with_predicate(self, predicate)
    if len(matches) == 0:
        raise NonexistentPredicate()
    if len(matches) > 1:
        raise AmbiguousPredicate()
    return matches[0]


def objects_for_predicate(self: Any, predicate: Any) -> list[Any]:
    """Return the object envelopes of all assertions matching *predicate*."""
    result = []
    for a in assertions_with_predicate(self, predicate):
        obj = a.as_object()
        if obj is not None:
            result.append(obj)
    return result


def object_for_predicate(self: Any, predicate: Any) -> Any:
    """Return the single object envelope for assertions matching *predicate*."""
    from ._error import AmbiguousPredicate, NonexistentPredicate

    objs = objects_for_predicate(self, predicate)
    if len(objs) == 0:
        raise NonexistentPredicate()
    if len(objs) > 1:
        raise AmbiguousPredicate()
    return objs[0]


def extract_object_for_predicate(self: Any, predicate: Any, type_class: type[T]) -> T:
    """Extract the typed object value for the assertion matching *predicate*."""
    from ._envelope_encodable import extract_subject

    obj = object_for_predicate(self, predicate)
    return extract_subject(obj, type_class)


def extract_optional_object_for_predicate(
    self: Any, predicate: Any, type_class: type[T]
) -> T | None:
    """Extract the typed object for *predicate*, or ``None`` if not found."""
    from ._error import NonexistentPredicate
    from ._envelope_encodable import extract_subject

    try:
        obj = object_for_predicate(self, predicate)
        return extract_subject(obj, type_class)
    except NonexistentPredicate:
        return None


def extract_object_for_predicate_with_default(
    self: Any, predicate: Any, type_class: type[T], default: T
) -> T:
    """Extract the typed object for *predicate*, or *default* if not found."""
    result = extract_optional_object_for_predicate(self, predicate, type_class)
    return result if result is not None else default


def extract_objects_for_predicate(
    self: Any, predicate: Any, type_class: type[T]
) -> list[T]:
    """Extract typed objects for all assertions matching *predicate*."""
    from ._envelope_encodable import extract_subject

    objs = objects_for_predicate(self, predicate)
    return [extract_subject(o, type_class) for o in objs]


def elements_count(self: Any) -> int:
    """Count the total number of elements in this envelope recursively."""
    from ._envelope_case import CaseType

    ct = self.case_type
    if ct == CaseType.NODE:
        count = 1 + self.case.subject.elements_count()
        for a in self.case.assertions:
            count += a.elements_count()
        return count
    if ct == CaseType.WRAPPED:
        return 1 + self.case.envelope.elements_count()
    if ct == CaseType.ASSERTION:
        a = self.case.assertion
        return 1 + a.predicate.elements_count() + a.object.elements_count()
    # Leaf, Elided, KnownValue, Encrypted, Compressed
    return 1


def extract_subject_method(self: Any, type_class: type[T]) -> T:
    """Extract the envelope's subject as the given type (method version)."""
    from ._envelope_encodable import extract_subject
    return extract_subject(self, type_class)


def extract_subject_auto(self: Any, type_class: type[T] | None = None) -> Any:
    """Extract the envelope's subject, optionally as a specific type.

    If *type_class* is given, delegates to the typed extraction.
    Otherwise auto-detects the type from the envelope case and CBOR tags.
    """
    if type_class is not None:
        return extract_subject_method(self, type_class)
    from ._envelope_encodable import _auto_extract_subject
    return _auto_extract_subject(self)


def extract_object_method(self: Any, type_class: type[T]) -> T:
    """Extract the envelope's object as the given type."""
    from ._envelope_encodable import extract_subject
    obj = self.try_object()
    return extract_subject(obj, type_class)


def extract_predicate_method(self: Any, type_class: type[T]) -> T:
    """Extract the envelope's predicate as the given type."""
    from ._envelope_encodable import extract_subject
    pred = self.try_predicate()
    return extract_subject(pred, type_class)


def set_position(self: Any, position: int) -> Any:
    """Return a copy of the envelope with a single ``'position'`` assertion."""
    import known_values

    from ._error import InvalidFormat

    position_assertions = assertions_with_predicate(self, known_values.POSITION)
    if len(position_assertions) > 1:
        raise InvalidFormat()

    envelope = (
        self.remove_assertion(position_assertions[0])
        if position_assertions
        else self
    )
    return envelope.add_assertion(known_values.POSITION, position)


def position(self: Any) -> int:
    """Extract the envelope's ``'position'`` assertion value."""
    import known_values

    from ._envelope_encodable import extract_subject

    return extract_subject(object_for_predicate(self, known_values.POSITION), int)


def remove_position(self: Any) -> Any:
    """Return a copy of the envelope without its ``'position'`` assertion."""
    import known_values

    from ._error import InvalidFormat

    position_assertions = assertions_with_predicate(self, known_values.POSITION)
    if len(position_assertions) > 1:
        raise InvalidFormat()
    if position_assertions:
        return self.remove_assertion(position_assertions[0])
    return self
