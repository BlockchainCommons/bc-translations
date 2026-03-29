"""Tests for provenance mark identifiers."""

from __future__ import annotations

from datetime import datetime, timedelta, timezone

import pytest
from dcbor import Date

from provenance_mark import (
    PROVENANCE_MARK_PREFIX,
    Error,
    ProvenanceMark,
    ProvenanceMarkGenerator,
    ProvenanceMarkResolution,
)


def _test_date(index: int) -> Date:
    base = datetime(2023, 6, 20, 12, 0, 0, tzinfo=timezone.utc)
    return Date.from_datetime(base + timedelta(days=index))


def _make_marks(
    count: int,
    resolution: ProvenanceMarkResolution = ProvenanceMarkResolution.Low,
) -> list[ProvenanceMark]:
    generator = ProvenanceMarkGenerator.new_with_passphrase(resolution, "Wolf")
    return [generator.next(_test_date(index), None) for index in range(count)]


def test_id_returns_32_bytes() -> None:
    for resolution in ProvenanceMarkResolution:
        for mark in _make_marks(3, resolution):
            assert len(mark.id()) == 32


def test_id_preserves_hash_prefix() -> None:
    for resolution in ProvenanceMarkResolution:
        for mark in _make_marks(3, resolution):
            assert mark.id()[: len(mark.hash())] == mark.hash()


def test_id_hex_is_64_chars() -> None:
    for mark in _make_marks(5):
        assert len(mark.id_hex()) == 64


def test_id_hex_encodes_full_id() -> None:
    mark = _make_marks(1)[0]
    assert mark.id_hex() == mark.id().hex()


def test_id_bytewords_word_count() -> None:
    mark = _make_marks(1)[0]
    for count in range(4, 33):
        assert len(mark.id_bytewords(count, False).split(" ")) == count


def test_id_bytewords_prefix_extends_shorter() -> None:
    mark = _make_marks(1)[0]
    assert mark.id_bytewords(8, False).startswith(mark.id_bytewords(4, False))


def test_id_bytewords_with_prefix_flag() -> None:
    mark = _make_marks(1)[0]
    without_prefix = mark.id_bytewords(4, False)
    with_prefix = mark.id_bytewords(4, True)
    assert with_prefix.startswith(PROVENANCE_MARK_PREFIX + " ")
    assert with_prefix.removeprefix(PROVENANCE_MARK_PREFIX + " ") == without_prefix


def test_id_bytemoji_word_count() -> None:
    mark = _make_marks(1)[0]
    for count in range(4, 33):
        assert len(mark.id_bytemoji(count, False).split(" ")) == count


def test_id_bytewords_minimal_length() -> None:
    mark = _make_marks(1)[0]
    for count in range(4, 33):
        assert len(mark.id_bytewords_minimal(count, False)) == count * 2


def test_id_bytewords_minimal_is_uppercase() -> None:
    identifier = _make_marks(1)[0].id_bytewords_minimal(4, False)
    assert identifier == identifier.upper()


def test_id_bytewords_minimal_extends_shorter() -> None:
    mark = _make_marks(1)[0]
    assert mark.id_bytewords_minimal(8, False).startswith(
        mark.id_bytewords_minimal(4, False)
    )


@pytest.mark.parametrize(
    ("method_name", "count"),
    (
        ("id_bytewords", 3),
        ("id_bytewords", 33),
        ("id_bytemoji", 33),
        ("id_bytewords_minimal", 3),
    ),
)
def test_identifier_length_checks(method_name: str, count: int) -> None:
    mark = _make_marks(1)[0]
    with pytest.raises(ValueError, match=r"word_count must be 4..=32"):
        getattr(mark, method_name)(count, False)


def test_disambiguated_no_collisions() -> None:
    identifiers = ProvenanceMark.disambiguated_id_bytewords(_make_marks(5), False)
    assert len(identifiers) == 5
    assert all(len(identifier.split(" ")) == 4 for identifier in identifiers)


def test_disambiguated_empty() -> None:
    assert ProvenanceMark.disambiguated_id_bytewords([], False) == []


def test_disambiguated_single_mark() -> None:
    identifiers = ProvenanceMark.disambiguated_id_bytewords(_make_marks(1), False)
    assert len(identifiers) == 1
    assert len(identifiers[0].split(" ")) == 4


def test_disambiguated_selective_extension() -> None:
    marks = _make_marks(5)
    identifiers = ProvenanceMark.disambiguated_id_bytewords(marks, False)
    assert all(len(identifier.split(" ")) == 4 for identifier in identifiers)

    identifiers = ProvenanceMark.disambiguated_id_bytewords(
        [marks[0], marks[1], marks[2], marks[0]],
        False,
    )
    assert len(identifiers[1].split(" ")) == 4
    assert len(identifiers[2].split(" ")) == 4
    assert len(identifiers[0].split(" ")) == 32
    assert identifiers[0] == identifiers[3]


def test_disambiguated_all_results_unique_except_identical() -> None:
    identifiers = ProvenanceMark.disambiguated_id_bytewords(_make_marks(10), False)
    assert len(set(identifiers)) == len(identifiers)


def test_disambiguated_bytemoji_same_prefix_lengths() -> None:
    marks = _make_marks(3)
    word_ids = ProvenanceMark.disambiguated_id_bytewords(
        [marks[0], marks[1], marks[0]],
        False,
    )
    emoji_ids = ProvenanceMark.disambiguated_id_bytemoji(
        [marks[0], marks[1], marks[0]],
        False,
    )
    assert len(word_ids) == len(emoji_ids)
    for word_id, emoji_id in zip(word_ids, emoji_ids):
        assert len(word_id.split(" ")) == len(emoji_id.split(" "))


def test_disambiguated_with_prefix() -> None:
    marks = _make_marks(3)
    without_prefix = ProvenanceMark.disambiguated_id_bytewords(marks, False)
    with_prefix = ProvenanceMark.disambiguated_id_bytewords(marks, True)
    for bare, prefixed in zip(without_prefix, with_prefix):
        assert prefixed.startswith(PROVENANCE_MARK_PREFIX + " ")
        assert prefixed.removeprefix(PROVENANCE_MARK_PREFIX + " ") == bare
