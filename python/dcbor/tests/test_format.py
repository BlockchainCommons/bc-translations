"""Tests for CBOR diagnostic and hex-annotated formatting."""

from __future__ import annotations

import textwrap

from dcbor import CBOR, Date, Map, Tag, register_tags


def _run_format_test(
    test_name: str,
    cbor: CBOR,
    expected_description: str,
    expected_debug_description: str,
    expected_diagnostic: str,
    expected_diagnostic_annotated: str,
    expected_diagnostic_flat: str,
    expected_summary: str,
    expected_hex: str,
    expected_hex_annotated: str,
):
    description = str(cbor)
    assert description == expected_description, (
        f"description mismatch in test '{test_name}':\n"
        f"  expected: {expected_description!r}\n"
        f"  actual  : {description!r}"
    )

    debug_description = repr(cbor)
    assert debug_description == expected_debug_description, (
        f"debug_description mismatch in test '{test_name}':\n"
        f"  expected: {expected_debug_description!r}\n"
        f"  actual  : {debug_description!r}"
    )

    diagnostic = cbor.diagnostic()
    assert diagnostic == expected_diagnostic, (
        f"diagnostic mismatch in test '{test_name}':\n"
        f"  expected: {expected_diagnostic!r}\n"
        f"  actual  : {diagnostic!r}"
    )

    diagnostic_annotated = cbor.diagnostic_annotated()
    assert diagnostic_annotated == expected_diagnostic_annotated, (
        f"diagnostic_annotated mismatch in test '{test_name}':\n"
        f"  expected: {expected_diagnostic_annotated!r}\n"
        f"  actual  : {diagnostic_annotated!r}"
    )

    diagnostic_flat = cbor.diagnostic_flat()
    assert diagnostic_flat == expected_diagnostic_flat, (
        f"diagnostic_flat mismatch in test '{test_name}':\n"
        f"  expected: {expected_diagnostic_flat!r}\n"
        f"  actual  : {diagnostic_flat!r}"
    )

    summary = cbor.summary()
    assert summary == expected_summary, (
        f"summary mismatch in test '{test_name}':\n"
        f"  expected: {expected_summary!r}\n"
        f"  actual  : {summary!r}"
    )

    hex_val = cbor.hex()
    assert hex_val == expected_hex, (
        f"hex mismatch in test '{test_name}':\n"
        f"  expected: {expected_hex!r}\n"
        f"  actual  : {hex_val!r}"
    )

    hex_annotated = cbor.hex_annotated()
    assert hex_annotated == expected_hex_annotated, (
        f"hex_annotated mismatch in test '{test_name}':\n"
        f"  expected: {expected_hex_annotated!r}\n"
        f"  actual  : {hex_annotated!r}"
    )


def test_format_simple_1():
    _run_format_test(
        "format_simple_1",
        CBOR.cbor_false(),
        "false",
        "simple(false)",
        "false",
        "false",
        "false",
        "false",
        "f4",
        "f4  # false",
    )


def test_format_simple_2():
    _run_format_test(
        "format_simple_2",
        CBOR.cbor_true(),
        "true",
        "simple(true)",
        "true",
        "true",
        "true",
        "true",
        "f5",
        "f5  # true",
    )


def test_format_simple_3():
    _run_format_test(
        "format_simple_3",
        CBOR.null(),
        "null",
        "simple(null)",
        "null",
        "null",
        "null",
        "null",
        "f6",
        "f6  # null",
    )


def test_format_unsigned():
    _run_format_test(
        "format_unsigned_0",
        CBOR.from_int(0),
        "0",
        "unsigned(0)",
        "0",
        "0",
        "0",
        "0",
        "00",
        "00  # unsigned(0)",
    )
    _run_format_test(
        "format_unsigned_23",
        CBOR.from_int(23),
        "23",
        "unsigned(23)",
        "23",
        "23",
        "23",
        "23",
        "17",
        "17  # unsigned(23)",
    )
    _run_format_test(
        "format_unsigned_65546",
        CBOR.from_int(65546),
        "65546",
        "unsigned(65546)",
        "65546",
        "65546",
        "65546",
        "65546",
        "1a0001000a",
        "1a0001000a  # unsigned(65546)",
    )
    _run_format_test(
        "format_unsigned_1000000000",
        CBOR.from_int(1000000000),
        "1000000000",
        "unsigned(1000000000)",
        "1000000000",
        "1000000000",
        "1000000000",
        "1000000000",
        "1a3b9aca00",
        "1a3b9aca00  # unsigned(1000000000)",
    )


