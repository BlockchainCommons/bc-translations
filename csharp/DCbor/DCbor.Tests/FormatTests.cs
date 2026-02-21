namespace BlockchainCommons.DCbor.Tests;

public class FormatTests
{
    // --- Helper ---

    private static void Run(
        string testName,
        Cbor cbor,
        string expectedDescription,
        string expectedDebugDescription,
        string expectedDiagnostic,
        string expectedDiagnosticAnnotated,
        string expectedDiagnosticFlat,
        string expectedSummary,
        string expectedHex,
        string expectedHexAnnotated)
    {
        Assert.Equal(expectedDescription, cbor.ToString());
        Assert.Equal(expectedDebugDescription, cbor.DebugDescription);
        Assert.Equal(expectedDiagnostic, cbor.Diagnostic());
        Assert.Equal(expectedDiagnosticAnnotated, cbor.DiagnosticAnnotated());
        Assert.Equal(expectedDiagnosticFlat, cbor.DiagnosticFlat());
        Assert.Equal(expectedSummary, cbor.Summary());
        Assert.Equal(expectedHex, cbor.Hex());
        Assert.Equal(expectedHexAnnotated, cbor.HexAnnotated());
    }

    // --- Tests ---

    [Fact]
    public void FormatSimple1()
    {
        Run("format_simple_1",
            Cbor.False(),
            "false",
            "simple(false)",
            "false",
            "false",
            "false",
            "false",
            "f4",
            "f4  # false");
    }

    [Fact]
    public void FormatSimple2()
    {
        Run("format_simple_2",
            Cbor.True(),
            "true",
            "simple(true)",
            "true",
            "true",
            "true",
            "true",
            "f5",
            "f5  # true");
    }

    [Fact]
    public void FormatSimple3()
    {
        Run("format_simple_3",
            Cbor.Null(),
            "null",
            "simple(null)",
            "null",
            "null",
            "null",
            "null",
            "f6",
            "f6  # null");
    }

    [Fact]
    public void FormatUnsigned()
    {
        Run("format_unsigned_0",
            Cbor.FromInt(0),
            "0",
            "unsigned(0)",
            "0",
            "0",
            "0",
            "0",
            "00",
            "00  # unsigned(0)");

        Run("format_unsigned_23",
            Cbor.FromInt(23),
            "23",
            "unsigned(23)",
            "23",
            "23",
            "23",
            "23",
            "17",
            "17  # unsigned(23)");

        Run("format_unsigned_65546",
            Cbor.FromInt(65546),
            "65546",
            "unsigned(65546)",
            "65546",
            "65546",
            "65546",
            "65546",
            "1a0001000a",
            "1a0001000a  # unsigned(65546)");

        Run("format_unsigned_1000000000",
            Cbor.FromInt(1000000000),
            "1000000000",
            "unsigned(1000000000)",
            "1000000000",
            "1000000000",
            "1000000000",
            "1000000000",
            "1a3b9aca00",
            "1a3b9aca00  # unsigned(1000000000)");
    }

    [Fact]
    public void FormatNegative()
    {
        Run("format_negative_neg1",
            Cbor.FromInt(-1),
            "-1",
            "negative(-1)",
            "-1",
            "-1",
            "-1",
            "-1",
            "20",
            "20  # negative(-1)");

        Run("format_negative_neg1000",
            Cbor.FromInt(-1000),
            "-1000",
            "negative(-1000)",
            "-1000",
            "-1000",
            "-1000",
            "-1000",
            "3903e7",
            "3903e7  # negative(-1000)");

        Run("format_negative_neg1000000",
            Cbor.FromInt(-1000000),
            "-1000000",
            "negative(-1000000)",
            "-1000000",
            "-1000000",
            "-1000000",
            "-1000000",
            "3a000f423f",
            "3a000f423f  # negative(-1000000)");
    }

