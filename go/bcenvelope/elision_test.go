package bcenvelope

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
)

func TestEnvelopeElision(t *testing.T) {
	e1 := helloEnvelope()

	e2 := e1.Elide()
	if !e1.IsEquivalentTo(e2) {
		t.Fatal("elided should be equivalent")
	}
	if e1.IsIdenticalTo(e2) {
		t.Fatal("elided should not be identical")
	}

	assertActualExpected(t, e2.Format(), `ELIDED`)

	e3, err := e2.Unelide(e1)
	if err != nil {
		t.Fatalf("unelide failed: %v", err)
	}
	if !e3.IsEquivalentTo(e1) {
		t.Fatal("unelided should be equivalent to original")
	}
	assertActualExpected(t, e3.Format(), `"Hello."`)
}

func TestSingleAssertionRemoveElision(t *testing.T) {
	// The original Envelope
	e1 := singleAssertionEnvelope()
	assertActualExpected(t, e1.Format(), `"Alice" [
    "knows": "Bob"
]`)

	// Elide the entire envelope
	e2 := checkEncoding(t, e1.ElideRemovingTarget(e1))
	assertActualExpected(t, e2.Format(), `ELIDED`)

	// Elide just the envelope's subject
	e3 := checkEncoding(t, e1.ElideRemovingTarget(NewEnvelope("Alice")))
	assertActualExpected(t, e3.Format(), `ELIDED [
    "knows": "Bob"
]`)

	// Elide just the assertion's predicate
	e4 := checkEncoding(t, e1.ElideRemovingTarget(NewEnvelope("knows")))
	assertActualExpected(t, e4.Format(), `"Alice" [
    ELIDED: "Bob"
]`)

	// Elide just the assertion's object
	e5 := checkEncoding(t, e1.ElideRemovingTarget(NewEnvelope("Bob")))
	assertActualExpected(t, e5.Format(), `"Alice" [
    "knows": ELIDED
]`)

	// Elide the entire assertion
	e6 := checkEncoding(t, e1.ElideRemovingTarget(assertionEnvelope()))
	assertActualExpected(t, e6.Format(), `"Alice" [
    ELIDED
]`)
}

func TestDoubleAssertionRemoveElision(t *testing.T) {
	// The original Envelope
	e1 := doubleAssertionEnvelope()
	assertActualExpected(t, e1.Format(), `"Alice" [
    "knows": "Bob"
    "knows": "Carol"
]`)

	// Elide the entire envelope
	e2 := checkEncoding(t, e1.ElideRemovingTarget(e1))
	assertActualExpected(t, e2.Format(), `ELIDED`)

	// Elide just the envelope's subject
	e3 := checkEncoding(t, e1.ElideRemovingTarget(NewEnvelope("Alice")))
	assertActualExpected(t, e3.Format(), `ELIDED [
    "knows": "Bob"
    "knows": "Carol"
]`)

	// Elide just the assertion's predicate
	e4 := checkEncoding(t, e1.ElideRemovingTarget(NewEnvelope("knows")))
	assertActualExpected(t, e4.Format(), `"Alice" [
    ELIDED: "Bob"
    ELIDED: "Carol"
]`)

	// Elide just the assertion's object
	e5 := checkEncoding(t, e1.ElideRemovingTarget(NewEnvelope("Bob")))
	assertActualExpected(t, e5.Format(), `"Alice" [
    "knows": "Carol"
    "knows": ELIDED
]`)

	// Elide the entire assertion
	e6 := checkEncoding(t, e1.ElideRemovingTarget(assertionEnvelope()))
	assertActualExpected(t, e6.Format(), `"Alice" [
    "knows": "Carol"
    ELIDED
]`)
}

