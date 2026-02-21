package dcbor

import "testing"

func runFormatCheck(
	t *testing.T,
	name string,
	cbor CBOR,
	expectedDisplay string,
	expectedDebug string,
	expectedDiagnostic string,
	expectedDiagnosticAnnotated string,
	expectedDiagnosticFlat string,
	expectedSummary string,
	expectedHex string,
	expectedHexAnnotated string,
) {
	t.Helper()

	if expectedDisplay != "" {
		if got := cbor.String(); got != expectedDisplay {
			t.Fatalf("%s display mismatch: got %q want %q", name, got, expectedDisplay)
		}
	}
	if expectedDebug != "" {
		if got := cbor.DebugString(); got != expectedDebug {
			t.Fatalf("%s debug mismatch: got %q want %q", name, got, expectedDebug)
		}
	}
	if expectedDiagnostic != "" {
		if got := cbor.Diagnostic(); got != expectedDiagnostic {
			t.Fatalf("%s diagnostic mismatch: got %q want %q", name, got, expectedDiagnostic)
		}
	}
	if expectedDiagnosticAnnotated != "" {
		if got := cbor.DiagnosticAnnotated(); got != expectedDiagnosticAnnotated {
			t.Fatalf("%s diagnostic_annotated mismatch: got %q want %q", name, got, expectedDiagnosticAnnotated)
		}
	}
	if expectedDiagnosticFlat != "" {
		if got := cbor.DiagnosticFlat(); got != expectedDiagnosticFlat {
			t.Fatalf("%s diagnostic_flat mismatch: got %q want %q", name, got, expectedDiagnosticFlat)
		}
	}
	if expectedSummary != "" {
		if got := cbor.Summary(); got != expectedSummary {
			t.Fatalf("%s summary mismatch: got %q want %q", name, got, expectedSummary)
		}
	}
	if expectedHex != "" {
		if got := cbor.Hex(); got != expectedHex {
			t.Fatalf("%s hex mismatch: got %q want %q", name, got, expectedHex)
		}
	}
	if expectedHexAnnotated != "" {
		if got := cbor.HexAnnotated(); got != expectedHexAnnotated {
			t.Fatalf("%s hex_annotated mismatch: got %q want %q", name, got, expectedHexAnnotated)
		}
	}
}

func TestFormatSimpleParity(t *testing.T) {
	runFormatCheck(
		t,
		"false",
		False(),
		"false",
		"simple(false)",
		"false",
		"false",
		"false",
		"false",
		"f4",
		"f4  # false",
	)
	runFormatCheck(
		t,
		"true",
		True(),
		"true",
		"simple(true)",
		"true",
		"true",
		"true",
		"true",
		"f5",
		"f5  # true",
	)
	runFormatCheck(
		t,
		"null",
		Null(),
		"null",
		"simple(null)",
		"null",
		"null",
		"null",
		"null",
		"f6",
		"f6  # null",
	)
}

func TestFormatUnsignedAndNegativeParity(t *testing.T) {
	runFormatCheck(
		t,
		"unsigned_65546",
		MustFromAny(65546),
		"65546",
		"unsigned(65546)",
		"65546",
		"65546",
		"65546",
		"65546",
		"1a0001000a",
		"1a 00 01 00 0a  # unsigned(65546)",
	)
	runFormatCheck(
		t,
		"negative_1000000",
		MustFromAny(-1000000),
		"-1000000",
		"negative(-1000000)",
		"-1000000",
		"-1000000",
		"-1000000",
		"-1000000",
		"3a000f423f",
		"3a 00 0f 42 3f  # negative(-1000000)",
	)
}

