"""Hex-encoded CBOR bytes output for envelopes."""

from __future__ import annotations

from typing import TYPE_CHECKING

from ._format_context import FormatContextOpt, with_format_context

if TYPE_CHECKING:
    from ._envelope import Envelope


def hex_format(
    envelope: Envelope,
    *,
    annotate: bool = True,
    context: FormatContextOpt | None = None,
) -> str:
    """Return the CBOR hex dump of an envelope.

    Parameters
    ----------
    envelope:
        The envelope to format.
    annotate:
        If ``True`` (default), annotate the hex dump with tag names.
    context:
        Format context to use.  Defaults to global.
    """
    if context is None:
        context = FormatContextOpt.global_()

    cbor = envelope.tagged_cbor()

    if annotate:
        if context.is_none:
            return cbor.hex_annotated()
        if context.is_global:
            return with_format_context(
                lambda ctx: cbor.hex_annotated(tags_store=ctx.tags)
            )
        return cbor.hex_annotated(tags_store=context.context.tags)
    else:
        return cbor.hex()
