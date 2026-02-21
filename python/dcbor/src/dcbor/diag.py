from __future__ import annotations

from typing import TYPE_CHECKING

from .string_util import flanked

if TYPE_CHECKING:
    from .cbor import CBOR
    from .tags_store import CBORSummarizer, TagsStore

_NO_TAGS = object()


class _DiagItem:
    pass


class _DiagLeaf(_DiagItem):
    __slots__ = ("text",)

    def __init__(self, text: str) -> None:
        self.text = text


class _DiagGroup(_DiagItem):
    __slots__ = ("begin", "end", "items", "is_pairs", "comment")

    def __init__(
        self,
        begin: str,
        end: str,
        items: list[_DiagItem],
        is_pairs: bool,
        comment: str | None,
    ) -> None:
        self.begin = begin
        self.end = end
        self.items = items
        self.is_pairs = is_pairs
        self.comment = comment


def _total_strings_len(item: _DiagItem) -> int:
    if isinstance(item, _DiagLeaf):
        return len(item.text)
    if isinstance(item, _DiagGroup):
        return sum(_total_strings_len(child) for child in item.items)
    return 0


def _greatest_strings_len(item: _DiagItem) -> int:
    if isinstance(item, _DiagLeaf):
        return len(item.text)
    if isinstance(item, _DiagGroup):
        if not item.items:
            return 0
        return max(_total_strings_len(child) for child in item.items)
    return 0


def _is_group(item: _DiagItem) -> bool:
    return isinstance(item, _DiagGroup)


def _contains_group(item: _DiagItem) -> bool:
    if isinstance(item, _DiagGroup):
        return any(_is_group(child) for child in item.items)
    return False


def _joined(elements: list[str], item_separator: str, pair_separator: str | None) -> str:
    pair_sep = pair_separator if pair_separator is not None else item_separator
    result: list[str] = []
    length = len(elements)
    for i, elem in enumerate(elements):
        result.append(elem)
        if i != length - 1:
            if i & 1 != 0:
                result.append(item_separator)
            else:
                result.append(pair_sep)
    return "".join(result)


def _format_line(
    level: int,
    flat: bool,
    string: str,
    separator: str,
    comment: str | None,
) -> str:
    indent = "" if flat else " " * (level * 4)
    result = f"{indent}{string}{separator}"
    if comment is not None:
        return f"{result}   / {comment} /"
    return result


def _single_line(
    item: _DiagItem,
    level: int,
    separator: str,
    flat: bool,
) -> str:
    if isinstance(item, _DiagLeaf):
        return _format_line(level, flat, item.text, separator, None)
    if isinstance(item, _DiagGroup):
        components: list[str] = []
        for child in item.items:
            if isinstance(child, _DiagLeaf):
                components.append(child.text)
            else:
                components.append(_single_line(child, level + 1, separator, flat))
        pair_sep = ": " if item.is_pairs else ", "
        body = _joined(components, ", ", pair_sep)
        text = flanked(body, item.begin, item.end)
        return _format_line(level, flat, text, separator, item.comment)
    return ""


def _multiline(
    item: _DiagItem,
    level: int,
    separator: str,
    flat: bool,
) -> str:
    if isinstance(item, _DiagLeaf):
        return item.text
    if isinstance(item, _DiagGroup):
        lines: list[str] = []
        lines.append(
            _format_line(level, False, item.begin, "", item.comment)
        )
        for i, child in enumerate(item.items):
            if i == len(item.items) - 1:
                sep = ""
            elif item.is_pairs and i & 1 == 0:
                sep = ":"
            else:
                sep = ","
            lines.append(_format_item(child, level + 1, sep, flat))
        lines.append(_format_line(level, flat, item.end, separator, None))
        return "\n".join(lines)
    return ""


def _format_item(
    item: _DiagItem,
    level: int,
    separator: str,
    flat: bool,
) -> str:
    if isinstance(item, _DiagLeaf):
        return _format_line(level, flat, item.text, separator, None)
    if not flat and (
        _contains_group(item)
        or _total_strings_len(item) > 20
        or _greatest_strings_len(item) > 20
    ):
        return _multiline(item, level, separator, flat)
    return _single_line(item, level, separator, flat)


def _build_diag_item(
    cbor: CBOR,
    annotate: bool,
    summarize: bool,
    tags_store: TagsStore | None,
) -> _DiagItem:
    from .cbor import CBORCase

    match cbor.case:
        case (
            CBORCase.UNSIGNED
            | CBORCase.NEGATIVE
            | CBORCase.BYTE_STRING
            | CBORCase.TEXT
            | CBORCase.SIMPLE
        ):
            return _DiagLeaf(cbor._leaf_str())

        case CBORCase.ARRAY:
            items = [
                _build_diag_item(child, annotate, summarize, tags_store)
                for child in cbor.value
            ]
            return _DiagGroup("[", "]", items, False, None)

        case CBORCase.MAP:
            items: list[_DiagItem] = []
            for key, value in cbor.value.iter():
                items.append(_build_diag_item(key, annotate, summarize, tags_store))
                items.append(_build_diag_item(value, annotate, summarize, tags_store))
            return _DiagGroup("{", "}", items, True, None)

        case CBORCase.TAGGED:
            tag, item = cbor.value

            if summarize:
                summarizer = _get_summarizer(tag.value, tags_store)
                if summarizer is not None:
                    try:
                        summary_text = summarizer(item, True)
                        return _DiagLeaf(summary_text)
                    except Exception as e:
                        return _DiagLeaf(f"<error: {e}>")

            comment: str | None = None
            if annotate:
                comment = _get_assigned_name(tag, tags_store)

            child_item = _build_diag_item(item, annotate, summarize, tags_store)
            begin = str(tag.value) + "("
            return _DiagGroup(begin, ")", [child_item], False, comment)

    return _DiagLeaf("")


def _get_summarizer(tag_value: int, tags_store: TagsStore | None) -> CBORSummarizer | None:
    if tags_store is None:
        from .tags_store import _get_global_tags
        store = _get_global_tags()
        return store.summarizer(tag_value)
    if tags_store is _NO_TAGS:
        return None
    return tags_store.summarizer(tag_value)


def _get_assigned_name(tag: object, tags_store: TagsStore | None) -> str | None:
    if tags_store is None:
        from .tags_store import _get_global_tags
        store = _get_global_tags()
        return store.assigned_name_for_tag(tag)
    if tags_store is _NO_TAGS:
        return None
    return tags_store.assigned_name_for_tag(tag)


def diagnostic_impl(
    cbor: CBOR,
    *,
    annotate: bool = False,
    flat: bool = False,
    summarize: bool = False,
    tags_store: TagsStore | None = None,
) -> str:
    if summarize:
        flat = True
    item = _build_diag_item(cbor, annotate, summarize, tags_store)
    return _format_item(item, 0, "", flat)