func TestFormatStringAndNestedArrayParity(t *testing.T) {
	runFormatCheck(
		t,
		"string",
		MustFromAny("Test"),
		`"Test"`,
		`text("Test")`,
		`"Test"`,
		`"Test"`,
		`"Test"`,
		`"Test"`,
		"6454657374",
		`64        # text(4)
    54657374  # "Test"`,
	)

	a := arrayFromAny(1, 2, 3)
	b := NewCBORArray([]CBOR{MustFromAny("A"), MustFromAny("B"), MustFromAny("C")})
	c := NewCBORArray([]CBOR{a, b})

	// expected-text-output-rubric:
	const nestedDiagnostic = `[
    [1, 2, 3],
    ["A", "B", "C"]
]`
	runFormatCheck(
		t,
		"nested_array",
		c,
		`[[1, 2, 3], ["A", "B", "C"]]`,
		`array([array([unsigned(1), unsigned(2), unsigned(3)]), array([text("A"), text("B"), text("C")])])`,
		nestedDiagnostic,
		nestedDiagnostic,
		`[[1, 2, 3], ["A", "B", "C"]]`,
		`[[1, 2, 3], ["A", "B", "C"]]`,
		"828301020383614161426143",
		`82  # array(2)
    83  # array(3)
        01  # unsigned(1)
        02  # unsigned(2)
        03  # unsigned(3)
    83  # array(3)
        61  # text(1)
            41  # "A"
        61  # text(1)
            42  # "B"
        61  # text(1)
            43  # "C"`,
	)

	runFormatCheck(
		t,
		"tagged_100",
		ToTaggedValue(TagWithValue(100), MustFromAny("Hello")),
		`100("Hello")`,
		`tagged(100, text("Hello"))`,
		`100("Hello")`,
		`100("Hello")`,
		`100("Hello")`,
		`100("Hello")`,
		"d8646548656c6c6f",
		`d8 64       # tag(100)
    65          # text(5)
        48656c6c6f  # "Hello"`,
	)
}

func TestFormatMapKeyOrderAndDateParity(t *testing.T) {
	m := NewMap()
	m.MustInsertAny(-1, 3)
	m.Insert(NewCBORArray([]CBOR{MustFromAny(-1)}), MustFromAny(7))
	m.MustInsertAny("z", 4)
	m.MustInsertAny(10, 1)
	m.MustInsertAny(false, 8)
	m.MustInsertAny(100, 2)
	m.MustInsertAny("aa", 5)
	m.Insert(NewCBORArray([]CBOR{MustFromAny(100)}), MustFromAny(6))
	mapCBOR := NewCBORMap(m)
	const keyOrderDiagnostic = `{
    10: 1,
    100: 2,
    -1: 3,
    "z": 4,
    "aa": 5,
    [100]: 6,
    [-1]: 7,
    false: 8
}`

	runFormatCheck(
		t,
		"map_key_order",
		mapCBOR,
		`{10: 1, 100: 2, -1: 3, "z": 4, "aa": 5, [100]: 6, [-1]: 7, false: 8}`,
		`map({0x0a: (unsigned(10), unsigned(1)), 0x1864: (unsigned(100), unsigned(2)), 0x20: (negative(-1), unsigned(3)), 0x617a: (text("z"), unsigned(4)), 0x626161: (text("aa"), unsigned(5)), 0x811864: (array([unsigned(100)]), unsigned(6)), 0x8120: (array([negative(-1)]), unsigned(7)), 0xf4: (simple(false), unsigned(8))})`,
		keyOrderDiagnostic,
		keyOrderDiagnostic,
		`{10: 1, 100: 2, -1: 3, "z": 4, "aa": 5, [100]: 6, [-1]: 7, false: 8}`,
		`{10: 1, 100: 2, -1: 3, "z": 4, "aa": 5, [100]: 6, [-1]: 7, false: 8}`,
		"a80a011864022003617a046261610581186406812007f408",
		`a8     # map(8)
    0a     # unsigned(10)
    01     # unsigned(1)
    18 64  # unsigned(100)
    02     # unsigned(2)
    20     # negative(-1)
    03     # unsigned(3)
    61     # text(1)
        7a     # "z"
    04     # unsigned(4)
    62     # text(2)
        6161   # "aa"
    05     # unsigned(5)
    81     # array(1)
        18 64  # unsigned(100)
    06     # unsigned(6)
    81     # array(1)
        20     # negative(-1)
    07     # unsigned(7)
    f4     # false
    08     # unsigned(8)`,
	)

	RegisterTags()

	runFormatCheck(
		t,
		"date_negative",
		DateFromTimestamp(-100.0).TaggedCBOR(),
		"date(-100)",
		"tagged(date, negative(-100))",
		"1(-100)",
		"1(-100)   / date /",
		"1(-100)",
		"1969-12-31T23:58:20Z",
		"c13863",
		`c1     # tag(1) date
    38 63  # negative(-100)`,
	)
	runFormatCheck(
		t,
		"date_positive",
		DateFromTimestamp(1647887071.0).TaggedCBOR(),
		"date(1647887071)",
		"tagged(date, unsigned(1647887071))",
		"1(1647887071)",
		"1(1647887071)   / date /",
		"1(1647887071)",
		"2022-03-21T18:24:31Z",
		"c11a6238c2df",
		`c1              # tag(1) date
    1a 62 38 c2 df  # unsigned(1647887071)`,
	)
	runFormatCheck(
		t,
		"date_fractional",
		DateFromTimestamp(0.5).TaggedCBOR(),
		"date(0.5)",
		"tagged(date, simple(0.5))",
		"1(0.5)",
		"1(0.5)   / date /",
		"1(0.5)",
		"1970-01-01",
		"c1f93800",
		`c1        # tag(1) date
    f9 38 00  # 0.5`,
	)
}

