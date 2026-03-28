"""String utility functions for envelope formatting."""

from __future__ import annotations


def flanked_by(string: str, left: str, right: str) -> str:
    """Return *string* surrounded by *left* and *right*."""
    return f"{left}{string}{right}"
