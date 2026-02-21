from __future__ import annotations

import math

from .float_utils import f64_cbor_data
from .varint import MajorType, encode_varint


class Simple:
    __slots__ = ("_kind", "_float_val")

    _KIND_FALSE = 0
    _KIND_TRUE = 1
    _KIND_NULL = 2
    _KIND_FLOAT = 3

    def __init__(self, kind: int, float_val: float | None = None) -> None:
        self._kind = kind
        self._float_val = float_val

    @staticmethod
    def make_false() -> Simple:
        return Simple(Simple._KIND_FALSE)

    @staticmethod
    def make_true() -> Simple:
        return Simple(Simple._KIND_TRUE)

    @staticmethod
    def make_null() -> Simple:
        return Simple(Simple._KIND_NULL)

    @staticmethod
    def make_float(value: float) -> Simple:
        return Simple(Simple._KIND_FLOAT, value)

    @property
    def is_false(self) -> bool:
        return self._kind == self._KIND_FALSE

    @property
    def is_true(self) -> bool:
        return self._kind == self._KIND_TRUE

    @property
    def is_null(self) -> bool:
        return self._kind == self._KIND_NULL

    @property
    def is_float(self) -> bool:
        return self._kind == self._KIND_FLOAT

    @property
    def is_nan(self) -> bool:
        return self._kind == self._KIND_FLOAT and math.isnan(self._float_val)

    @property
    def float_value(self) -> float | None:
        if self._kind == self._KIND_FLOAT:
            return self._float_val
        return None

    def cbor_data(self) -> bytes:
        match self._kind:
            case Simple._KIND_FALSE:
                return encode_varint(20, MajorType.SIMPLE)
            case Simple._KIND_TRUE:
                return encode_varint(21, MajorType.SIMPLE)
            case Simple._KIND_NULL:
                return encode_varint(22, MajorType.SIMPLE)
            case Simple._KIND_FLOAT:
                return f64_cbor_data(self._float_val)

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, Simple):
            return NotImplemented
        if self._kind != other._kind:
            return False
        if self._kind == self._KIND_FLOAT:
            return self._float_val == other._float_val or (
                math.isnan(self._float_val) and math.isnan(other._float_val)
            )
        return True

    def __hash__(self) -> int:
        if self._kind == self._KIND_FLOAT:
            import struct
            return hash((self._kind, struct.pack(">d", self._float_val)))
        return hash(self._kind)

    def __repr__(self) -> str:
        match self._kind:
            case Simple._KIND_FALSE:
                return "false"
            case Simple._KIND_TRUE:
                return "true"
            case Simple._KIND_NULL:
                return "null"
            case Simple._KIND_FLOAT:
                if math.isnan(self._float_val):
                    return "NaN"
                return repr(self._float_val)

    def __str__(self) -> str:
        match self._kind:
            case Simple._KIND_FALSE:
                return "false"
            case Simple._KIND_TRUE:
                return "true"
            case Simple._KIND_NULL:
                return "null"
            case Simple._KIND_FLOAT:
                v = self._float_val
                if math.isnan(v):
                    return "NaN"
                if math.isinf(v):
                    return "Infinity" if v > 0 else "-Infinity"
                return repr(v)