func TestSingleAssertionRevealElision(t *testing.T) {
	// The original Envelope
	e1 := singleAssertionEnvelope()
	assertActualExpected(t, e1.Format(), `"Alice" [
    "knows": "Bob"
]`)

	// Elide revealing nothing
	e2 := checkEncoding(t, e1.ElideRevealingArray(nil))
	assertActualExpected(t, e2.Format(), `ELIDED`)

	// Reveal just the envelope's structure
	e3 := checkEncoding(t, e1.ElideRevealingArray([]DigestProvider{e1}))
	assertActualExpected(t, e3.Format(), `ELIDED [
    ELIDED
]`)

	// Reveal just the envelope's subject
	e4 := checkEncoding(t, e1.ElideRevealingArray([]DigestProvider{e1, NewEnvelope("Alice")}))
	assertActualExpected(t, e4.Format(), `"Alice" [
    ELIDED
]`)

	// Reveal just the assertion's structure.
	e5 := checkEncoding(t, e1.ElideRevealingArray([]DigestProvider{e1, assertionEnvelope()}))
	assertActualExpected(t, e5.Format(), `ELIDED [
    ELIDED: ELIDED
]`)

	// Reveal just the assertion's predicate
	e6 := checkEncoding(t, e1.ElideRevealingArray([]DigestProvider{
		e1,
		assertionEnvelope(),
		NewEnvelope("knows"),
	}))
	assertActualExpected(t, e6.Format(), `ELIDED [
    "knows": ELIDED
]`)

	// Reveal just the assertion's object
	e7 := checkEncoding(t, e1.ElideRevealingArray([]DigestProvider{
		e1,
		assertionEnvelope(),
		NewEnvelope("Bob"),
	}))
	assertActualExpected(t, e7.Format(), `ELIDED [
    ELIDED: "Bob"
]`)
}

func TestDoubleAssertionRevealElision(t *testing.T) {
	// The original Envelope
	e1 := doubleAssertionEnvelope()
	assertActualExpected(t, e1.Format(), `"Alice" [
    "knows": "Bob"
    "knows": "Carol"
]`)

	// Elide revealing nothing
	e2 := checkEncoding(t, e1.ElideRevealingArray(nil))
	assertActualExpected(t, e2.Format(), `ELIDED`)

	// Reveal just the envelope's structure
	e3 := checkEncoding(t, e1.ElideRevealingArray([]DigestProvider{e1}))
	assertActualExpected(t, e3.Format(), `ELIDED [
    ELIDED (2)
]`)

	// Reveal just the envelope's subject
	e4 := checkEncoding(t, e1.ElideRevealingArray([]DigestProvider{e1, NewEnvelope("Alice")}))
	assertActualExpected(t, e4.Format(), `"Alice" [
    ELIDED (2)
]`)

	// Reveal just the assertion's structure.
	e5 := checkEncoding(t, e1.ElideRevealingArray([]DigestProvider{e1, assertionEnvelope()}))
	assertActualExpected(t, e5.Format(), `ELIDED [
    ELIDED: ELIDED
    ELIDED
]`)

	// Reveal just the assertion's predicate
	e6 := checkEncoding(t, e1.ElideRevealingArray([]DigestProvider{
		e1,
		assertionEnvelope(),
		NewEnvelope("knows"),
	}))
	assertActualExpected(t, e6.Format(), `ELIDED [
    "knows": ELIDED
    ELIDED
]`)

	// Reveal just the assertion's object
	e7 := checkEncoding(t, e1.ElideRevealingArray([]DigestProvider{
		e1,
		assertionEnvelope(),
		NewEnvelope("Bob"),
	}))
	assertActualExpected(t, e7.Format(), `ELIDED [
    ELIDED: "Bob"
    ELIDED
]`)
}

