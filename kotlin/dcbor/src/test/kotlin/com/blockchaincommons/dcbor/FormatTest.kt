package com.blockchaincommons.dcbor

import kotlin.test.Test
import kotlin.test.assertEquals

class FormatTest {

    private fun run(
        testName: String,
        cbor: Cbor,
        expectedDescription: String,
        expectedDebugDescription: String,
        expectedDiagnostic: String,
        expectedDiagnosticAnnotated: String,
        expectedDiagnosticFlat: String,
        expectedSummary: String,
        expectedHex: String,
        expectedHexAnnotated: String
    ) {
        assertEquals(expectedDescription, cbor.description, "description in test '$testName'")
        assertEquals(expectedDebugDescription, cbor.debugDescription, "debug_description in test '$testName'")
        assertEquals(expectedDiagnostic, cbor.diagnostic(), "diagnostic in test '$testName'")
        assertEquals(expectedDiagnosticAnnotated, cbor.diagnosticAnnotated(), "diagnostic_annotated in test '$testName'")
        assertEquals(expectedDiagnosticFlat, cbor.diagnosticFlat, "diagnostic_flat in test '$testName'")
        assertEquals(expectedSummary, cbor.summary(), "summary in test '$testName'")
        assertEquals(expectedHex, cbor.hex, "hex in test '$testName'")
        assertEquals(expectedHexAnnotated, cbor.hexAnnotated(), "hex_annotated in test '$testName'")
    }

    @Test
    fun formatSimple1() {
        run(
            "format_simple_1",
            Cbor.`false`(),
            "false",
            "simple(false)",
            "false",
            "false",
            "false",
            "false",
            "f4",
            "f4  # false"
        )
    }

    @Test
    fun formatSimple2() {
        run(
            "format_simple_2",
            Cbor.`true`(),
            "true",
            "simple(true)",
            "true",
            "true",
            "true",
            "true",
            "f5",
            "f5  # true"
        )
    }

    @Test
    fun formatSimple3() {
        run(
            "format_simple_3",
            Cbor.`null`(),
            "null",
            "simple(null)",
            "null",
            "null",
            "null",
            "null",
            "f6",
            "f6  # null"
        )
    }

    @Test
    fun formatUnsigned() {
        run(
            "format_unsigned_0",
            0.toCbor(),
            "0",
            "unsigned(0)",
            "0",
            "0",
            "0",
            "0",
            "00",
            "00  # unsigned(0)"
        )
        run(
            "format_unsigned_23",
            23.toCbor(),
            "23",
            "unsigned(23)",
            "23",
            "23",
            "23",
            "23",
            "17",
            "17  # unsigned(23)"
        )
        run(
            "format_unsigned_65546",
            65546.toCbor(),
            "65546",
            "unsigned(65546)",
            "65546",
            "65546",
            "65546",
            "65546",
            "1a0001000a",
            "1a0001000a  # unsigned(65546)"
        )
        run(
            "format_unsigned_1000000000",
            1000000000.toCbor(),
            "1000000000",
            "unsigned(1000000000)",
            "1000000000",
            "1000000000",
            "1000000000",
            "1000000000",
            "1a3b9aca00",
            "1a3b9aca00  # unsigned(1000000000)"
        )
    }

    @Test
    fun formatNegative() {
        run(
            "format_negative_neg1",
            (-1).toCbor(),
            "-1",
            "negative(-1)",
            "-1",
            "-1",
            "-1",
            "-1",
            "20",
            "20  # negative(-1)"
        )
        run(
            "format_negative_neg1000",
            (-1000).toCbor(),
            "-1000",
            "negative(-1000)",
            "-1000",
            "-1000",
            "-1000",
            "-1000",
            "3903e7",
            "3903e7  # negative(-1000)"
        )
        run(
            "format_negative_neg1000000",
            (-1000000).toCbor(),
            "-1000000",
            "negative(-1000000)",
            "-1000000",
            "-1000000",
            "-1000000",
            "-1000000",
            "3a000f423f",
            "3a000f423f  # negative(-1000000)"
        )
    }

