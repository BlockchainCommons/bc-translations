"""Tests for validation reporting."""

from __future__ import annotations

import json
from datetime import datetime, timedelta, timezone
from textwrap import dedent

import pytest
from dcbor import Date

from provenance_mark import (
    Error,
    ProvenanceMark,
    ProvenanceMarkGenerator,
    ProvenanceMarkResolution,
    ValidationReport,
    ValidationReportFormat,
)


def _test_date(index: int) -> Date:
    base = datetime(2023, 6, 20, 12, 0, 0, tzinfo=timezone.utc)
    return Date.from_datetime(base + timedelta(days=index))


def _create_test_marks(
    count: int,
    resolution: ProvenanceMarkResolution,
    passphrase: str,
    *,
    info: object | None = None,
) -> list[ProvenanceMark]:
    generator = ProvenanceMarkGenerator.new_with_passphrase(resolution, passphrase)
    return [generator.next(_test_date(index), info) for index in range(count)]


def _expected_text(block: str) -> str:
    stripped = block.strip()
    if not stripped:
        return ""

    lines = stripped.splitlines()
    first = lines[0].strip()
    rest = lines[1:]
    non_empty = [line for line in rest if line.strip()]
    if non_empty:
        indent = min(len(line) - len(line.lstrip()) for line in non_empty)
        rest = [line[indent:] if len(line) >= indent else line for line in rest]
    return "\n".join([first, *rest]).rstrip()


def _assert_report(
    report: ValidationReport,
    expected: dict[str, dict[str, str]],
    case_name: str,
) -> None:
    case = expected[case_name]
    if "json_pretty" in case:
        assert report.format(ValidationReportFormat.JsonPretty) == json.dumps(
            json.loads(case["json_pretty"]),
            indent=2,
            ensure_ascii=False,
        )
    if "json_compact" in case:
        assert report.format(ValidationReportFormat.JsonCompact) == json.dumps(
            json.loads(case["json_compact"]),
            separators=(",", ":"),
            ensure_ascii=False,
        )
    if "text" in case:
        assert report.format(ValidationReportFormat.Text) == _expected_text(case["text"])


def test_validate_empty(validate_expected: dict[str, dict[str, str]]) -> None:
    report = ValidationReport.validate([])
    assert ProvenanceMark.validate([]).format(ValidationReportFormat.JsonPretty) == (
        report.format(ValidationReportFormat.JsonPretty)
    )
    _assert_report(report, validate_expected, "test_validate_empty")


def test_validate_single_mark(validate_expected: dict[str, dict[str, str]]) -> None:
    report = ValidationReport.validate(
        _create_test_marks(1, ProvenanceMarkResolution.Low, "test")
    )
    _assert_report(report, validate_expected, "test_validate_single_mark")


def test_validate_valid_sequence(validate_expected: dict[str, dict[str, str]]) -> None:
    report = ValidationReport.validate(
        _create_test_marks(5, ProvenanceMarkResolution.Low, "test")
    )
    _assert_report(report, validate_expected, "test_validate_valid_sequence")


def test_validate_deduplication(validate_expected: dict[str, dict[str, str]]) -> None:
    marks = _create_test_marks(3, ProvenanceMarkResolution.Low, "test")
    report = ValidationReport.validate([*marks, marks[0], marks[1], marks[0]])
    _assert_report(report, validate_expected, "test_validate_deduplication")


def test_validate_multiple_chains(validate_expected: dict[str, dict[str, str]]) -> None:
    marks = _create_test_marks(3, ProvenanceMarkResolution.Low, "alice")
    marks.extend(_create_test_marks(3, ProvenanceMarkResolution.Low, "bob"))
    report = ValidationReport.validate(marks)
    _assert_report(report, validate_expected, "test_validate_multiple_chains")


def test_validate_missing_genesis(validate_expected: dict[str, dict[str, str]]) -> None:
    report = ValidationReport.validate(
        _create_test_marks(5, ProvenanceMarkResolution.Low, "test")[1:]
    )
    _assert_report(report, validate_expected, "test_validate_missing_genesis")


def test_validate_sequence_gap(validate_expected: dict[str, dict[str, str]]) -> None:
    marks = _create_test_marks(5, ProvenanceMarkResolution.Low, "test")
    report = ValidationReport.validate([marks[0], marks[1], marks[3], marks[4]])
    _assert_report(report, validate_expected, "test_validate_sequence_gap")


def test_validate_out_of_order(validate_expected: dict[str, dict[str, str]]) -> None:
    marks = _create_test_marks(5, ProvenanceMarkResolution.Low, "test")
    report = ValidationReport.validate([marks[0], marks[1], marks[3], marks[2], marks[4]])
    _assert_report(report, validate_expected, "test_validate_out_of_order")