func TestDigests(t *testing.T) {
	e1 := doubleAssertionEnvelope()
	assertActualExpected(t, e1.Format(), `"Alice" [
    "knows": "Bob"
    "knows": "Carol"
]`)

	e2 := checkEncoding(t, e1.ElideRevealingSet(e1.Digests(0)))
	assertActualExpected(t, e2.Format(), `ELIDED`)

	e3 := checkEncoding(t, e1.ElideRevealingSet(e1.Digests(1)))
	assertActualExpected(t, e3.Format(), `"Alice" [
    ELIDED (2)
]`)

	e4 := checkEncoding(t, e1.ElideRevealingSet(e1.Digests(2)))
	assertActualExpected(t, e4.Format(), `"Alice" [
    ELIDED: ELIDED
    ELIDED: ELIDED
]`)

	e5 := checkEncoding(t, e1.ElideRevealingSet(e1.Digests(3)))
	assertActualExpected(t, e5.Format(), `"Alice" [
    "knows": "Bob"
    "knows": "Carol"
]`)
}

func TestTargetReveal(t *testing.T) {
	e1 := doubleAssertionEnvelope().AddAssertion("livesAt", "123 Main St.")
	assertActualExpected(t, e1.Format(), `"Alice" [
    "knows": "Bob"
    "knows": "Carol"
    "livesAt": "123 Main St."
]`)

	target := make(map[bccomponents.Digest]struct{})
	// Reveal the Envelope structure
	for d := range e1.Digests(1) {
		target[d] = struct{}{}
	}
	// Reveal everything about the subject
	for d := range e1.Subject().DeepDigests() {
		target[d] = struct{}{}
	}
	// Reveal everything about one of the assertions
	for d := range assertionEnvelope().DeepDigests() {
		target[d] = struct{}{}
	}
	// Reveal the specific `livesAt` assertion
	livesAtAssertion, err := e1.AssertionWithPredicate("livesAt")
	if err != nil {
		t.Fatalf("livesAt assertion not found: %v", err)
	}
	for d := range livesAtAssertion.DeepDigests() {
		target[d] = struct{}{}
	}
	e2 := checkEncoding(t, e1.ElideRevealingSet(target))
	assertActualExpected(t, e2.Format(), `"Alice" [
    "knows": "Bob"
    "livesAt": "123 Main St."
    ELIDED
]`)
}

func TestTargetedRemove(t *testing.T) {
	e1 := doubleAssertionEnvelope().AddAssertion("livesAt", "123 Main St.")
	assertActualExpected(t, e1.Format(), `"Alice" [
    "knows": "Bob"
    "knows": "Carol"
    "livesAt": "123 Main St."
]`)

	target2 := make(map[bccomponents.Digest]struct{})
	// Hide one of the assertions
	for d := range assertionEnvelope().Digests(1) {
		target2[d] = struct{}{}
	}
	e2 := checkEncoding(t, e1.ElideRemovingSet(target2))
	assertActualExpected(t, e2.Format(), `"Alice" [
    "knows": "Carol"
    "livesAt": "123 Main St."
    ELIDED
]`)

	target3 := make(map[bccomponents.Digest]struct{})
	// Hide one of the assertions by finding its predicate
	livesAtAssertion, err := e1.AssertionWithPredicate("livesAt")
	if err != nil {
		t.Fatalf("livesAt assertion not found: %v", err)
	}
	for d := range livesAtAssertion.DeepDigests() {
		target3[d] = struct{}{}
	}
	e3 := checkEncoding(t, e1.ElideRemovingSet(target3))
	assertActualExpected(t, e3.Format(), `"Alice" [
    "knows": "Bob"
    "knows": "Carol"
    ELIDED
]`)

	// Semantically equivalent
	if !e1.IsEquivalentTo(e3) {
		t.Fatal("should be equivalent")
	}

	// Structurally different
	if e1.IsIdenticalTo(e3) {
		t.Fatal("should not be identical")
	}
}

