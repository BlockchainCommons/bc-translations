"""Tests translated from `src/known_values_registry.rs`."""

from __future__ import annotations

from known_values import IS_A, KNOWN_VALUES, SELF, VALUE


def test_registry_smoke() -> None:
    assert IS_A.value == 1
    assert IS_A.name == "isA"

    known_values = KNOWN_VALUES.get()
    assert known_values.known_value_named("isA") is not None
    assert known_values.known_value_named("isA").value == 1


def test_known_values_source_behavior_omits_value_and_self() -> None:
    assert VALUE.value == 25
    assert SELF.value == 706

    known_values = KNOWN_VALUES.get()
    assert known_values.known_value_named("value") is None
    assert known_values.known_value_named("Self") is None
