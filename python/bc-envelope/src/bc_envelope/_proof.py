"""Inclusion proof extension for Gordian Envelope.

Allows a holder to prove that specific elements exist within an envelope
without revealing the entire contents, leveraging the Merkle-like digest tree.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from bc_components import Digest

from ._envelope_case import CaseType

if TYPE_CHECKING:
    from ._envelope import Envelope


# ---------------------------------------------------------------------------
# Public API
# ---------------------------------------------------------------------------

def proof_contains_set(
    self: Envelope,
    target: set[Digest],
) -> Envelope | None:
    """Create an inclusion proof for the given set of target digests.

    Returns ``None`` if not all targets can be proven.
    """
    reveal_set = _reveal_set_of_set(self, target)
    if not target.issubset(reveal_set):
        return None
    return self.elide_revealing_set(reveal_set).elide_removing_set(target)


def proof_contains_target(
    self: Envelope,
    target: Envelope,
) -> Envelope | None:
    """Create an inclusion proof for a single target envelope."""
    return proof_contains_set(self, {target.digest()})


def confirm_contains_set(
    self: Envelope,
    target: set[Digest],
    proof: Envelope,
) -> bool:
    """Verify that *proof* demonstrates all *target* elements exist."""
    return self.digest() == proof.digest() and _contains_all(proof, target)


def confirm_contains_target(
    self: Envelope,
    target: Envelope,
    proof: Envelope,
) -> bool:
    """Verify that *proof* demonstrates *target* exists."""
    return confirm_contains_set(self, {target.digest()}, proof)


# ---------------------------------------------------------------------------
# Internal helpers
# ---------------------------------------------------------------------------

def _reveal_set_of_set(
    self: Envelope,
    target: set[Digest],
) -> set[Digest]:
    """Collect all digests needed to reveal the target set."""
    result: set[Digest] = set()
    _reveal_sets(self, target, set(), result)
    return result


def _reveal_sets(
    self: Envelope,
    target: set[Digest],
    current: set[Digest],
    result: set[Digest],
) -> None:
    """Recursively build the set of digests from root to each target element."""
    current = set(current)
    current.add(self.digest())

    if self.digest() in target:
        result.update(current)

    case = self.case
    ct = case.case_type

    if ct == CaseType.NODE:
        _reveal_sets(case.subject, target, current, result)
        for assertion in case.assertions:
            _reveal_sets(assertion, target, current, result)
    elif ct == CaseType.WRAPPED:
        _reveal_sets(case.envelope, target, current, result)
    elif ct == CaseType.ASSERTION:
        a = case.assertion
        _reveal_sets(a.predicate, target, current, result)
        _reveal_sets(a.object, target, current, result)


def _contains_all(self: Envelope, target: set[Digest]) -> bool:
    """Check if the envelope contains all target digests."""
    target = set(target)
    _remove_all_found(self, target)
    return len(target) == 0


def _remove_all_found(self: Envelope, target: set[Digest]) -> None:
    """Recursively remove found digests from the target set."""
    target.discard(self.digest())
    if not target:
        return

    case = self.case
    ct = case.case_type

    if ct == CaseType.NODE:
        _remove_all_found(case.subject, target)
        for assertion in case.assertions:
            _remove_all_found(assertion, target)
    elif ct == CaseType.WRAPPED:
        _remove_all_found(case.envelope, target)
    elif ct == CaseType.ASSERTION:
        a = case.assertion
        _remove_all_found(a.predicate, target)
        _remove_all_found(a.object, target)