func TestWalkReplaceBasic(t *testing.T) {
	alice := NewEnvelope("Alice")
	bob := NewEnvelope("Bob")
	charlie := NewEnvelope("Charlie")

	// Create an envelope with Bob referenced multiple times
	envelope := alice.
		AddAssertion("knows", bob).
		AddAssertion("likes", bob)

	assertActualExpected(t, envelope.Format(), `"Alice" [
    "knows": "Bob"
    "likes": "Bob"
]`)

	// Replace all instances of Bob with Charlie
	target := make(map[bccomponents.Digest]struct{})
	target[bob.Digest()] = struct{}{}

	modified, err := envelope.WalkReplace(target, charlie)
	if err != nil {
		t.Fatalf("walk replace failed: %v", err)
	}

	assertActualExpected(t, modified.Format(), `"Alice" [
    "knows": "Charlie"
    "likes": "Charlie"
]`)

	// The structure is different (different content)
	if modified.IsEquivalentTo(envelope) {
		t.Fatal("modified should not be equivalent to original")
	}
}

func TestWalkReplaceSubject(t *testing.T) {
	alice := NewEnvelope("Alice")
	bob := NewEnvelope("Bob")
	carol := NewEnvelope("Carol")

	envelope := alice.AddAssertion("knows", bob)

	assertActualExpected(t, envelope.Format(), `"Alice" [
    "knows": "Bob"
]`)

	// Replace the subject (Alice) with Carol
	target := make(map[bccomponents.Digest]struct{})
	target[alice.Digest()] = struct{}{}

	modified, err := envelope.WalkReplace(target, carol)
	if err != nil {
		t.Fatalf("walk replace failed: %v", err)
	}

	assertActualExpected(t, modified.Format(), `"Carol" [
    "knows": "Bob"
]`)
}

func TestWalkReplaceNested(t *testing.T) {
	alice := NewEnvelope("Alice")
	bob := NewEnvelope("Bob")
	charlie := NewEnvelope("Charlie")

	// Create a nested structure with Bob appearing at multiple levels
	inner := bob.AddAssertion("friend", bob)
	envelope := alice.AddAssertion("knows", inner)

	assertActualExpected(t, envelope.Format(), `"Alice" [
    "knows": "Bob" [
        "friend": "Bob"
    ]
]`)

	// Replace all instances of Bob with Charlie
	target := make(map[bccomponents.Digest]struct{})
	target[bob.Digest()] = struct{}{}

	modified, err := envelope.WalkReplace(target, charlie)
	if err != nil {
		t.Fatalf("walk replace failed: %v", err)
	}

	assertActualExpected(t, modified.Format(), `"Alice" [
    "knows": "Charlie" [
        "friend": "Charlie"
    ]
]`)
}

func TestWalkReplaceWrapped(t *testing.T) {
	alice := NewEnvelope("Alice")
	bob := NewEnvelope("Bob")
	charlie := NewEnvelope("Charlie")

	// Create a wrapped envelope containing Bob
	wrapped := bob.Wrap()
	envelope := alice.AddAssertion("data", wrapped)

	assertActualExpected(t, envelope.Format(), `"Alice" [
    "data": {
        "Bob"
    }
]`)

	// Replace Bob with Charlie
	target := make(map[bccomponents.Digest]struct{})
	target[bob.Digest()] = struct{}{}

	modified, err := envelope.WalkReplace(target, charlie)
	if err != nil {
		t.Fatalf("walk replace failed: %v", err)
	}

	assertActualExpected(t, modified.Format(), `"Alice" [
    "data": {
        "Charlie"
    }
]`)
}

func TestWalkReplaceNoMatch(t *testing.T) {
	alice := NewEnvelope("Alice")
	bob := NewEnvelope("Bob")
	charlie := NewEnvelope("Charlie")
	dave := NewEnvelope("Dave")

	envelope := alice.AddAssertion("knows", bob)

	assertActualExpected(t, envelope.Format(), `"Alice" [
    "knows": "Bob"
]`)

	// Try to replace Dave (who doesn't exist in the envelope)
	target := make(map[bccomponents.Digest]struct{})
	target[dave.Digest()] = struct{}{}

	modified, err := envelope.WalkReplace(target, charlie)
	if err != nil {
		t.Fatalf("walk replace failed: %v", err)
	}

	// Should be identical since nothing matched
	assertActualExpected(t, modified.Format(), `"Alice" [
    "knows": "Bob"
]`)

	if !modified.IsIdenticalTo(envelope) {
		t.Fatal("modified should be identical to original when no match")
	}
}

