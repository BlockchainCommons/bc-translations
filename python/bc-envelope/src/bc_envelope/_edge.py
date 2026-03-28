"""Edge extension for Gordian Envelope (BCR-2026-003).

Provides edge assertions representing verifiable claims with ``'isA'``,
``'source'``, and ``'target'`` predicates, plus an ``Edges`` container
and ``Edgeable`` protocol.
"""

from __future__ import annotations

from typing import TYPE_CHECKING, Protocol, runtime_checkable

from bc_components import Digest
from known_values import IS_A, SOURCE, TARGET, EDGE
import known_values

from ._envelope_case import CaseType

if TYPE_CHECKING:
    from ._envelope import Envelope


# ===========================================================================
# Envelope-level methods
# ===========================================================================

def add_edge_envelope(self: Envelope, edge: Envelope) -> Envelope:
    """Add a ``'edge': <edge>`` assertion."""
    return self.add_assertion(known_values.EDGE, edge)


def edges(self: Envelope) -> list[Envelope]:
    """Return all edge object envelopes."""
    return self.objects_for_predicate(known_values.EDGE)


def validate_edge(self: Envelope) -> None:
    """Validate an edge envelope per BCR-2026-003."""
    from ._error import (
        EdgeDuplicateIsA,
        EdgeDuplicateSource,
        EdgeDuplicateTarget,
        EdgeMissingIsA,
        EdgeMissingSource,
        EdgeMissingTarget,
        EdgeUnexpectedAssertion,
    )

    inner = self.subject().try_unwrap() if self.subject().is_wrapped() else self

    seen_is_a = False
    seen_source = False
    seen_target = False

    for assertion in inner.assertions():
        try:
            pred_kv = assertion.try_predicate().try_known_value()
        except Exception:
            raise EdgeUnexpectedAssertion()
        val = pred_kv.value
        if val == IS_A.value:
            if seen_is_a:
                raise EdgeDuplicateIsA()
            seen_is_a = True
        elif val == SOURCE.value:
            if seen_source:
                raise EdgeDuplicateSource()
            seen_source = True
        elif val == TARGET.value:
            if seen_target:
                raise EdgeDuplicateTarget()
            seen_target = True
        else:
            raise EdgeUnexpectedAssertion()

    if not seen_is_a:
        raise EdgeMissingIsA()
    if not seen_source:
        raise EdgeMissingSource()
    if not seen_target:
        raise EdgeMissingTarget()


def edge_is_a(self: Envelope) -> Envelope:
    """Extract the ``'isA'`` object from an edge envelope."""
    inner = self.subject().try_unwrap() if self.subject().is_wrapped() else self
    return inner.object_for_predicate(known_values.IS_A)


def edge_source(self: Envelope) -> Envelope:
    """Extract the ``'source'`` object from an edge envelope."""
    inner = self.subject().try_unwrap() if self.subject().is_wrapped() else self
    return inner.object_for_predicate(known_values.SOURCE)


def edge_target(self: Envelope) -> Envelope:
    """Extract the ``'target'`` object from an edge envelope."""
    inner = self.subject().try_unwrap() if self.subject().is_wrapped() else self
    return inner.object_for_predicate(known_values.TARGET)


def edge_subject(self: Envelope) -> Envelope:
    """Extract the edge's subject identifier."""
    inner = self.subject().try_unwrap() if self.subject().is_wrapped() else self
    return inner.subject()


def edges_matching(
    self: Envelope,
    is_a_filter: Envelope | None = None,
    source_filter: Envelope | None = None,
    target_filter: Envelope | None = None,
    subject_filter: Envelope | None = None,
) -> list[Envelope]:
    """Return edges matching all specified criteria."""
    all_edges = edges(self)
    matching: list[Envelope] = []

    for edge in all_edges:
        if is_a_filter is not None:
            try:
                ea = edge_is_a(edge)
            except Exception:
                continue
            if not ea.is_equivalent_to(is_a_filter):
                continue

        if source_filter is not None:
            try:
                es = edge_source(edge)
            except Exception:
                continue
            if not es.is_equivalent_to(source_filter):
                continue

        if target_filter is not None:
            try:
                et = edge_target(edge)
            except Exception:
                continue
            if not et.is_equivalent_to(target_filter):
                continue

        if subject_filter is not None:
            try:
                esub = edge_subject(edge)
            except Exception:
                continue
            if not esub.is_equivalent_to(subject_filter):
                continue

        matching.append(edge)

    return matching


# ===========================================================================
# Edges container
# ===========================================================================

class Edges:
    """A container for edge envelopes keyed by digest."""

    __slots__ = ("_envelopes",)

    def __init__(self) -> None:
        self._envelopes: dict[Digest, Envelope] = {}

    def add(self, edge_envelope: Envelope) -> None:
        self._envelopes[edge_envelope.digest()] = edge_envelope

    def get(self, digest: Digest) -> Envelope | None:
        return self._envelopes.get(digest)

    def remove(self, digest: Digest) -> Envelope | None:
        return self._envelopes.pop(digest, None)

    def clear(self) -> None:
        self._envelopes.clear()

    @property
    def is_empty(self) -> bool:
        return len(self._envelopes) == 0

    def __len__(self) -> int:
        return len(self._envelopes)

    def __iter__(self):
        return iter(self._envelopes.items())

    def add_to_envelope(self, envelope: Envelope) -> Envelope:
        result = envelope
        for edge_envelope in self._envelopes.values():
            result = result.add_assertion(known_values.EDGE, edge_envelope)
        return result

    @staticmethod
    def from_envelope(envelope: Envelope) -> Edges:
        e = Edges()
        for edge in edges(envelope):
            e._envelopes[edge.digest()] = edge
        return e

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Edges):
            return NotImplemented
        return self._envelopes == other._envelopes

    def __repr__(self) -> str:
        return f"Edges({len(self._envelopes)})"


# ===========================================================================
# Edgeable protocol
# ===========================================================================

@runtime_checkable
class Edgeable(Protocol):
    """Protocol for types that can have edges."""

    def get_edges(self) -> Edges: ...
