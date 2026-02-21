"""Tests for CBOR encoding and decoding."""

from __future__ import annotations

import math

import pytest

from dcbor import (
    CBOR,
    ByteString,
    Date,
    Map,
    MisorderedMapKey,
    NonCanonicalNumeric,
    NonCanonicalString,
    Set,
    Tag,
    UnusedData,
)


def _check_cbor(value, expected_debug: str, expected_display: str, expected_data: str):
    cbor = CBOR.from_value(value) if not isinstance(value, CBOR) else value
    assert repr(cbor) == expected_debug
    assert str(cbor) == expected_display
    data = cbor.to_cbor_data()
    assert data.hex() == expected_data
    decoded = CBOR.from_data(data)
    assert cbor == decoded


def _check_cbor_decode(data_hex: str, expected_debug: str, expected_display: str):
    cbor = CBOR.from_hex(data_hex)
    assert repr(cbor) == expected_debug
    assert str(cbor) == expected_display


# --- Unsigned integers ---

def test_encode_unsigned():
    _check_cbor(0, "unsigned(0)", "0", "00")
    _check_cbor(1, "unsigned(1)", "1", "01")
    _check_cbor(23, "unsigned(23)", "23", "17")
    _check_cbor(24, "unsigned(24)", "24", "1818")
    _check_cbor(255, "unsigned(255)", "255", "18ff")
    _check_cbor(65535, "unsigned(65535)", "65535", "19ffff")
    _check_cbor(65536, "unsigned(65536)", "65536", "1a00010000")
    _check_cbor(4294967295, "unsigned(4294967295)", "4294967295", "1affffffff")
    _check_cbor(4294967296, "unsigned(4294967296)", "4294967296", "1b0000000100000000")
    _check_cbor(
        18446744073709551615,
        "unsigned(18446744073709551615)",
        "18446744073709551615",
        "1bffffffffffffffff",
    )


# --- Signed integers ---

def test_encode_signed():
    _check_cbor(-1, "negative(-1)", "-1", "20")
    _check_cbor(-2, "negative(-2)", "-2", "21")
    _check_cbor(-127, "negative(-127)", "-127", "387e")
    _check_cbor(-128, "negative(-128)", "-128", "387f")
    _check_cbor(127, "unsigned(127)", "127", "187f")
    _check_cbor(-32768, "negative(-32768)", "-32768", "397fff")
    _check_cbor(32767, "unsigned(32767)", "32767", "197fff")
    _check_cbor(-2147483648, "negative(-2147483648)", "-2147483648", "3a7fffffff")
    _check_cbor(2147483647, "unsigned(2147483647)", "2147483647", "1a7fffffff")
    _check_cbor(
        -9223372036854775808,
        "negative(-9223372036854775808)",
        "-9223372036854775808",
        "3b7fffffffffffffff",
    )
    _check_cbor(
        9223372036854775807,
        "unsigned(9223372036854775807)",
        "9223372036854775807",
        "1b7fffffffffffffff",
    )


# --- Byte strings ---

def test_encode_bytes():
    _check_cbor(
        CBOR.from_bytes(bytes.fromhex("00112233")),
        "bytes(00112233)",
        "h'00112233'",
        "4400112233",
    )
    _check_cbor(
        CBOR.from_bytes(bytes.fromhex(
            "c0a7da14e5847c526244f7e083d26fe33f86d2313ad2b77164233444423a50a7"
        )),
        "bytes(c0a7da14e5847c526244f7e083d26fe33f86d2313ad2b77164233444423a50a7)",
        "h'c0a7da14e5847c526244f7e083d26fe33f86d2313ad2b77164233444423a50a7'",
        "5820c0a7da14e5847c526244f7e083d26fe33f86d2313ad2b77164233444423a50a7",
    )
    _check_cbor(
        CBOR.from_bytes(bytes([0x11, 0x22, 0x33])),
        "bytes(112233)",
        "h'112233'",
        "43112233",
    )


# --- Text strings ---

def test_encode_string():
    _check_cbor(
        "Hello",
        'text("Hello")',
        '"Hello"',
        "6548656c6c6f",
    )


def test_normalized_string():
    composed = "\u00e9"
    decomposed = "\u0065\u0301"
    assert composed != decomposed

    cbor1 = CBOR.from_value(composed).to_cbor_data()
    cbor2 = CBOR.from_value(decomposed).to_cbor_data()
    assert cbor1 == cbor2

    # Non-NFC encoded text should fail to decode
    with pytest.raises(NonCanonicalString):
        CBOR.from_data(bytes.fromhex("6365cc81"))


# --- Arrays ---

