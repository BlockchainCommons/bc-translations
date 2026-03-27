package bcenvelope

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

func TestEncodingDigest(t *testing.T) {
	checkEncoding(t, NewEnvelope(bccomponents.DigestFromImage([]byte("Hello."))))
}

func TestEncoding1(t *testing.T) {
	e := NewEnvelope("Hello.")

	assertActualExpected(t, e.DiagnosticAnnotated(),
		`200(   / envelope /
    201("Hello.")   / leaf /
)`)
}

func TestEncoding2(t *testing.T) {
	array := []dcbor.CBOR{
		dcbor.NewCBORUnsigned(1),
		dcbor.NewCBORUnsigned(2),
		dcbor.NewCBORUnsigned(3),
	}
	e := NewEnvelope(dcbor.NewCBORArray(array))

	assertActualExpected(t, e.DiagnosticAnnotated(),
		`200(   / envelope /
    201(   / leaf /
        [1, 2, 3]
    )
)`)
}

func TestEncoding3(t *testing.T) {
	e1 := checkEncoding(t, NewAssertionEnvelope("A", "B"))
	e2 := checkEncoding(t, NewAssertionEnvelope("C", "D"))
	e3 := checkEncoding(t, NewAssertionEnvelope("E", "F"))

	e4, err := e2.AddAssertionEnvelope(EnvelopeEncodableEnvelope{e3})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}

	assertActualExpected(t, e4.Format(),
		`{
    "C": "D"
} [
    "E": "F"
]`)

	assertActualExpected(t, e4.DiagnosticAnnotated(),
		`200(   / envelope /
    [
        {
            201("C"):   / leaf /
            201("D")   / leaf /
        },
        {
            201("E"):   / leaf /
            201("F")   / leaf /
        }
    ]
)`)

	checkEncoding(t, e4)

	e5, err := e1.AddAssertionEnvelope(EnvelopeEncodableEnvelope{e4})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	e5 = checkEncoding(t, e5)

	assertActualExpected(t, e5.Format(),
		`{
    "A": "B"
} [
    {
        "C": "D"
    } [
        "E": "F"
    ]
]`)

	assertActualExpected(t, e5.DiagnosticAnnotated(),
		`200(   / envelope /
    [
        {
            201("A"):   / leaf /
            201("B")   / leaf /
        },
        [
            {
                201("C"):   / leaf /
                201("D")   / leaf /
            },
            {
                201("E"):   / leaf /
                201("F")   / leaf /
            }
        ]
    ]
)`)
}
