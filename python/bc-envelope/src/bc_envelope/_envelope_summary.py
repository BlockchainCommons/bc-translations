"""Short text summaries per envelope case.

Generates brief human-readable description of envelope content for use
in tree formatting and other concise displays.
"""

from __future__ import annotations

from typing import TYPE_CHECKING

from dcbor import CBOR, CBORCase

from ._envelope_case import CaseType
from ._format_context import (
    FormatContext,
    FormatContextOpt,
    flanked_by,
    with_format_context,
)

if TYPE_CHECKING:
    from ._envelope import Envelope


def cbor_envelope_summary(
    cbor: CBOR,
    max_length: int,
    context: FormatContextOpt,
) -> str:
    """Return a short summary string for a CBOR value in envelope context."""
    case = cbor.case

    if case == CBORCase.UNSIGNED:
        return str(cbor.value)
    if case == CBORCase.NEGATIVE:
        return str(-1 - cbor.value)
    if case == CBORCase.BYTE_STRING:
        return f"Bytes({len(cbor.value.data)})"
    if case == CBORCase.TEXT:
        s: str = cbor.value
        if len(s) > max_length:
            s = s[:max_length] + "\u2026"  # ellipsis
        s = s.replace("\n", "\\n")
        return flanked_by(s, '"', '"')
    if case == CBORCase.SIMPLE:
        return str(cbor.value)

    # For ARRAY, MAP, TAGGED: use CBOR summary/diagnostic
    if context.is_none:
        return cbor.summary()
    if context.is_global:
        return with_format_context(
            lambda ctx: cbor.summary(tags_store=ctx.tags)
        )
    return cbor.summary(tags_store=context.context.tags)


def envelope_summary(
    envelope: Envelope,
    max_length: int,
    context: FormatContext,
) -> str:
    """Return a short summary string for an envelope."""
    from known_values import KnownValuesStore

    ct = envelope.case_type
    case = envelope.case

    if ct == CaseType.NODE:
        return "NODE"
    if ct == CaseType.LEAF:
        return cbor_envelope_summary(
            case.cbor, max_length, FormatContextOpt.custom(context)
        )
    if ct == CaseType.WRAPPED:
        return "WRAPPED"
    if ct == CaseType.ASSERTION:
        return "ASSERTION"
    if ct == CaseType.ELIDED:
        return "ELIDED"
    if ct == CaseType.KNOWN_VALUE:
        kv = KnownValuesStore.known_value_for_raw_value(
            case.value.value, context.known_values
        )
        return flanked_by(str(kv), "'", "'")
    if ct == CaseType.ENCRYPTED:
        return "ENCRYPTED"
    if ct == CaseType.COMPRESSED:
        return "COMPRESSED"

    return "<unknown>"  # pragma: no cover