    @Test
    fun formatString() {
        run(
            "format_string",
            "Test".toCbor(),
            "\"Test\"",
            "text(\"Test\")",
            "\"Test\"",
            "\"Test\"",
            "\"Test\"",
            "\"Test\"",
            "6454657374",
            """
            64              # text(4)
                54657374    # "Test"
            """.trimIndent()
        )
    }

    @Test
    fun formatSimpleArray() {
        run(
            "format_simple_array",
            listOf(1.toCbor(), 2.toCbor(), 3.toCbor()).toCbor(),
            "[1, 2, 3]",
            "array([unsigned(1), unsigned(2), unsigned(3)])",
            "[1, 2, 3]",
            "[1, 2, 3]",
            "[1, 2, 3]",
            "[1, 2, 3]",
            "83010203",
            """
            83      # array(3)
                01  # unsigned(1)
                02  # unsigned(2)
                03  # unsigned(3)
            """.trimIndent()
        )
    }

    @Test
    fun formatNestedArray() {
        val a = listOf(1.toCbor(), 2.toCbor(), 3.toCbor()).toCbor()
        val b = listOf("A".toCbor(), "B".toCbor(), "C".toCbor()).toCbor()
        val c = listOf(a, b).toCbor()
        run(
            "format_nested_array",
            c,
            """[[1, 2, 3], ["A", "B", "C"]]""",
            """array([array([unsigned(1), unsigned(2), unsigned(3)]), array([text("A"), text("B"), text("C")])])""",
            """
            [
                [1, 2, 3],
                ["A", "B", "C"]
            ]
            """.trimIndent(),
            """
            [
                [1, 2, 3],
                ["A", "B", "C"]
            ]
            """.trimIndent(),
            """[[1, 2, 3], ["A", "B", "C"]]""",
            """[[1, 2, 3], ["A", "B", "C"]]""",
            "828301020383614161426143",
            """
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
                        43  # "C"
            """.trimIndent()
        )
    }

    @Test
    fun formatMap() {
        val map = CborMap()
        map.insert(1.toCbor(), "A".toCbor())
        map.insert(2.toCbor(), "B".toCbor())
        run(
            "format_map",
            Cbor.fromMap(map),
            """{1: "A", 2: "B"}""",
            """map({0x01: (unsigned(1), text("A")), 0x02: (unsigned(2), text("B"))})""",
            """{1: "A", 2: "B"}""",
            """{1: "A", 2: "B"}""",
            """{1: "A", 2: "B"}""",
            """{1: "A", 2: "B"}""",
            "a2016141026142",
            """
            a2          # map(2)
                01      # unsigned(1)
                61      # text(1)
                    41  # "A"
                02      # unsigned(2)
                61      # text(1)
                    42  # "B"
            """.trimIndent()
        )
    }

    @Test
    fun formatTagged() {
        val a = Cbor.taggedValue(100uL, "Hello".toCbor())
        run(
            "format_tagged",
            a,
            """100("Hello")""",
            """tagged(100, text("Hello"))""",
            """100("Hello")""",
            """100("Hello")""",
            """100("Hello")""",
            """100("Hello")""",
            "d8646548656c6c6f",
            """
            d8 64               # tag(100)
                65              # text(5)
                    48656c6c6f  # "Hello"
            """.trimIndent()
        )
    }

    @Test
    fun formatDate() {
        registerTags()
        run(
            "format_date_negative",
            CborDate.fromTimestamp(-100.0).taggedCbor(),
            "date(-100)",
            "tagged(date, negative(-100))",
            "1(-100)",
            "1(-100)   / date /",
            "1(-100)",
            "1969-12-31T23:58:20Z",
            "c13863",
            """
            c1          # tag(1) date
                3863    # negative(-100)
            """.trimIndent()
        )
        run(
            "format_date_positive",
            CborDate.fromTimestamp(1647887071.0).taggedCbor(),
            "date(1647887071)",
            "tagged(date, unsigned(1647887071))",
            "1(1647887071)",
            "1(1647887071)   / date /",
            "1(1647887071)",
            "2022-03-21T18:24:31Z",
            "c11a6238c2df",
            """
            c1              # tag(1) date
                1a6238c2df  # unsigned(1647887071)
            """.trimIndent()
        )
    }