    [Fact]
    public void FormatString()
    {
        Run("format_string",
            Cbor.FromString("Test"),
            "\"Test\"",
            "text(\"Test\")",
            "\"Test\"",
            "\"Test\"",
            "\"Test\"",
            "\"Test\"",
            "6454657374",
            "64              # text(4)\n" +
            "    54657374    # \"Test\"");
    }

    [Fact]
    public void FormatSimpleArray()
    {
        Run("format_simple_array",
            new Cbor(CborCase.Array(new List<Cbor>
            {
                Cbor.FromInt(1), Cbor.FromInt(2), Cbor.FromInt(3)
            })),
            "[1, 2, 3]",
            "array([unsigned(1), unsigned(2), unsigned(3)])",
            "[1, 2, 3]",
            "[1, 2, 3]",
            "[1, 2, 3]",
            "[1, 2, 3]",
            "83010203",
            "83      # array(3)\n" +
            "    01  # unsigned(1)\n" +
            "    02  # unsigned(2)\n" +
            "    03  # unsigned(3)");
    }

    [Fact]
    public void FormatNestedArray()
    {
        var a = new Cbor(CborCase.Array(new List<Cbor>
        {
            Cbor.FromInt(1), Cbor.FromInt(2), Cbor.FromInt(3)
        }));
        var b = new Cbor(CborCase.Array(new List<Cbor>
        {
            Cbor.FromString("A"), Cbor.FromString("B"), Cbor.FromString("C")
        }));
        var c = new Cbor(CborCase.Array(new List<Cbor> { a, b }));

        Run("format_nested_array",
            c,
            "[[1, 2, 3], [\"A\", \"B\", \"C\"]]",
            "array([array([unsigned(1), unsigned(2), unsigned(3)]), array([text(\"A\"), text(\"B\"), text(\"C\")])])",
            "[\n" +
            "    [1, 2, 3],\n" +
            "    [\"A\", \"B\", \"C\"]\n" +
            "]",
            "[\n" +
            "    [1, 2, 3],\n" +
            "    [\"A\", \"B\", \"C\"]\n" +
            "]",
            "[[1, 2, 3], [\"A\", \"B\", \"C\"]]",
            "[[1, 2, 3], [\"A\", \"B\", \"C\"]]",
            "828301020383614161426143",
            "82              # array(2)\n" +
            "    83          # array(3)\n" +
            "        01      # unsigned(1)\n" +
            "        02      # unsigned(2)\n" +
            "        03      # unsigned(3)\n" +
            "    83          # array(3)\n" +
            "        61      # text(1)\n" +
            "            41  # \"A\"\n" +
            "        61      # text(1)\n" +
            "            42  # \"B\"\n" +
            "        61      # text(1)\n" +
            "            43  # \"C\"");
    }

    [Fact]
    public void FormatMap()
    {
        var map = new CborMap();
        map.Insert(Cbor.FromInt(1), Cbor.FromString("A"));
        map.Insert(Cbor.FromInt(2), Cbor.FromString("B"));

        Run("format_map",
            new Cbor(CborCase.Map(map)),
            "{1: \"A\", 2: \"B\"}",
            "map({0x01: (unsigned(1), text(\"A\")), 0x02: (unsigned(2), text(\"B\"))})",
            "{1: \"A\", 2: \"B\"}",
            "{1: \"A\", 2: \"B\"}",
            "{1: \"A\", 2: \"B\"}",
            "{1: \"A\", 2: \"B\"}",
            "a2016141026142",
            "a2          # map(2)\n" +
            "    01      # unsigned(1)\n" +
            "    61      # text(1)\n" +
            "        41  # \"A\"\n" +
            "    02      # unsigned(2)\n" +
            "    61      # text(1)\n" +
            "        42  # \"B\"");
    }