def test_format_negative():
    _run_format_test(
        "format_negative_neg1",
        CBOR.from_int(-1),
        "-1",
        "negative(-1)",
        "-1",
        "-1",
        "-1",
        "-1",
        "20",
        "20  # negative(-1)",
    )
    _run_format_test(
        "format_negative_neg1000",
        CBOR.from_int(-1000),
        "-1000",
        "negative(-1000)",
        "-1000",
        "-1000",
        "-1000",
        "-1000",
        "3903e7",
        "3903e7  # negative(-1000)",
    )
    _run_format_test(
        "format_negative_neg1000000",
        CBOR.from_int(-1000000),
        "-1000000",
        "negative(-1000000)",
        "-1000000",
        "-1000000",
        "-1000000",
        "-1000000",
        "3a000f423f",
        "3a000f423f  # negative(-1000000)",
    )


def test_format_string():
    _run_format_test(
        "format_string",
        CBOR.from_text("Test"),
        '"Test"',
        'text("Test")',
        '"Test"',
        '"Test"',
        '"Test"',
        '"Test"',
        "6454657374",
        textwrap.dedent("""\
            64              # text(4)
                54657374    # "Test\""""),
    )


def test_format_simple_array():
    _run_format_test(
        "format_simple_array",
        CBOR.from_array([CBOR.from_int(1), CBOR.from_int(2), CBOR.from_int(3)]),
        "[1, 2, 3]",
        "array([unsigned(1), unsigned(2), unsigned(3)])",
        "[1, 2, 3]",
        "[1, 2, 3]",
        "[1, 2, 3]",
        "[1, 2, 3]",
        "83010203",
        textwrap.dedent("""\
            83      # array(3)
                01  # unsigned(1)
                02  # unsigned(2)
                03  # unsigned(3)"""),
    )


def test_format_nested_array():
    a = CBOR.from_array([CBOR.from_int(1), CBOR.from_int(2), CBOR.from_int(3)])
    b = CBOR.from_array([CBOR.from_text("A"), CBOR.from_text("B"), CBOR.from_text("C")])
    c = CBOR.from_array([a, b])
    _run_format_test(
        "format_nested_array",
        c,
        '[[1, 2, 3], ["A", "B", "C"]]',
        'array([array([unsigned(1), unsigned(2), unsigned(3)]), array([text("A"), text("B"), text("C")])])',
        textwrap.dedent("""\
            [
                [1, 2, 3],
                ["A", "B", "C"]
            ]"""),
        textwrap.dedent("""\
            [
                [1, 2, 3],
                ["A", "B", "C"]
            ]"""),
        '[[1, 2, 3], ["A", "B", "C"]]',
        '[[1, 2, 3], ["A", "B", "C"]]',
        "828301020383614161426143",
        textwrap.dedent("""\
            82              # array(2)
                83          # array(3)
                    01      # unsigned(1)
                    02      # unsigned(2)
                    03      # unsigned(3)
                83          # array(3)
                    61      # text(1)
                        41  # "A"
                    61      # text(1)
                        42  # "B"
                    61      # text(1)
                        43  # "C\""""),
    )


def test_format_map():
    m = Map()
    m.insert(1, CBOR.from_text("A"))
    m.insert(2, CBOR.from_text("B"))
    _run_format_test(
        "format_map",
        CBOR.from_map(m),
        '{1: "A", 2: "B"}',
        'map({0x01: (unsigned(1), text("A")), 0x02: (unsigned(2), text("B"))})',
        '{1: "A", 2: "B"}',
        '{1: "A", 2: "B"}',
        '{1: "A", 2: "B"}',
        '{1: "A", 2: "B"}',
        "a2016141026142",
        textwrap.dedent("""\
            a2          # map(2)
                01      # unsigned(1)
                61      # text(1)
                    41  # "A"
                02      # unsigned(2)
                61      # text(1)
                    42  # "B\""""),
    )


def test_format_tagged():
    a = CBOR.from_tagged_value(100, CBOR.from_text("Hello"))
    _run_format_test(
        "format_tagged",
        a,
        '100("Hello")',
        'tagged(100, text("Hello"))',
        '100("Hello")',
        '100("Hello")',
        '100("Hello")',
        '100("Hello")',
        "d8646548656c6c6f",
        textwrap.dedent("""\
            d8 64               # tag(100)
                65              # text(5)
                    48656c6c6f  # "Hello\""""),
    )


