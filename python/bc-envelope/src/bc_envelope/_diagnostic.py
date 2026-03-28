"""CBOR diagnostic notation output for envelopes.

Produces RFC-8949 Section 8 diagnostic notation for the underlying CBOR
structure of an envelope, optionally with annotations.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from ._format_context import with_format_context

if TYPE_CHECKING:
    from ._envelope import Envelope


def diagnostic(envelope: Envelope) -> str:
    """Return the CBOR diagnostic notation for this envelope."""
    return envelope.tagged_cbor().diagnostic()


def diagnostic_annotated(envelope: Envelope) -> str:
    """Return the annotated CBOR diagnostic notation for this envelope.

    Uses the global format context for tag annotations.
    """
    return with_format_context(
        lambda context: envelope.tagged_cbor().diagnostic_annotated(
            tags_store=context.tags
        )
    )
