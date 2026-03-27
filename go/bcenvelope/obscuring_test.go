package bcenvelope

import (
	"strings"
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
)

func TestObscuring(t *testing.T) {
	key := bccomponents.NewSymmetricKey()

	envelope := NewEnvelope(plaintextHello)
	if envelope.IsObscured() {
		t.Fatal("plain envelope should not be obscured")
	}

	// Encrypted
	encrypted, err := envelope.EncryptSubject(key)
	if err != nil {
		t.Fatalf("encrypt failed: %v", err)
	}
	if !encrypted.IsObscured() {
		t.Fatal("encrypted should be obscured")
	}

	// Elided
	elided := envelope.Elide()
	if !elided.IsObscured() {
		t.Fatal("elided should be obscured")
	}

	// Compressed
	compressed, err := envelope.Compress()
	if err != nil {
		t.Fatalf("compress failed: %v", err)
	}
	if !compressed.IsObscured() {
		t.Fatal("compressed should be obscured")
	}

	// ENCRYPTION

	// Cannot encrypt an encrypted envelope.
	_, err = encrypted.EncryptSubject(key)
	if err == nil {
		t.Fatal("expected error encrypting encrypted envelope")
	}

	// Cannot encrypt an elided envelope.
	_, err = elided.EncryptSubject(key)
	if err == nil {
		t.Fatal("expected error encrypting elided envelope")
	}

	// OK to encrypt a compressed envelope.
	encryptedCompressed, err := compressed.EncryptSubject(key)
	if err != nil {
		t.Fatalf("encrypt compressed failed: %v", err)
	}
	if !encryptedCompressed.IsEncrypted() {
		t.Fatal("encrypted compressed should be encrypted")
	}

	// ELISION

	// OK to elide an encrypted envelope.
	elidedEncrypted := encrypted.Elide()
	if !elidedEncrypted.IsElided() {
		t.Fatal("elided encrypted should be elided")
	}

	// Eliding an elided envelope is idempotent.
	elidedElided := elided.Elide()
	if !elidedElided.IsElided() {
		t.Fatal("elided elided should be elided")
	}

	// OK to elide a compressed envelope.
	elidedCompressed := compressed.Elide()
	if !elidedCompressed.IsElided() {
		t.Fatal("elided compressed should be elided")
	}

	// COMPRESSION

	// Cannot compress an encrypted envelope.
	_, err = encrypted.Compress()
	if err == nil {
		t.Fatal("expected error compressing encrypted envelope")
	}

	// Cannot compress an elided envelope.
	_, err = elided.Compress()
	if err == nil {
		t.Fatal("expected error compressing elided envelope")
	}

	// Compressing a compressed envelope is idempotent.
	compressedCompressed, err := compressed.Compress()
	if err != nil {
		t.Fatalf("compress compressed failed: %v", err)
	}
	if !compressedCompressed.IsCompressed() {
		t.Fatal("compressed compressed should be compressed")
	}
}

