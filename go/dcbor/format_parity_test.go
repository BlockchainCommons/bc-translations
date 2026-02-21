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

	if got := cbor.String(); got != expectedDisplay {
		t.Fatalf("%s display mismatch: got %q want %q", name, got, expectedDisplay)
	}
	if got := cbor.DebugString(); got != expectedDebug {
		t.Fatalf("%s debug mismatch: got %q want %q", name, got, expectedDebug)
	}
	if got := cbor.Diagnostic(); got != expectedDiagnostic {
		t.Fatalf("%s diagnostic mismatch: got %q want %q", name, got, expectedDiagnostic)
	}
	if got := cbor.DiagnosticAnnotated(); got != expectedDiagnosticAnnotated {
		t.Fatalf("%s diagnostic_annotated mismatch: got %q want %q", name, got, expectedDiagnosticAnnotated)
	}
	if got := cbor.DiagnosticFlat(); got != expectedDiagnosticFlat {
		t.Fatalf("%s diagnostic_flat mismatch: got %q want %q", name, got, expectedDiagnosticFlat)
	}
	if got := cbor.Summary(); got != expectedSummary {
		t.Fatalf("%s summary mismatch: got %q want %q", name, got, expectedSummary)
	}
	if got := cbor.Hex(); got != expectedHex {
		t.Fatalf("%s hex mismatch: got %q want %q", name, got, expectedHex)
	}
	if got := cbor.HexAnnotated(); got != expectedHexAnnotated {
		t.Fatalf("%s hex_annotated mismatch: got %q want %q", name, got, expectedHexAnnotated)
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