    [Fact]
    public void FormatTagged()
    {
        var a = Cbor.ToTaggedValue(100, Cbor.FromString("Hello"));
        Run("format_tagged",
            a,
            "100(\"Hello\")",
            "tagged(100, text(\"Hello\"))",
            "100(\"Hello\")",
            "100(\"Hello\")",
            "100(\"Hello\")",
            "100(\"Hello\")",
            "d8646548656c6c6f",
            "d8 64               # tag(100)\n" +
            "    65              # text(5)\n" +
            "        48656c6c6f  # \"Hello\"");
    }

    [Fact]
    public void FormatDate()
    {
        GlobalTags.RegisterTags();

        Run("format_date_negative",
            CborDate.FromTimestamp(-100.0).TaggedCbor(),
            "date(-100)",
            "tagged(date, negative(-100))",
            "1(-100)",
            "1(-100)   / date /",
            "1(-100)",
            "1969-12-31T23:58:20Z",
            "c13863",
            "c1          # tag(1) date\n" +
            "    3863    # negative(-100)");

        Run("format_date_positive",
            CborDate.FromTimestamp(1647887071.0).TaggedCbor(),
            "date(1647887071)",
            "tagged(date, unsigned(1647887071))",
            "1(1647887071)",
            "1(1647887071)   / date /",
            "1(1647887071)",
            "2022-03-21T18:24:31Z",
            "c11a6238c2df",
            "c1              # tag(1) date\n" +
            "    1a6238c2df  # unsigned(1647887071)");
    }

    [Fact]
    public void FormatFractionalDate()
    {
        GlobalTags.RegisterTags();

        Run("format_fractional_date",
            CborDate.FromTimestamp(0.5).TaggedCbor(),
            "date(0.5)",
            "tagged(date, simple(0.5))",
            "1(0.5)",
            "1(0.5)   / date /",
            "1(0.5)",
            "1970-01-01",
            "c1f93800",
            "c1          # tag(1) date\n" +
            "    f93800  # 0.5");
    }

    [Fact]
    public void FormatStructure()
    {
        var encodedCborHex = "d83183015829536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e82d902c3820158402b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710ad902c3820158400f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900";
        var cbor = Cbor.TryFromHex(encodedCborHex);

        var description = "49([1, h'536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e', [707([1, h'2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a']), 707([1, h'0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900'])]])";
        var debugDescription = "tagged(49, array([unsigned(1), bytes(536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e), array([tagged(707, array([unsigned(1), bytes(2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a)])), tagged(707, array([unsigned(1), bytes(0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900)]))])]))";
        var diagnostic =
            "49(\n" +
            "    [\n" +
            "        1,\n" +
            "        h'536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e',\n" +
            "        [\n" +
            "            707(\n" +
            "                [\n" +
            "                    1,\n" +
            "                    h'2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a'\n" +
            "                ]\n" +
            "            ),\n" +
            "            707(\n" +
            "                [\n" +
            "                    1,\n" +
            "                    h'0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900'\n" +
            "                ]\n" +
            "            )\n" +
            "        ]\n" +
            "    ]\n" +
            ")";
        var diagnosticFlat = "49([1, h'536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e', [707([1, h'2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a']), 707([1, h'0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900'])]])";
        var hex = "d83183015829536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e82d902c3820158402b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710ad902c3820158400f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900";
        var hexAnnotated =
            "d8 31                                   # tag(49)\n" +
            "    83                                  # array(3)\n" +
            "        01                              # unsigned(1)\n" +
            "        5829                            # bytes(41)\n" +
            "            536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e # \"Some mysteries aren't meant to be solved.\"\n" +
            "        82                              # array(2)\n" +
            "            d9 02c3                     # tag(707)\n" +
            "                82                      # array(2)\n" +
            "                    01                  # unsigned(1)\n" +
            "                    5840                # bytes(64)\n" +
            "                        2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a\n" +
            "            d9 02c3                     # tag(707)\n" +
            "                82                      # array(2)\n" +
            "                    01                  # unsigned(1)\n" +
            "                    5840                # bytes(64)\n" +
            "                        0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900";

        Run("format_structure",
            cbor,
            description,
            debugDescription,
            diagnostic,
            diagnostic,     // annotated same as diagnostic (no known tags)
            diagnosticFlat,
            diagnosticFlat,  // summary same as flat (no known tags)
            hex,
            hexAnnotated);
    }

