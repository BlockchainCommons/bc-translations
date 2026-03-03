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

    @property
    def identifier(self) -> int:
        """The 16-bit share set identifier."""
        return self._identifier

    @property
    def group_index(self) -> int:
        """The group index for this share."""
        return self._group_index

    @property
    def group_threshold(self) -> int:
        """The group threshold for recovery."""
        return self._group_threshold

    @property
    def group_count(self) -> int:
        """The total number of groups."""
        return self._group_count

    @property
    def member_index(self) -> int:
        """The member index within the group."""
        return self._member_index

    @property
    def member_threshold(self) -> int:
        """The member threshold for the group."""
        return self._member_threshold

    @property
    def value(self) -> Secret:
        """The share's secret value."""
        return self._value
