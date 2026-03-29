"""Validation report model for provenance-mark."""

from __future__ import annotations

import json
from dataclasses import dataclass
from enum import Enum

from typing import TYPE_CHECKING

from dcbor import Date

from ._error import Error

if TYPE_CHECKING:
    from ._mark import ProvenanceMark


class ValidationReportFormat(Enum):
    """Output format for validation reports."""

    Text = "Text"
    JsonCompact = "JsonCompact"
    JsonPretty = "JsonPretty"


@dataclass(frozen=True, slots=True)
class ValidationIssue:
    """One validation issue associated with a mark."""

    issue_type: str
    data: dict[str, object] | None = None

    def to_json_obj(self) -> dict[str, object]:
        result: dict[str, object] = {"type": self.issue_type}
        if self.data is not None:
            payload: dict[str, object] = {}
            for key, value in self.data.items():
                if isinstance(value, bytes):
                    payload[key] = value.hex()
                else:
                    payload[key] = str(value) if hasattr(value, "datetime_value") else value
            result["data"] = payload
        return result

    def __str__(self) -> str:
        if self.issue_type == "HashMismatch":
            assert self.data is not None
            return (
                "hash mismatch: expected "
                f"{bytes(self.data['expected']).hex()}, got "
                f"{bytes(self.data['actual']).hex()}"
            )
        if self.issue_type == "KeyMismatch":
            return "key mismatch: current hash was not generated from next key"
        if self.issue_type == "SequenceGap":
            assert self.data is not None
            return (
                f"sequence number gap: expected {self.data['expected']}, "
                f"got {self.data['actual']}"
            )
        if self.issue_type == "DateOrdering":
            assert self.data is not None
            return (
                "date must be equal or later: previous is "
                f"{self.data['previous']}, next is {self.data['next']}"
            )
        if self.issue_type == "NonGenesisAtZero":
            return "non-genesis mark at sequence 0"
        if self.issue_type == "InvalidGenesisKey":
            return "genesis mark must have key equal to chain_id"
        return self.issue_type


def validation_issue_to_string(issue: ValidationIssue) -> str:
    return str(issue)


@dataclass(frozen=True, slots=True)
class FlaggedMark:
    """A mark plus its validation issues."""

    mark: ProvenanceMark
    issues: tuple[ValidationIssue, ...] = ()

    @staticmethod
    def new(mark: ProvenanceMark) -> FlaggedMark:
        return FlaggedMark(mark)

    @staticmethod
    def with_issue(mark: ProvenanceMark, issue: ValidationIssue) -> FlaggedMark:
        return FlaggedMark(mark, (issue,))

    def to_json_obj(self) -> dict[str, object]:
        return {
            "mark": self.mark.ur_string(),
            "issues": [issue.to_json_obj() for issue in self.issues],
        }


@dataclass(frozen=True, slots=True)
class SequenceReport:
    """A contiguous sequence of marks within one chain."""

    start_seq: int
    end_seq: int
    marks: tuple[FlaggedMark, ...]

    def to_json_obj(self) -> dict[str, object]:
        return {
            "start_seq": self.start_seq,
            "end_seq": self.end_seq,
            "marks": [mark.to_json_obj() for mark in self.marks],
        }


@dataclass(frozen=True, slots=True)
class ChainReport:
    """Validation report for one chain ID."""

    chain_id: bytes
    has_genesis: bool
    marks: tuple[ProvenanceMark, ...]
    sequences: tuple[SequenceReport, ...]

    def chain_id_hex(self) -> str:
        return self.chain_id.hex()

    def to_json_obj(self) -> dict[str, object]:
        return {
            "chain_id": self.chain_id.hex(),
            "has_genesis": self.has_genesis,
            "marks": [mark.ur_string() for mark in self.marks],
            "sequences": [sequence.to_json_obj() for sequence in self.sequences],
        }