def test_format_date():
    register_tags()

    _run_format_test(
        "format_date_negative",
        Date.from_timestamp(-100.0).to_tagged_cbor(),
        "date(-100)",
        "tagged(date, negative(-100))",
        "1(-100)",
        "1(-100)   / date /",
        "1(-100)",
        "1969-12-31T23:58:20Z",
        "c13863",
        textwrap.dedent("""\
            c1          # tag(1) date
                3863    # negative(-100)"""),
    )

    _run_format_test(
        "format_date_positive",
        Date.from_timestamp(1647887071.0).to_tagged_cbor(),
        "date(1647887071)",
        "tagged(date, unsigned(1647887071))",
        "1(1647887071)",
        "1(1647887071)   / date /",
        "1(1647887071)",
        "2022-03-21T18:24:31Z",
        "c11a6238c2df",
        textwrap.dedent("""\
            c1              # tag(1) date
                1a6238c2df  # unsigned(1647887071)"""),
    )


def test_format_fractional_date():
    register_tags()

    _run_format_test(
        "format_fractional_date",
        Date.from_timestamp(0.5).to_tagged_cbor(),
        "date(0.5)",
        "tagged(date, simple(0.5))",
        "1(0.5)",
        "1(0.5)   / date /",
        "1(0.5)",
        "1970-01-01",
        "c1f93800",
        textwrap.dedent("""\
            c1          # tag(1) date
                f93800  # 0.5"""),
    )


def test_format_key_order():
    m = Map()
    m.insert(-1, CBOR.from_int(3))
    m.insert(CBOR.from_array([CBOR.from_int(-1)]), CBOR.from_int(7))
    m.insert("z", CBOR.from_int(4))
    m.insert(10, CBOR.from_int(1))
    m.insert(False, CBOR.from_int(8))
    m.insert(100, CBOR.from_int(2))
    m.insert("aa", CBOR.from_int(5))
    m.insert(CBOR.from_array([CBOR.from_int(100)]), CBOR.from_int(6))

    cbor = CBOR.from_map(m)
    description = '{10: 1, 100: 2, -1: 3, "z": 4, "aa": 5, [100]: 6, [-1]: 7, false: 8}'
    debug_description = 'map({0x0a: (unsigned(10), unsigned(1)), 0x1864: (unsigned(100), unsigned(2)), 0x20: (negative(-1), unsigned(3)), 0x617a: (text("z"), unsigned(4)), 0x626161: (text("aa"), unsigned(5)), 0x811864: (array([unsigned(100)]), unsigned(6)), 0x8120: (array([negative(-1)]), unsigned(7)), 0xf4: (simple(false), unsigned(8))})'
    diagnostic = textwrap.dedent("""\
        {
            10:
            1,
            100:
            2,
            -1:
            3,
            "z":
            4,
            "aa":
            5,
            [100]:
            6,
            [-1]:
            7,
            false:
            8
        }""")
    diagnostic_flat = '{10: 1, 100: 2, -1: 3, "z": 4, "aa": 5, [100]: 6, [-1]: 7, false: 8}'
    hex_val = "a80a011864022003617a046261610581186406812007f408"
    hex_annotated = textwrap.dedent("""\
        a8              # map(8)
            0a          # unsigned(10)
            01          # unsigned(1)
            1864        # unsigned(100)
            02          # unsigned(2)
            20          # negative(-1)
            03          # unsigned(3)
            61          # text(1)
                7a      # "z"
            04          # unsigned(4)
            62          # text(2)
                6161    # "aa"
            05          # unsigned(5)
            81          # array(1)
                1864    # unsigned(100)
            06          # unsigned(6)
            81          # array(1)
                20      # negative(-1)
            07          # unsigned(7)
            f4          # false
            08          # unsigned(8)""")
    _run_format_test(
        "format_key_order",
        cbor,
        description,
        debug_description,
        diagnostic,
        diagnostic,
        diagnostic_flat,
        diagnostic_flat,
        hex_val,
        hex_annotated,
    )