    [Fact]
    public void FormatStructure2()
    {
        GlobalTags.RegisterTags();

        var encodedCborHex = "d9012ca4015059f2293a5bce7d4de59e71b4207ac5d202c11a6035970003754461726b20507572706c652041717561204c6f766504787b4c6f72656d20697073756d20646f6c6f722073697420616d65742c20636f6e73656374657475722061646970697363696e6720656c69742c2073656420646f20656975736d6f642074656d706f7220696e6369646964756e74207574206c61626f726520657420646f6c6f7265206d61676e6120616c697175612e";
        var cbor = Cbor.TryFromHex(encodedCborHex);

        var description = "300({1: h'59f2293a5bce7d4de59e71b4207ac5d2', 2: 1(1614124800), 3: \"Dark Purple Aqua Love\", 4: \"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\"})";
        var debugDescription = "tagged(300, map({0x01: (unsigned(1), bytes(59f2293a5bce7d4de59e71b4207ac5d2)), 0x02: (unsigned(2), tagged(1, unsigned(1614124800))), 0x03: (unsigned(3), text(\"Dark Purple Aqua Love\")), 0x04: (unsigned(4), text(\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\"))}))";
        var diagnostic =
            "300(\n" +
            "    {\n" +
            "        1:\n" +
            "        h'59f2293a5bce7d4de59e71b4207ac5d2',\n" +
            "        2:\n" +
            "        1(1614124800),\n" +
            "        3:\n" +
            "        \"Dark Purple Aqua Love\",\n" +
            "        4:\n" +
            "        \"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\"\n" +
            "    }\n" +
            ")";
        var diagnosticAnnotated =
            "300(\n" +
            "    {\n" +
            "        1:\n" +
            "        h'59f2293a5bce7d4de59e71b4207ac5d2',\n" +
            "        2:\n" +
            "        1(1614124800),   / date /\n" +
            "        3:\n" +
            "        \"Dark Purple Aqua Love\",\n" +
            "        4:\n" +
            "        \"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\"\n" +
            "    }\n" +
            ")";
        var diagnosticFlat = "300({1: h'59f2293a5bce7d4de59e71b4207ac5d2', 2: 1(1614124800), 3: \"Dark Purple Aqua Love\", 4: \"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\"})";
        var summary = "300({1: h'59f2293a5bce7d4de59e71b4207ac5d2', 2: 2021-02-24, 3: \"Dark Purple Aqua Love\", 4: \"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\"})";
        var hex = "d9012ca4015059f2293a5bce7d4de59e71b4207ac5d202c11a6035970003754461726b20507572706c652041717561204c6f766504787b4c6f72656d20697073756d20646f6c6f722073697420616d65742c20636f6e73656374657475722061646970697363696e6720656c69742c2073656420646f20656975736d6f642074656d706f7220696e6369646964756e74207574206c61626f726520657420646f6c6f7265206d61676e6120616c697175612e";
        var hexAnnotated =
            "d9 012c                                 # tag(300)\n" +
            "    a4                                  # map(4)\n" +
            "        01                              # unsigned(1)\n" +
            "        50                              # bytes(16)\n" +
            "            59f2293a5bce7d4de59e71b4207ac5d2\n" +
            "        02                              # unsigned(2)\n" +
            "        c1                              # tag(1) date\n" +
            "            1a60359700                  # unsigned(1614124800)\n" +
            "        03                              # unsigned(3)\n" +
            "        75                              # text(21)\n" +
            "            4461726b20507572706c652041717561204c6f7665 # \"Dark Purple Aqua Love\"\n" +
            "        04                              # unsigned(4)\n" +
            "        78 7b                           # text(123)\n" +
            "            4c6f72656d20697073756d20646f6c6f722073697420616d65742c20636f6e73656374657475722061646970697363696e6720656c69742c2073656420646f20656975736d6f642074656d706f7220696e6369646964756e74207574206c61626f726520657420646f6c6f7265206d61676e6120616c697175612e # \"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\"";

        Run("format_structure_2",
            cbor,
            description,
            debugDescription,
            diagnostic,
            diagnosticAnnotated,
            diagnosticFlat,
            summary,
            hex,
            hexAnnotated);
    }