@dataclass(frozen=True, slots=True)
class ValidationReport:
    """Full validation report for a set of provenance marks."""

    marks: tuple[ProvenanceMark, ...]
    chains: tuple[ChainReport, ...]

    @staticmethod
    def validate(marks: list[ProvenanceMark]) -> ValidationReport:
        seen: set[ProvenanceMark] = set()
        deduplicated: list[ProvenanceMark] = []
        for mark in marks:
            if mark not in seen:
                seen.add(mark)
                deduplicated.append(mark)

        chain_bins: dict[bytes, list[ProvenanceMark]] = {}
        for mark in deduplicated:
            chain_bins.setdefault(mark.chain_id(), []).append(mark)

        chains: list[ChainReport] = []
        for chain_id, chain_marks in chain_bins.items():
            chain_marks.sort(key=lambda mark: mark.seq())
            first = chain_marks[0]
            has_genesis = first.seq() == 0 and first.is_genesis()
            chains.append(
                ChainReport(
                    chain_id=chain_id,
                    has_genesis=has_genesis,
                    marks=tuple(chain_marks),
                    sequences=tuple(_build_sequence_bins(chain_marks)),
                )
            )

        chains.sort(key=lambda chain: chain.chain_id)
        return ValidationReport(tuple(deduplicated), tuple(chains))

    def format(self, report_format: ValidationReportFormat) -> str:
        if report_format is ValidationReportFormat.Text:
            return self._format_text()
        json_obj = self.to_json_obj()
        if report_format is ValidationReportFormat.JsonCompact:
            return json.dumps(json_obj, separators=(",", ":"), ensure_ascii=False)
        return json.dumps(json_obj, indent=2, ensure_ascii=False)

    def _format_text(self) -> str:
        if not self._is_interesting():
            return ""

        lines: list[str] = [
            f"Total marks: {len(self.marks)}",
            f"Chains: {len(self.chains)}",
            "",
        ]
        for chain_index, chain in enumerate(self.chains, start=1):
            short_chain_id = chain.chain_id_hex()[:8]
            lines.append(f"Chain {chain_index}: {short_chain_id}")
            if not chain.has_genesis:
                lines.append("  Warning: No genesis mark found")
            for sequence in chain.sequences:
                for flagged_mark in sequence.marks:
                    mark = flagged_mark.mark
                    annotations: list[str] = []
                    if mark.is_genesis():
                        annotations.append("genesis mark")
                    for issue in flagged_mark.issues:
                        if issue.issue_type == "SequenceGap":
                            assert issue.data is not None
                            annotations.append(f"gap: {issue.data['expected']} missing")
                        elif issue.issue_type == "DateOrdering":
                            assert issue.data is not None
                            annotations.append(
                                f"date {issue.data['previous']} < {issue.data['next']}"
                            )
                        elif issue.issue_type == "HashMismatch":
                            annotations.append("hash mismatch")
                        elif issue.issue_type == "KeyMismatch":
                            annotations.append("key mismatch")
                        elif issue.issue_type == "NonGenesisAtZero":
                            annotations.append("non-genesis at seq 0")
                        elif issue.issue_type == "InvalidGenesisKey":
                            annotations.append("invalid genesis key")
                    if annotations:
                        lines.append(
                            f"  {mark.seq()}: {mark.id_hex()[:8]} ({', '.join(annotations)})"
                        )
                    else:
                        lines.append(f"  {mark.seq()}: {mark.id_hex()[:8]}")
            lines.append("")
        return "\n".join(lines).rstrip()

    def _is_interesting(self) -> bool:
        if not self.chains:
            return False
        if any(not chain.has_genesis for chain in self.chains):
            return True
        if len(self.chains) == 1:
            chain = self.chains[0]
            if len(chain.sequences) == 1 and all(
                not flagged.issues for flagged in chain.sequences[0].marks
            ):
                return False
        return True

    def has_issues(self) -> bool:
        if any(not chain.has_genesis for chain in self.chains):
            return True
        for chain in self.chains:
            for sequence in chain.sequences:
                if any(flagged.issues for flagged in sequence.marks):
                    return True
        if len(self.chains) > 1:
            return True
        return len(self.chains) == 1 and len(self.chains[0].sequences) > 1

    def to_json_obj(self) -> dict[str, object]:
        return {
            "marks": [mark.ur_string() for mark in self.marks],
            "chains": [chain.to_json_obj() for chain in self.chains],
        }


def _sequence_gap(expected: int, actual: int) -> ValidationIssue:
    return ValidationIssue("SequenceGap", {"expected": expected, "actual": actual})


def _hash_mismatch(expected: bytes, actual: bytes) -> ValidationIssue:
    return ValidationIssue("HashMismatch", {"expected": expected, "actual": actual})


def _date_ordering(previous: Date, next_: Date) -> ValidationIssue:
    return ValidationIssue("DateOrdering", {"previous": previous, "next": next_})


def _build_sequence_bins(marks: list[ProvenanceMark]) -> list[SequenceReport]:
    sequences: list[SequenceReport] = []
    current: list[FlaggedMark] = []
    for index, mark in enumerate(marks):
        if index == 0:
            current.append(FlaggedMark.new(mark))
            continue
        previous = marks[index - 1]
        try:
            previous.precedes_opt(mark)
            current.append(FlaggedMark.new(mark))
        except Error as exc:
            if current:
                sequences.append(_create_sequence_report(current))
            issue = (
                exc.validation_issue
                if isinstance(exc.validation_issue, ValidationIssue)
                else ValidationIssue("KeyMismatch")
            )
            current = [FlaggedMark.with_issue(mark, issue)]
    if current:
        sequences.append(_create_sequence_report(current))
    return sequences


def _create_sequence_report(marks: list[FlaggedMark]) -> SequenceReport:
    start_seq = marks[0].mark.seq() if marks else 0
    end_seq = marks[-1].mark.seq() if marks else 0
    return SequenceReport(start_seq, end_seq, tuple(marks))

