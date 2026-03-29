"""Tests for utility and support types."""

from __future__ import annotations

import base64
from datetime import datetime, timezone

import pytest
from dcbor import Date

from provenance_mark import (
    Error,
    ProvenanceMarkResolution,
    ProvenanceSeed,
    RngState,
    deserialize_date,
    deserialize_seq,
    parse_date,
    parse_seed,
    resolution_from_cbor,
    resolution_from_u8,
    resolution_to_cbor,
    serialize_date,
    serialize_seq,
)
from provenance_mark._util import deserialize_cbor, serialize_cbor


def test_error_model() -> None:
    issue = object()
    error = Error("TestCode", "test message", issue)
    assert error.code == "TestCode"
    assert error.validation_issue is issue
    assert str(error) == "test message"


def test_provenance_seed_round_trip() -> None:
    seed = ProvenanceSeed.new_with_passphrase("test")
    assert len(seed.to_bytes()) == 32
    assert ProvenanceSeed.from_json(seed.to_json()) == seed
    assert ProvenanceSeed.from_cbor(seed.to_cbor()) == seed
    assert parse_seed(seed.to_json()) == seed


def test_parse_seed_rejects_invalid_length() -> None:
    invalid = base64.b64encode(b"short").decode("ascii")
    with pytest.raises(Error) as exc:
        parse_seed(invalid)
    assert exc.value.code == "InvalidSeedLength"


def test_parse_date_round_trip() -> None:
    date = parse_date("2023-06-20T12:34:56Z")
    assert date == Date.from_datetime(
        datetime(2023, 6, 20, 12, 34, 56, tzinfo=timezone.utc)
    )

    with pytest.raises(Error) as exc:
        parse_date("not-a-date")
    assert exc.value.code == "InvalidDate"


def test_resolution_helpers() -> None:
    date = Date.from_string("2023-06-20T12:00:00Z")
    for resolution in ProvenanceMarkResolution:
        assert resolution_from_u8(int(resolution)) == resolution
        assert resolution_from_cbor(resolution_to_cbor(resolution)) == resolution

        seq = 42
        assert deserialize_seq(resolution, serialize_seq(resolution, seq)) == seq
        assert deserialize_date(resolution, serialize_date(resolution, date)) == (
            resolution.deserialize_date(resolution.serialize_date(date))
        )

    with pytest.raises(Error):
        serialize_seq(ProvenanceMarkResolution.Low, 0x1_0000)


def test_serialize_and_deserialize_cbor() -> None:
    state = RngState.from_bytes(bytes(range(32)))
    encoded = serialize_cbor(state.to_cbor_data())
    assert deserialize_cbor(encoded) == state.to_cbor_data()