func TestWalkReplaceMultipleTargets(t *testing.T) {
	alice := NewEnvelope("Alice")
	bob := NewEnvelope("Bob")
	carol := NewEnvelope("Carol")
	replacement := NewEnvelope("REDACTED")

	envelope := alice.
		AddAssertion("knows", bob).
		AddAssertion("likes", carol)

	assertActualExpected(t, envelope.Format(), `"Alice" [
    "knows": "Bob"
    "likes": "Carol"
]`)

	// Replace both Bob and Carol with REDACTED
	target := make(map[bccomponents.Digest]struct{})
	target[bob.Digest()] = struct{}{}
	target[carol.Digest()] = struct{}{}

	modified, err := envelope.WalkReplace(target, replacement)
	if err != nil {
		t.Fatalf("walk replace failed: %v", err)
	}

	assertActualExpected(t, modified.Format(), `"Alice" [
    "knows": "REDACTED"
    "likes": "REDACTED"
]`)
}

func TestWalkReplaceElided(t *testing.T) {
	alice := NewEnvelope("Alice")
	bob := NewEnvelope("Bob")
	charlie := NewEnvelope("Charlie")

	// Create an envelope with Bob, then elide Bob
	envelope := alice.
		AddAssertion("knows", bob).
		AddAssertion("likes", bob)

	assertActualExpected(t, envelope.Format(), `"Alice" [
    "knows": "Bob"
    "likes": "Bob"
]`)

	// Elide Bob
	elided := envelope.ElideRemovingTarget(bob)

	assertActualExpected(t, elided.Format(), `"Alice" [
    "knows": ELIDED
    "likes": ELIDED
]`)

	// Replace the elided Bob with Charlie
	// This works because the elided node has Bob's digest
	target := make(map[bccomponents.Digest]struct{})
	target[bob.Digest()] = struct{}{}

	modified, err := elided.WalkReplace(target, charlie)
	if err != nil {
		t.Fatalf("walk replace failed: %v", err)
	}

	assertActualExpected(t, modified.Format(), `"Alice" [
    "knows": "Charlie"
    "likes": "Charlie"
]`)

	// Verify that the elided nodes were replaced
	if modified.IsEquivalentTo(envelope) {
		t.Fatal("modified should not be equivalent to original")
	}
	if modified.IsEquivalentTo(elided) {
		t.Fatal("modified should not be equivalent to elided")
	}
}

func TestWalkReplaceAssertionWithNonAssertionFails(t *testing.T) {
	alice := NewEnvelope("Alice")
	bob := NewEnvelope("Bob")
	charlie := NewEnvelope("Charlie")

	envelope := alice.AddAssertion("knows", bob)

	// Get the assertion's digest
	knowsAssertion, err := envelope.AssertionWithPredicate("knows")
	if err != nil {
		t.Fatalf("assertion not found: %v", err)
	}
	assertionDigest := knowsAssertion.Digest()

	// Try to replace the entire assertion with Charlie (a non-assertion)
	target := make(map[bccomponents.Digest]struct{})
	target[assertionDigest] = struct{}{}

	_, err = envelope.WalkReplace(target, charlie)

	// This should fail because we're replacing an assertion with a
	// non-assertion
	if err == nil {
		t.Fatal("expected error when replacing assertion with non-assertion")
	}
	if err.Error() != "invalid format" {
		t.Fatalf("expected 'invalid format' error, got: %v", err)
	}
}
