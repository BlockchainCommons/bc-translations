from __future__ import annotations

import struct
from enum import IntEnum


class MajorType(IntEnum):
    UNSIGNED = 0
    NEGATIVE = 1
    BYTE_STRING = 2
    TEXT = 3
    ARRAY = 4
    MAP = 5
    TAGGED = 6
    SIMPLE = 7


def _type_bits(major_type: MajorType) -> int:
    return major_type << 5


def encode_varint(value: int, major_type: MajorType) -> bytes:
    if value <= 23:
        return bytes([value | _type_bits(major_type)])
    elif value <= 0xFF:
        return bytes([0x18 | _type_bits(major_type), value])
    elif value <= 0xFFFF:
        return bytes([0x19 | _type_bits(major_type)]) + struct.pack(">H", value)
    elif value <= 0xFFFFFFFF:
        return bytes([0x1A | _type_bits(major_type)]) + struct.pack(">I", value)
    else:
        return bytes([0x1B | _type_bits(major_type)]) + struct.pack(">Q", value)


def encode_int(value: int, major_type: MajorType) -> bytes:
    if value <= 0xFF:
        return bytes([0x18 | _type_bits(major_type), value])
    elif value <= 0xFFFF:
        return bytes([0x19 | _type_bits(major_type)]) + struct.pack(">H", value)
    elif value <= 0xFFFFFFFF:
        return bytes([0x1A | _type_bits(major_type)]) + struct.pack(">I", value)
    else:
        return bytes([0x1B | _type_bits(major_type)]) + struct.pack(">Q", value)
