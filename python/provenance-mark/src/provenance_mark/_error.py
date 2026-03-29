"""Error types for provenance-mark."""

from __future__ import annotations


class Error(Exception):
    """Base exception for provenance-mark failures."""

    __slots__ = ("code", "validation_issue")

    def __init__(
        self,
        code: str,
        message: str,
        validation_issue: object | None = None,
    ) -> None:
        super().__init__(message)
        self.code = code
        self.validation_issue = validation_issue


ProvenanceMarkError = Error