    @Test
    fun formatFractionalDate() {
        registerTags()
        run(
            "format_fractional_date",
            CborDate.fromTimestamp(0.5).taggedCbor(),
            "date(0.5)",
            "tagged(date, simple(0.5))",
            "1(0.5)",
            "1(0.5)   / date /",
            "1(0.5)",
            "1970-01-01",
            "c1f93800",
            """
            c1          # tag(1) date
                f93800  # 0.5
            """.trimIndent()
        )
    }

    @Test
    fun formatKeyOrder() {
        val m = CborMap()
        m.insert((-1).toCbor(), 3.toCbor())
        m.insert(listOf((-1).toCbor()).toCbor(), 7.toCbor())
        m.insert("z".toCbor(), 4.toCbor())
        m.insert(10.toCbor(), 1.toCbor())
        m.insert(false.toCbor(), 8.toCbor())
        m.insert(100.toCbor(), 2.toCbor())
        m.insert("aa".toCbor(), 5.toCbor())
        m.insert(listOf(100.toCbor()).toCbor(), 6.toCbor())

        val cbor = Cbor.fromMap(m)
        assertEquals(
            """{10: 1, 100: 2, -1: 3, "z": 4, "aa": 5, [100]: 6, [-1]: 7, false: 8}""",
            cbor.diagnosticFlat
        )
        assertEquals(
            "a80a011864022003617a046261610581186406812007f408",
            cbor.hex
        )
    }

    @Test
    fun formatStructure() {
        val hex = "d83183015829536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e82d902c3820158402b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710ad902c3820158400f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900"
        val cbor = Cbor.tryFromHex(hex)
        run(
            "format_structure",
            cbor,
            "49([1, h'536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e', [707([1, h'2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a']), 707([1, h'0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900'])]])",
            "tagged(49, array([unsigned(1), bytes(536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e), array([tagged(707, array([unsigned(1), bytes(2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a)])), tagged(707, array([unsigned(1), bytes(0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900)]))])]))",
            """
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
            )
            """.trimIndent(),
            """
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
            )
            """.trimIndent(),
            "49([1, h'536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e', [707([1, h'2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a']), 707([1, h'0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900'])]])",
            "49([1, h'536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e', [707([1, h'2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a']), 707([1, h'0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900'])]])",
            hex,
            """
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
                                    0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900
            """.trimIndent()
        )
    }

    @Test
    fun formatStructure2() {
        registerTags()
        val hex = "d9012ca4015059f2293a5bce7d4de59e71b4207ac5d202c11a6035970003754461726b20507572706c652041717561204c6f766504787b4c6f72656d20697073756d20646f6c6f722073697420616d65742c20636f6e73656374657475722061646970697363696e6720656c69742c2073656420646f20656975736d6f642074656d706f7220696e6369646964756e74207574206c61626f726520657420646f6c6f7265206d61676e6120616c697175612e"
        val cbor = Cbor.tryFromHex(hex)
        run(
            "format_structure_2",
            cbor,
            """300({1: h'59f2293a5bce7d4de59e71b4207ac5d2', 2: 1(1614124800), 3: "Dark Purple Aqua Love", 4: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."})""",
            """tagged(300, map({0x01: (unsigned(1), bytes(59f2293a5bce7d4de59e71b4207ac5d2)), 0x02: (unsigned(2), tagged(1, unsigned(1614124800))), 0x03: (unsigned(3), text("Dark Purple Aqua Love")), 0x04: (unsigned(4), text("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."))}))""",
            """
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
            )
            """.trimIndent(),
            """
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
            )
            """.trimIndent(),
            """300({1: h'59f2293a5bce7d4de59e71b4207ac5d2', 2: 1(1614124800), 3: "Dark Purple Aqua Love", 4: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."})""",
            """300({1: h'59f2293a5bce7d4de59e71b4207ac5d2', 2: 2021-02-24, 3: "Dark Purple Aqua Love", 4: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."})""",
            hex,
            """
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
                        4c6f72656d20697073756d20646f6c6f722073697420616d65742c20636f6e73656374657475722061646970697363696e6720656c69742c2073656420646f20656975736d6f642074656d706f7220696e6369646964756e74207574206c61626f726520657420646f6c6f7265206d61676e6120616c697175612e # "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
            """.trimIndent()
        )
    }
}
