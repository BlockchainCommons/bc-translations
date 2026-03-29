"""Tests for provenance marks and generators."""

from __future__ import annotations

import json
from datetime import datetime, timedelta, timezone

import pytest
from dcbor import Date

from provenance_mark import (
    ProvenanceMark,
    ProvenanceMarkGenerator,
    ProvenanceMarkInfo,
    ProvenanceMarkResolution,
    ProvenanceSeed,
)

VECTOR_CASES = (
    "test_low",
    "test_low_with_info",
    "test_medium",
    "test_medium_with_info",
    "test_quartile",
    "test_quartile_with_info",
    "test_high",
    "test_high_with_info",
)

RESOLUTION_BY_NAME = {
    "low": ProvenanceMarkResolution.Low,
    "medium": ProvenanceMarkResolution.Medium,
    "quartile": ProvenanceMarkResolution.Quartile,
    "high": ProvenanceMarkResolution.High,
}


def _test_date(index: int) -> Date:
    base = datetime(2023, 6, 20, 12, 0, 0, tzinfo=timezone.utc)
    return Date.from_datetime(base + timedelta(days=index))


def _make_marks(
    resolution: ProvenanceMarkResolution,
    *,
    include_info: bool,
) -> list[ProvenanceMark]:
    generator = ProvenanceMarkGenerator.new_with_passphrase(resolution, "Wolf")
    encoded = json.dumps(generator.to_json())
    info = "Lorem ipsum sit dolor amet." if include_info else None
    marks: list[ProvenanceMark] = []
    for index in range(10):
        generator = ProvenanceMarkGenerator.from_json(encoded)
        marks.append(generator.next(_test_date(index), info))
        encoded = json.dumps(generator.to_json())
    return marks


@pytest.mark.parametrize("case_name", VECTOR_CASES)
def test_mark_vectors(
    case_name: str,
    mark_vectors: dict[str, dict[str, object]],
) -> None:
    case = mark_vectors[case_name]
    resolution = RESOLUTION_BY_NAME[str(case["resolution"])]
    marks = _make_marks(resolution, include_info=bool(case["include_info"]))

    assert ProvenanceMark.is_sequence_valid(marks)
    assert marks[1].precedes(marks[0]) is False
    assert all(str(mark).startswith("ProvenanceMark(") for mark in marks)

    assert [repr(mark) for mark in marks] == case["expected_debug"]
    assert [mark.to_bytewords() for mark in marks] == case["expected_bytewords"]
    assert [mark.id_bytewords(4, False) for mark in marks] == case["expected_id_words"]
    assert [mark.id_bytemoji(4, False) for mark in marks] == case["expected_bytemoji_ids"]
    assert [mark.ur_string() for mark in marks] == case["expected_urs"]
    assert [mark.to_url("https://example.com/validate") for mark in marks] == case["expected_urls"]

    for mark in marks:
        assert ProvenanceMark.from_message(resolution, mark.message()) == mark
        assert ProvenanceMark.from_bytewords(resolution, mark.to_bytewords()) == mark
        assert ProvenanceMark.from_ur_string(mark.ur_string()) == mark
        assert ProvenanceMark.from_url(mark.to_url("https://example.com/validate")) == mark
        assert ProvenanceMark.from_json(json.dumps(mark.to_json())) == mark


def test_envelope_and_mark_info_round_trip() -> None:
    generator = ProvenanceMarkGenerator.new_with_seed(
        ProvenanceMarkResolution.High,
        ProvenanceSeed.new_with_passphrase("test"),
    )
    mark = generator.next(Date.from_string("2025-10-26"), "Info field content")

    assert ProvenanceMarkGenerator.from_envelope(generator.to_envelope()) == generator
    assert ProvenanceMark.from_envelope(mark.to_envelope()) == mark
    assert mark.info() is not None
    assert mark.info().try_text() == "Info field content"
    assert mark.id_hex() in mark.to_envelope().format()

    info = ProvenanceMarkInfo.new(mark, "Commentary")
    summary = info.markdown_summary()
    assert str(info.ur()) in summary
    assert info.bytewords() in summary
    assert info.bytemoji() in summary
    assert info.comment() == "Commentary"
    assert ProvenanceMarkInfo.from_json(json.dumps(info.to_json())) == info
