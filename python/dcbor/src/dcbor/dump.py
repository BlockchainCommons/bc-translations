from __future__ import annotations

from typing import TYPE_CHECKING

from .string_util import flanked, sanitized
from .varint import MajorType, encode_varint

if TYPE_CHECKING:
    from .cbor import CBOR
    from .tag import Tag
    from .tags_store import TagsStore

_NO_TAGS = object()


class _DumpItem:
    __slots__ = ("level", "data", "note")

    def __init__(
        self,
        level: int,
        data: list[bytes],
        note: str | None,
    ) -> None:
        self.level = level
        self.data = data
        self.note = note

    def format_first_column(self) -> str:
        indent = " " * (self.level * 4)
        hex_parts = [d.hex() for d in self.data if len(d) > 0]
        hex_str = " ".join(hex_parts)
        return indent + hex_str

    def format(self, note_column: int) -> str:
        col1 = self.format_first_column()
        if self.note is not None:
            padding_count = max(1, min(39, note_column) - len(col1) + 1)
            padding = " " * padding_count
            col2 = f"# {self.note}"
            return col1 + padding + col2
        return col1


def _dump_items(cbor: CBOR, level: int, tags_store: TagsStore | None) -> list[_DumpItem]:
    from .cbor import CBORCase

    match cbor.case:
        case CBORCase.UNSIGNED:
            return [_DumpItem(
                level,
                [cbor.to_cbor_data()],
                f"unsigned({cbor.value})",
            )]

        case CBORCase.NEGATIVE:
            return [_DumpItem(
                level,
                [cbor.to_cbor_data()],
                f"negative({-1 - cbor.value})",
            )]

        case CBORCase.BYTE_STRING:
            bs = cbor.value
            data = bs.data
            header = encode_varint(len(data), MajorType.BYTE_STRING)
            items = [_DumpItem(level, [header], f"bytes({len(data)})")]
            if len(data) > 0:
                note: str | None = None
                try:
                    text = data.decode("utf-8")
                    san = sanitized(text)
                    if san is not None:
                        note = flanked(san, '"', '"')
                except (UnicodeDecodeError, ValueError):
                    pass
                items.append(_DumpItem(level + 1, [data], note))
            return items

        case CBORCase.TEXT:
            text = cbor.value
            utf8_data = text.encode("utf-8")
            header = encode_varint(len(utf8_data), MajorType.TEXT)
            header_data = [bytes([header[0]]), header[1:]]
            return [
                _DumpItem(level, header_data, f"text({len(utf8_data)})"),
                _DumpItem(level + 1, [utf8_data], flanked(text, '"', '"')),
            ]

        case CBORCase.SIMPLE:
            data = cbor.value.cbor_data()
            note = str(cbor.value)
            return [_DumpItem(level, [data], note)]

        case CBORCase.TAGGED:
            tag, item = cbor.value
            header = encode_varint(tag.value, MajorType.TAGGED)
            header_data = [bytes([header[0]]), header[1:]]
            note_parts = [f"tag({tag.value})"]
            assigned = _get_assigned_name_dump(tag, tags_store)
            if assigned is not None:
                note_parts.append(assigned)
            tag_note = " ".join(note_parts)
            result = [_DumpItem(level, header_data, tag_note)]
            result.extend(_dump_items(item, level + 1, tags_store))
            return result

        case CBORCase.ARRAY:
            arr = cbor.value
            header = encode_varint(len(arr), MajorType.ARRAY)
            header_data = [bytes([header[0]]), header[1:]]
            result = [_DumpItem(level, header_data, f"array({len(arr)})")]
            for child in arr:
                result.extend(_dump_items(child, level + 1, tags_store))
            return result

        case CBORCase.MAP:
            m = cbor.value
            entry_count = len(m)
            header = encode_varint(entry_count, MajorType.MAP)
            header_data = [bytes([header[0]]), header[1:]]
            result = [_DumpItem(level, header_data, f"map({entry_count})")]
            for key, val in m.iter():
                result.extend(_dump_items(key, level + 1, tags_store))
                result.extend(_dump_items(val, level + 1, tags_store))
            return result

    return []


def _get_assigned_name_dump(tag: Tag, tags_store: TagsStore | None) -> str | None:
    if tags_store is None:
        from .tags_store import _get_global_tags
        store = _get_global_tags()
        return store.assigned_name_for_tag(tag)
    if tags_store is _NO_TAGS:
        return None
    return tags_store.assigned_name_for_tag(tag)


def hex_annotated_impl(cbor: CBOR, *, tags_store: TagsStore | None = None) -> str:
    items = _dump_items(cbor, 0, tags_store)
    if not items:
        return ""
    note_column = max(len(item.format_first_column()) for item in items)
    note_column = ((note_column + 4) & ~3) - 1
    lines = [item.format(note_column) for item in items]
    return "\n".join(lines)
