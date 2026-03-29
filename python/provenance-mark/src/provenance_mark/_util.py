"""Utility helpers for provenance-mark."""

from __future__ import annotations

import base64
import json
from typing import Any

from bc_ur import UR
from dcbor import CBOR, Date

from ._error import Error
from ._seed import ProvenanceSeed


def b64encode(data: bytes | bytearray) -> str:
    return base64.b64encode(bytes(data)).decode("ascii")


def b64decode(value: str) -> bytes:
    try:
        return base64.b64decode(value, validate=True)
    except Exception as exc:  # pragma: no cover - stdlib detail
        raise Error("Base64", f"base64 decoding error: {exc}") from exc


def parse_seed(value: str) -> ProvenanceSeed:
    try:
        return ProvenanceSeed.from_json(json.loads(json.dumps(value)))
    except Exception as exc:
        raise Error("InvalidSeedLength", str(exc)) from exc


def parse_date(value: str) -> Date:
    try:
        return Date.from_string(value)
    except Exception as exc:
        raise Error("InvalidDate", str(exc)) from exc


def to_cbor_value(value: Any) -> CBOR:
    if isinstance(value, CBOR):
        return value
    if hasattr(value, "to_cbor"):
        return value.to_cbor()
    if isinstance(value, Date):
        return value.to_tagged_cbor()
    return CBOR.from_value(value)


def serialize_base64(data: bytes | bytearray) -> str:
    return b64encode(data)


def deserialize_base64(value: str) -> bytes:
    return b64decode(value)


def serialize_cbor(data: bytes | bytearray) -> str:
    return b64encode(data)


def deserialize_cbor(value: str) -> bytes:
    decoded = b64decode(value)
    CBOR.from_data(decoded)
    return decoded


def serialize_iso8601(date: Date) -> str:
    return str(date)


def deserialize_iso8601(value: str) -> Date:
    return Date.from_string(value)


def serialize_ur(ur: UR) -> str:
    return ur.string()


def deserialize_ur(value: str) -> UR:
    try:
        return UR.from_ur_string(value)
    except Exception as exc:
        raise Error("Url", f"URL parsing error: {exc}") from exc

