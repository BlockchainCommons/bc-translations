"""Type system extension for Gordian Envelope.

Adds, queries, and verifies types using the ``'isA'`` known-value predicate,
semantically equivalent to RDF ``rdf:type``.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from known_values import KnownValue
import known_values as kv

if TYPE_CHECKING:
    from ._envelope import Envelope


# ---------------------------------------------------------------------------
# Public API
# ---------------------------------------------------------------------------

def add_type(self: Envelope, type_object: object) -> Envelope:
    """Add a ``'isA': type_object`` assertion."""
    return self.add_assertion(kv.IS_A, type_object)


def types(self: Envelope) -> list[Envelope]:
    """Return all type objects from ``'isA'`` assertions."""
    return self.objects_for_predicate(kv.IS_A)


def get_type(self: Envelope) -> Envelope:
    """Return the single type, raising ``AmbiguousType`` if more than one exists."""
    from ._error import AmbiguousType

    t = types(self)
    if len(t) == 1:
        return t[0]
    raise AmbiguousType()


def has_type(self: Envelope, t: object) -> bool:
    """Return whether the envelope has a type matching *t* (by digest)."""
    from ._envelope import Envelope as Env

    e = Env(t) if not isinstance(t, Env) else t
    return any(x.digest() == e.digest() for x in types(self))


def has_type_value(self: Envelope, t: KnownValue) -> bool:
    """Return whether the envelope has a type matching the ``KnownValue`` *t*."""
    from ._envelope import Envelope as Env

    type_envelope = Env(t)
    return any(x.digest() == type_envelope.digest() for x in types(self))


def check_type_value(self: Envelope, t: KnownValue) -> None:
    """Raise ``InvalidType`` unless the envelope has type *t*."""
    from ._error import InvalidType

    if not has_type_value(self, t):
        raise InvalidType()


def check_type(self: Envelope, t: object) -> None:
    """Raise ``InvalidType`` unless the envelope has type *t*."""
    from ._error import InvalidType

    if not has_type(self, t):
        raise InvalidType()
