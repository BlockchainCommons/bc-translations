package bccomponents

import (
	"testing"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	sskr "github.com/nickel-blockchaincommons/sskr-go"
)

func TestSSKRRoundtrip(t *testing.T) {
	RegisterTags()

	secretData := []byte{
		0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
		0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10,
	}
	secret, err := sskr.NewSecret(secretData)
	if err != nil {
		t.Fatalf("NewSecret failed: %v", err)
	}

	groupSpec, err := sskr.NewGroupSpec(2, 3)
	if err != nil {
		t.Fatalf("NewGroupSpec failed: %v", err)
	}
	spec, err := sskr.NewSpec(1, []sskr.GroupSpec{groupSpec})
	if err != nil {
		t.Fatalf("NewSpec failed: %v", err)
	}

	groups, err := SSKRGenerate(&spec, &secret)
	if err != nil {
		t.Fatalf("SSKRGenerate failed: %v", err)
	}

	// Verify structure
	if len(groups) != 1 {
		t.Fatalf("expected 1 group, got %d", len(groups))
	}
	if len(groups[0]) != 3 {
		t.Fatalf("expected 3 shares in group 0, got %d", len(groups[0]))
	}

	// Select first 2 shares (meeting threshold of 2)
	selectedShares := []SSKRShare{groups[0][0], groups[0][1]}

	combined, err := SSKRCombine(selectedShares)
	if err != nil {
		t.Fatalf("SSKRCombine failed: %v", err)
	}
	if !combined.Equal(secret) {
		t.Errorf("combined secret does not match original: got %v, want %v", combined.Data(), secretData)
	}
}

func TestSSKRMultipleGroups(t *testing.T) {
	RegisterTags()

	secretData := []byte{
		0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
		0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10,
	}
	secret, err := sskr.NewSecret(secretData)
	if err != nil {
		t.Fatalf("NewSecret failed: %v", err)
	}

	group1, err := sskr.NewGroupSpec(2, 3)
	if err != nil {
		t.Fatalf("NewGroupSpec(2,3) failed: %v", err)
	}
	group2, err := sskr.NewGroupSpec(3, 5)
	if err != nil {
		t.Fatalf("NewGroupSpec(3,5) failed: %v", err)
	}
	spec, err := sskr.NewSpec(2, []sskr.GroupSpec{group1, group2})
	if err != nil {
		t.Fatalf("NewSpec failed: %v", err)
	}

	groups, err := SSKRGenerate(&spec, &secret)
	if err != nil {
		t.Fatalf("SSKRGenerate failed: %v", err)
	}

	// Verify structure
	if len(groups) != 2 {
		t.Fatalf("expected 2 groups, got %d", len(groups))
	}
	if len(groups[0]) != 3 {
		t.Fatalf("expected 3 shares in group 0, got %d", len(groups[0]))
	}
	if len(groups[1]) != 5 {
		t.Fatalf("expected 5 shares in group 1, got %d", len(groups[1]))
	}

	// Select 2 from group 0 and 3 from group 1 (meets both thresholds)
	selectedShares := []SSKRShare{
		groups[0][0], groups[0][1],
		groups[1][0], groups[1][1], groups[1][2],
	}

	combined, err := SSKRCombine(selectedShares)
	if err != nil {
		t.Fatalf("SSKRCombine failed: %v", err)
	}
	if !combined.Equal(secret) {
		t.Errorf("combined secret does not match original")
	}
}

