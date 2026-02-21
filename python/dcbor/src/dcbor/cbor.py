from __future__ import annotations

import math
from enum import IntEnum
from typing import TYPE_CHECKING, Any

from .byte_string import ByteString
from .error import (
    DCBORError,
    MissingMapKey,
    OutOfRange,
    WrongTag,
    WrongType,
)
from .float_utils import _exact_neg_u64_from_float, _exact_uint_from_float
from .simple import Simple
from .string_util import to_nfc
from .tag import Tag
from .varint import MajorType, encode_varint

if TYPE_CHECKING:
    from .map import Map

_U64_MAX = 0xFFFFFFFFFFFFFFFF


class CBORCase(IntEnum):
    """Discriminant enum identifying which CBOR major type a CBOR value holds."""

    UNSIGNED = 0
    NEGATIVE = 1
    BYTE_STRING = 2
    TEXT = 3
    ARRAY = 4
    MAP = 5
    TAGGED = 6
    SIMPLE = 7


class CBOR:
    """Deterministic CBOR value supporting all standard major types."""

    __slots__ = ("_case", "_value")

    def __init__(self, case: CBORCase, value: Any) -> None:
        self._case = case
        self._value = value

    # --- Case accessors ---

    @property
    def case(self) -> CBORCase:
        return self._case

    @property
    def value(self) -> Any:
        return self._value

    # --- Factory methods for construction ---

    @staticmethod
    def from_int(value: int) -> CBOR:
        """Create a CBOR unsigned or negative integer from a Python int."""
        if value < 0:
            n = -1 - value
            if n < 0 or n > _U64_MAX:
                raise ValueError(f"integer {value} out of CBOR range")
            return CBOR(CBORCase.NEGATIVE, n)
        else:
            if value > _U64_MAX:
                raise ValueError(f"integer {value} out of CBOR range")
            return CBOR(CBORCase.UNSIGNED, value)

    @staticmethod
    def from_float(value: float) -> CBOR:
        """Create a CBOR value from a float, preferring integer encoding when lossless."""
        if value < 0.0:
            neg = _exact_neg_u64_from_float(value)
            if neg is not None:
                return CBOR(CBORCase.NEGATIVE, neg)
        pos = _exact_uint_from_float(value, _U64_MAX)
        if pos is not None:
            return CBOR(CBORCase.UNSIGNED, pos)
        return CBOR(CBORCase.SIMPLE, Simple.make_float(value))

    @staticmethod
    def from_bool(value: bool) -> CBOR:
        """Create a CBOR simple true or false value."""
        if value:
            return CBOR(CBORCase.SIMPLE, Simple.make_true())
        return CBOR(CBORCase.SIMPLE, Simple.make_false())

    @staticmethod
    def from_bytes(data: bytes | bytearray | ByteString) -> CBOR:
        """Create a CBOR byte string from bytes, bytearray, or ByteString."""
        if isinstance(data, ByteString):
            return CBOR(CBORCase.BYTE_STRING, data)
        return CBOR(CBORCase.BYTE_STRING, ByteString(data))

    @staticmethod
    def from_text(value: str) -> CBOR:
        """Create a CBOR text string from a Python str."""
        return CBOR(CBORCase.TEXT, value)

    @staticmethod
    def from_array(items: list[CBOR]) -> CBOR:
        """Create a CBOR array from a list of CBOR values."""
        return CBOR(CBORCase.ARRAY, list(items))

    @staticmethod
    def from_map(m: Map) -> CBOR:
        """Create a CBOR map value wrapping a Map instance."""
        return CBOR(CBORCase.MAP, m)

    @staticmethod
    def from_tagged_value(tag: Tag | int, item: CBOR) -> CBOR:
        """Create a CBOR tagged value from a tag (or raw tag number) and a CBOR item."""
        if isinstance(tag, int):
            tag = Tag(tag)
        return CBOR(CBORCase.TAGGED, (tag, item))

    @staticmethod
    def null() -> CBOR:
        return CBOR(CBORCase.SIMPLE, Simple.make_null())

    @staticmethod
    def cbor_true() -> CBOR:
        return CBOR(CBORCase.SIMPLE, Simple.make_true())

    @staticmethod
    def cbor_false() -> CBOR:
        return CBOR(CBORCase.SIMPLE, Simple.make_false())

    @staticmethod
    def nan() -> CBOR:
        return CBOR(CBORCase.SIMPLE, Simple.make_float(float("nan")))

    @staticmethod
    def from_value(value: int | float | str | bool | bytes | bytearray | ByteString | CBOR | list | None) -> CBOR:
        """Convert a native Python value to CBOR; accepts int, float, str, bool, bytes, list, or CBOR."""
        if isinstance(value, CBOR):
            return value
        if isinstance(value, bool):
            return CBOR.from_bool(value)
        if isinstance(value, int):
            return CBOR.from_int(value)
        if isinstance(value, float):
            return CBOR.from_float(value)
        if isinstance(value, str):
            return CBOR.from_text(value)
        if isinstance(value, (bytes, bytearray, ByteString)):
            return CBOR.from_bytes(value)
        if isinstance(value, Tag):
            raise TypeError("Use CBOR.from_tagged_value(tag, item) for tagged values")
        if isinstance(value, list):
            return CBOR.from_array([CBOR.from_value(x) for x in value])
        raise TypeError(f"Cannot convert {type(value).__name__} to CBOR")

    # --- Encoding ---

    def to_cbor_data(self) -> bytes:
        """Encode this value to deterministic CBOR bytes."""
        match self._case:
            case CBORCase.UNSIGNED:
                return encode_varint(self._value, MajorType.UNSIGNED)
            case CBORCase.NEGATIVE:
                return encode_varint(self._value, MajorType.NEGATIVE)
            case CBORCase.BYTE_STRING:
                data = self._value.data
                return encode_varint(len(data), MajorType.BYTE_STRING) + data
            case CBORCase.TEXT:
                text = to_nfc(self._value)
                encoded = text.encode("utf-8")
                return encode_varint(len(encoded), MajorType.TEXT) + encoded
            case CBORCase.ARRAY:
                result = encode_varint(len(self._value), MajorType.ARRAY)
                for item in self._value:
                    result += item.to_cbor_data()
                return result
            case CBORCase.MAP:
                return self._value.cbor_data()
            case CBORCase.TAGGED:
                tag, item = self._value
                return (
                    encode_varint(tag.value, MajorType.TAGGED)
                    + item.to_cbor_data()
                )
            case CBORCase.SIMPLE:
                return self._value.cbor_data()

    @staticmethod
    def from_data(data: bytes | bytearray) -> CBOR:
        """Decode a CBOR value from raw bytes, raising on any non-canonical encoding."""
        from .decode import decode_cbor
        return decode_cbor(data)

    @staticmethod
    def from_hex(hex_str: str) -> CBOR:
        """Decode a CBOR value from a hexadecimal string."""
        return CBOR.from_data(bytes.fromhex(hex_str))

    # --- Type checks ---

    @property
    def is_unsigned(self) -> bool:
        return self._case == CBORCase.UNSIGNED

    @property
    def is_negative(self) -> bool:
        return self._case == CBORCase.NEGATIVE

    @property
    def is_byte_string(self) -> bool:
        return self._case == CBORCase.BYTE_STRING

    @property
    def is_text(self) -> bool:
        return self._case == CBORCase.TEXT

    @property
    def is_array(self) -> bool:
        return self._case == CBORCase.ARRAY

    @property
    def is_map(self) -> bool:
        return self._case == CBORCase.MAP

    @property
    def is_tagged_value(self) -> bool:
        return self._case == CBORCase.TAGGED

    @property
    def is_simple(self) -> bool:
        return self._case == CBORCase.SIMPLE

    @property
    def is_bool(self) -> bool:
        if self._case != CBORCase.SIMPLE:
            return False
        s: Simple = self._value
        return s.is_true or s.is_false

    @property
    def is_true(self) -> bool:
        return self._case == CBORCase.SIMPLE and self._value.is_true

    @property
    def is_false(self) -> bool:
        return self._case == CBORCase.SIMPLE and self._value.is_false

    @property
    def is_null(self) -> bool:
        return self._case == CBORCase.SIMPLE and self._value.is_null

    @property
    def is_nan(self) -> bool:
        return self._case == CBORCase.SIMPLE and self._value.is_nan

    @property
    def is_float(self) -> bool:
        return self._case == CBORCase.SIMPLE and self._value.is_float

    @property
    def is_number(self) -> bool:
        return self._case in (
            CBORCase.UNSIGNED,
            CBORCase.NEGATIVE,
        ) or self.is_float

    # --- Typed extractors ---

    def try_int(self) -> int:
        if self._case == CBORCase.UNSIGNED:
            return self._value
        if self._case == CBORCase.NEGATIVE:
            return -1 - self._value
        raise WrongType()

    def try_float(self) -> float:
        if self._case == CBORCase.UNSIGNED:
            return float(self._value)
        if self._case == CBORCase.NEGATIVE:
            return float(-1 - self._value)
        if self._case == CBORCase.SIMPLE and self._value.is_float:
            return self._value.float_value
        raise WrongType()

    def try_bool(self) -> bool:
        if self._case == CBORCase.SIMPLE:
            if self._value.is_true:
                return True
            if self._value.is_false:
                return False
        raise WrongType()

    def as_bool(self) -> bool | None:
        try:
            return self.try_bool()
        except WrongType:
            return None

    def try_byte_string(self) -> bytes:
        if self._case == CBORCase.BYTE_STRING:
            return self._value.data
        raise WrongType()

    def as_byte_string(self) -> bytes | None:
        if self._case == CBORCase.BYTE_STRING:
            return self._value.data
        return None

    def try_text(self) -> str:
        if self._case == CBORCase.TEXT:
            return self._value
        raise WrongType()

    def as_text(self) -> str | None:
        if self._case == CBORCase.TEXT:
            return self._value
        return None

    def try_array(self) -> list[CBOR]:
        if self._case == CBORCase.ARRAY:
            return list(self._value)
        raise WrongType()

    def as_array(self) -> list[CBOR] | None:
        if self._case == CBORCase.ARRAY:
            return list(self._value)
        return None

    def try_map(self) -> Map:
        from .map import Map
        if self._case == CBORCase.MAP:
            return self._value
        raise WrongType()

    def as_map(self) -> Map | None:
        if self._case == CBORCase.MAP:
            return self._value
        return None

    def try_tagged_value(self) -> tuple[Tag, CBOR]:
        if self._case == CBORCase.TAGGED:
            return self._value
        raise WrongType()

    def as_tagged_value(self) -> tuple[Tag, CBOR] | None:
        if self._case == CBORCase.TAGGED:
            return self._value
        return None

    def try_expected_tagged_value(self, expected_tag: Tag | int) -> CBOR:
        if isinstance(expected_tag, int):
            expected_tag = Tag(expected_tag)
        if self._case == CBORCase.TAGGED:
            tag, item = self._value
            if tag == expected_tag:
                return item
            raise WrongTag(expected_tag, tag)
        raise WrongType()

    def try_simple(self) -> Simple:
        if self._case == CBORCase.SIMPLE:
            return self._value
        raise WrongType()

    # --- Diagnostics (delegate to diag module) ---

    def diagnostic(self) -> str:
        from .diag import diagnostic_impl
        return diagnostic_impl(self, annotate=False, flat=False, summarize=False)

    def diagnostic_annotated(self, tags_store: object = None) -> str:
        from .diag import diagnostic_impl
        return diagnostic_impl(
            self, annotate=True, flat=False, summarize=False,
            tags_store=tags_store,
        )

    def diagnostic_flat(self) -> str:
        from .diag import diagnostic_impl
        return diagnostic_impl(self, annotate=False, flat=True, summarize=False)

    def summary(self, tags_store: object = None) -> str:
        from .diag import diagnostic_impl
        return diagnostic_impl(
            self, annotate=True, flat=True, summarize=True,
            tags_store=tags_store,
        )

    # --- Hex dump (delegate to dump module) ---

    def hex(self) -> str:
        """Return the deterministic CBOR encoding of this value as a hex string."""
        return self.to_cbor_data().hex()

    def hex_annotated(self, tags_store: object = None) -> str:
        from .dump import hex_annotated_impl
        return hex_annotated_impl(self, tags_store=tags_store)

    # --- Walk (delegate to walk module) ---

    def walk(self, state: object, visitor: object) -> object:
        from .walk import walk_impl
        return walk_impl(self, state, visitor)

    # --- Equality and hashing ---

    def __eq__(self, other: object) -> bool:
        if not isinstance(other, CBOR):
            return NotImplemented
        if self._case != other._case:
            return False
        if self._case == CBORCase.TAGGED:
            t1, v1 = self._value
            t2, v2 = other._value
            return t1 == t2 and v1 == v2
        return self._value == other._value

    def __hash__(self) -> int:
        match self._case:
            case CBORCase.UNSIGNED | CBORCase.NEGATIVE | CBORCase.TEXT:
                return hash((self._case, self._value))
            case CBORCase.BYTE_STRING:
                return hash((self._case, self._value.data))
            case CBORCase.ARRAY:
                return hash((self._case, tuple(self._value)))
            case CBORCase.MAP:
                return hash((self._case, self._value))
            case CBORCase.TAGGED:
                tag, item = self._value
                return hash((self._case, tag, item))
            case CBORCase.SIMPLE:
                return hash((self._case, self._value))

    # --- Leaf formatting (avoids circular call through diag module) ---

    def _leaf_str(self) -> str | None:
        """Direct string for leaf CBOR types (used by diagnostic and __str__)."""
        match self._case:
            case CBORCase.UNSIGNED:
                return str(self._value)
            case CBORCase.NEGATIVE:
                return str(-1 - self._value)
            case CBORCase.BYTE_STRING:
                return f"h'{self._value.data.hex()}'"
            case CBORCase.TEXT:
                return f'"{self._value}"'
            case CBORCase.SIMPLE:
                return str(self._value)
        return None

    def debug_description(self) -> str:
        """Detailed debug representation showing the CBOR structure."""
        match self._case:
            case CBORCase.UNSIGNED:
                return f"unsigned({self._value})"
            case CBORCase.NEGATIVE:
                return f"negative({-1 - self._value})"
            case CBORCase.BYTE_STRING:
                return f"bytes({self._value.data.hex()})"
            case CBORCase.TEXT:
                return f'text("{self._value}")'
            case CBORCase.ARRAY:
                items = ", ".join(item.debug_description() for item in self._value)
                return f"array([{items}])"
            case CBORCase.MAP:
                pairs = []
                for key, val in self._value.iter():
                    key_hex = "0x" + key.to_cbor_data().hex()
                    pairs.append(
                        f"{key_hex}: ({key.debug_description()}, {val.debug_description()})"
                    )
                return "map({" + ", ".join(pairs) + "})"
            case CBORCase.TAGGED:
                tag, item = self._value
                return f"tagged({tag}, {item.debug_description()})"
            case CBORCase.SIMPLE:
                return f"simple({self._value!r})"

    def __repr__(self) -> str:
        return self.debug_description()

    def __str__(self) -> str:
        leaf = self._leaf_str()
        if leaf is not None:
            return leaf
        match self._case:
            case CBORCase.ARRAY:
                return "[" + ", ".join(str(item) for item in self._value) + "]"
            case CBORCase.MAP:
                pairs = []
                for key, val in self._value.iter():
                    pairs.append(f"{key}: {val}")
                return "{" + ", ".join(pairs) + "}"
            case CBORCase.TAGGED:
                tag, item = self._value
                if tag.name is not None:
                    return f"{tag.name}({item})"
                return f"{tag.value}({item})"
