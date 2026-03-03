"""Internal share representation used during encode/decode."""

from __future__ import annotations

from .secret import Secret


class SSKRShare:
    """Internal structure for decoded share metadata and value."""

    __slots__ = (
        "_identifier",
        "_group_index",
        "_group_threshold",
        "_group_count",
        "_member_index",
        "_member_threshold",
        "_value",
    )

    def __init__(
        self,
        identifier: int,
        group_index: int,
        group_threshold: int,
        group_count: int,
        member_index: int,
        member_threshold: int,
        value: Secret,
    ) -> None:
        self._identifier = identifier
        self._group_index = group_index
        self._group_threshold = group_threshold
        self._group_count = group_count
        self._member_index = member_index
        self._member_threshold = member_threshold
        self._value = value

    def identifier(self) -> int:
        return self._identifier

    def group_index(self) -> int:
        return self._group_index

    def group_threshold(self) -> int:
        return self._group_threshold

    def group_count(self) -> int:
        return self._group_count

    def member_index(self) -> int:
        return self._member_index

    def member_threshold(self) -> int:
        return self._member_threshold

    def value(self) -> Secret:
        return self._value
