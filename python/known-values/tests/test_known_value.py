"""Additional tests for KnownValue and KnownValuesStore behavior."""

from __future__ import annotations

import pytest
from dcbor import CBOR

from known_values import (
    ConfigError,
    DirectoryConfig,
    IS_A,
    KNOWN_VALUES,
    KnownValue,
    KnownValuesStore,
    NOTE,
    add_search_paths,
    set_directory_config,
)


def test_known_value_name_and_assigned_name() -> None:
    named_value = KnownValue(1, "isA")
    assert named_value.value == 1
    assert named_value.assigned_name == "isA"
    assert named_value.name == "isA"

    unnamed_value = KnownValue(42)
    assert unnamed_value.assigned_name is None
    assert unnamed_value.name == "42"


def test_known_value_equality_and_hash_ignore_assigned_name() -> None:
    left = KnownValue(100, "left")
    right = KnownValue(100, "right")
    other = KnownValue(101, "left")

    assert left == right
    assert hash(left) == hash(right)
    assert left != other


def test_known_value_cbor_round_trip_and_digest() -> None:
    known_value = KnownValue(500, "fiveHundred")
    digest = known_value.digest()

    round_tripped = KnownValue.from_tagged_cbor_data(
        known_value.tagged_cbor_data()
    )
    assert round_tripped == known_value
    assert round_tripped.assigned_name is None
    assert digest == known_value.digest()
    assert known_value.to_cbor() == known_value.tagged_cbor()
    assert known_value.to_cbor_data() == known_value.tagged_cbor_data()
    assert KnownValue.from_untagged_cbor_data(
        known_value.untagged_cbor().to_cbor_data()
    ) == known_value
    assert KnownValue.from_cbor(known_value.tagged_cbor()) == known_value
    assert int(known_value) == 500
    assert repr(known_value) == "KnownValue(500, 'fiveHundred')"
    assert str(known_value) == "fiveHundred"

    unnamed = KnownValue(9)
    assert repr(unnamed) == "KnownValue(9)"
    assert str(unnamed) == "9"


def test_known_value_from_negative_cbor_raises() -> None:
    with pytest.raises(Exception):
        KnownValue.from_untagged_cbor(CBOR.from_int(-1))


def test_known_value_constructor_validation() -> None:
    with pytest.raises(ValueError):
        KnownValue(True)

    with pytest.raises(ValueError):
        KnownValue(2**64)


def test_known_value_comparison_with_other_type() -> None:
    assert KnownValue(1).__eq__(object()) is NotImplemented


def test_known_values_store_lookup_helpers() -> None:
    store = KnownValuesStore([IS_A, NOTE])

    assert store.assigned_name_for(IS_A) == "isA"
    assert store.name_for(IS_A) == "isA"
    assert store.name_for(KnownValue(999)) == "999"
    assert store.known_value_named("note") == NOTE

    assert KnownValuesStore.known_value_for_raw_value(1, store) == IS_A
    assert KnownValuesStore.known_value_for_raw_value(999, store).name == "999"
    assert KnownValuesStore.known_value_for_name("isA", store) == IS_A
    assert KnownValuesStore.known_value_for_name("missing", store) is None
    assert KnownValuesStore.known_value_for_name("isA", None) is None
    assert KnownValuesStore.name_for_known_value(IS_A, store) == "isA"
    assert (
        KnownValuesStore.name_for_known_value(KnownValue(999), store)
        == "999"
    )


def test_store_overrides_same_raw_value_remove_old_name() -> None:
    store = KnownValuesStore([KnownValue(1, "oldName")])
    store.insert(KnownValue(1, "newName"))

    assert store.known_value_named("oldName") is None
    assert store.known_value_named("newName").value == 1
    assert "KnownValuesStore" in repr(store)


def test_store_len_and_contains() -> None:
    store = KnownValuesStore([IS_A, NOTE])
    assert len(store) == 2
    assert IS_A in store
    assert KnownValue(999) not in store
    assert 1 in store
    assert 999 not in store


def test_configuration_is_locked_after_global_registry_access() -> None:
    KNOWN_VALUES.get()

    with pytest.raises(ConfigError):
        set_directory_config(DirectoryConfig.with_paths([]))

    with pytest.raises(ConfigError):
        add_search_paths([])
