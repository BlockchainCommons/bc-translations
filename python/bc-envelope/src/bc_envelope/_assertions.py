"""Assertion management for Envelope.

Provides methods for adding, removing, and replacing assertions on
envelopes.  All methods return *new* envelopes — the original is never
mutated.
"""

from __future__ import annotations

from typing import TYPE_CHECKING, Any

from ._envelope_case import CaseType

if TYPE_CHECKING:
    from ._envelope import Envelope


# ---------------------------------------------------------------------------
# Adding assertions
# ---------------------------------------------------------------------------


def add_assertion(
    self: Envelope,
    predicate: Any,
    obj: Any,
) -> Envelope:
    """Return a new envelope with an assertion for *predicate* / *obj*.

    Both *predicate* and *obj* are converted to envelopes via
    ``Envelope.new_assertion()``.
    """
    from ._envelope import Envelope as Env

    assertion = Env.new_assertion(predicate, obj)
    return add_optional_assertion_envelope(self, assertion)


def add_assertion_envelope(
    self: Envelope,
    assertion_envelope: Any,
) -> Envelope:
    """Return a new envelope with *assertion_envelope* added.

    *assertion_envelope* is converted to an envelope if it is not one
    already.  It must be a valid assertion or obscured envelope.

    Raises
    ------
    InvalidFormat
        If the envelope is not a valid assertion.
    """
    from ._envelope import Envelope as Env, envelope_encodable

    if not isinstance(assertion_envelope, Env):
        assertion_envelope = envelope_encodable(assertion_envelope)
    return _add_optional_assertion_envelope_impl(self, assertion_envelope)


def add_assertion_envelopes(
    self: Envelope,
    assertions: list[Envelope],
) -> Envelope:
    """Return a new envelope with all *assertions* added.

    Raises
    ------
    InvalidFormat
        If any envelope is not a valid assertion.
    """
    result = self
    for a in assertions:
        result = add_assertion_envelope(result, a)
    return result


def add_optional_assertion_envelope(
    self: Envelope,
    assertion: Envelope | None,
) -> Envelope:
    """Add *assertion* if it is not ``None``, otherwise return self.

    Duplicate assertions (same digest) are silently ignored.

    Raises
    ------
    InvalidFormat
        If the envelope is not a valid assertion.
    """
    return _add_optional_assertion_envelope_impl(self, assertion)


def add_optional_assertion(
    self: Envelope,
    predicate: Any,
    obj: Any | None,
) -> Envelope:
    """Add an assertion only if *obj* is not ``None``."""
    from ._envelope import Envelope as Env

    if obj is None:
        return self
    assertion = Env.new_assertion(predicate, obj)
    return add_assertion_envelope(self, assertion)


def add_nonempty_string_assertion(
    self: Envelope,
    predicate: Any,
    string: str,
) -> Envelope:
    """Add an assertion only if *string* is non-empty."""
    if not string:
        return self
    return add_assertion(self, predicate, string)


def add_assertions(
    self: Envelope,
    assertions: list[Envelope],
) -> Envelope:
    """Add all *assertions* (pre-constructed assertion envelopes)."""
    result = self
    for a in assertions:
        result = add_assertion_envelope(result, a)
    return result


# ---------------------------------------------------------------------------
# Conditional assertions
# ---------------------------------------------------------------------------


def add_assertion_if(
    self: Envelope,
    condition: bool,
    predicate: Any,
    obj: Any,
) -> Envelope:
    """Add an assertion only if *condition* is ``True``."""
    if condition:
        return add_assertion(self, predicate, obj)
    return self


def add_assertion_envelope_if(
    self: Envelope,
    condition: bool,
    assertion_envelope: Envelope,
) -> Envelope:
    """Add *assertion_envelope* only if *condition* is ``True``.

    Raises
    ------
    InvalidFormat
        If the envelope is not a valid assertion and *condition* is ``True``.
    """
    if condition:
        return add_assertion_envelope(self, assertion_envelope)
    return self


# ---------------------------------------------------------------------------
# Removing / replacing assertions
# ---------------------------------------------------------------------------


def remove_assertion(self: Envelope, target: Envelope) -> Envelope:
    """Return a new envelope with *target* assertion removed.

    If the assertion is not found the same envelope is returned.
    If removing the last assertion, returns just the subject.
    """
    from ._envelope import Envelope as Env

    current_assertions = self.assertions()
    target_digest = target.digest()
    index = None
    for i, a in enumerate(current_assertions):
        if a.digest() == target_digest:
            index = i
            break

    if index is None:
        return self

    remaining = current_assertions[:index] + current_assertions[index + 1 :]
    if not remaining:
        return self.subject()
    return Env.new_with_unchecked_assertions(self.subject(), remaining)


def replace_assertion(
    self: Envelope,
    old: Envelope,
    new: Envelope,
) -> Envelope:
    """Replace *old* assertion with *new*.

    If *old* is not found, *new* is simply added.

    Raises
    ------
    InvalidFormat
        If *new* is not a valid assertion.
    """
    return add_assertion_envelope(remove_assertion(self, old), new)


def replace_subject(self: Envelope, new_subject: Envelope) -> Envelope:
    """Return a new envelope with subject replaced, keeping assertions."""
    result = new_subject
    for a in self.assertions():
        result = add_assertion_envelope(result, a)
    return result


# ---------------------------------------------------------------------------
# Internal helper
# ---------------------------------------------------------------------------


def _add_optional_assertion_envelope_impl(
    self: Envelope,
    assertion: Envelope | None,
) -> Envelope:
    from ._envelope import Envelope as Env
    from ._error import InvalidFormat

    if assertion is None:
        return self

    if not assertion.is_subject_assertion() and not assertion.is_subject_obscured():
        raise InvalidFormat()

    case = self.case
    if case.case_type == CaseType.NODE:
        # Check for duplicate
        for a in case.assertions:
            if a.digest() == assertion.digest():
                return self
        new_assertions = list(case.assertions) + [assertion]
        return Env.new_with_unchecked_assertions(case.subject, new_assertions)

    # Not a node — create a new node with self as subject
    return Env.new_with_unchecked_assertions(self.subject(), [assertion])
