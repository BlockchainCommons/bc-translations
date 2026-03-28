"""Envelope hierarchy traversal using a visitor pattern.

Provides two traversal modes:

* **Structure traversal** (``hide_nodes=False``): visits every element
  including Node containers.
* **Tree traversal** (``hide_nodes=True``): skips Node elements and focuses
  on semantic content.
"""

from __future__ import annotations

from enum import Enum, auto
from typing import TYPE_CHECKING, Callable, TypeVar

from ._envelope_case import CaseType

if TYPE_CHECKING:
    from ._envelope import Envelope

S = TypeVar("S")

# Visitor signature:
#   (envelope, level, edge_type, state) -> (new_state, stop)
Visitor = Callable[["Envelope", int, "EdgeType", S], tuple[S, bool]]


class EdgeType(Enum):
    """Type of incoming edge provided to the visitor during traversal."""

    NONE = auto()
    SUBJECT = auto()
    ASSERTION = auto()
    PREDICATE = auto()
    OBJECT = auto()
    CONTENT = auto()

    @property
    def label(self) -> str | None:
        """Short text label used in tree formatting, or ``None``."""
        return _EDGE_LABELS.get(self)


_EDGE_LABELS: dict[EdgeType, str] = {
    EdgeType.SUBJECT: "subj",
    EdgeType.CONTENT: "cont",
    EdgeType.PREDICATE: "pred",
    EdgeType.OBJECT: "obj",
}


# ---------------------------------------------------------------------------
# Public entry points
# ---------------------------------------------------------------------------


def walk(
    envelope: Envelope,
    hide_nodes: bool,
    state: S,
    visitor: Visitor[S],
) -> None:
    """Walk the envelope, calling *visitor* for each element.

    Parameters
    ----------
    envelope:
        The root envelope to walk.
    hide_nodes:
        If ``True``, Node containers are skipped (tree traversal).
        If ``False``, every element is visited (structure traversal).
    state:
        Initial state passed to the root visitor call.
    visitor:
        ``(envelope, level, edge_type, state) -> (state, stop)``
    """
    if hide_nodes:
        _walk_tree_impl(envelope, 0, EdgeType.NONE, state, visitor)
    else:
        _walk_structure_impl(envelope, 0, EdgeType.NONE, state, visitor)


# ---------------------------------------------------------------------------
# Internal recursive helpers
# ---------------------------------------------------------------------------


def _walk_structure_impl(
    envelope: Envelope,
    level: int,
    incoming_edge: EdgeType,
    state: S,
    visitor: Visitor[S],
) -> None:
    state, stop = visitor(envelope, level, incoming_edge, state)
    if stop:
        return

    next_level = level + 1
    ct = envelope.case_type
    case = envelope.case

    if ct == CaseType.NODE:
        _walk_structure_impl(case.subject, next_level, EdgeType.SUBJECT, state, visitor)
        for assertion in case.assertions:
            _walk_structure_impl(assertion, next_level, EdgeType.ASSERTION, state, visitor)
    elif ct == CaseType.WRAPPED:
        _walk_structure_impl(case.envelope, next_level, EdgeType.CONTENT, state, visitor)
    elif ct == CaseType.ASSERTION:
        a = case.assertion
        _walk_structure_impl(a.predicate, next_level, EdgeType.PREDICATE, state, visitor)
        _walk_structure_impl(a.object, next_level, EdgeType.OBJECT, state, visitor)


def _walk_tree_impl(
    envelope: Envelope,
    level: int,
    incoming_edge: EdgeType,
    state: S,
    visitor: Visitor[S],
) -> S:
    subject_level = level
    if not envelope.is_node():
        state, stop = visitor(envelope, level, incoming_edge, state)
        if stop:
            return state
        subject_level = level + 1

    ct = envelope.case_type
    case = envelope.case

    if ct == CaseType.NODE:
        assertion_state = _walk_tree_impl(
            case.subject, subject_level, EdgeType.SUBJECT, state, visitor
        )
        assertion_level = subject_level + 1
        for assertion in case.assertions:
            _walk_tree_impl(
                assertion, assertion_level, EdgeType.ASSERTION, assertion_state, visitor
            )
    elif ct == CaseType.WRAPPED:
        _walk_tree_impl(case.envelope, subject_level, EdgeType.CONTENT, state, visitor)
    elif ct == CaseType.ASSERTION:
        a = case.assertion
        _walk_tree_impl(a.predicate, subject_level, EdgeType.PREDICATE, state, visitor)
        _walk_tree_impl(a.object, subject_level, EdgeType.OBJECT, state, visitor)

    return state
