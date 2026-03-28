"""Envelope wrapping and unwrapping.

Wrapping an envelope allows treating the entire envelope (including its
assertions) as a single unit, so that new assertions can be added *about*
the envelope as a whole.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from ._envelope_case import CaseType

if TYPE_CHECKING:
    from ._envelope import Envelope


def wrap(self: Envelope) -> Envelope:
    """Return a new envelope that wraps *self*."""
    from ._envelope import Envelope as Env

    return Env.new_wrapped(self)


def try_unwrap(self: Envelope) -> Envelope:
    """Extract the inner envelope from a Wrapped case.

    Raises
    ------
    NotWrapped
        If this envelope's subject is not a wrapped envelope.
    """
    from ._error import NotWrapped

    subject_case = self.subject().case
    if subject_case.case_type == CaseType.WRAPPED:
        return subject_case.envelope
    raise NotWrapped()