func TestFormatComplexStructuresParity(t *testing.T) {
	RegisterTags()

	structureHex := "d83183015829536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e82d902c3820158402b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710ad902c3820158400f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900"
	structure, err := TryFromHex(structureHex)
	if err != nil {
		t.Fatalf("TryFromHex structure failed: %v", err)
	}
	const structureDisplay = "49([1, h'536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e', [707([1, h'2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a']), 707([1, h'0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900'])]])"
	const structureDebug = "tagged(49, array([unsigned(1), bytes(536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e), array([tagged(707, array([unsigned(1), bytes(2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a)])), tagged(707, array([unsigned(1), bytes(0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900)]))])]))"
	// expected-text-output-rubric:
	const structureDiagnostic = `49([
        1,
        h'536f6d65206d7973746572696573206172656e2774206d65616e7420746f20626520736f6c7665642e',
        [
            707([1, h'2b9238e19eafbc154b49ec89edd4e0fb1368e97332c6913b4beb637d1875824f3e43bd7fb0c41fb574f08ce00247413d3ce2d9466e0ccfa4a89b92504982710a']),
            707([1, h'0f9c7af36804ffe5313c00115e5a31aa56814abaa77ff301da53d48613496e9c51a98b36d55f6fb5634fdb0123910cfa4904f1c60523df41013dc3749b377900'])
        ]
    ])`
	runFormatCheck(
		t,
		"format_structure",
		structure,
		structureDisplay,
		structureDebug,
		structureDiagnostic,
		structureDiagnostic,
		structureDisplay,
		structureDisplay,
		structureHex,
		"",
	)

	structure2Hex := "d9012ca4015059f2293a5bce7d4de59e71b4207ac5d202c11a6035970003754461726b20507572706c652041717561204c6f766504787b4c6f72656d20697073756d20646f6c6f722073697420616d65742c20636f6e73656374657475722061646970697363696e6720656c69742c2073656420646f20656975736d6f642074656d706f7220696e6369646964756e74207574206c61626f726520657420646f6c6f7265206d61676e6120616c697175612e"
	structure2, err := TryFromHex(structure2Hex)
	if err != nil {
		t.Fatalf("TryFromHex structure_2 failed: %v", err)
	}
	const structure2Display = `300({1: h'59f2293a5bce7d4de59e71b4207ac5d2', 2: 1(1614124800), 3: "Dark Purple Aqua Love", 4: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."})`
	const structure2Debug = `tagged(300, map({0x01: (unsigned(1), bytes(59f2293a5bce7d4de59e71b4207ac5d2)), 0x02: (unsigned(2), tagged(1, unsigned(1614124800))), 0x03: (unsigned(3), text("Dark Purple Aqua Love")), 0x04: (unsigned(4), text("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."))}))`
	// expected-text-output-rubric:
	const structure2Diagnostic = `300({
        1: h'59f2293a5bce7d4de59e71b4207ac5d2',
        2: 1(1614124800),
        3: "Dark Purple Aqua Love",
        4: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
    })`
	// expected-text-output-rubric:
	const structure2DiagnosticAnnotated = `300({
        1: h'59f2293a5bce7d4de59e71b4207ac5d2',
        2: 1(1614124800)   / date /,
        3: "Dark Purple Aqua Love",
        4: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
    })`
	const structure2Summary = `300({1: h'59f2293a5bce7d4de59e71b4207ac5d2', 2: 2021-02-24, 3: "Dark Purple Aqua Love", 4: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."})`
	runFormatCheck(
		t,
		"format_structure_2",
		structure2,
		structure2Display,
		structure2Debug,
		structure2Diagnostic,
		structure2DiagnosticAnnotated,
		structure2Display,
		structure2Summary,
		structure2Hex,
		"",
	)
}
