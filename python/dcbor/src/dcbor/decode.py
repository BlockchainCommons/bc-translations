from __future__ import annotations

import struct

from .cbor import CBOR, CBORCase
from .byte_string import ByteString
from .error import (
    InvalidSimpleValue,
    InvalidString,
    NonCanonicalNumeric,
    NonCanonicalString,
    Underrun,
    UnusedData,
    UnsupportedHeaderValue,
)
from .float_utils import (
    f16_from_bits,
    f32_from_bits,
    f64_from_bits,
    validate_canonical_f16,
    validate_canonical_f32,
    validate_canonical_f64,
)
from .map import Map
from .simple import Simple
from .string_util import is_nfc
from .tag import Tag
from .varint import MajorType


def decode_cbor(data: bytes | bytearray) -> CBOR:
    """Decode bytes into a CBOR value, raising if any trailing bytes remain or encoding is non-canonical."""
    data = bytes(data)
    cbor, consumed = _decode_cbor_internal(data)
    remaining = len(data) - consumed
    if remaining > 0:
        raise UnusedData(remaining)
    return cbor


def _parse_header(header: int) -> tuple[MajorType, int]:
    major_type = MajorType(header >> 5)
    header_value = header & 0x1F
    return (major_type, header_value)


def _parse_header_varint(data: bytes) -> tuple[MajorType, int, int]:
    if len(data) == 0:
        raise Underrun()
    header = data[0]
    major_type, header_value = _parse_header(header)
    data_remaining = len(data) - 1

    if header_value <= 23:
        return (major_type, header_value, 1)
    elif header_value == 24:
        if data_remaining < 1:
            raise Underrun()
        val = data[1]
        if val < 24:
            raise NonCanonicalNumeric()
        return (major_type, val, 2)
    elif header_value == 25:
        if data_remaining < 2:
            raise Underrun()
        val = (data[1] << 8) | data[2]
        if val <= 0xFF and header != 0xF9:
            raise NonCanonicalNumeric()
        return (major_type, val, 3)
    elif header_value == 26:
        if data_remaining < 4:
            raise Underrun()
        val = (data[1] << 24) | (data[2] << 16) | (data[3] << 8) | data[4]
        if val <= 0xFFFF and header != 0xFA:
            raise NonCanonicalNumeric()
        return (major_type, val, 5)
    elif header_value == 27:
        if data_remaining < 8:
            raise Underrun()
        val = (
            (data[1] << 56) | (data[2] << 48) | (data[3] << 40) | (data[4] << 32)
            | (data[5] << 24) | (data[6] << 16) | (data[7] << 8) | data[8]
        )
        if val <= 0xFFFFFFFF and header != 0xFB:
            raise NonCanonicalNumeric()
        return (major_type, val, 9)
    else:
        raise UnsupportedHeaderValue(header_value)


def _parse_bytes(data: bytes, length: int) -> bytes:
    if len(data) < length:
        raise Underrun()
    return data[:length]


def _decode_cbor_internal(data: bytes) -> tuple[CBOR, int]:
    if len(data) == 0:
        raise Underrun()

    major_type, value, header_varint_len = _parse_header_varint(data)

    match major_type:
        case MajorType.UNSIGNED:
            return (CBOR(CBORCase.UNSIGNED, value), header_varint_len)

        case MajorType.NEGATIVE:
            return (CBOR(CBORCase.NEGATIVE, value), header_varint_len)

        case MajorType.BYTE_STRING:
            data_len = value
            raw = _parse_bytes(data[header_varint_len:], data_len)
            return (
                CBOR(CBORCase.BYTE_STRING, ByteString(raw)),
                header_varint_len + data_len,
            )

        case MajorType.TEXT:
            data_len = value
            raw = _parse_bytes(data[header_varint_len:], data_len)
            try:
                text = raw.decode("utf-8")
            except UnicodeDecodeError as e:
                raise InvalidString(str(e)) from e
            if not is_nfc(text):
                raise NonCanonicalString()
            return (CBOR(CBORCase.TEXT, text), header_varint_len + data_len)

        case MajorType.ARRAY:
            pos = header_varint_len
            items: list[CBOR] = []
            for _ in range(value):
                item, item_len = _decode_cbor_internal(data[pos:])
                items.append(item)
                pos += item_len
            return (CBOR(CBORCase.ARRAY, items), pos)

        case MajorType.MAP:
            pos = header_varint_len
            m = Map()
            for _ in range(value):
                key, key_len = _decode_cbor_internal(data[pos:])
                pos += key_len
                val, val_len = _decode_cbor_internal(data[pos:])
                pos += val_len
                m.insert_next(key, val)
            return (CBOR(CBORCase.MAP, m), pos)

        case MajorType.TAGGED:
            item, item_len = _decode_cbor_internal(data[header_varint_len:])
            tagged = CBOR.from_tagged_value(Tag(value), item)
            return (tagged, header_varint_len + item_len)

        case MajorType.SIMPLE:
            if header_varint_len == 3:
                bits = value
                f = f16_from_bits(bits)
                validate_canonical_f16(f, bits)
                return (CBOR.from_float(f), header_varint_len)
            elif header_varint_len == 5:
                f = f32_from_bits(value)
                validate_canonical_f32(f)
                return (CBOR.from_float(f), header_varint_len)
            elif header_varint_len == 9:
                f = f64_from_bits(value)
                validate_canonical_f64(f)
                return (CBOR.from_float(f), header_varint_len)
            else:
                if value == 20:
                    return (CBOR.cbor_false(), header_varint_len)
                elif value == 21:
                    return (CBOR.cbor_true(), header_varint_len)
                elif value == 22:
                    return (CBOR.null(), header_varint_len)
                else:
                    raise InvalidSimpleValue()