func TestNodesMatching(t *testing.T) {
	envelope := NewEnvelope("Alice").
		AddAssertion("knows", "Bob").
		AddAssertion("age", 30).
		AddAssertion("city", "Boston")

	// Get some digests for targeting
	knowsAssertion, err := envelope.AssertionWithPredicate("knows")
	if err != nil {
		t.Fatalf("knows assertion not found: %v", err)
	}
	knowsDigest := knowsAssertion.Digest()

	ageAssertion, err := envelope.AssertionWithPredicate("age")
	if err != nil {
		t.Fatalf("age assertion not found: %v", err)
	}
	ageDigest := ageAssertion.Digest()

	// Elide one assertion, compress another
	elideTarget := make(map[bccomponents.Digest]struct{})
	elideTarget[knowsDigest] = struct{}{}

	compressTarget := make(map[bccomponents.Digest]struct{})
	compressTarget[ageDigest] = struct{}{}

	obscured := envelope.ElideRemovingSet(elideTarget)
	obscured = obscured.ElideRemovingSetWithAction(
		compressTarget,
		ObscureActionCompress,
		nil,
	)

	assertActualExpected(t, obscured.Format(), `"Alice" [
    "city": "Boston"
    COMPRESSED
    ELIDED
]`)

	// Test finding elided nodes
	elidedNodes := obscured.NodesMatching(nil, []ObscureType{ObscureTypeElided})
	if _, ok := elidedNodes[knowsDigest]; !ok {
		t.Fatal("expected knows digest in elided nodes")
	}

	// Test finding compressed nodes
	compressedNodes := obscured.NodesMatching(nil, []ObscureType{ObscureTypeCompressed})
	if _, ok := compressedNodes[ageDigest]; !ok {
		t.Fatal("expected age digest in compressed nodes")
	}

	// Test finding with target filter
	targetFilter := make(map[bccomponents.Digest]struct{})
	targetFilter[knowsDigest] = struct{}{}
	filtered := obscured.NodesMatching(targetFilter, []ObscureType{ObscureTypeElided})
	if len(filtered) != 1 {
		t.Fatalf("expected 1 filtered, got %d", len(filtered))
	}
	if _, ok := filtered[knowsDigest]; !ok {
		t.Fatal("expected knows digest in filtered")
	}

	// Test finding all obscured nodes (no type filter)
	allInTarget := obscured.NodesMatching(elideTarget, nil)
	if len(allInTarget) != 1 {
		t.Fatalf("expected 1 in target, got %d", len(allInTarget))
	}
	if _, ok := allInTarget[knowsDigest]; !ok {
		t.Fatal("expected knows digest in allInTarget")
	}

	// Test with no matches
	noMatchTarget := make(map[bccomponents.Digest]struct{})
	noMatchDigest := bccomponents.DigestFromImage([]byte("nonexistent"))
	noMatchTarget[noMatchDigest] = struct{}{}
	noMatches := obscured.NodesMatching(noMatchTarget, []ObscureType{ObscureTypeElided})
	if len(noMatches) != 0 {
		t.Fatalf("expected 0 no-matches, got %d", len(noMatches))
	}
}

func TestWalkUnelide(t *testing.T) {
	alice := NewEnvelope("Alice")
	bob := NewEnvelope("Bob")
	carol := NewEnvelope("Carol")

	envelope := NewEnvelope("Alice").
		AddAssertion("knows", "Bob").
		AddAssertion("friend", "Carol")

	// Elide multiple parts
	elided := envelope.
		ElideRemovingTarget(alice).
		ElideRemovingTarget(bob)

	// Verify parts are elided
	assertActualExpected(t, elided.Format(), `ELIDED [
    "friend": "Carol"
    "knows": ELIDED
]`)

	// Restore with walk_unelide
	restored := elided.WalkUnelide([]*Envelope{alice, bob, carol})

	assertActualExpected(t, restored.Format(), `"Alice" [
    "friend": "Carol"
    "knows": "Bob"
]`)

	// Test with partial restoration (only some envelopes provided)
	partial := elided.WalkUnelide([]*Envelope{alice})
	assertActualExpected(t, partial.Format(), `"Alice" [
    "friend": "Carol"
    "knows": ELIDED
]`)

	// Test with no matching envelopes
	unchanged := elided.WalkUnelide(nil)
	if !unchanged.IsIdenticalTo(elided) {
		t.Fatal("unchanged should be identical to elided")
	}
}

