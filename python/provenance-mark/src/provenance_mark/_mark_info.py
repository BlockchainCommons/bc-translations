"""Convenience summary wrapper for provenance marks."""

from __future__ import annotations

import json
from dataclasses import dataclass

from bc_ur import UR

from ._mark import ProvenanceMark


@dataclass(frozen=True, slots=True)
class ProvenanceMarkInfo:
    """Summary information for a provenance mark."""

    _mark: ProvenanceMark
    _ur: UR
    _bytewords: str
    _bytemoji: str
    _comment: str

    @staticmethod
    def new(mark: ProvenanceMark, comment: str) -> ProvenanceMarkInfo:
        return ProvenanceMarkInfo(
            mark,
            mark.to_ur(),
            mark.id_bytewords(4, True),
            mark.id_bytemoji(4, True),
            comment,
        )

    def mark(self) -> ProvenanceMark:
        return self._mark

    def ur(self) -> UR:
        return self._ur

    def bytewords(self) -> str:
        return self._bytewords

    def bytemoji(self) -> str:
        return self._bytemoji

    def comment(self) -> str:
        return self._comment

    def markdown_summary(self) -> str:
        lines = [
            "---",
            "",
            str(self._mark.date()),
            "",
            f"#### {self._ur}",
            "",
            f"#### `{self._bytewords}`",
            "",
            self._bytemoji,
            "",
        ]
        if self._comment:
            lines.extend([self._comment, ""])
        return "\n".join(lines)

    def to_json(self) -> dict[str, object]:
        result: dict[str, object] = {
            "ur": str(self._ur),
            "bytewords": self._bytewords,
            "bytemoji": self._bytemoji,
            "mark": self._mark.to_json(),
        }
        if self._comment:
            result["comment"] = self._comment
        return result

    @staticmethod
    def from_json(value: str | dict[str, object]) -> ProvenanceMarkInfo:
        payload = value if isinstance(value, dict) else json.loads(value)
        ur_value = UR.from_ur_string(str(payload["ur"]))
        comment = str(payload.get("comment", ""))
        return ProvenanceMarkInfo(
            ProvenanceMark.from_ur(ur_value),
            ur_value,
            str(payload["bytewords"]),
            str(payload["bytemoji"]),
            comment,
        )

