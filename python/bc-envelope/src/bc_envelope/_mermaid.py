"""Mermaid diagram format for envelopes.

Produces Mermaid flowchart syntax for visualizing envelope structure as
directed graphs.
"""

from __future__ import annotations

import enum
from dataclasses import dataclass, field
from typing import TYPE_CHECKING

from bc_components import Digest

from ._envelope_case import CaseType
from ._envelope_summary import envelope_summary
from ._format_context import FormatContextOpt, with_format_context

if TYPE_CHECKING:
    from ._envelope import Envelope
    from ._walk import EdgeType


# ---------------------------------------------------------------------------
# MermaidOrientation
# ---------------------------------------------------------------------------

class MermaidOrientation(enum.Enum):
    LEFT_TO_RIGHT = "LR"
    TOP_TO_BOTTOM = "TB"
    RIGHT_TO_LEFT = "RL"
    BOTTOM_TO_TOP = "BT"


# ---------------------------------------------------------------------------
# MermaidTheme
# ---------------------------------------------------------------------------

class MermaidTheme(enum.Enum):
    DEFAULT = "default"
    NEUTRAL = "neutral"
    DARK = "dark"
    FOREST = "forest"
    BASE = "base"


# ---------------------------------------------------------------------------
# MermaidFormatOpts
# ---------------------------------------------------------------------------

@dataclass
class MermaidFormatOpts:
    """Options controlling Mermaid diagram output."""

    hide_nodes: bool = False
    monochrome: bool = False
    theme: MermaidTheme = MermaidTheme.DEFAULT
    orientation: MermaidOrientation = MermaidOrientation.LEFT_TO_RIGHT
    highlighting_target: set[Digest] = field(default_factory=set)
    context: FormatContextOpt = field(default_factory=FormatContextOpt.global_)


# ---------------------------------------------------------------------------
# Color helpers
# ---------------------------------------------------------------------------

_NODE_COLORS: dict[CaseType, str] = {
    CaseType.NODE: "red",
    CaseType.LEAF: "teal",
    CaseType.WRAPPED: "blue",
    CaseType.ASSERTION: "green",
    CaseType.ELIDED: "gray",
    CaseType.KNOWN_VALUE: "goldenrod",
    CaseType.ENCRYPTED: "coral",
    CaseType.COMPRESSED: "purple",
}


def _node_color(envelope: Envelope) -> str:
    return _NODE_COLORS.get(envelope.case_type, "gray")


_MERMAID_FRAMES: dict[CaseType, tuple[str, str]] = {
    CaseType.NODE: ("((", "))"),
    CaseType.LEAF: ("[", "]"),
    CaseType.WRAPPED: ("[/", "\\]"),
    CaseType.ASSERTION: ("([", "])"),
    CaseType.ELIDED: ("{{", "}}"),
    CaseType.KNOWN_VALUE: ("[/", "/]"),
    CaseType.ENCRYPTED: (">", "]"),
    CaseType.COMPRESSED: ("[[", "]]"),
}


def _mermaid_frame(envelope: Envelope) -> tuple[str, str]:
    return _MERMAID_FRAMES.get(envelope.case_type, ("[", "]"))


def _link_stroke_color(edge_type: EdgeType) -> str | None:
    from ._walk import EdgeType as ET

    _COLORS: dict[ET, str] = {
        ET.SUBJECT: "red",
        ET.CONTENT: "blue",
        ET.PREDICATE: "cyan",
        ET.OBJECT: "magenta",
    }
    return _COLORS.get(edge_type)


# ---------------------------------------------------------------------------
# MermaidElement (internal)
# ---------------------------------------------------------------------------

