"""Internal notation rendering for envelope formatting.

Renders each envelope case as a formatted string with annotations.
Produces the human-readable "envelope notation" that shows the semantic
structure of envelopes.
"""

from __future__ import annotations

import enum
from typing import TYPE_CHECKING

from dcbor import CBORCase
from known_values import IS_A

from ._envelope_case import CaseType
from ._envelope_summary import cbor_envelope_summary
from ._format_context import (
    FormatContextOpt,
    flanked_by,
    with_format_context,
)

if TYPE_CHECKING:
    from dcbor import CBOR

    from ._assertion import Assertion
    from ._envelope import Envelope


# ---------------------------------------------------------------------------
# EnvelopeFormatItem -- represents one element of formatted output
# ---------------------------------------------------------------------------

class FormatItemKind(enum.IntEnum):
    BEGIN = 1
    END = 2
    ITEM = 3
    SEPARATOR = 4
    LIST = 5


class EnvelopeFormatItem:
    """Represents one element in the formatted envelope notation."""

    __slots__ = ("kind", "text", "items")

    def __init__(
        self,
        kind: FormatItemKind,
        text: str = "",
        items: list[EnvelopeFormatItem] | None = None,
    ) -> None:
        self.kind = kind
        self.text = text
        self.items: list[EnvelopeFormatItem] = items if items is not None else []

    @staticmethod
    def begin(delimiter: str) -> EnvelopeFormatItem:
        return EnvelopeFormatItem(FormatItemKind.BEGIN, delimiter)

    @staticmethod
    def end(delimiter: str) -> EnvelopeFormatItem:
        return EnvelopeFormatItem(FormatItemKind.END, delimiter)

    @staticmethod
    def item(text: str) -> EnvelopeFormatItem:
        return EnvelopeFormatItem(FormatItemKind.ITEM, text)

    @staticmethod
    def separator() -> EnvelopeFormatItem:
        return EnvelopeFormatItem(FormatItemKind.SEPARATOR)

    @staticmethod
    def list_(items: list[EnvelopeFormatItem]) -> EnvelopeFormatItem:
        return EnvelopeFormatItem(FormatItemKind.LIST, items=items)

    # -- Comparison for sorting assertions --

    def _sort_key(self) -> tuple:
        if self.kind == FormatItemKind.LIST:
            return (self.kind, "", tuple(i._sort_key() for i in self.items))
        return (self.kind, self.text, ())

    def __lt__(self, other: object) -> bool:
        if not isinstance(other, EnvelopeFormatItem):
            return NotImplemented
        return self._sort_key() < other._sort_key()

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, EnvelopeFormatItem):
            return NotImplemented
        return (
            self.kind == other.kind
            and self.text == other.text
            and self.items == other.items
        )

    def __hash__(self) -> int:
        return hash((self.kind, self.text))


# ---------------------------------------------------------------------------
# Flatten / Nicen / Indent helpers
# ---------------------------------------------------------------------------

def _flatten(item: EnvelopeFormatItem) -> list[EnvelopeFormatItem]:
    if item.kind == FormatItemKind.LIST:
        result: list[EnvelopeFormatItem] = []
        for child in item.items:
            result.extend(_flatten(child))
        return result
    return [item]


def _nicen(items: list[EnvelopeFormatItem]) -> list[EnvelopeFormatItem]:
    inp = list(items)
    result: list[EnvelopeFormatItem] = []
    while inp:
        current = inp.pop(0)
        if not inp:
            result.append(current)
            break
        if (
            current.kind == FormatItemKind.END
            and inp[0].kind == FormatItemKind.BEGIN
        ):
            result.append(
                EnvelopeFormatItem.end(f"{current.text} {inp[0].text}")
            )
            result.append(EnvelopeFormatItem.begin(""))
            inp.pop(0)
        else:
            result.append(current)
    return result


def _indent(level: int) -> str:
    return " " * (level * 4)


def _add_space_at_end_if_needed(s: str) -> str:
    if not s:
        return " "
    if s.endswith(" "):
        return s
    return s + " "


# ---------------------------------------------------------------------------
# Format flat / hierarchical
# ---------------------------------------------------------------------------

