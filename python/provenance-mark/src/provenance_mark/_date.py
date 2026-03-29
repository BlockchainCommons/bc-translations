"""Date serialization helpers for provenance-mark."""

from __future__ import annotations

import calendar
from datetime import datetime, timedelta, timezone

from dcbor import Date

from ._error import Error

_REFERENCE_DATE = datetime(2001, 1, 1, tzinfo=timezone.utc)
_MAX_6_BYTE = 0xE5940A78A7FF


def _delta_seconds(date: Date) -> int:
    delta = date.datetime_value - _REFERENCE_DATE
    return delta.days * 86400 + delta.seconds


def _delta_milliseconds(date: Date) -> int:
    delta = date.datetime_value - _REFERENCE_DATE
    return (
        (delta.days * 86400 + delta.seconds) * 1000
        + delta.microseconds // 1000
    )


def serialize_2_bytes(date: Date) -> bytes:
    dt = date.datetime_value
    year = dt.year
    month = dt.month
    day = dt.day

    yy = year - 2023
    if yy < 0 or yy >= 128:
        raise Error(
            "YearOutOfRange",
            (
                "year out of range for 2-byte serialization: "
                f"must be between 2023-2150, got {year}"
            ),
        )
    if month < 1 or month > 12 or day < 1 or day > 31:
        raise Error(
            "InvalidMonthOrDay",
            f"invalid month ({month}) or day ({day}) for year {year}",
        )

    value = (yy << 9) | (month << 5) | day
    return value.to_bytes(2, "big")


def deserialize_2_bytes(data: bytes | bytearray) -> Date:
    if len(data) != 2:
        raise Error(
            "ResolutionError",
            f"invalid date length: expected 2, 4, or 6 bytes, got {len(data)}",
        )
    value = int.from_bytes(bytes(data), "big")
    day = value & 0b11111
    month = (value >> 5) & 0b1111
    year = ((value >> 9) & 0b1111111) + 2023
    if month < 1 or month > 12:
        raise Error(
            "InvalidMonthOrDay",
            f"invalid month ({month}) or day ({day}) for year {year}",
        )
    if day not in range_of_days_in_month(year, month):
        raise Error(
            "InvalidMonthOrDay",
            f"invalid month ({month}) or day ({day}) for year {year}",
        )
    try:
        return Date.from_datetime(datetime(year, month, day, tzinfo=timezone.utc))
    except ValueError as exc:
        raise Error(
            "InvalidDate",
            f"Cannot construct date {year}-{month:02}-{day:02}",
        ) from exc


def serialize_4_bytes(date: Date) -> bytes:
    seconds = _delta_seconds(date)
    if seconds < 0 or seconds > 0xFFFFFFFF:
        raise Error("DateOutOfRange", "seconds value too large for u32")
    return seconds.to_bytes(4, "big")


def deserialize_4_bytes(data: bytes | bytearray) -> Date:
    if len(data) != 4:
        raise Error(
            "ResolutionError",
            f"invalid date length: expected 2, 4, or 6 bytes, got {len(data)}",
        )
    seconds = int.from_bytes(bytes(data), "big")
    return Date.from_datetime(_REFERENCE_DATE + timedelta(seconds=seconds))


def serialize_6_bytes(date: Date) -> bytes:
    milliseconds = _delta_milliseconds(date)
    if milliseconds < 0:
        raise Error("DateOutOfRange", "milliseconds value too large for u64")
    if milliseconds > _MAX_6_BYTE:
        raise Error(
            "DateOutOfRange",
            "date exceeds maximum representable value",
        )
    return milliseconds.to_bytes(8, "big")[2:]


def deserialize_6_bytes(data: bytes | bytearray) -> Date:
    if len(data) != 6:
        raise Error(
            "ResolutionError",
            f"invalid date length: expected 2, 4, or 6 bytes, got {len(data)}",
        )
    milliseconds = int.from_bytes(b"\x00\x00" + bytes(data), "big")
    if milliseconds > _MAX_6_BYTE:
        raise Error(
            "DateOutOfRange",
            "date exceeds maximum representable value",
        )
    return Date.from_datetime(
        _REFERENCE_DATE + timedelta(milliseconds=milliseconds)
    )


def range_of_days_in_month(year: int, month: int) -> range:
    return range(1, calendar.monthrange(year, month)[1] + 1)

