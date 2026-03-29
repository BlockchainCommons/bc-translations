"""Shared pytest fixtures for provenance-mark tests."""

from __future__ import annotations

import json
from pathlib import Path

import pytest

from provenance_mark import ProvenanceMark

REPO_ROOT = Path(__file__).resolve().parents[3]
KOTLIN_FIXTURES = REPO_ROOT / "kotlin" / "provenance-mark" / "bin" / "test"


@pytest.fixture(autouse=True)
def _register_tags() -> None:
    ProvenanceMark.register_tags()


@pytest.fixture(scope="session")
def mark_vectors() -> dict[str, dict[str, object]]:
    return json.loads((KOTLIN_FIXTURES / "mark_vectors.json").read_text())


@pytest.fixture(scope="session")
def validate_expected() -> dict[str, dict[str, str]]:
    return json.loads((KOTLIN_FIXTURES / "validate_expected.json").read_text())