func TestWalkDecrypt(t *testing.T) {
	key1 := bccomponents.NewSymmetricKey()
	key2 := bccomponents.NewSymmetricKey()
	key3 := bccomponents.NewSymmetricKey()

	envelope := NewEnvelope("Alice").
		AddAssertion("knows", "Bob").
		AddAssertion("age", 30).
		AddAssertion("city", "Boston")

	// Encrypt different parts with different keys
	knowsAssertion, err := envelope.AssertionWithPredicate("knows")
	if err != nil {
		t.Fatalf("knows assertion not found: %v", err)
	}
	ageAssertion, err := envelope.AssertionWithPredicate("age")
	if err != nil {
		t.Fatalf("age assertion not found: %v", err)
	}

	encrypt1Target := make(map[bccomponents.Digest]struct{})
	encrypt1Target[knowsAssertion.Digest()] = struct{}{}

	encrypt2Target := make(map[bccomponents.Digest]struct{})
	encrypt2Target[ageAssertion.Digest()] = struct{}{}

	encrypted := envelope.ElideSetWithAction(
		encrypt1Target, false, ObscureActionEncrypt, &ObscureActionEncryptWithKey{Key: key1},
	)
	encrypted = encrypted.ElideSetWithAction(
		encrypt2Target, false, ObscureActionEncrypt, &ObscureActionEncryptWithKey{Key: key2},
	)

	// Verify parts are encrypted
	assertActualExpected(t, encrypted.Format(), `"Alice" [
    "city": "Boston"
    ENCRYPTED (2)
]`)

	// Decrypt with all keys
	decrypted := encrypted.WalkDecrypt([]bccomponents.SymmetricKey{key1, key2})
	assertActualExpected(t, decrypted.Format(), `"Alice" [
    "age": 30
    "city": "Boston"
    "knows": "Bob"
]`)

	// Decrypt with only one key (partial decryption)
	partial := encrypted.WalkDecrypt([]bccomponents.SymmetricKey{key1})
	if partial.IsIdenticalTo(encrypted) {
		t.Fatal("partial should differ from encrypted")
	}
	// Note: partial is still equivalent because encrypted nodes preserve digests
	if !partial.IsEquivalentTo(envelope) {
		t.Fatal("partial should be equivalent to original")
	}

	// There should still be one encrypted node remaining
	partialFormat := partial.Format()
	if !strings.Contains(partialFormat, "ENCRYPTED") {
		t.Fatal("expected ENCRYPTED in partial format")
	}
	if !strings.Contains(partialFormat, `"knows": "Bob"`) {
		t.Fatal("expected 'knows': 'Bob' in partial format")
	}

	// Decrypt with wrong key (should be unchanged)
	unchanged := encrypted.WalkDecrypt([]bccomponents.SymmetricKey{key3})
	if !unchanged.IsIdenticalTo(encrypted) {
		t.Fatal("unchanged should be identical to encrypted")
	}
}

func TestWalkDecompress(t *testing.T) {
	envelope := NewEnvelope("Alice").
		AddAssertion("knows", "Bob").
		AddAssertion("bio", strings.Repeat("A", 1000)).
		AddAssertion("description", strings.Repeat("B", 1000))

	// Compress multiple parts
	bioAssertion, err := envelope.AssertionWithPredicate("bio")
	if err != nil {
		t.Fatalf("bio assertion not found: %v", err)
	}
	descAssertion, err := envelope.AssertionWithPredicate("description")
	if err != nil {
		t.Fatalf("description assertion not found: %v", err)
	}

	bioDigest := bioAssertion.Digest()
	descDigest := descAssertion.Digest()

	compressTarget := make(map[bccomponents.Digest]struct{})
	compressTarget[bioDigest] = struct{}{}
	compressTarget[descDigest] = struct{}{}

	compressed := envelope.ElideRemovingSetWithAction(
		compressTarget,
		ObscureActionCompress,
		nil,
	)

	// Verify parts are compressed
	assertActualExpected(t, compressed.Format(), `"Alice" [
    "knows": "Bob"
    COMPRESSED (2)
]`)

	// Decompress all
	decompressed := compressed.WalkDecompress(nil)
	if !decompressed.IsEquivalentTo(envelope) {
		t.Fatal("decompressed should be equivalent to original")
	}

	// Decompress with target filter (only one node)
	target := make(map[bccomponents.Digest]struct{})
	target[bioDigest] = struct{}{}

	partialDecomp := compressed.WalkDecompress(target)
	if partialDecomp.IsIdenticalTo(compressed) {
		t.Fatal("partial should differ from compressed")
	}
	// Note: partial is still equivalent because compressed nodes preserve digests
	if !partialDecomp.IsEquivalentTo(envelope) {
		t.Fatal("partial should be equivalent to original")
	}

	// Bio should be decompressed but description still compressed
	stillCompressed := partialDecomp.NodesMatching(nil, []ObscureType{ObscureTypeCompressed})
	if _, ok := stillCompressed[descDigest]; !ok {
		t.Fatal("expected description still compressed")
	}
	if _, ok := stillCompressed[bioDigest]; ok {
		t.Fatal("expected bio to be decompressed")
	}

	// Decompress with non-matching target (should be unchanged)
	noMatch := make(map[bccomponents.Digest]struct{})
	noMatchDigest := bccomponents.DigestFromImage([]byte("nonexistent"))
	noMatch[noMatchDigest] = struct{}{}
	unchanged := compressed.WalkDecompress(noMatch)
	if !unchanged.IsIdenticalTo(compressed) {
		t.Fatal("unchanged should be identical to compressed")
	}
}

