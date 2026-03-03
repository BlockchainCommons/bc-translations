"""Split specification models for SSKR."""

from __future__ import annotations

from collections.abc import Sequence

from .constants import MAX_SHARE_COUNT
from .error import (
    GroupCountInvalidError,
    GroupSpecInvalidError,
    GroupThresholdInvalidError,
    MemberCountInvalidError,
    MemberThresholdInvalidError,
)


class Spec:
    """A specification for an SSKR split."""

    __slots__ = ("_group_threshold", "_groups")

    def __init__(self, group_threshold: int, groups: tuple["GroupSpec", ...]) -> None:
        self._group_threshold = group_threshold
        self._groups = groups

    @classmethod
    def new(cls, group_threshold: int, groups: Sequence["GroupSpec"]) -> "Spec":
        """Create a new validated split specification."""
        if group_threshold == 0:
            raise GroupThresholdInvalidError()
        if group_threshold > len(groups):
            raise GroupThresholdInvalidError()
        if len(groups) > MAX_SHARE_COUNT:
            raise GroupCountInvalidError()
        return cls(group_threshold, tuple(groups))

    def group_threshold(self) -> int:
        """Return the group threshold."""
        return self._group_threshold

    def groups(self) -> tuple["GroupSpec", ...]:
        """Return the configured groups."""
        return self._groups

    def group_count(self) -> int:
        """Return the number of groups."""
        return len(self._groups)

    def share_count(self) -> int:
        """Return the total number of shares across groups."""
        return sum(group.member_count() for group in self._groups)

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Spec):
            return NotImplemented
        return (
            self._group_threshold == other._group_threshold and self._groups == other._groups
        )

    def __repr__(self) -> str:
        return f"Spec(group_threshold={self._group_threshold}, groups={list(self._groups)!r})"


class GroupSpec:
    """A specification for a group of shares within an SSKR split."""

    __slots__ = ("_member_threshold", "_member_count")

    def __init__(self, member_threshold: int, member_count: int) -> None:
        self._member_threshold = member_threshold
        self._member_count = member_count

    @classmethod
    def new(cls, member_threshold: int, member_count: int) -> "GroupSpec":
        """Create a new validated group specification."""
        if member_count == 0:
            raise MemberCountInvalidError()
        if member_count > MAX_SHARE_COUNT:
            raise MemberCountInvalidError()
        if member_threshold > member_count:
            raise MemberThresholdInvalidError()
        return cls(member_threshold, member_count)

    def member_threshold(self) -> int:
        """Return the member threshold for this group."""
        return self._member_threshold

    def member_count(self) -> int:
        """Return the member share count for this group."""
        return self._member_count

    @classmethod
    def parse(cls, source: str) -> "GroupSpec":
        """Parse a group specification in `M-of-N` format."""
        parts = source.split("-")
        if len(parts) != 3:
            raise GroupSpecInvalidError()
        try:
            member_threshold = int(parts[0], 10)
        except ValueError as exc:
            raise GroupSpecInvalidError() from exc
        if parts[1] != "of":
            raise GroupSpecInvalidError()
        try:
            member_count = int(parts[2], 10)
        except ValueError as exc:
            raise GroupSpecInvalidError() from exc
        return cls.new(member_threshold, member_count)

    @classmethod
    def default(cls) -> "GroupSpec":
        """Return the default one-of-one group specification."""
        return cls.new(1, 1)

    def __str__(self) -> str:
        return f"{self._member_threshold}-of-{self._member_count}"

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, GroupSpec):
            return NotImplemented
        return (
            self._member_threshold == other._member_threshold
            and self._member_count == other._member_count
        )

    def __repr__(self) -> str:
        return (
            f"GroupSpec(member_threshold={self._member_threshold}, "
            f"member_count={self._member_count})"
        )