def _format_flat(item: EnvelopeFormatItem) -> str:
    line = ""
    for it in _flatten(item):
        if it.kind == FormatItemKind.BEGIN:
            if not line.endswith(" "):
                line += " "
            line += it.text
            line += " "
        elif it.kind == FormatItemKind.END:
            if not line.endswith(" "):
                line += " "
            line += it.text
            line += " "
        elif it.kind == FormatItemKind.ITEM:
            line += it.text
        elif it.kind == FormatItemKind.SEPARATOR:
            line = line.rstrip() + ", "
        elif it.kind == FormatItemKind.LIST:
            for child in it.items:
                line += _format_flat(child)
    return line


def _format_hierarchical(item: EnvelopeFormatItem) -> str:
    lines: list[str] = []
    level = 0
    current_line = ""

    for it in _nicen(_flatten(item)):
        if it.kind == FormatItemKind.BEGIN:
            if it.text:
                if not current_line:
                    c = it.text
                else:
                    c = _add_space_at_end_if_needed(current_line) + it.text
                lines.append(_indent(level) + c + "\n")
            level += 1
            current_line = ""
        elif it.kind == FormatItemKind.END:
            if current_line:
                lines.append(_indent(level) + current_line + "\n")
                current_line = ""
            level -= 1
            lines.append(_indent(level) + it.text + "\n")
        elif it.kind == FormatItemKind.ITEM:
            current_line += it.text
        elif it.kind == FormatItemKind.SEPARATOR:
            if current_line:
                lines.append(_indent(level) + current_line + "\n")
                current_line = ""
        elif it.kind == FormatItemKind.LIST:
            lines.append("<list>")

    if current_line:
        lines.append(current_line)

    return "".join(lines)


# ---------------------------------------------------------------------------
# Per-type format_item builders
# ---------------------------------------------------------------------------

def _cbor_format_item(
    cbor: CBOR,
    flat: bool,
    context: FormatContextOpt,
) -> EnvelopeFormatItem:
    """Format a CBOR value for envelope notation."""
    from ._cbor import envelope_tags, from_untagged_cbor

    # Check if this is a tagged envelope (tag 200)
    if cbor.case == CBORCase.TAGGED:
        tag, inner = cbor.value
        tags = envelope_tags()
        if tags and tag.value == tags[0].value:
            try:
                env = from_untagged_cbor(inner)
                return _envelope_format_item(env, flat, context)
            except Exception:
                return EnvelopeFormatItem.item("<error>")

    try:
        text = cbor_envelope_summary(cbor, 2**63, context)
        return EnvelopeFormatItem.item(text)
    except Exception:
        return EnvelopeFormatItem.item("<error>")


def _known_value_format_item(
    kv: object,  # KnownValue
    flat: bool,
    context: FormatContextOpt,
) -> EnvelopeFormatItem:
    """Format a KnownValue for envelope notation."""
    from known_values import KnownValue as KV

    value: KV = kv  # type: ignore[assignment]

    if context.is_none:
        name = value.name
    elif context.is_global:
        def _lookup(ctx: object) -> str:
            from ._format_context import FormatContext
            fc: FormatContext = ctx  # type: ignore[assignment]
            assigned = fc.known_values.assigned_name_for(value)
            return assigned if assigned is not None else value.name
        name = with_format_context(_lookup)
    else:
        assigned = context.context.known_values.assigned_name_for(value)
        name = assigned if assigned is not None else value.name

    return EnvelopeFormatItem.item(flanked_by(name, "'", "'"))


def _assertion_format_item(
    assertion: Assertion,
    flat: bool,
    context: FormatContextOpt,
) -> EnvelopeFormatItem:
    """Format an Assertion for envelope notation."""
    return EnvelopeFormatItem.list_([
        _envelope_format_item(assertion.predicate, flat, context),
        EnvelopeFormatItem.item(": "),
        _envelope_format_item(assertion.object, flat, context),
    ])


def _envelope_format_item(
    envelope: Envelope,
    flat: bool,
    context: FormatContextOpt,
) -> EnvelopeFormatItem:
    """Build the format item tree for an envelope."""
    ct = envelope.case_type
    case = envelope.case

    if ct == CaseType.LEAF:
        return _cbor_format_item(case.cbor, flat, context)

    if ct == CaseType.WRAPPED:
        return EnvelopeFormatItem.list_([
            EnvelopeFormatItem.begin("{"),
            _envelope_format_item(case.envelope, flat, context),
            EnvelopeFormatItem.end("}"),
        ])

    if ct == CaseType.ASSERTION:
        return _assertion_format_item(case.assertion, flat, context)

    if ct == CaseType.KNOWN_VALUE:
        return _known_value_format_item(case.value, flat, context)

    if ct == CaseType.ENCRYPTED:
        return EnvelopeFormatItem.item("ENCRYPTED")

    if ct == CaseType.COMPRESSED:
        return EnvelopeFormatItem.item("COMPRESSED")

    if ct == CaseType.ELIDED:
        return EnvelopeFormatItem.item("ELIDED")

    if ct == CaseType.NODE:
        return _format_node(case.subject, case.assertions, flat, context)

    return EnvelopeFormatItem.item("<error>")  # pragma: no cover


