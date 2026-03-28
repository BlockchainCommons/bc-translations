"""Digest-tree operations for Envelope.

Methods for collecting, comparing, and analyzing the digest tree that
underpins every Gordian Envelope.
"""

from __future__ import annotations

import sys
from typing import TYPE_CHECKING

from bc_components import Digest

from ._envelope_case import CaseType
from ._walk import EdgeType, walk

if TYPE_CHECKING:
    from ._envelope import Envelope


def digests(envelope: Envelope, level_limit: int) -> set[Digest]:
    """Return the set of digests in *envelope*, down to *level_limit* levels.

    Level 0 returns an empty set.  ``sys.maxsize`` returns all digests.
    """
    result: set[Digest] = set()

    def visitor(env: Envelope, level: int, _edge: EdgeType, state: None) -> tuple[None, bool]:
        if level < level_limit:
            result.add(env.digest())
            result.add(env.subject().digest())
        return (None, False)

    walk(envelope, False, None, visitor)
    return result


def deep_digests(envelope: Envelope) -> set[Digest]:
    """Return the set of all digests in *envelope* at all levels."""
    return digests(envelope, sys.maxsize)


def shallow_digests(envelope: Envelope) -> set[Digest]:
    """Return the set of digests in *envelope* down to its second level only."""
    return digests(envelope, 2)


def structural_digest(envelope: Envelope) -> Digest:
    """Return a digest that captures both the content and the structural form.

    Unlike the regular ``digest()``, this also encodes whether each part
    is elided, encrypted, or compressed, enabling structural comparisons.
    """
    image = bytearray()

    def visitor(env: Envelope, _level: int, _edge: EdgeType, state: None) -> tuple[None, bool]:
        ct = env.case_type
        if ct == CaseType.ELIDED:
            image.append(1)
        elif ct == CaseType.ENCRYPTED:
            image.append(0)
        elif ct == CaseType.COMPRESSED:
            image.append(2)
        image.extend(env.digest().data)
        return (None, False)

    walk(envelope, False, None, visitor)
    return Digest.from_image(bytes(image))


def is_equivalent_to(envelope: Envelope, other: Envelope) -> bool:
    """``True`` if *envelope* and *other* are semantically equivalent (same digest).

    Complexity: O(1).
    """
    return envelope.digest() == other.digest()


def is_identical_to(envelope: Envelope, other: Envelope) -> bool:
    """``True`` if *envelope* and *other* have both the same content and structure.

    First checks semantic equivalence (O(1)); if equivalent, compares
    structural digests (O(m + n)).
    """
    if not is_equivalent_to(envelope, other):
        return False
    return structural_digest(envelope) == structural_digest(other)
