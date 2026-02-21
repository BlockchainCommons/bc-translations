from __future__ import annotations

import threading
from collections.abc import Callable
from typing import TYPE_CHECKING, TypeVar

from .tag import Tag, TagValue

if TYPE_CHECKING:
    from .cbor import CBOR

T = TypeVar("T")


CBORSummarizer = Callable[["CBOR", bool], str]


class TagsStore:
    """Registry mapping CBOR tag numbers to named Tag objects and optional diagnostic summarizers."""

    __slots__ = ("_tags_by_value", "_tags_by_name", "_summarizers")

    def __init__(self, tags: list[Tag] | None = None) -> None:
        self._tags_by_value: dict[TagValue, Tag] = {}
        self._tags_by_name: dict[str, Tag] = {}
        self._summarizers: dict[TagValue, CBORSummarizer] = {}
        if tags:
            for tag in tags:
                self._do_insert(tag)

    def insert(self, tag: Tag) -> None:
        self._do_insert(tag)

    def insert_all(self, tags: list[Tag]) -> None:
        for tag in tags:
            self._do_insert(tag)

    def set_summarizer(self, tag_value: TagValue, summarizer: CBORSummarizer) -> None:
        self._summarizers[tag_value] = summarizer

    def _do_insert(self, tag: Tag) -> None:
        name = tag.name
        if name is None or name == "":
            raise ValueError("Tag must have a non-empty name")
        old = self._tags_by_value.get(tag.value)
        if old is not None:
            old_name = old.name
            if old_name != name:
                raise ValueError(
                    f"Attempt to register tag: {tag.value} '{old_name}' "
                    f"with different name: '{name}'"
                )
        self._tags_by_value[tag.value] = tag
        self._tags_by_name[name] = tag

    def assigned_name_for_tag(self, tag: Tag) -> str | None:
        found = self._tags_by_value.get(tag.value)
        if found is not None:
            return found.name
        return None

    def name_for_tag(self, tag: Tag) -> str:
        name = self.assigned_name_for_tag(tag)
        if name is not None:
            return name
        return str(tag.value)

    def tag_for_value(self, value: TagValue) -> Tag | None:
        return self._tags_by_value.get(value)

    def tag_for_name(self, name: str) -> Tag | None:
        return self._tags_by_name.get(name)

    def name_for_value(self, value: TagValue) -> str:
        tag = self.tag_for_value(value)
        if tag is not None and tag.name is not None:
            return tag.name
        return str(value)

    def summarizer(self, tag_value: TagValue) -> CBORSummarizer | None:
        return self._summarizers.get(tag_value)


_global_lock = threading.Lock()
_global_tags: TagsStore | None = None


def _get_global_tags() -> TagsStore:
    global _global_tags
    if _global_tags is None:
        with _global_lock:
            if _global_tags is None:
                _global_tags = TagsStore()
    return _global_tags


def with_tags(action: Callable[[TagsStore], T]) -> T:
    return action(_get_global_tags())


def with_tags_mut(action: Callable[[TagsStore], T]) -> T:
    with _global_lock:
        return action(_get_global_tags())


TAG_DATE: TagValue = 1
_TAG_NAME_DATE = "date"


def tags_for_values(values: list[TagValue]) -> list[Tag]:
    store = _get_global_tags()
    result: list[Tag] = []
    for value in values:
        tag = store.tag_for_value(value)
        if tag is not None:
            result.append(tag)
        else:
            result.append(Tag.with_value(value))
    return result


def register_tags_in(store: TagsStore) -> None:
    from .date import Date

    store.insert(Tag(TAG_DATE, _TAG_NAME_DATE))

    def date_summarizer(cbor: CBOR, flat: bool) -> str:
        return str(Date.from_untagged_cbor(cbor))

    store.set_summarizer(TAG_DATE, date_summarizer)


def register_tags() -> None:
    """Register the standard dcbor tags (date, etc.) in the global TagsStore."""
    with_tags_mut(lambda store: register_tags_in(store))