def test_encode_array():
    _check_cbor(
        CBOR.from_array([]),
        "array([])",
        "[]",
        "80",
    )
    _check_cbor(
        CBOR.from_array([CBOR.from_int(1), CBOR.from_int(2), CBOR.from_int(3)]),
        "array([unsigned(1), unsigned(2), unsigned(3)])",
        "[1, 2, 3]",
        "83010203",
    )


def test_encode_heterogenous_array():
    array = [CBOR.from_int(1), CBOR.from_text("Hello"),
             CBOR.from_array([CBOR.from_int(1), CBOR.from_int(2), CBOR.from_int(3)])]
    cbor = CBOR.from_array(array)
    assert repr(cbor) == 'array([unsigned(1), text("Hello"), array([unsigned(1), unsigned(2), unsigned(3)])])'
    assert str(cbor) == '[1, "Hello", [1, 2, 3]]'
    assert cbor.hex() == "83016548656c6c6f83010203"

    data = cbor.to_cbor_data()
    decoded = CBOR.from_data(data)
    assert cbor == decoded


# --- Maps ---

def test_encode_map():
    m = Map()
    _check_cbor(CBOR.from_map(m), "map({})", "{}", "a0")

    m.insert(-1, CBOR.from_int(3))
    m.insert(CBOR.from_array([CBOR.from_int(-1)]), CBOR.from_int(7))
    m.insert("z", CBOR.from_int(4))
    m.insert(10, CBOR.from_int(1))
    m.insert(False, CBOR.from_int(8))
    m.insert(100, CBOR.from_int(2))
    m.insert("aa", CBOR.from_int(5))
    m.insert(CBOR.from_array([CBOR.from_int(100)]), CBOR.from_int(6))

    cbor = CBOR.from_map(m)
    assert str(cbor) == '{10: 1, 100: 2, -1: 3, "z": 4, "aa": 5, [100]: 6, [-1]: 7, false: 8}'
    assert cbor.hex() == "a80a011864022003617a046261610581186406812007f408"


def test_encode_map_misordered():
    with pytest.raises(MisorderedMapKey):
        CBOR.from_hex("a2026141016142")


def test_map_getitem_raises_keyerror():
    m = Map()
    m.insert("x", 1)

    assert m["x"] == CBOR.from_int(1)
    with pytest.raises(KeyError):
        _ = m["missing"]


def test_set_contains_protocol():
    s = Set.from_list([1, "hello", 3])

    assert 1 in s
    assert "hello" in s
    assert 2 not in s
    assert object() not in s


# --- Tagged values ---

def test_encode_tagged():
    _check_cbor(
        CBOR.from_tagged_value(1, CBOR.from_text("Hello")),
        'tagged(1, text("Hello"))',
        '1("Hello")',
        "c16548656c6c6f",
    )


# --- Boolean values ---

def test_encode_value():
    _check_cbor(False, "simple(false)", "false", "f4")
    _check_cbor(True, "simple(true)", "true", "f5")


# --- Envelope (nested tagged) ---

def test_encode_envelope():
    alice = CBOR.from_tagged_value(200, CBOR.from_tagged_value(201, CBOR.from_text("Alice")))
    knows = CBOR.from_tagged_value(200, CBOR.from_tagged_value(201, CBOR.from_text("knows")))
    bob = CBOR.from_tagged_value(200, CBOR.from_tagged_value(201, CBOR.from_text("Bob")))
    knows_bob = CBOR.from_tagged_value(
        200,
        CBOR.from_tagged_value(221, CBOR.from_array([knows, bob])),
    )
    envelope = CBOR.from_tagged_value(200, CBOR.from_array([alice, knows_bob]))
    assert str(envelope) == '200([200(201("Alice")), 200(221([200(201("knows")), 200(201("Bob"))]))])'
    data = envelope.to_cbor_data()
    assert data.hex() == "d8c882d8c8d8c965416c696365d8c8d8dd82d8c8d8c9656b6e6f7773d8c8d8c963426f62"
    decoded = CBOR.from_data(data)
    assert envelope == decoded


# --- Floating point ---