    [Fact]
    public void FormatKeyOrder()
    {
        var m = new CborMap();
        m.Insert(Cbor.FromInt(-1), Cbor.FromInt(3));
        m.Insert(new Cbor(CborCase.Array(new List<Cbor> { Cbor.FromInt(-1) })), Cbor.FromInt(7));
        m.Insert(Cbor.FromString("z"), Cbor.FromInt(4));
        m.Insert(Cbor.FromInt(10), Cbor.FromInt(1));
        m.Insert(Cbor.FromBool(false), Cbor.FromInt(8));
        m.Insert(Cbor.FromInt(100), Cbor.FromInt(2));
        m.Insert(Cbor.FromString("aa"), Cbor.FromInt(5));
        m.Insert(new Cbor(CborCase.Array(new List<Cbor> { Cbor.FromInt(100) })), Cbor.FromInt(6));

        var cbor = new Cbor(CborCase.Map(m));

        var description = "{10: 1, 100: 2, -1: 3, \"z\": 4, \"aa\": 5, [100]: 6, [-1]: 7, false: 8}";
        var debugDescription = "map({0x0a: (unsigned(10), unsigned(1)), 0x1864: (unsigned(100), unsigned(2)), 0x20: (negative(-1), unsigned(3)), 0x617a: (text(\"z\"), unsigned(4)), 0x626161: (text(\"aa\"), unsigned(5)), 0x811864: (array([unsigned(100)]), unsigned(6)), 0x8120: (array([negative(-1)]), unsigned(7)), 0xf4: (simple(false), unsigned(8))})";
        var diagnostic =
            "{\n" +
            "    10:\n" +
            "    1,\n" +
            "    100:\n" +
            "    2,\n" +
            "    -1:\n" +
            "    3,\n" +
            "    \"z\":\n" +
            "    4,\n" +
            "    \"aa\":\n" +
            "    5,\n" +
            "    [100]:\n" +
            "    6,\n" +
            "    [-1]:\n" +
            "    7,\n" +
            "    false:\n" +
            "    8\n" +
            "}";
        var diagnosticFlat = "{10: 1, 100: 2, -1: 3, \"z\": 4, \"aa\": 5, [100]: 6, [-1]: 7, false: 8}";
        var hex = "a80a011864022003617a046261610581186406812007f408";
        var hexAnnotated =
            "a8              # map(8)\n" +
            "    0a          # unsigned(10)\n" +
            "    01          # unsigned(1)\n" +
            "    1864        # unsigned(100)\n" +
            "    02          # unsigned(2)\n" +
            "    20          # negative(-1)\n" +
            "    03          # unsigned(3)\n" +
            "    61          # text(1)\n" +
            "        7a      # \"z\"\n" +
            "    04          # unsigned(4)\n" +
            "    62          # text(2)\n" +
            "        6161    # \"aa\"\n" +
            "    05          # unsigned(5)\n" +
            "    81          # array(1)\n" +
            "        1864    # unsigned(100)\n" +
            "    06          # unsigned(6)\n" +
            "    81          # array(1)\n" +
            "        20      # negative(-1)\n" +
            "    07          # unsigned(7)\n" +
            "    f4          # false\n" +
            "    08          # unsigned(8)";

        Run("format_key_order",
            cbor,
            description,
            debugDescription,
            diagnostic,
            diagnostic,     // annotated same (no known tags in map)
            diagnosticFlat,
            diagnosticFlat,  // summary same (no known tags in map)
            hex,
            hexAnnotated);
    }
}
