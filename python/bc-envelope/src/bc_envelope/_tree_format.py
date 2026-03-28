"""Tree format for envelope visualization.

Produces a textual tree representation of an envelope showing digests,
edge types, and content summaries for debugging and understanding structure.
"""

from __future__ import annotations

import enum
from dataclasses import dataclass, field
from typing import TYPE_CHECKING

from bc_components import Digest

from ._envelope_summary import envelope_summary
from ._format_context import (
    FormatContext,
    FormatContextOpt,
    with_format_context,
)

if TYPE_CHECKING:
    from ._envelope import Envelope
    from ._walk import EdgeType


# ---------------------------------------------------------------------------
# DigestDisplayFormat
# ---------------------------------------------------------------------------

class DigestDisplayFormat(enum.Enum):
    """How to display envelope digests in tree format."""

    SHORT = "short"
    FULL = "full"
    UR = "ur"


# ---------------------------------------------------------------------------
# TreeFormatOpts
# ---------------------------------------------------------------------------

@dataclass
class TreeFormatOpts:
    """Options controlling tree format output."""

    hide_nodes: bool = False
    highlighting_target: set[Digest] = field(default_factory=set)
    context: FormatContextOpt = field(default_factory=FormatContextOpt.global_)
    digest_display: DigestDisplayFormat = DigestDisplayFormat.SHORT


# ---------------------------------------------------------------------------
# TreeElement (internal)
# ---------------------------------------------------------------------------

class _TreeElement:
    __slots__ = (
        "level",
        "envelope",
        "incoming_edge",
        "show_id",
        "is_highlighted",
    )

    def __init__(
        self,
        level: int,
        envelope: Envelope,
        incoming_edge: EdgeType,
        show_id: bool,
        is_highlighted: bool,
    ) -> None:
        self.level = level
        self.envelope = envelope
        self.incoming_edge = incoming_edge
        self.show_id = show_id
        self.is_highlighted = is_highlighted

    def to_string(
        self,
        context: FormatContext,
        digest_display: DigestDisplayFormat,
    ) -> str:
        parts: list[str] = []
        if self.is_highlighted:
            parts.append("*")
        if self.show_id:
            parts.append(short_id(self.envelope, digest_display))
        label = self.incoming_edge.label
        if label is not None:
            parts.append(label)
        parts.append(envelope_summary(self.envelope, 40, context))
        indent_str = " " * (self.level * 4)
        return indent_str + " ".join(parts)


# ---------------------------------------------------------------------------
# short_id helper
# ---------------------------------------------------------------------------

def short_id(envelope: Envelope, fmt: DigestDisplayFormat) -> str:
    """Return a text representation of the envelope's digest."""
    d = envelope.digest()
    if fmt == DigestDisplayFormat.SHORT:
        return d.short_description()
    if fmt == DigestDisplayFormat.FULL:
        return d.hex()
    # UR
    return d.ur_string()


# ---------------------------------------------------------------------------
# Tree format public API
# ---------------------------------------------------------------------------

def tree_format(
    envelope: Envelope,
    opts: TreeFormatOpts | None = None,
) -> str:
    """Return a tree-formatted string representation of an envelope."""
    if opts is None:
        opts = TreeFormatOpts()

    from ._walk import EdgeType, walk

    elements: list[_TreeElement] = []

    def visitor(
        env: Envelope,
        level: int,
        incoming_edge: EdgeType,
        state: None,
    ) -> tuple[None, bool]:
        elem = _TreeElement(
            level=level,
            envelope=env,
            incoming_edge=incoming_edge,
            show_id=not opts.hide_nodes,
            is_highlighted=env.digest() in opts.highlighting_target,
        )
        elements.append(elem)
        return (None, False)

    walk(envelope, opts.hide_nodes, None, visitor)

    def _format_elements(
        elems: list[_TreeElement], ctx: FormatContext
    ) -> str:
        return "\n".join(
            e.to_string(ctx, opts.digest_display) for e in elems
        )

    if opts.context.is_none:
        return _format_elements(elements, FormatContext())
    if opts.context.is_global:
        return with_format_context(
            lambda ctx: _format_elements(elements, ctx)
        )
    return _format_elements(elements, opts.context.context)