def test_encode_float():
    # Shortest accurate representation
    _check_cbor(CBOR.from_float(1.5), "simple(1.5)", "1.5", "f93e00")
    _check_cbor(CBOR.from_float(2345678.25), "simple(2345678.25)", "2345678.25", "fa4a0f2b39")
    _check_cbor(CBOR.from_float(1.2), "simple(1.2)", "1.2", "fb3ff3333333333333")
    _check_cbor(CBOR.from_float(float("inf")), "simple(inf)", "Infinity", "f97c00")

    # Floats that represent integers encode as integers
    _check_cbor(CBOR.from_float(42.0), "unsigned(42)", "42", "182a")
    _check_cbor(CBOR.from_float(2345678.0), "unsigned(2345678)", "2345678", "1a0023cace")
    _check_cbor(CBOR.from_float(-2345678.0), "negative(-2345678)", "-2345678", "3a0023cacd")

    # Negative zero encodes as integer zero
    _check_cbor(CBOR.from_float(-0.0), "unsigned(0)", "0", "00")

    # Smallest half-precision subnormal
    _check_cbor(
        CBOR.from_float(5.960464477539063e-8),
        "simple(5.960464477539063e-08)",
        "5.960464477539063e-08",
        "f90001",
    )

    # Smallest single subnormal
    _check_cbor(
        CBOR.from_float(1.401298464324817e-45),
        "simple(1.401298464324817e-45)",
        "1.401298464324817e-45",
        "fa00000001",
    )

    # Smallest double subnormal
    _check_cbor(CBOR.from_float(5e-324), "simple(5e-324)", "5e-324", "fb0000000000000001")

    # Smallest double normal
    _check_cbor(
        CBOR.from_float(2.2250738585072014e-308),
        "simple(2.2250738585072014e-308)",
        "2.2250738585072014e-308",
        "fb0010000000000000",
    )

    # Smallest half-precision normal
    _check_cbor(
        CBOR.from_float(6.103515625e-5),
        "simple(6.103515625e-05)",
        "6.103515625e-05",
        "f90400",
    )

    # Largest possible half-precision encodes as integer
    _check_cbor(CBOR.from_float(65504.0), "unsigned(65504)", "65504", "19ffe0")

    # Exponent 24 to test single exponent boundary
    _check_cbor(CBOR.from_float(33554430.0), "unsigned(33554430)", "33554430", "1a01fffffe")

    # Most negative double that converts to int64
    _check_cbor(
        CBOR.from_float(-9223372036854774784.0),
        "negative(-9223372036854774784)",
        "-9223372036854774784",
        "3b7ffffffffffffbff",
    )

    # Largest double that can convert to uint64
    _check_cbor(
        CBOR.from_float(18446744073709550000.0),
        "unsigned(18446744073709549568)",
        "18446744073709549568",
        "1bfffffffffffff800",
    )

    # Just too large for uint64
    _check_cbor(
        CBOR.from_float(18446744073709552000.0),
        "simple(1.8446744073709552e+19)",
        "1.8446744073709552e+19",
        "fa5f800000",
    )

    # Least negative float not representable as Int64
    _check_cbor(
        CBOR.from_float(-9223372036854777856.0),
        "negative(-9223372036854777856)",
        "-9223372036854777856",
        "3b80000000000007ff",
    )

    # Next to most negative float encodable as 65-bit neg
    _check_cbor(
        CBOR.from_float(-18446744073709549568.0),
        "negative(-18446744073709549568)",
        "-18446744073709549568",
        "3bfffffffffffff7ff",
    )

    # Most negative encodable as a 65-bit neg
    _check_cbor(
        CBOR.from_float(-18446744073709551616.0),
        "negative(-18446744073709551616)",
        "-18446744073709551616",
        "3bffffffffffffffff",
    )

    # Large negative that converts to negative int
    _check_cbor(
        CBOR.from_float(-18446742974197924000.0),
        "negative(-18446742974197923840)",
        "-18446742974197923840",
        "3bfffffeffffffffff",
    )

    # Largest possible single
    _check_cbor(
        CBOR.from_float(3.4028234663852886e38),
        "simple(3.4028234663852886e+38)",
        "3.4028234663852886e+38",
        "fa7f7fffff",
    )

    # Largest double
    _check_cbor(
        CBOR.from_float(1.7976931348623157e308),
        "simple(1.7976931348623157e+308)",
        "1.7976931348623157e+308",
        "fb7fefffffffffffff",
    )


# --- NaN encoding ---

def test_encode_nan():
    canonical_nan_data = bytes.fromhex("f97e00")
    cbor = CBOR.from_float(float("nan"))
    assert cbor.to_cbor_data() == canonical_nan_data


def test_decode_nan():
    # Canonical NaN decodes
    cbor = CBOR.from_data(bytes.fromhex("f97e00"))
    f = cbor.try_float()
    assert math.isnan(f)

    # Non-canonical NaNs fail
    with pytest.raises(NonCanonicalNumeric):
        CBOR.from_data(bytes.fromhex("f97e01"))
    with pytest.raises(NonCanonicalNumeric):
        CBOR.from_data(bytes.fromhex("faffc00001"))
    with pytest.raises(NonCanonicalNumeric):
        CBOR.from_data(bytes.fromhex("fb7ff9100000000001"))


