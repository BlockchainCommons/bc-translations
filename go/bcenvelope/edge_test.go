package bcenvelope

import (
	"strings"
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// Helper to create a basic edge envelope with the three required assertions.
func makeEdge(subject, isA string, source, target *Envelope) *Envelope {
	return NewEnvelope(subject).
		AddAssertion(knownvalues.IsA, isA).
		AddAssertion(knownvalues.Source, source).
		AddAssertion(knownvalues.Target, target)
}

// Helper to create an XID-like identifier envelope.
func xidLike(name string) *Envelope { return NewEnvelope(name) }

// -------------------------------------------------------------------
// Edge construction and format
// -------------------------------------------------------------------

func TestEdgeBasicFormat(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("credential-1", "foaf:Person", alice, alice)

	assertActualExpected(t, edge.Format(), `"credential-1" [
    'isA': "foaf:Person"
    'source': "Alice"
    'target': "Alice"
]`)
}

func TestEdgeRelationshipFormat(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")
	edge := makeEdge("knows-bob", "schema:colleague", alice, bob)

	assertActualExpected(t, edge.Format(), `"knows-bob" [
    'isA': "schema:colleague"
    'source': "Alice"
    'target': "Bob"
]`)
}

// -------------------------------------------------------------------
// Edge validation
// -------------------------------------------------------------------

func TestValidateEdgeValid(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("cred-1", "foaf:Person", alice, alice)
	if err := edge.ValidateEdge(); err != nil {
		t.Fatalf("expected valid edge, got: %v", err)
	}
}

func TestValidateEdgeMissingIsA(t *testing.T) {
	alice := xidLike("Alice")
	edge := NewEnvelope("cred-1").
		AddAssertion(knownvalues.Source, alice).
		AddAssertion(knownvalues.Target, alice)
	err := edge.ValidateEdge()
	if err != ErrEdgeMissingIsA {
		t.Fatalf("expected ErrEdgeMissingIsA, got: %v", err)
	}
}

func TestValidateEdgeMissingSource(t *testing.T) {
	alice := xidLike("Alice")
	edge := NewEnvelope("cred-1").
		AddAssertion(knownvalues.IsA, "foaf:Person").
		AddAssertion(knownvalues.Target, alice)
	err := edge.ValidateEdge()
	if err != ErrEdgeMissingSource {
		t.Fatalf("expected ErrEdgeMissingSource, got: %v", err)
	}
}

func TestValidateEdgeMissingTarget(t *testing.T) {
	alice := xidLike("Alice")
	edge := NewEnvelope("cred-1").
		AddAssertion(knownvalues.IsA, "foaf:Person").
		AddAssertion(knownvalues.Source, alice)
	err := edge.ValidateEdge()
	if err != ErrEdgeMissingTarget {
		t.Fatalf("expected ErrEdgeMissingTarget, got: %v", err)
	}
}

func TestValidateEdgeNoAssertions(t *testing.T) {
	edge := NewEnvelope("cred-1")
	err := edge.ValidateEdge()
	if err != ErrEdgeMissingIsA {
		t.Fatalf("expected ErrEdgeMissingIsA, got: %v", err)
	}
}

func TestValidateEdgeDuplicateIsA(t *testing.T) {
	alice := xidLike("Alice")
	edge := NewEnvelope("cred-1").
		AddAssertion(knownvalues.IsA, "foaf:Person").
		AddAssertion(knownvalues.IsA, "schema:Thing").
		AddAssertion(knownvalues.Source, alice).
		AddAssertion(knownvalues.Target, alice)
	err := edge.ValidateEdge()
	if err != ErrEdgeDuplicateIsA {
		t.Fatalf("expected ErrEdgeDuplicateIsA, got: %v", err)
	}
}

func TestValidateEdgeDuplicateSource(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")
	edge := NewEnvelope("cred-1").
		AddAssertion(knownvalues.IsA, "foaf:Person").
		AddAssertion(knownvalues.Source, alice).
		AddAssertion(knownvalues.Source, bob).
		AddAssertion(knownvalues.Target, alice)
	err := edge.ValidateEdge()
	if err != ErrEdgeDuplicateSource {
		t.Fatalf("expected ErrEdgeDuplicateSource, got: %v", err)
	}
}

func TestValidateEdgeDuplicateTarget(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")
	edge := NewEnvelope("cred-1").
		AddAssertion(knownvalues.IsA, "foaf:Person").
		AddAssertion(knownvalues.Source, alice).
		AddAssertion(knownvalues.Target, alice).
		AddAssertion(knownvalues.Target, bob)
	err := edge.ValidateEdge()
	if err != ErrEdgeDuplicateTarget {
		t.Fatalf("expected ErrEdgeDuplicateTarget, got: %v", err)
	}
}

func TestValidateEdgeWrappedSigned(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("cred-1", "foaf:Person", alice, alice)

	// Wrap and sign the edge
	signedEdge := edge.Wrap().AddSignature(alicePrivateKey().PrivateKeys())

	// Signed (wrapped) edge should still validate
	if err := signedEdge.ValidateEdge(); err != nil {
		t.Fatalf("expected signed edge to validate, got: %v", err)
	}
}

// -------------------------------------------------------------------
// Edge accessor methods
// -------------------------------------------------------------------

func TestEdgeIsA(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("cred-1", "foaf:Person", alice, alice)

	isA, err := edge.EdgeIsA()
	if err != nil {
		t.Fatalf("edge_is_a failed: %v", err)
	}
	assertActualExpected(t, isA.Format(), `"foaf:Person"`)
}

func TestEdgeSource(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("cred-1", "foaf:Person", alice, alice)

	source, err := edge.EdgeSource()
	if err != nil {
		t.Fatalf("edge_source failed: %v", err)
	}
	assertActualExpected(t, source.Format(), `"Alice"`)
}

func TestEdgeTarget(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")
	edge := makeEdge("knows-bob", "schema:colleague", alice, bob)

	target, err := edge.EdgeTarget()
	if err != nil {
		t.Fatalf("edge_target failed: %v", err)
	}
	assertActualExpected(t, target.Format(), `"Bob"`)
}

func TestEdgeSubject(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("my-credential", "foaf:Person", alice, alice)

	subject, err := edge.EdgeSubject()
	if err != nil {
		t.Fatalf("edge_subject failed: %v", err)
	}
	assertActualExpected(t, subject.Format(), `"my-credential"`)
}

func TestEdgeAccessorsOnSignedEdge(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")
	edge := makeEdge("cred-1", "foaf:Person", alice, bob)

	signedEdge := edge.Wrap().AddSignature(alicePrivateKey().PrivateKeys())

	// Accessors should work through the wrapped/signed layer
	isA, err := signedEdge.EdgeIsA()
	if err != nil {
		t.Fatalf("edge_is_a failed: %v", err)
	}
	assertActualExpected(t, isA.Format(), `"foaf:Person"`)

	source, err := signedEdge.EdgeSource()
	if err != nil {
		t.Fatalf("edge_source failed: %v", err)
	}
	assertActualExpected(t, source.Format(), `"Alice"`)

	target, err := signedEdge.EdgeTarget()
	if err != nil {
		t.Fatalf("edge_target failed: %v", err)
	}
	assertActualExpected(t, target.Format(), `"Bob"`)

	subject, err := signedEdge.EdgeSubject()
	if err != nil {
		t.Fatalf("edge_subject failed: %v", err)
	}
	assertActualExpected(t, subject.Format(), `"cred-1"`)
}

// -------------------------------------------------------------------
// Adding edges to envelopes
// -------------------------------------------------------------------

func TestAddEdgeEnvelope(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("cred-1", "foaf:Person", alice, alice)

	doc := NewEnvelope("Alice").AddEdgeEnvelope(edge)

	assertActualExpected(t, doc.Format(), `"Alice" [
    'edge': "cred-1" [
        'isA': "foaf:Person"
        'source': "Alice"
        'target': "Alice"
    ]
]`)
}

func TestAddMultipleEdges(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")
	edge1 := makeEdge("self-desc", "foaf:Person", alice, alice)
	edge2 := makeEdge("knows-bob", "schema:colleague", alice, bob)

	doc := NewEnvelope("Alice").
		AddEdgeEnvelope(edge1).
		AddEdgeEnvelope(edge2)

	edges, err := doc.Edges()
	if err != nil {
		t.Fatalf("edges failed: %v", err)
	}
	if len(edges) != 2 {
		t.Fatalf("expected 2 edges, got %d", len(edges))
	}

	formatted := doc.Format()
	if !strings.Contains(formatted, "'edge'") {
		t.Fatal("expected 'edge' in formatted output")
	}
	if !strings.Contains(formatted, `"self-desc"`) {
		t.Fatal("expected 'self-desc' in formatted output")
	}
	if !strings.Contains(formatted, `"knows-bob"`) {
		t.Fatal("expected 'knows-bob' in formatted output")
	}
}

// -------------------------------------------------------------------
// Edges retrieval via envelope
// -------------------------------------------------------------------

func TestEdgesEmpty(t *testing.T) {
	doc := NewEnvelope("Alice")
	edges, err := doc.Edges()
	if err != nil {
		t.Fatalf("edges failed: %v", err)
	}
	if len(edges) != 0 {
		t.Fatalf("expected 0 edges, got %d", len(edges))
	}
}

func TestEdgesRetrieval(t *testing.T) {
	alice := xidLike("Alice")
	edge1 := makeEdge("cred-1", "foaf:Person", alice, alice)
	edge2 := makeEdge("cred-2", "schema:Thing", alice, alice)

	doc := NewEnvelope("Alice").
		AddEdgeEnvelope(edge1).
		AddEdgeEnvelope(edge2)

	edges, err := doc.Edges()
	if err != nil {
		t.Fatalf("edges failed: %v", err)
	}
	if len(edges) != 2 {
		t.Fatalf("expected 2 edges, got %d", len(edges))
	}

	// Each retrieved edge should be a valid edge
	for _, edge := range edges {
		if err := edge.ValidateEdge(); err != nil {
			t.Fatalf("edge validation failed: %v", err)
		}
	}
}

// -------------------------------------------------------------------
// Edges container (add / get / remove / clear / len)
// -------------------------------------------------------------------

func TestEdgesContainerNewIsEmpty(t *testing.T) {
	edges := NewEdgesContainer()
	if !edges.IsEmpty() {
		t.Fatal("expected empty")
	}
	if edges.Len() != 0 {
		t.Fatalf("expected len 0, got %d", edges.Len())
	}
}

func TestEdgesContainerAddAndGet(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("cred-1", "foaf:Person", alice, alice)
	digest := edge.Digest()

	edges := NewEdgesContainer()
	edges.Add(edge)

	if edges.IsEmpty() {
		t.Fatal("expected non-empty")
	}
	if edges.Len() != 1 {
		t.Fatalf("expected len 1, got %d", edges.Len())
	}
	got := edges.Get(digest)
	if got == nil {
		t.Fatal("expected to find edge by digest")
	}
	if !got.IsEquivalentTo(edge) {
		t.Fatal("retrieved edge not equivalent")
	}
}

func TestEdgesContainerRemove(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("cred-1", "foaf:Person", alice, alice)
	digest := edge.Digest()

	edges := NewEdgesContainer()
	edges.Add(edge)

	removed := edges.Remove(digest)
	if removed == nil {
		t.Fatal("expected removed edge")
	}
	if !edges.IsEmpty() {
		t.Fatal("expected empty after remove")
	}
}

func TestEdgesContainerRemoveNonexistent(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("cred-1", "foaf:Person", alice, alice)

	edges := NewEdgesContainer()
	removed := edges.Remove(edge.Digest())
	if removed != nil {
		t.Fatal("expected nil for nonexistent remove")
	}
}

func TestEdgesContainerClear(t *testing.T) {
	alice := xidLike("Alice")
	edge1 := makeEdge("cred-1", "foaf:Person", alice, alice)
	edge2 := makeEdge("cred-2", "schema:Thing", alice, alice)

	edges := NewEdgesContainer()
	edges.Add(edge1)
	edges.Add(edge2)
	if edges.Len() != 2 {
		t.Fatalf("expected len 2, got %d", edges.Len())
	}

	edges.Clear()
	if !edges.IsEmpty() {
		t.Fatal("expected empty after clear")
	}
	if edges.Len() != 0 {
		t.Fatalf("expected len 0 after clear, got %d", edges.Len())
	}
}

func TestEdgesContainerIter(t *testing.T) {
	alice := xidLike("Alice")
	edge1 := makeEdge("cred-1", "foaf:Person", alice, alice)
	edge2 := makeEdge("cred-2", "schema:Thing", alice, alice)

	edges := NewEdgesContainer()
	edges.Add(edge1)
	edges.Add(edge2)

	// Count by iterating over the internal map
	count := 0
	for range edges.envelopes {
		count++
	}
	if count != 2 {
		t.Fatalf("expected 2 entries, got %d", count)
	}
}

// -------------------------------------------------------------------
// Edges container round-trip: add_to_envelope / try_from_envelope
// -------------------------------------------------------------------

func TestEdgesContainerRoundtrip(t *testing.T) {
	alice := xidLike("Alice")
	edge1 := makeEdge("cred-1", "foaf:Person", alice, alice)
	edge2 := makeEdge("cred-2", "schema:Thing", alice, alice)

	edges := NewEdgesContainer()
	edges.Add(edge1)
	edges.Add(edge2)

	// Serialize to envelope
	doc := NewEnvelope("Alice")
	docWithEdges := edges.AddToEnvelope(doc)

	// Deserialize back
	recovered, err := EdgesContainerFromEnvelope(docWithEdges)
	if err != nil {
		t.Fatalf("from envelope failed: %v", err)
	}
	if recovered.Len() != 2 {
		t.Fatalf("expected 2 recovered edges, got %d", recovered.Len())
	}
	if recovered.Get(edge1.Digest()) == nil {
		t.Fatal("expected to find edge1")
	}
	if recovered.Get(edge2.Digest()) == nil {
		t.Fatal("expected to find edge2")
	}
}

func TestEdgesContainerRoundtripEmpty(t *testing.T) {
	edges := NewEdgesContainer()
	doc := NewEnvelope("Alice")
	docWithEdges := edges.AddToEnvelope(doc)

	recovered, err := EdgesContainerFromEnvelope(docWithEdges)
	if err != nil {
		t.Fatalf("from envelope failed: %v", err)
	}
	if !recovered.IsEmpty() {
		t.Fatal("expected empty recovered container")
	}
}

func TestEdgesContainerRoundtripPreservesFormat(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")
	edge := makeEdge("knows-bob", "schema:colleague", alice, bob)

	edges := NewEdgesContainer()
	edges.Add(edge)

	doc := edges.AddToEnvelope(NewEnvelope("Alice"))

	assertActualExpected(t, doc.Format(), `"Alice" [
    'edge': "knows-bob" [
        'isA': "schema:colleague"
        'source': "Alice"
        'target': "Bob"
    ]
]`)

	recovered, err := EdgesContainerFromEnvelope(doc)
	if err != nil {
		t.Fatalf("from envelope failed: %v", err)
	}
	if recovered.Len() != 1 {
		t.Fatalf("expected 1 recovered edge, got %d", recovered.Len())
	}
}

// -------------------------------------------------------------------
// Edgeable trait
// -------------------------------------------------------------------

func TestEdgeableDefaultMethods(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("cred-1", "foaf:Person", alice, alice)
	digest := edge.Digest()

	edges := NewEdgesContainer()
	edges.Add(edge)

	if edges.IsEmpty() {
		t.Fatal("expected non-empty")
	}
	if edges.Len() != 1 {
		t.Fatalf("expected len 1, got %d", edges.Len())
	}
	if edges.Get(digest) == nil {
		t.Fatal("expected to find edge by digest")
	}

	removed := edges.Remove(digest)
	if removed == nil {
		t.Fatal("expected removed edge")
	}
	if !edges.IsEmpty() {
		t.Fatal("expected empty after remove")
	}
}

// -------------------------------------------------------------------
// edges_matching — filtering by criteria
// -------------------------------------------------------------------

func TestEdgesMatchingNoFilters(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")
	edge1 := makeEdge("self-desc", "foaf:Person", alice, alice)
	edge2 := makeEdge("knows-bob", "schema:colleague", alice, bob)

	doc := NewEnvelope("Alice").
		AddEdgeEnvelope(edge1).
		AddEdgeEnvelope(edge2)

	// No filters => all edges
	matching, err := doc.EdgesMatching(nil, nil, nil, nil)
	if err != nil {
		t.Fatalf("edges_matching failed: %v", err)
	}
	if len(matching) != 2 {
		t.Fatalf("expected 2 matching, got %d", len(matching))
	}
}

func TestEdgesMatchingByIsA(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")
	edge1 := makeEdge("self-desc", "foaf:Person", alice, alice)
	edge2 := makeEdge("knows-bob", "schema:colleague", alice, bob)
	edge3 := makeEdge("self-thing", "foaf:Person", alice, alice)

	doc := NewEnvelope("Alice").
		AddEdgeEnvelope(edge1).
		AddEdgeEnvelope(edge2).
		AddEdgeEnvelope(edge3)

	isAPerson := NewEnvelope("foaf:Person")
	matching, err := doc.EdgesMatching(isAPerson, nil, nil, nil)
	if err != nil {
		t.Fatalf("edges_matching failed: %v", err)
	}
	if len(matching) != 2 {
		t.Fatalf("expected 2 matching foaf:Person, got %d", len(matching))
	}

	isAColleague := NewEnvelope("schema:colleague")
	matching, err = doc.EdgesMatching(isAColleague, nil, nil, nil)
	if err != nil {
		t.Fatalf("edges_matching failed: %v", err)
	}
	if len(matching) != 1 {
		t.Fatalf("expected 1 matching schema:colleague, got %d", len(matching))
	}

	isANone := NewEnvelope("nonexistent")
	matching, err = doc.EdgesMatching(isANone, nil, nil, nil)
	if err != nil {
		t.Fatalf("edges_matching failed: %v", err)
	}
	if len(matching) != 0 {
		t.Fatalf("expected 0 matching nonexistent, got %d", len(matching))
	}
}

func TestEdgesMatchingBySource(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")
	edge1 := makeEdge("alice-claim", "foaf:Person", alice, alice)
	edge2 := makeEdge("bob-claim", "foaf:Person", bob, alice)

	doc := NewEnvelope("Alice").
		AddEdgeEnvelope(edge1).
		AddEdgeEnvelope(edge2)

	matching, err := doc.EdgesMatching(nil, alice, nil, nil)
	if err != nil {
		t.Fatalf("edges_matching failed: %v", err)
	}
	if len(matching) != 1 {
		t.Fatalf("expected 1 matching source Alice, got %d", len(matching))
	}

	matching, err = doc.EdgesMatching(nil, bob, nil, nil)
	if err != nil {
		t.Fatalf("edges_matching failed: %v", err)
	}
	if len(matching) != 1 {
		t.Fatalf("expected 1 matching source Bob, got %d", len(matching))
	}

	carol := xidLike("Carol")
	matching, err = doc.EdgesMatching(nil, carol, nil, nil)
	if err != nil {
		t.Fatalf("edges_matching failed: %v", err)
	}
	if len(matching) != 0 {
		t.Fatalf("expected 0 matching source Carol, got %d", len(matching))
	}
}

func TestEdgesMatchingByTarget(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")
	edge1 := makeEdge("self-desc", "foaf:Person", alice, alice)
	edge2 := makeEdge("knows-bob", "schema:colleague", alice, bob)

	doc := NewEnvelope("Alice").
		AddEdgeEnvelope(edge1).
		AddEdgeEnvelope(edge2)

	matching, err := doc.EdgesMatching(nil, nil, alice, nil)
	if err != nil {
		t.Fatalf("edges_matching failed: %v", err)
	}
	if len(matching) != 1 {
		t.Fatalf("expected 1 matching target Alice, got %d", len(matching))
	}

	matching, err = doc.EdgesMatching(nil, nil, bob, nil)
	if err != nil {
		t.Fatalf("edges_matching failed: %v", err)
	}
	if len(matching) != 1 {
		t.Fatalf("expected 1 matching target Bob, got %d", len(matching))
	}
}

func TestEdgesMatchingBySubject(t *testing.T) {
	alice := xidLike("Alice")
	edge1 := makeEdge("self-desc", "foaf:Person", alice, alice)
	edge2 := makeEdge("cred-2", "schema:Thing", alice, alice)

	doc := NewEnvelope("Alice").
		AddEdgeEnvelope(edge1).
		AddEdgeEnvelope(edge2)

	subjectFilter := NewEnvelope("self-desc")
	matching, err := doc.EdgesMatching(nil, nil, nil, subjectFilter)
	if err != nil {
		t.Fatalf("edges_matching failed: %v", err)
	}
	if len(matching) != 1 {
		t.Fatalf("expected 1 matching subject self-desc, got %d", len(matching))
	}

	subjectFilter = NewEnvelope("nonexistent")
	matching, err = doc.EdgesMatching(nil, nil, nil, subjectFilter)
	if err != nil {
		t.Fatalf("edges_matching failed: %v", err)
	}
	if len(matching) != 0 {
		t.Fatalf("expected 0 matching nonexistent subject, got %d", len(matching))
	}
}

func TestEdgesMatchingCombinedFilters(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")
	edge1 := makeEdge("self-desc", "foaf:Person", alice, alice)
	edge2 := makeEdge("self-thing", "foaf:Person", alice, alice)
	edge3 := makeEdge("knows-bob", "foaf:Person", alice, bob)

	doc := NewEnvelope("Alice").
		AddEdgeEnvelope(edge1).
		AddEdgeEnvelope(edge2).
		AddEdgeEnvelope(edge3)

	// All three are foaf:Person
	isA := NewEnvelope("foaf:Person")
	matching, err := doc.EdgesMatching(isA, nil, nil, nil)
	if err != nil {
		t.Fatalf("failed: %v", err)
	}
	if len(matching) != 3 {
		t.Fatalf("expected 3 foaf:Person, got %d", len(matching))
	}

	// foaf:Person + target Alice => 2 (self-desc, self-thing)
	matching, err = doc.EdgesMatching(isA, nil, alice, nil)
	if err != nil {
		t.Fatalf("failed: %v", err)
	}
	if len(matching) != 2 {
		t.Fatalf("expected 2, got %d", len(matching))
	}

	// foaf:Person + target Bob => 1 (knows-bob)
	matching, err = doc.EdgesMatching(isA, nil, bob, nil)
	if err != nil {
		t.Fatalf("failed: %v", err)
	}
	if len(matching) != 1 {
		t.Fatalf("expected 1, got %d", len(matching))
	}

	// foaf:Person + target Alice + subject "self-desc" => 1
	subj := NewEnvelope("self-desc")
	matching, err = doc.EdgesMatching(isA, nil, alice, subj)
	if err != nil {
		t.Fatalf("failed: %v", err)
	}
	if len(matching) != 1 {
		t.Fatalf("expected 1, got %d", len(matching))
	}

	// foaf:Person + source Alice + target Bob + subject "knows-bob" => 1
	subj = NewEnvelope("knows-bob")
	matching, err = doc.EdgesMatching(isA, alice, bob, subj)
	if err != nil {
		t.Fatalf("failed: %v", err)
	}
	if len(matching) != 1 {
		t.Fatalf("expected 1, got %d", len(matching))
	}

	// All filters that match nothing
	subj = NewEnvelope("nonexistent")
	matching, err = doc.EdgesMatching(isA, alice, alice, subj)
	if err != nil {
		t.Fatalf("failed: %v", err)
	}
	if len(matching) != 0 {
		t.Fatalf("expected 0, got %d", len(matching))
	}
}

// -------------------------------------------------------------------
// Signed edges with format verification
// -------------------------------------------------------------------

func TestSignedEdgeFormat(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("cred-1", "foaf:Person", alice, alice)

	signedEdge := edge.Wrap().AddSignature(alicePrivateKey().PrivateKeys())

	assertActualExpected(t, signedEdge.Format(), `{
    "cred-1" [
        'isA': "foaf:Person"
        'source': "Alice"
        'target': "Alice"
    ]
} [
    'signed': Signature
]`)
}

func TestSignedEdgeOnDocumentFormat(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("cred-1", "foaf:Person", alice, alice)
	signedEdge := edge.Wrap().AddSignature(alicePrivateKey().PrivateKeys())

	doc := NewEnvelope("Alice").
		AddAssertion("knows", "Bob").
		AddEdgeEnvelope(signedEdge)

	formatted := doc.Format()
	if !strings.Contains(formatted, "'edge': {") {
		t.Fatal("expected 'edge': { in formatted output")
	}
	if !strings.Contains(formatted, "'signed': Signature") {
		t.Fatal("expected 'signed': Signature in formatted output")
	}
	if !strings.Contains(formatted, `'isA': "foaf:Person"`) {
		t.Fatal("expected 'isA': \"foaf:Person\" in formatted output")
	}
}

// -------------------------------------------------------------------
// Edge coexistence with attachments
// -------------------------------------------------------------------

func TestEdgesCoexistWithAttachments(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("cred-1", "foaf:Person", alice, alice)

	conformsTo := "https://example.com/v1"
	doc := NewEnvelope("Alice").
		AddAttachment("Metadata", "com.example", &conformsTo).
		AddEdgeEnvelope(edge)

	// Both should be present
	edges, err := doc.Edges()
	if err != nil {
		t.Fatalf("edges failed: %v", err)
	}
	if len(edges) != 1 {
		t.Fatalf("expected 1 edge, got %d", len(edges))
	}

	attachments, err := doc.Attachments()
	if err != nil {
		t.Fatalf("attachments failed: %v", err)
	}
	if len(attachments) != 1 {
		t.Fatalf("expected 1 attachment, got %d", len(attachments))
	}

	formatted := doc.Format()
	if !strings.Contains(formatted, "'edge'") {
		t.Fatal("expected 'edge' in formatted output")
	}
	if !strings.Contains(formatted, "'attachment'") {
		t.Fatal("expected 'attachment' in formatted output")
	}
}

// -------------------------------------------------------------------
// Edge UR round-trip
// -------------------------------------------------------------------

func TestEdgeURRoundtrip(t *testing.T) {
	alice := xidLike("Alice")
	edge := makeEdge("cred-1", "foaf:Person", alice, alice)

	doc := NewEnvelope("Alice").AddEdgeEnvelope(edge)

	// Round-trip through CBOR
	cbor := doc.TaggedCBOR()
	recovered, err := EnvelopeFromTaggedCBORValue(cbor)
	if err != nil {
		t.Fatalf("from CBOR failed: %v", err)
	}
	if !recovered.IsEquivalentTo(doc) {
		t.Fatal("recovered not equivalent")
	}

	recoveredEdges, err := recovered.Edges()
	if err != nil {
		t.Fatalf("edges failed: %v", err)
	}
	if len(recoveredEdges) != 1 {
		t.Fatalf("expected 1 edge, got %d", len(recoveredEdges))
	}
	if !recoveredEdges[0].IsEquivalentTo(edge) {
		t.Fatal("recovered edge not equivalent")
	}
}

func TestMultipleEdgesURRoundtrip(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")
	edge1 := makeEdge("self-desc", "foaf:Person", alice, alice)
	edge2 := makeEdge("knows-bob", "schema:colleague", alice, bob)
	edge3 := makeEdge("project", "schema:CreativeWork", alice, bob)

	doc := NewEnvelope("Alice").
		AddEdgeEnvelope(edge1).
		AddEdgeEnvelope(edge2).
		AddEdgeEnvelope(edge3)

	cbor := doc.TaggedCBOR()
	recovered, err := EnvelopeFromTaggedCBORValue(cbor)
	if err != nil {
		t.Fatalf("from CBOR failed: %v", err)
	}
	if !recovered.IsEquivalentTo(doc) {
		t.Fatal("recovered not equivalent")
	}

	recoveredEdges, err := recovered.Edges()
	if err != nil {
		t.Fatalf("edges failed: %v", err)
	}
	if len(recoveredEdges) != 3 {
		t.Fatalf("expected 3 edges, got %d", len(recoveredEdges))
	}
}

// -------------------------------------------------------------------
// Edge with extra assertions beyond the required three
// -------------------------------------------------------------------

func TestEdgeWithAdditionalAssertions(t *testing.T) {
	alice := xidLike("Alice")
	bob := xidLike("Bob")

	edge := NewEnvelope("knows-bob").
		AddAssertion(knownvalues.IsA, "schema:colleague").
		AddAssertion(knownvalues.Source, alice).
		AddAssertion(knownvalues.Target, bob).
		AddAssertion("department", "Engineering").
		AddAssertion("since", "2024-01-15")

	err := edge.ValidateEdge()
	if err != ErrEdgeUnexpectedAssertion {
		t.Fatalf("expected ErrEdgeUnexpectedAssertion, got: %v", err)
	}
}

func TestEdgeWithClaimDetailOnTarget(t *testing.T) {
	alice := xidLike("Alice")
	target := xidLike("Bob").
		AddAssertion("department", "Engineering").
		AddAssertion("since", "2024-01-15")
	edge := makeEdge("knows-bob", "schema:colleague", alice, target)
	if err := edge.ValidateEdge(); err != nil {
		t.Fatalf("expected valid edge, got: %v", err)
	}
}

func TestEdgeWithClaimDetailOnSource(t *testing.T) {
	uri, err := bccomponents.NewURI("https://example.com/xid/")
	if err != nil {
		t.Fatalf("URI creation failed: %v", err)
	}
	source := xidLike("Alice").
		AddAssertion(knownvalues.DereferenceVia, uri)
	target := xidLike("Bob")
	edge := makeEdge("knows-bob", "schema:colleague", source, target)
	if err := edge.ValidateEdge(); err != nil {
		t.Fatalf("expected valid edge, got: %v", err)
	}
}
