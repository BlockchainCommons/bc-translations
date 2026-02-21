from __future__ import annotations

from datetime import datetime, timezone
from typing import TYPE_CHECKING

from .error import InvalidDate

if TYPE_CHECKING:
    from .cbor import CBOR


class Date:
    """CBOR date/time value encoded as a tag-1 numeric timestamp."""

    __slots__ = ("_dt",)

    def __init__(self, dt: datetime) -> None:
        if dt.tzinfo is None:
            dt = dt.replace(tzinfo=timezone.utc)
        self._dt = dt

    @staticmethod
    def from_datetime(dt: datetime) -> Date:
        return Date(dt)

    @staticmethod
    def from_ymd(year: int, month: int, day: int) -> Date:
        dt = datetime(year, month, day, tzinfo=timezone.utc)
        return Date(dt)

    @staticmethod
    def from_ymd_hms(
        year: int,
        month: int,
        day: int,
        hour: int,
        minute: int,
        second: int,
    ) -> Date:
        dt = datetime(year, month, day, hour, minute, second, tzinfo=timezone.utc)
        return Date(dt)

    @staticmethod
    def from_timestamp(seconds_since_epoch: float) -> Date:
        dt = datetime.fromtimestamp(seconds_since_epoch, tz=timezone.utc)
        return Date(dt)

    @staticmethod
    def from_string(value: str) -> Date:
        # Try RFC 3339 / ISO 8601 with time
        for fmt in (
            "%Y-%m-%dT%H:%M:%SZ",
            "%Y-%m-%dT%H:%M:%S%z",
            "%Y-%m-%dT%H:%M:%S.%fZ",
            "%Y-%m-%dT%H:%M:%S.%f%z",
        ):
            try:
                dt = datetime.strptime(value, fmt)
                if dt.tzinfo is None:
                    dt = dt.replace(tzinfo=timezone.utc)
                return Date(dt)
            except ValueError:
                continue

        # Try date-only
        try:
            dt = datetime.strptime(value, "%Y-%m-%d")
            dt = dt.replace(tzinfo=timezone.utc)
            return Date(dt)
        except ValueError:
            pass

        raise InvalidDate(f"Invalid date string: {value!r}")

    @staticmethod
    def now() -> Date:
        return Date(datetime.now(timezone.utc))

    @property
    def datetime_value(self) -> datetime:
        return self._dt

    def timestamp(self) -> float:
        return self._dt.timestamp()

    @staticmethod
    def from_untagged_cbor(cbor: CBOR) -> Date:
        n = cbor.try_float()
        return Date.from_timestamp(n)

    def to_untagged_cbor(self) -> CBOR:
        from .cbor import CBOR
        return CBOR.from_float(self.timestamp())

    def to_tagged_cbor(self) -> CBOR:
        from .cbor import CBOR
        from .tags_store import TAG_DATE, tags_for_values
        return CBOR.from_tagged_value(tags_for_values([TAG_DATE])[0], self.to_untagged_cbor())

    @staticmethod
    def from_tagged_cbor(cbor: CBOR) -> Date:
        from .tags_store import TAG_DATE
        item = cbor.try_expected_tagged_value(TAG_DATE)
        return Date.from_untagged_cbor(item)

    def __eq__(self, other: object) -> bool:
        if isinstance(other, Date):
            return self._dt == other._dt
        return NotImplemented

    def __hash__(self) -> int:
        return hash(self._dt)

    def __lt__(self, other: Date) -> bool:
        return self._dt < other._dt

    def __le__(self, other: Date) -> bool:
        return self._dt <= other._dt

    def __gt__(self, other: Date) -> bool:
        return self._dt > other._dt

    def __ge__(self, other: Date) -> bool:
        return self._dt >= other._dt

    def __str__(self) -> str:
        dt = self._dt
        if dt.hour == 0 and dt.minute == 0 and dt.second == 0:
            return dt.strftime("%Y-%m-%d")
        return dt.strftime("%Y-%m-%dT%H:%M:%SZ")

    def __repr__(self) -> str:
        return f"Date({self})"