# --- Infinity encoding ---

def test_encode_infinity():
    canonical_inf = bytes.fromhex("f97c00")
    canonical_neg_inf = bytes.fromhex("f9fc00")
    assert CBOR.from_float(float("inf")).to_cbor_data() == canonical_inf
    assert CBOR.from_float(float("-inf")).to_cbor_data() == canonical_neg_inf


def test_decode_infinity():
    cbor = CBOR.from_data(bytes.fromhex("f97c00"))
    assert cbor.try_float() == float("inf")

    cbor = CBOR.from_data(bytes.fromhex("f9fc00"))
    assert cbor.try_float() == float("-inf")

    # Non-canonical infinities fail
    with pytest.raises(NonCanonicalNumeric):
        CBOR.from_data(bytes.fromhex("fa7f800000"))
    with pytest.raises(NonCanonicalNumeric):
        CBOR.from_data(bytes.fromhex("fb7ff0000000000000"))
    with pytest.raises(NonCanonicalNumeric):
        CBOR.from_data(bytes.fromhex("faff800000"))
    with pytest.raises(NonCanonicalNumeric):
        CBOR.from_data(bytes.fromhex("fbfff0000000000000"))


# --- Non-canonical float detection ---

def test_non_canonical_float():
    # 1.5 encoded as f64 instead of f16
    with pytest.raises(NonCanonicalNumeric):
        CBOR.from_hex("FB3FF8000000000000")

    # Float that could be encoded as integer
    with pytest.raises(NonCanonicalNumeric):
        CBOR.from_hex("F94A00")


# --- Unused data detection ---

def test_unused_data():
    with pytest.raises(UnusedData):
        CBOR.from_hex("0001")


# --- 65-bit negative decode ---

def test_65bit_negative_decode():
    _check_cbor_decode(
        "3b8000000000000000",
        "negative(-9223372036854775809)",
        "-9223372036854775809",
    )
    _check_cbor_decode(
        "3bfffffffffffffffe",
        "negative(-18446744073709551615)",
        "-18446744073709551615",
    )


# --- Date encoding ---

def test_encode_date():
    date = Date.from_timestamp(1675854714.0)
    cbor = date.to_tagged_cbor()
    assert repr(cbor) == "tagged(1, unsigned(1675854714))"
    assert str(cbor) == "1(1675854714)"
    assert cbor.to_cbor_data().hex() == "c11a63e3837a"


# --- Integer-float coercion ---

def test_int_coerced_to_float():
    c = CBOR.from_int(42)
    f = c.try_float()
    assert f == 42.0
    c2 = CBOR.from_float(f)
    assert c2 == c
    i = c.try_int()
    assert i == 42


def test_fail_float_coerced_to_int():
    c = CBOR.from_float(42.5)
    f = c.try_float()
    assert f == 42.5
    with pytest.raises(Exception):
        c.try_int()


# --- Null ---

def test_encode_null():
    _check_cbor(CBOR.null(), "simple(null)", "null", "f6")


# --- Usage tests ---

def test_usage_1():
    items = [CBOR.from_int(1000), CBOR.from_int(2000), CBOR.from_int(3000)]
    cbor = CBOR.from_array(items)
    assert cbor.hex() == "831903e81907d0190bb8"


def test_usage_2():
    data = bytes.fromhex("831903e81907d0190bb8")
    cbor = CBOR.from_data(data)
    assert cbor.diagnostic() == "[1000, 2000, 3000]"


def test_encode_map_with_map_keys():
    k1 = Map()
    k1.insert(1, CBOR.from_int(2))
    k2 = Map()
    k2.insert(3, CBOR.from_int(4))
    m = Map()
    m.insert(CBOR.from_map(k1), CBOR.from_int(5))
    m.insert(CBOR.from_map(k2), CBOR.from_int(6))
    cbor = CBOR.from_map(m)
    assert repr(cbor) == 'map({0xa10102: (map({0x01: (unsigned(1), unsigned(2))}), unsigned(5)), 0xa10304: (map({0x03: (unsigned(3), unsigned(4))}), unsigned(6))})'
    assert str(cbor) == '{{1: 2}: 5, {3: 4}: 6}'
    assert cbor.hex() == "a2a1010205a1030406"


def test_tag_formatting():
    tag = Tag(1, "A")
    assert str(tag) == "A"
    tag2 = Tag(2)
    assert str(tag2) == "2"
