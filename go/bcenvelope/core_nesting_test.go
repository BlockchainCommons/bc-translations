package bcenvelope

import (
	"testing"
)

func TestPredicateEnclosures(t *testing.T) {
	alice := NewEnvelope("Alice")
	knows := NewEnvelope("knows")
	bob := NewEnvelope("Bob")

	a := NewEnvelope("A")
	b := NewEnvelope("B")

	knowsBob := NewAssertionEnvelope(
		EnvelopeEncodableEnvelope{knows},
		EnvelopeEncodableEnvelope{bob},
	)
	assertActualExpected(t, knowsBob.Format(), `"knows": "Bob"`)

	ab := NewAssertionEnvelope(
		EnvelopeEncodableEnvelope{a},
		EnvelopeEncodableEnvelope{b},
	)
	assertActualExpected(t, ab.Format(), `"A": "B"`)

	// knows [A: B] : Bob
	knowsWithAB, err := knows.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	knowsABBob := checkEncoding(t, NewAssertionEnvelope(
		EnvelopeEncodableEnvelope{knowsWithAB},
		EnvelopeEncodableEnvelope{bob},
	))
	assertActualExpected(t, knowsABBob.Format(),
		`"knows" [
    "A": "B"
]
: "Bob"`)

	// knows : Bob [A: B]
	bobWithAB, err := bob.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	knowsBobAB := checkEncoding(t, NewAssertionEnvelope(
		EnvelopeEncodableEnvelope{knows},
		EnvelopeEncodableEnvelope{bobWithAB},
	))
	assertActualExpected(t, knowsBobAB.Format(),
		`"knows": "Bob" [
    "A": "B"
]`)

	// {knows: Bob} [A: B]
	knowsBobEncloseAB, err := knowsBob.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	knowsBobEncloseAB = checkEncoding(t, knowsBobEncloseAB)
	assertActualExpected(t, knowsBobEncloseAB.Format(),
		`{
    "knows": "Bob"
} [
    "A": "B"
]`)

	// Alice [knows: Bob]
	aliceKnowsBob, err := alice.AddAssertionEnvelope(EnvelopeEncodableEnvelope{knowsBob})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	aliceKnowsBob = checkEncoding(t, aliceKnowsBob)
	assertActualExpected(t, aliceKnowsBob.Format(),
		`"Alice" [
    "knows": "Bob"
]`)

	// Alice [A: B, knows: Bob]
	aliceABKnowsBob, err := aliceKnowsBob.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	aliceABKnowsBob = checkEncoding(t, aliceABKnowsBob)
	assertActualExpected(t, aliceABKnowsBob.Format(),
		`"Alice" [
    "A": "B"
    "knows": "Bob"
]`)

	// Alice [knows [A: B] : Bob]
	knowsWithAB2, err := knows.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	aliceKnowsABBob, err := alice.AddAssertionEnvelope(EnvelopeEncodableEnvelope{
		NewAssertionEnvelope(
			EnvelopeEncodableEnvelope{knowsWithAB2},
			EnvelopeEncodableEnvelope{bob},
		),
	})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	aliceKnowsABBob = checkEncoding(t, aliceKnowsABBob)
	assertActualExpected(t, aliceKnowsABBob.Format(),
		`"Alice" [
    "knows" [
        "A": "B"
    ]
    : "Bob"
]`)

	// Alice [knows : Bob [A: B]]
	bobWithAB2, err := bob.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	aliceKnowsBobAB, err := alice.AddAssertionEnvelope(EnvelopeEncodableEnvelope{
		NewAssertionEnvelope(
			EnvelopeEncodableEnvelope{knows},
			EnvelopeEncodableEnvelope{bobWithAB2},
		),
	})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	aliceKnowsBobAB = checkEncoding(t, aliceKnowsBobAB)
	assertActualExpected(t, aliceKnowsBobAB.Format(),
		`"Alice" [
    "knows": "Bob" [
        "A": "B"
    ]
]`)

	// Alice [knows [A: B] : Bob [A: B]]
	knowsWithAB3, err := knows.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	bobWithAB3, err := bob.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	aliceKnowsABBobAB, err := alice.AddAssertionEnvelope(EnvelopeEncodableEnvelope{
		NewAssertionEnvelope(
			EnvelopeEncodableEnvelope{knowsWithAB3},
			EnvelopeEncodableEnvelope{bobWithAB3},
		),
	})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	aliceKnowsABBobAB = checkEncoding(t, aliceKnowsABBobAB)
	assertActualExpected(t, aliceKnowsABBobAB.Format(),
		`"Alice" [
    "knows" [
        "A": "B"
    ]
    : "Bob" [
        "A": "B"
    ]
]`)

	// Alice [A: B, knows [A: B] : Bob [A: B]]
	aliceWithAB, err := alice.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	knowsWithAB4, err := knows.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	bobWithAB4, err := bob.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	aliceABKnowsABBobAB, err := aliceWithAB.AddAssertionEnvelope(EnvelopeEncodableEnvelope{
		NewAssertionEnvelope(
			EnvelopeEncodableEnvelope{knowsWithAB4},
			EnvelopeEncodableEnvelope{bobWithAB4},
		),
	})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	aliceABKnowsABBobAB = checkEncoding(t, aliceABKnowsABBobAB)
	assertActualExpected(t, aliceABKnowsABBobAB.Format(),
		`"Alice" [
    "A": "B"
    "knows" [
        "A": "B"
    ]
    : "Bob" [
        "A": "B"
    ]
]`)

	// Alice [A: B, {knows [A: B] : Bob [A: B]} [A: B]]
	aliceWithAB2, err := alice.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	knowsWithAB5, err := knows.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	bobWithAB5, err := bob.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	innerAssertion := NewAssertionEnvelope(
		EnvelopeEncodableEnvelope{knowsWithAB5},
		EnvelopeEncodableEnvelope{bobWithAB5},
	)
	innerAssertionWithAB, err := innerAssertion.AddAssertionEnvelope(EnvelopeEncodableEnvelope{ab})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	aliceABKnowsABBobABEncloseAB, err := aliceWithAB2.AddAssertionEnvelope(EnvelopeEncodableEnvelope{innerAssertionWithAB})
	if err != nil {
		t.Fatalf("AddAssertionEnvelope failed: %v", err)
	}
	aliceABKnowsABBobABEncloseAB = checkEncoding(t, aliceABKnowsABBobABEncloseAB)
	assertActualExpected(t, aliceABKnowsABBobABEncloseAB.Format(),
		`"Alice" [
    {
        "knows" [
            "A": "B"
        ]
        : "Bob" [
            "A": "B"
        ]
    } [
        "A": "B"
    ]
    "A": "B"
]`)
}

