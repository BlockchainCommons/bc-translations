from __future__ import annotations

import math
import struct

from .varint import MajorType, encode_varint

CBOR_NAN = bytes([0xF9, 0x7E, 0x00])

_U16_MAX = 0xFFFF
_U32_MAX = 0xFFFFFFFF
_U64_MAX = 0xFFFFFFFFFFFFFFFF


def _f16_bits(value: float) -> int:
    return struct.unpack(">H", struct.pack(">e", value))[0]


def _f32_bits(value: float) -> int:
    return struct.unpack(">I", struct.pack(">f", value))[0]


def _f64_bits(value: float) -> int:
    return struct.unpack(">Q", struct.pack(">d", value))[0]


def f16_from_bits(bits: int) -> float:
    return struct.unpack(">e", struct.pack(">H", bits))[0]


def f32_from_bits(bits: int) -> float:
    return struct.unpack(">f", struct.pack(">I", bits))[0]


def f64_from_bits(bits: int) -> float:
    return struct.unpack(">d", struct.pack(">Q", bits))[0]


def _round_trip_f32(value: float) -> float:
    try:
        return struct.unpack(">f", struct.pack(">f", value))[0]
    except (OverflowError, struct.error):
        return float("inf") if value > 0 else float("-inf")


def _round_trip_f16(value: float) -> float:
    try:
        return struct.unpack(">e", struct.pack(">e", value))[0]
    except (OverflowError, struct.error):
        return float("inf") if value > 0 else float("-inf")


def _float_eq(a: float, b: float) -> bool:
    return a == b or (math.isnan(a) and math.isnan(b))


def _exact_uint_from_float(value: float, max_val: int) -> int | None:
    if not math.isfinite(value):
        return None
    if value <= -1.0 or value >= float(max_val) + 1.0:
        return None
    frac, _ = math.modf(value)
    if frac != 0.0:
        return None
    return int(value)


def _exact_neg_u64_from_float(value: float) -> int | None:
    if value >= 0.0 or not math.isfinite(value):
        return None
    frac, _ = math.modf(value)
    if frac != 0.0:
        return None
    as_int = int(value)
    neg_val = -1 - as_int
    if neg_val < 0 or neg_val > _U64_MAX:
        return None
    return neg_val


def _encode_unsigned(n: int) -> bytes:
    return encode_varint(n, MajorType.UNSIGNED)


def _encode_negative(n: int) -> bytes:
    return encode_varint(n, MajorType.NEGATIVE)


def _f16_cbor_data(value: float) -> bytes:
    f64_val = value
    neg = _exact_neg_u64_from_float(f64_val)
    if neg is not None:
        return _encode_negative(neg)
    pos = _exact_uint_from_float(f64_val, _U16_MAX)
    if pos is not None:
        return _encode_unsigned(pos)
    if math.isnan(value):
        return CBOR_NAN
    return bytes([0xF9]) + struct.pack(">H", _f16_bits(value))


def _f32_cbor_data(value: float) -> bytes:
    f16_val = _round_trip_f16(value)
    if _float_eq(f16_val, value):
        return _f16_cbor_data(f16_val)
    neg = _exact_neg_u64_from_float(value)
    if neg is not None:
        return _encode_negative(neg)
    pos = _exact_uint_from_float(value, _U32_MAX)
    if pos is not None:
        return _encode_unsigned(pos)
    if math.isnan(value):
        return CBOR_NAN
    return bytes([0xFA]) + struct.pack(">I", _f32_bits(value))


def f64_cbor_data(value: float) -> bytes:
    f32_val = _round_trip_f32(value)
    if _float_eq(f32_val, value):
        return _f32_cbor_data(f32_val)
    neg = _exact_neg_u64_from_float(value)
    if neg is not None:
        return _encode_negative(neg)
    pos = _exact_uint_from_float(value, _U64_MAX)
    if pos is not None:
        return _encode_unsigned(pos)
    if math.isnan(value):
        return CBOR_NAN
    return bytes([0xFB]) + struct.pack(">Q", _f64_bits(value))


def validate_canonical_f64(value: float) -> None:
    from .error import NonCanonicalNumeric
    f32_val = _round_trip_f32(value)
    if f32_val == value:
        raise NonCanonicalNumeric()
    if math.isfinite(value) and value == float(int(value)):
        n = int(value)
        if n >= 0 and n <= _U64_MAX:
            raise NonCanonicalNumeric()
        if n < 0 and 0 <= (-1 - n) <= _U64_MAX:
            raise NonCanonicalNumeric()
    if math.isnan(value):
        raise NonCanonicalNumeric()


def validate_canonical_f32(value: float) -> None:
    from .error import NonCanonicalNumeric
    f16_val = _round_trip_f16(value)
    if f16_val == value:
        raise NonCanonicalNumeric()
    f32_val = _round_trip_f32(value)
    if math.isfinite(f32_val) and f32_val == float(int(f32_val)):
        n = int(f32_val)
        if n >= 0 and n <= _U64_MAX:
            raise NonCanonicalNumeric()
        if n < 0 and 0 <= (-1 - n) <= _U64_MAX:
            raise NonCanonicalNumeric()
    if math.isnan(value):
        raise NonCanonicalNumeric()


def validate_canonical_f16(value: float, bits: int) -> None:
    from .error import NonCanonicalNumeric
    if math.isfinite(value) and value == float(int(value)):
        raise NonCanonicalNumeric()
    if math.isnan(value) and bits != 0x7E00:
        raise NonCanonicalNumeric()