func TestMixedObscurationOperations(t *testing.T) {
	key := bccomponents.NewSymmetricKey()

	envelope := NewEnvelope("Alice").
		AddAssertion("knows", "Bob").
		AddAssertion("age", 30).
		AddAssertion("bio", strings.Repeat("A", 1000))

	knowsAssertion, err := envelope.AssertionWithPredicate("knows")
	if err != nil {
		t.Fatalf("knows assertion not found: %v", err)
	}
	ageAssertion, err := envelope.AssertionWithPredicate("age")
	if err != nil {
		t.Fatalf("age assertion not found: %v", err)
	}
	bioAssertion, err := envelope.AssertionWithPredicate("bio")
	if err != nil {
		t.Fatalf("bio assertion not found: %v", err)
	}

	knowsDigest := knowsAssertion.Digest()
	ageDigest := ageAssertion.Digest()
	bioDigest := bioAssertion.Digest()

	// Apply different obscuration types
	elideTarget := make(map[bccomponents.Digest]struct{})
	elideTarget[knowsDigest] = struct{}{}

	encryptTarget := make(map[bccomponents.Digest]struct{})
	encryptTarget[ageDigest] = struct{}{}

	compressTarget := make(map[bccomponents.Digest]struct{})
	compressTarget[bioDigest] = struct{}{}

	obscured := envelope.ElideRemovingSet(elideTarget)
	obscured = obscured.ElideRemovingSetWithAction(
		encryptTarget,
		ObscureActionEncrypt,
		&ObscureActionEncryptWithKey{Key: key},
	)
	obscured = obscured.ElideRemovingSetWithAction(
		compressTarget,
		ObscureActionCompress,
		nil,
	)

	// Verify different obscuration types
	elidedNodes := obscured.NodesMatching(nil, []ObscureType{ObscureTypeElided})
	encryptedNodes := obscured.NodesMatching(nil, []ObscureType{ObscureTypeEncrypted})
	compressedNodes := obscured.NodesMatching(nil, []ObscureType{ObscureTypeCompressed})

	if _, ok := elidedNodes[knowsDigest]; !ok {
		t.Fatal("expected knows in elided nodes")
	}
	if _, ok := encryptedNodes[ageDigest]; !ok {
		t.Fatal("expected age in encrypted nodes")
	}
	if _, ok := compressedNodes[bioDigest]; !ok {
		t.Fatal("expected bio in compressed nodes")
	}

	// Restore everything
	restored := obscured.
		WalkUnelide([]*Envelope{knowsAssertion}).
		WalkDecrypt([]bccomponents.SymmetricKey{key}).
		WalkDecompress(nil)

	if !restored.IsEquivalentTo(envelope) {
		t.Fatal("restored should be equivalent to original")
	}
}