def _format_node(
    subject: Envelope,
    assertions: list[Envelope],
    flat: bool,
    context: FormatContextOpt,
) -> EnvelopeFormatItem:
    """Build format items for a Node envelope."""
    subject_item = _envelope_format_item(subject, flat, context)

    elided_count = 0
    encrypted_count = 0
    compressed_count = 0
    type_assertion_items: list[list[EnvelopeFormatItem]] = []
    assertion_items: list[list[EnvelopeFormatItem]] = []

    for assertion_env in assertions:
        act = assertion_env.case_type
        if act == CaseType.ELIDED:
            elided_count += 1
        elif act == CaseType.ENCRYPTED:
            encrypted_count += 1
        elif act == CaseType.COMPRESSED:
            compressed_count += 1
        else:
            item = [_envelope_format_item(assertion_env, flat, context)]
            # Check if this is a type assertion (isA predicate)
            is_type_assertion = False
            pred_env = assertion_env.as_predicate()
            if pred_env is not None:
                kv = pred_env.subject().as_known_value()
                if kv is not None and kv == IS_A:
                    is_type_assertion = True

            if is_type_assertion:
                type_assertion_items.append(item)
            else:
                assertion_items.append(item)

    # Sort by format item comparison key
    def _group_sort_key(
        group: list[EnvelopeFormatItem],
    ) -> tuple:
        return tuple(i._sort_key() for i in group)

    type_assertion_items.sort(key=_group_sort_key)
    assertion_items.sort(key=_group_sort_key)

    # Prepend type assertions
    all_assertion_items = type_assertion_items + assertion_items

    # Add compressed/elided/encrypted markers (order matches Rust)
    if compressed_count > 1:
        all_assertion_items.append(
            [EnvelopeFormatItem.item(f"COMPRESSED ({compressed_count})")]
        )
    elif compressed_count > 0:
        all_assertion_items.append([EnvelopeFormatItem.item("COMPRESSED")])

    if elided_count > 1:
        all_assertion_items.append(
            [EnvelopeFormatItem.item(f"ELIDED ({elided_count})")]
        )
    elif elided_count > 0:
        all_assertion_items.append([EnvelopeFormatItem.item("ELIDED")])

    if encrypted_count > 1:
        all_assertion_items.append(
            [EnvelopeFormatItem.item(f"ENCRYPTED ({encrypted_count})")]
        )
    elif encrypted_count > 0:
        all_assertion_items.append([EnvelopeFormatItem.item("ENCRYPTED")])

    # Intersperse separators between assertion groups
    joined: list[EnvelopeFormatItem] = []
    for i, group in enumerate(all_assertion_items):
        if i > 0:
            joined.append(EnvelopeFormatItem.separator())
        joined.extend(group)

    # Build the result items
    items: list[EnvelopeFormatItem] = []
    needs_braces = subject.is_subject_assertion()

    if needs_braces:
        items.append(EnvelopeFormatItem.begin("{"))
    items.append(subject_item)
    if needs_braces:
        items.append(EnvelopeFormatItem.end("}"))
    items.append(EnvelopeFormatItem.begin("["))
    items.extend(joined)
    items.append(EnvelopeFormatItem.end("]"))

    return EnvelopeFormatItem.list_(items)


# ---------------------------------------------------------------------------
# Public formatting entry points
# ---------------------------------------------------------------------------

def format_envelope(
    envelope: Envelope,
    *,
    flat: bool = False,
    context: FormatContextOpt | None = None,
) -> str:
    """Return the envelope notation string for an envelope.

    Parameters
    ----------
    envelope:
        The envelope to format.
    flat:
        If ``True``, format on a single line.
    context:
        Format context to use.  Defaults to global.
    """
    if context is None:
        context = FormatContextOpt.global_()
    item = _envelope_format_item(envelope, flat, context)
    if flat:
        return _format_flat(item).strip()
    return _format_hierarchical(item).strip()
