package bcenvelope

import (
	"testing"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

func TestEnvelopeNonCorrelation(t *testing.T) {
	e1 := NewEnvelope("Hello.")

	// e1 correlates with its elision
	if !e1.IsEquivalentTo(e1.Elide()) {
		t.Error("e1 should correlate with its elision")
	}

	// e2 is the same message, but with random salt
	rng := bcrand.NewFakeRandomNumberGenerator()
	e2 := checkEncoding(t, e1.AddSaltUsing(rng))

	assertActualExpected(t, e2.Format(),
		`"Hello." [
    'salt': Salt
]`)

	assertActualExpected(t, e2.DiagnosticAnnotated(),
		`200(   / envelope /
    [
        201("Hello."),   / leaf /
        {
            15:
            201(   / leaf /
                40018(h'b559bbbf6cce2632')   / salt /
            )
        }
    ]
)`)

	assertActualExpected(t, e2.TreeFormat(),
		`4f0f2d55 NODE
    8cc96cdb subj "Hello."
    dd412f1d ASSERTION
        618975ce pred 'salt'
        7915f200 obj Salt`)

	// So even though its content is the same, it doesn't correlate.
	if e1.IsEquivalentTo(e2) {
		t.Error("e1 should not correlate with salted e2")
	}

	// And of course, neither does its elision.
	if e1.IsEquivalentTo(e2.Elide()) {
		t.Error("e1 should not correlate with elision of salted e2")
	}
}

func TestPredicateCorrelation(t *testing.T) {
	e1 := checkEncoding(t, NewEnvelope("Foo").AddAssertion("note", "Bar"))
	e2 := checkEncoding(t, NewEnvelope("Baz").AddAssertion("note", "Quux"))

	assertActualExpected(t, e1.Format(),
		`"Foo" [
    "note": "Bar"
]`)

	// e1 and e2 have the same predicate
	e1Pred, err := e1.Assertions()[0].TryPredicate()
	if err != nil {
		t.Fatalf("TryPredicate on e1 failed: %v", err)
	}
	e2Pred, err := e2.Assertions()[0].TryPredicate()
	if err != nil {
		t.Fatalf("TryPredicate on e2 failed: %v", err)
	}
	if !e1Pred.IsEquivalentTo(e2Pred) {
		t.Error("e1 and e2 predicates should be equivalent")
	}

	// Redact the entire contents of e1 without redacting the envelope itself.
	e1Elided := checkEncoding(t, e1.ElideRevealingTarget(e1))
	assertActualExpected(t, e1Elided.Format(),
		`ELIDED [
    ELIDED
]`)
}

func TestAddSalt(t *testing.T) {
	source := "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."

	e1, err := NewEnvelope("Alpha").
		AddSalt().
		Wrap().
		AddAssertionEnvelope(EnvelopeEncodableEnvelope{
			NewAssertionEnvelope(
				EnvelopeEncodableEnvelope{NewEnvelope(knownvalues.Note).AddSalt()},
				EnvelopeEncodableEnvelope{NewEnvelope(source).AddSalt()},
			),
		})
	if err != nil {
		t.Fatalf("building salted envelope failed: %v", err)
	}

	assertActualExpected(t, e1.Format(),
		`{
    "Alpha" [
        'salt': Salt
    ]
} [
    'note' [
        'salt': Salt
    ]
    : "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." [
        'salt': Salt
    ]
]`)

	e1Elided := checkEncoding(t, e1.ElideRevealingTarget(e1))
	assertActualExpected(t, e1Elided.Format(),
		`ELIDED [
    ELIDED
]`)
}
