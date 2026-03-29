"""Tests for provenance-mark date encodings."""

from __future__ import annotations

from datetime import datetime, timezone

import pytest
from dcbor import Date

from provenance_mark import (
    Error,
    deserialize_2_bytes,
    deserialize_4_bytes,
    deserialize_6_bytes,
    serialize_2_bytes,
    serialize_4_bytes,
    serialize_6_bytes,
)


def _utc_datetime(
    year: int,
    month: int,
    day: int,
    hour: int = 0,
    minute: int = 0,
    second: int = 0,
    microsecond: int = 0,
) -> Date:
    return Date.from_datetime(
        datetime(
            year,
            month,
            day,
            hour,
            minute,
            second,
            microsecond,
            tzinfo=timezone.utc,
        )
    )


def test_2_byte_dates() -> None:
    base_date = _utc_datetime(2023, 6, 20)
    assert serialize_2_bytes(base_date).hex() == "00d4"
    assert deserialize_2_bytes(bytes.fromhex("00d4")) == base_date

    assert deserialize_2_bytes(bytes.fromhex("0021")) == _utc_datetime(2023, 1, 1)
    assert deserialize_2_bytes(bytes.fromhex("ff9f")) == _utc_datetime(2150, 12, 31)

    with pytest.raises(Error):
        deserialize_2_bytes(bytes.fromhex("005e"))


def test_4_byte_dates() -> None:
    base_date = _utc_datetime(2023, 6, 20, 12, 34, 56)
    assert serialize_4_bytes(base_date) == bytes.fromhex("2a41d470")
    assert deserialize_4_bytes(bytes.fromhex("2a41d470")) == base_date

    assert deserialize_4_bytes(bytes.fromhex("00000000")) == _utc_datetime(2001, 1, 1)
    assert deserialize_4_bytes(bytes.fromhex("ffffffff")) == _utc_datetime(
        2137, 2, 7, 6, 28, 15
    )


def test_6_byte_dates() -> None:
    base_date = _utc_datetime(2023, 6, 20, 12, 34, 56, 789_000)
    assert serialize_6_bytes(base_date) == bytes.fromhex("00a51125d895")
    assert deserialize_6_bytes(bytes.fromhex("00a51125d895")) == base_date

    assert deserialize_6_bytes(bytes.fromhex("000000000000")) == _utc_datetime(2001, 1, 1)
    assert deserialize_6_bytes(bytes.fromhex("e5940a78a7ff")) == _utc_datetime(
        9999, 12, 31, 23, 59, 59, 999_000
    )

    with pytest.raises(Error):
        deserialize_6_bytes(bytes.fromhex("e5940a78a800"))