func TestSSKRShareAccessors(t *testing.T) {
	RegisterTags()

	secretData := []byte{
		0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
		0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10,
	}
	secret, err := sskr.NewSecret(secretData)
	if err != nil {
		t.Fatalf("NewSecret failed: %v", err)
	}

	group1, err := sskr.NewGroupSpec(2, 3)
	if err != nil {
		t.Fatalf("NewGroupSpec failed: %v", err)
	}
	spec, err := sskr.NewSpec(1, []sskr.GroupSpec{group1})
	if err != nil {
		t.Fatalf("NewSpec failed: %v", err)
	}

	groups, err := SSKRGenerate(&spec, &secret)
	if err != nil {
		t.Fatalf("SSKRGenerate failed: %v", err)
	}

	share0 := groups[0][0]
	share1 := groups[0][1]
	share2 := groups[0][2]

	// All shares in the same split should have the same identifier
	if share0.Identifier() != share1.Identifier() {
		t.Errorf("shares 0 and 1 have different identifiers: %d != %d", share0.Identifier(), share1.Identifier())
	}
	if share0.Identifier() != share2.Identifier() {
		t.Errorf("shares 0 and 2 have different identifiers: %d != %d", share0.Identifier(), share2.Identifier())
	}

	// Group threshold should be 1 (single group with threshold 1)
	if got := share0.GroupThreshold(); got != 1 {
		t.Errorf("GroupThreshold() = %d, want 1", got)
	}

	// Group count should be 1
	if got := share0.GroupCount(); got != 1 {
		t.Errorf("GroupCount() = %d, want 1", got)
	}

	// Group index should be 0
	if got := share0.GroupIndex(); got != 0 {
		t.Errorf("GroupIndex() = %d, want 0", got)
	}

	// Member threshold should be 2
	if got := share0.MemberThreshold(); got != 2 {
		t.Errorf("MemberThreshold() = %d, want 2", got)
	}

	// Member indices should be 0, 1, 2
	if got := share0.MemberIndex(); got != 0 {
		t.Errorf("share0.MemberIndex() = %d, want 0", got)
	}
	if got := share1.MemberIndex(); got != 1 {
		t.Errorf("share1.MemberIndex() = %d, want 1", got)
	}
	if got := share2.MemberIndex(); got != 2 {
		t.Errorf("share2.MemberIndex() = %d, want 2", got)
	}

	// IdentifierHex should be consistent
	if share0.IdentifierHex() != share1.IdentifierHex() {
		t.Errorf("IdentifierHex mismatch: %s != %s", share0.IdentifierHex(), share1.IdentifierHex())
	}

	// Hex should produce valid hex
	if len(share0.Hex()) == 0 {
		t.Error("Hex() returned empty string")
	}

	// String should contain identifier hex
	if got := share0.String(); len(got) == 0 {
		t.Error("String() returned empty string")
	}
}

func TestSSKRShareCBORRoundtrip(t *testing.T) {
	RegisterTags()

	secretData := []byte{
		0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
		0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10,
	}
	secret, err := sskr.NewSecret(secretData)
	if err != nil {
		t.Fatalf("NewSecret failed: %v", err)
	}

	groupSpec, err := sskr.NewGroupSpec(2, 3)
	if err != nil {
		t.Fatalf("NewGroupSpec failed: %v", err)
	}
	spec, err := sskr.NewSpec(1, []sskr.GroupSpec{groupSpec})
	if err != nil {
		t.Fatalf("NewSpec failed: %v", err)
	}

	groups, err := SSKRGenerate(&spec, &secret)
	if err != nil {
		t.Fatalf("SSKRGenerate failed: %v", err)
	}

	share := groups[0][0]

	// CBOR roundtrip
	cborData := share.TaggedCBOR().ToCBORData()
	cborVal, err := dcbor.TryFromData(cborData)
	if err != nil {
		t.Fatalf("TryFromData failed: %v", err)
	}
	decoded, err := DecodeTaggedSSKRShare(cborVal)
	if err != nil {
		t.Fatalf("DecodeTaggedSSKRShare failed: %v", err)
	}
	if share.Hex() != decoded.Hex() {
		t.Errorf("CBOR roundtrip failed: %s != %s", share.Hex(), decoded.Hex())
	}

	// UR roundtrip
	urString := SSKRShareToURString(share)
	decodedUR, err := SSKRShareFromURString(urString)
	if err != nil {
		t.Fatalf("SSKRShareFromURString failed: %v", err)
	}
	if share.Hex() != decodedUR.Hex() {
		t.Errorf("UR roundtrip failed: %s != %s", share.Hex(), decodedUR.Hex())
	}
}
