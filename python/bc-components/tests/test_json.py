"""Tests for JSON, translated from Rust bc-components/src/json.rs tests."""

from bc_components import JSON


def test_json_creation():
    """Test creating JSON from a string."""
    json = JSON.from_string('{"key": "value"}')
    assert json.as_str() == '{"key": "value"}'
    assert len(json) == 16
    assert json  # non-empty JSON is truthy


def test_json_from_bytes():
    """Test creating JSON from byte data."""
    data = b"[1, 2, 3]"
    json = JSON.from_data(data)
    assert json.data == data
    assert json.as_str() == "[1, 2, 3]"


def test_json_empty():
    """Test creating empty JSON."""
    json = JSON.from_string("")
    assert not json  # empty JSON is falsy
    assert len(json) == 0


def test_json_cbor_roundtrip():
    """Test CBOR encoding/decoding roundtrip."""
    json = JSON.from_string('{"name":"Alice","age":30}')
    cbor = json.tagged_cbor()
    json2 = JSON.from_tagged_cbor(cbor)
    assert json == json2


def test_json_hex():
    """Test hex encoding/decoding roundtrip."""
    json = JSON.from_string("test")
    hex_str = json.hex()
    json2 = JSON.from_hex(hex_str)
    assert json == json2


def test_json_debug():
    """Test debug representation."""
    json = JSON.from_string('{"test":true}')
    debug = repr(json)
    assert debug == 'JSON({"test":true})'


def test_json_clone():
    """Test that cloned JSON is equal."""
    json = JSON.from_string("original")
    import copy
    json2 = copy.copy(json)
    assert json == json2


def test_json_into_vec():
    """Test converting JSON into bytes."""
    json = JSON.from_string("data")
    assert bytes(json) == b"data"