def test_format_structure():
    encoded_cbor_hex = "d83183015829536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e82d902c3820158402b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710ad902c3820158400f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900"
    cbor = CBOR.from_hex(encoded_cbor_hex)
    description = "49([1, h'536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e', [707([1, h'2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a']), 707([1, h'0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900'])]])"
    debug_description = "tagged(49, array([unsigned(1), bytes(536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e), array([tagged(707, array([unsigned(1), bytes(2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a)])), tagged(707, array([unsigned(1), bytes(0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900)]))])]))"
    diagnostic = textwrap.dedent("""\
        49(
            [
                1,
                h'536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e',
                [
                    707(
                        [
                            1,
                            h'2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a'
                        ]
                    ),
                    707(
                        [
                            1,
                            h'0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900'
                        ]
                    )
                ]
            ]
        )""")
    diagnostic_flat = "49([1, h'536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e', [707([1, h'2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a']), 707([1, h'0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900'])]])"
    hex_val = encoded_cbor_hex
    hex_annotated = textwrap.dedent("""\
        d8 31                                   # tag(49)
            83                                  # array(3)
                01                              # unsigned(1)
                5829                            # bytes(41)
                    536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e # "Some mysteries aren't meant to be solved."
                82                              # array(2)
                    d9 02c3                     # tag(707)
                        82                      # array(2)
                            01                  # unsigned(1)
                            5840                # bytes(64)
                                2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a
                    d9 02c3                     # tag(707)
                        82                      # array(2)
                            01                  # unsigned(1)
                            5840                # bytes(64)
                                0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900""")
    _run_format_test(
        "format_structure",
        cbor,
        description,
        debug_description,
        diagnostic,
        diagnostic,
        diagnostic_flat,
        diagnostic_flat,
        hex_val,
        hex_annotated,
    )


def test_format_structure_2():
    register_tags()

    encoded_cbor_hex = "d9012ca4015059f2293a5bce7d4de59e71b4207ac5d202c11a6035970003754461726b20507572706c652041717561204c6f766504787b4c6f72656d20697073756d20646f6c6f722073697420616d65742c20636f6e73656374657475722061646970697363696e6720656c69742c2073656420646f20656975736d6f642074656d706f7220696e6369646964756e74207574206c61626f726520657420646f6c6f7265206d61676e6120616c697175612e"
    cbor = CBOR.from_hex(encoded_cbor_hex)
    description = '300({1: h\'59f2293a5bce7d4de59e71b4207ac5d2\', 2: 1(1614124800), 3: "Dark Purple Aqua Love", 4: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."})'
    debug_description = 'tagged(300, map({0x01: (unsigned(1), bytes(59f2293a5bce7d4de59e71b4207ac5d2)), 0x02: (unsigned(2), tagged(1, unsigned(1614124800))), 0x03: (unsigned(3), text("Dark Purple Aqua Love")), 0x04: (unsigned(4), text("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."))}))'
    diagnostic = textwrap.dedent("""\
        300(
            {
                1:
                h'59f2293a5bce7d4de59e71b4207ac5d2',
                2:
                1(1614124800),
                3:
                "Dark Purple Aqua Love",
                4:
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
            }
        )""")
    diagnostic_annotated = textwrap.dedent("""\
        300(
            {
                1:
                h'59f2293a5bce7d4de59e71b4207ac5d2',
                2:
                1(1614124800),   / date /
                3:
                "Dark Purple Aqua Love",
                4:
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
            }
        )""")
    diagnostic_flat = '300({1: h\'59f2293a5bce7d4de59e71b4207ac5d2\', 2: 1(1614124800), 3: "Dark Purple Aqua Love", 4: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."})'
    summary = '300({1: h\'59f2293a5bce7d4de59e71b4207ac5d2\', 2: 2021-02-24, 3: "Dark Purple Aqua Love", 4: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."})'
    hex_val = encoded_cbor_hex
    hex_annotated = textwrap.dedent("""\
        d9 012c                                 # tag(300)
            a4                                  # map(4)
                01                              # unsigned(1)
                50                              # bytes(16)
                    59f2293a5bce7d4de59e71b4207ac5d2
                02                              # unsigned(2)
                c1                              # tag(1) date
                    1a60359700                  # unsigned(1614124800)
                03                              # unsigned(3)
                75                              # text(21)
                    4461726b20507572706c652041717561204c6f7665 # "Dark Purple Aqua Love"
                04                              # unsigned(4)
                78 7b                           # text(123)
                    4c6f72656d20697073756d20646f6c6f722073697420616d65742c20636f6e73656374657475722061646970697363696e6720656c69742c2073656420646f20656975736d6f642074656d706f7220696e6369646964756e74207574206c61626f726520657420646f6c6f7265206d61676e6120616c697175612e # "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\"""")
    _run_format_test(
        "format_structure_2",
        cbor,
        description,
        debug_description,
        diagnostic,
        diagnostic_annotated,
        diagnostic_flat,
        summary,
        hex_val,
        hex_annotated,
    )