class _MermaidElement:
    __slots__ = (
        "id",
        "level",
        "envelope",
        "incoming_edge",
        "show_id",
        "is_highlighted",
        "parent",
    )

    def __init__(
        self,
        elem_id: int,
        level: int,
        envelope: Envelope,
        incoming_edge: EdgeType,
        show_id: bool,
        is_highlighted: bool,
        parent: _MermaidElement | None,
    ) -> None:
        self.id = elem_id
        self.level = level
        self.envelope = envelope
        self.incoming_edge = incoming_edge
        self.show_id = show_id
        self.is_highlighted = is_highlighted
        self.parent = parent

    def format_node(self, element_ids: set[int]) -> str:
        if self.id in element_ids:
            element_ids.discard(self.id)
            summary = with_format_context(
                lambda ctx: envelope_summary(self.envelope, 20, ctx).replace(
                    '"', "&quot;"
                )
            )
            lines = [summary]
            if self.show_id:
                lines.append(self.envelope.digest().short_description())
            lines_str = "<br>".join(lines)
            frame_l, frame_r = _mermaid_frame(self.envelope)
            return f'{self.id}{frame_l}"{lines_str}"{frame_r}'
        return str(self.id)

    def format_edge(self, element_ids: set[int]) -> str:
        assert self.parent is not None
        label = self.incoming_edge.label
        if label is not None:
            arrow = f"-- {label} -->"
        else:
            arrow = "-->"
        return (
            f"{self.parent.format_node(element_ids)} {arrow} "
            f"{self.format_node(element_ids)}"
        )


# ---------------------------------------------------------------------------
# Mermaid format public API
# ---------------------------------------------------------------------------

def mermaid_format(
    envelope: Envelope,
    opts: MermaidFormatOpts | None = None,
) -> str:
    """Return a Mermaid flowchart string for an envelope."""
    if opts is None:
        opts = MermaidFormatOpts()

    from ._walk import EdgeType, walk

    elements: list[_MermaidElement] = []
    next_id = [0]

    def visitor(
        env: Envelope,
        level: int,
        incoming_edge: EdgeType,
        parent: _MermaidElement | None,
    ) -> tuple[_MermaidElement | None, bool]:
        elem_id = next_id[0]
        next_id[0] += 1
        elem = _MermaidElement(
            elem_id=elem_id,
            level=level,
            envelope=env,
            incoming_edge=incoming_edge,
            show_id=not opts.hide_nodes,
            is_highlighted=env.digest() in opts.highlighting_target,
            parent=parent,
        )
        elements.append(elem)
        return (elem, False)

    walk(envelope, opts.hide_nodes, None, visitor)

    element_ids: set[int] = {e.id for e in elements}

    lines = [
        f"%%{{ init: {{ 'theme': '{opts.theme.value}', "
        f"'flowchart': {{ 'curve': 'basis' }} }} }}%%",
        f"graph {opts.orientation.value}",
    ]

    node_styles: list[str] = []
    link_styles: list[str] = []
    link_index = 0

    for element in elements:
        indent_str = "    " * element.level
        if element.parent is not None:
            # Edge element
            this_link_styles: list[str] = []
            if not opts.monochrome:
                color = _link_stroke_color(element.incoming_edge)
                if color is not None:
                    this_link_styles.append(f"stroke:{color}")
            if element.is_highlighted and element.parent.is_highlighted:
                this_link_styles.append("stroke-width:4px")
            else:
                this_link_styles.append("stroke-width:2px")
            if this_link_styles:
                link_styles.append(
                    f"linkStyle {link_index} {','.join(this_link_styles)}"
                )
            link_index += 1
            content = element.format_edge(element_ids)
        else:
            content = element.format_node(element_ids)

        # Node styles
        this_node_styles: list[str] = []
        if not opts.monochrome:
            this_node_styles.append(
                f"stroke:{_node_color(element.envelope)}"
            )
        if element.is_highlighted:
            this_node_styles.append("stroke-width:6px")
        else:
            this_node_styles.append("stroke-width:4px")
        if this_node_styles:
            node_styles.append(
                f"style {element.id} {','.join(this_node_styles)}"
            )

        lines.append(f"{indent_str}{content}")

    lines.extend(node_styles)
    lines.extend(link_styles)

    return "\n".join(lines)