def test_validate_hash_mismatch(validate_expected: dict[str, dict[str, str]]) -> None:
    marks = _create_test_marks(3, ProvenanceMarkResolution.Low, "test")
    bad_mark = ProvenanceMark.new(
        marks[1].res(),
        marks[1].key(),
        marks[0].hash(),
        marks[1].chain_id(),
        2,
        _test_date(2),
    )
    report = ValidationReport.validate([marks[0], marks[1], bad_mark])
    _assert_report(report, validate_expected, "test_validate_hash_mismatch")


def test_validate_date_ordering_violation(
    validate_expected: dict[str, dict[str, str]],
) -> None:
    report = ValidationReport.validate(
        _create_test_marks(3, ProvenanceMarkResolution.Low, "test")
    )
    _assert_report(report, validate_expected, "test_validate_date_ordering_violation")


def test_validate_multiple_sequences_in_chain(
    validate_expected: dict[str, dict[str, str]],
) -> None:
    marks = _create_test_marks(7, ProvenanceMarkResolution.Low, "test")
    report = ValidationReport.validate([marks[0], marks[1], marks[3], marks[4], marks[6]])
    _assert_report(report, validate_expected, "test_validate_multiple_sequences_in_chain")


def test_validate_precedes_opt() -> None:
    marks = _create_test_marks(3, ProvenanceMarkResolution.Low, "test")
    marks[0].precedes_opt(marks[1])
    marks[1].precedes_opt(marks[2])

    with pytest.raises(Error):
        marks[1].precedes_opt(marks[0])
    with pytest.raises(Error):
        marks[0].precedes_opt(marks[2])


def test_validate_chain_id_hex() -> None:
    marks = _create_test_marks(2, ProvenanceMarkResolution.Low, "test")
    report = ValidationReport.validate(marks)
    chain_id_hex = report.chains[0].chain_id_hex()
    assert all(character in "0123456789abcdef" for character in chain_id_hex)
    assert chain_id_hex == marks[0].chain_id().hex()


def test_validate_with_info(validate_expected: dict[str, dict[str, str]]) -> None:
    report = ValidationReport.validate(
        _create_test_marks(
            3,
            ProvenanceMarkResolution.Low,
            "test",
            info="Test info",
        )
    )
    _assert_report(report, validate_expected, "test_validate_with_info")


def test_validate_sorted_chains(validate_expected: dict[str, dict[str, str]]) -> None:
    marks = _create_test_marks(2, ProvenanceMarkResolution.Low, "zebra")
    marks.extend(_create_test_marks(2, ProvenanceMarkResolution.Low, "apple"))
    marks.extend(_create_test_marks(2, ProvenanceMarkResolution.Low, "middle"))
    report = ValidationReport.validate(marks)
    _assert_report(report, validate_expected, "test_validate_sorted_chains")


def test_validate_genesis_check(validate_expected: dict[str, dict[str, str]]) -> None:
    marks = _create_test_marks(3, ProvenanceMarkResolution.Low, "test")
    with_genesis = ValidationReport.validate(marks)
    assert with_genesis.chains[0].has_genesis is True

    without_genesis = ValidationReport.validate(marks[1:])
    assert without_genesis.chains[0].has_genesis is False
    _assert_report(without_genesis, validate_expected, "test_validate_genesis_check")


def test_validate_date_ordering_violation_constructed(
    validate_expected: dict[str, dict[str, str]],
) -> None:
    marks = _create_test_marks(2, ProvenanceMarkResolution.Low, "test")
    generator = ProvenanceMarkGenerator.new_with_passphrase(
        ProvenanceMarkResolution.Low,
        "test",
    )
    generator.next(marks[0].date(), None)
    earlier = Date.from_datetime(datetime(2023, 6, 19, 12, 0, 0, tzinfo=timezone.utc))
    bad_mark = generator.next(earlier, None)
    report = ValidationReport.validate([marks[0], bad_mark])
    _assert_report(
        report,
        validate_expected,
        "test_validate_date_ordering_violation_constructed",
    )


def test_validate_non_genesis_at_seq_zero(
    validate_expected: dict[str, dict[str, str]],
) -> None:
    marks = _create_test_marks(2, ProvenanceMarkResolution.Low, "test")
    bad_mark = ProvenanceMark.new(
        marks[1].res(),
        marks[1].key(),
        marks[1].hash(),
        marks[1].chain_id(),
        0,
        _test_date(1),
    )
    report = ValidationReport.validate([marks[0], bad_mark])
    _assert_report(report, validate_expected, "test_validate_non_genesis_at_seq_zero")


def test_validate_invalid_genesis_key_constructed(
    validate_expected: dict[str, dict[str, str]],
) -> None:
    marks = _create_test_marks(2, ProvenanceMarkResolution.Low, "test")
    bad_mark = ProvenanceMark.new(
        marks[1].res(),
        marks[1].chain_id(),
        marks[1].hash(),
        marks[1].chain_id(),
        1,
        _test_date(1),
    )
    report = ValidationReport.validate([marks[0], bad_mark])
    _assert_report(
        report,
        validate_expected,
        "test_validate_invalid_genesis_key_constructed",
    )