func TestNestingPlaintext(t *testing.T) {
	envelope := NewEnvelope("Hello.")
	assertActualExpected(t, envelope.Format(), `"Hello."`)

	elidedEnvelope := envelope.Elide()
	if !elidedEnvelope.IsEquivalentTo(envelope) {
		t.Error("elided envelope should be equivalent to original")
	}
	assertActualExpected(t, elidedEnvelope.Format(), "ELIDED")
}

func TestNestingOnce(t *testing.T) {
	envelope := checkEncoding(t, NewEnvelope("Hello.").Wrap())
	assertActualExpected(t, envelope.Format(),
		`{
    "Hello."
}`)

	elidedEnvelope := checkEncoding(t, NewEnvelope("Hello.").Elide().Wrap())
	if !elidedEnvelope.IsEquivalentTo(envelope) {
		t.Error("elided wrapped envelope should be equivalent to original wrapped")
	}
	assertActualExpected(t, elidedEnvelope.Format(),
		`{
    ELIDED
}`)
}

func TestNestingTwice(t *testing.T) {
	envelope := checkEncoding(t, NewEnvelope("Hello.").Wrap().Wrap())
	assertActualExpected(t, envelope.Format(),
		`{
    {
        "Hello."
    }
}`)

	inner, err := envelope.TryUnwrap()
	if err != nil {
		t.Fatalf("TryUnwrap failed: %v", err)
	}
	target, err := inner.TryUnwrap()
	if err != nil {
		t.Fatalf("TryUnwrap failed: %v", err)
	}
	elidedEnvelope := envelope.ElideRemovingTarget(target)
	assertActualExpected(t, elidedEnvelope.Format(),
		`{
    {
        ELIDED
    }
}`)
	if !envelope.IsEquivalentTo(elidedEnvelope) {
		t.Error("original should be equivalent to elided")
	}
}

func TestAssertionsOnAllPartsOfEnvelope(t *testing.T) {
	predicate := NewEnvelope("predicate").
		AddAssertion("predicate-predicate", "predicate-object")
	object := NewEnvelope("object").
		AddAssertion("object-predicate", "object-object")
	envelope := checkEncoding(t, NewEnvelope("subject").
		AddAssertion(predicate, object))

	assertActualExpected(t, envelope.Format(),
		`"subject" [
    "predicate" [
        "predicate-predicate": "predicate-object"
    ]
    : "object" [
        "object-predicate": "object-object"
    ]
]`)
}

func TestAssertionOnBareAssertion(t *testing.T) {
	envelope := NewAssertionEnvelope("predicate", "object").
		AddAssertion("assertion-predicate", "assertion-object")
	assertActualExpected(t, envelope.Format(),
		`{
    "predicate": "object"
} [
    "assertion-predicate": "assertion-object"
]`)
}
