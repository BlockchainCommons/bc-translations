package bccomponents

import (
	"bytes"
	"encoding/hex"
	"testing"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

func hexToArray32(h string) [32]byte {
	data, err := hex.DecodeString(h)
	if err != nil {
		panic("invalid hex: " + err.Error())
	}
	if len(data) != 32 {
		panic("hex string must decode to 32 bytes")
	}
	var arr [32]byte
	copy(arr[:], data)
	return arr
}

func TestXID(t *testing.T) {
	RegisterTags()

	data, err := hex.DecodeString("de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037")
	if err != nil {
		t.Fatal(err)
	}
	xid, err := XIDFromDataRef(data)
	if err != nil {
		t.Fatal(err)
	}

	if got := xid.Hex(); got != "de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037" {
		t.Errorf("Hex() = %s, want de2853684ae55803a08b36dd7f4e566649970601927330299fd333f33fecc037", got)
	}
	if got := xid.ShortDescription(); got != "de285368" {
		t.Errorf("ShortDescription() = %s, want de285368", got)
	}
	if got := xid.String(); got != "XID(de285368)" {
		t.Errorf("String() = %s, want XID(de285368)", got)
	}

	// UR roundtrip
	urString := XIDToURString(xid)
	expectedUR := "ur:xid/hdcxuedeguisgevwhdaxnbluenutlbglhfiygamsamadmojkdydtneteeowffhwprtemcaatledk"
	if urString != expectedUR {
		t.Errorf("UR string = %s, want %s", urString, expectedUR)
	}
	decoded, err := XIDFromURString(urString)
	if err != nil {
		t.Fatalf("XIDFromURString failed: %v", err)
	}
	if !decoded.Equal(xid) {
		t.Errorf("UR roundtrip failed: decoded XID does not match original")
	}

	// Bytewords identifier
	if got := xid.BytewordsIdentifier(true); got != "\U0001f167 URGE DICE GURU IRIS" {
		t.Errorf("BytewordsIdentifier(true) = %q, want %q", got, "\U0001f167 URGE DICE GURU IRIS")
	}

	// Bytemoji identifier
	if got := xid.BytemojiIdentifier(true); got != "\U0001f167 \U0001f43b \U0001f63b \U0001f35e \U0001f490" {
		t.Errorf("BytemojiIdentifier(true) = %q, want %q", got, "\U0001f167 \U0001f43b \U0001f63b \U0001f35e \U0001f490")
	}

	// CBOR roundtrip
	cborData := xid.TaggedCBOR().ToCBORData()
	decoded2, err := XIDFromTaggedCBOR(cborData)
	if err != nil {
		t.Fatalf("XIDFromTaggedCBOR failed: %v", err)
	}
	if !decoded2.Equal(xid) {
		t.Errorf("CBOR roundtrip failed: decoded XID does not match original")
	}
}

func TestXIDFromKey(t *testing.T) {
	RegisterTags()

	ecPriv := ECPrivateKeyFromData(hexToArray32("322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36"))
	sigPriv := NewSigningPrivateKeySchnorr(ecPriv)
	sigPub := sigPriv.PublicKey()

	xid := NewXID(sigPub)
	if got := xid.Hex(); got != "d40e0602674df1b732f5e025d04c45f2e74ed1652c5ae1740f6a5502dbbdcd47" {
		t.Errorf("XID Hex() = %s, want d40e0602674df1b732f5e025d04c45f2e74ed1652c5ae1740f6a5502dbbdcd47", got)
	}
	if !xid.Validate(sigPub) {
		t.Errorf("XID.Validate() returned false, want true")
	}
	if got := xid.ShortDescription(); got != "d40e0602" {
		t.Errorf("ShortDescription() = %s, want d40e0602", got)
	}

	ref := xid.Reference()
	if got := ref.RefHexShort(); got != "d40e0602" {
		t.Errorf("Reference.RefHexShort() = %s, want d40e0602", got)
	}
}

func TestARID(t *testing.T) {
	RegisterTags()

	// Test random creation
	arid1 := NewARID()
	arid2 := NewARID()
	if arid1.Equal(arid2) {
		t.Error("two random ARIDs should not be equal")
	}
	if len(arid1.Bytes()) != ARIDSize {
		t.Errorf("ARID bytes length = %d, want %d", len(arid1.Bytes()), ARIDSize)
	}

	// Test from data
	fixedData := hexToArray32("c66be27dbad7f615d4f9a1d67004c4a96573f614820d6966c8af00267de3e3c9")
	arid3 := ARIDFromData(fixedData)
	if got := arid3.Hex(); got != "c66be27dbad7f615d4f9a1d67004c4a96573f614820d6966c8af00267de3e3c9" {
		t.Errorf("ARID Hex() = %s, want c66be27dbad7f615d4f9a1d67004c4a96573f614820d6966c8af00267de3e3c9", got)
	}
	if got := arid3.ShortDescription(); got != "c66be27d" {
		t.Errorf("ARID ShortDescription() = %s, want c66be27d", got)
	}
	if got := arid3.String(); got != "ARID(c66be27dbad7f615d4f9a1d67004c4a96573f614820d6966c8af00267de3e3c9)" {
		t.Errorf("ARID String() = %s, want ARID(c66be27dbad7f615d4f9a1d67004c4a96573f614820d6966c8af00267de3e3c9)", got)
	}

	// Test CBOR roundtrip
	cborData := arid3.TaggedCBOR().ToCBORData()
	decoded, err := ARIDFromTaggedCBOR(cborData)
	if err != nil {
		t.Fatalf("ARIDFromTaggedCBOR failed: %v", err)
	}
	if !decoded.Equal(arid3) {
		t.Errorf("CBOR roundtrip failed: decoded ARID does not match original")
	}

	// Test from data ref
	arid4, err := ARIDFromDataRef(fixedData[:])
	if err != nil {
		t.Fatalf("ARIDFromDataRef failed: %v", err)
	}
	if !arid4.Equal(arid3) {
		t.Error("ARIDFromDataRef result does not match ARIDFromData")
	}

	// Test invalid size
	_, err = ARIDFromDataRef([]byte{1, 2, 3})
	if err == nil {
		t.Error("ARIDFromDataRef should fail on short data")
	}

	// Test from hex
	arid5 := ARIDFromHex("c66be27dbad7f615d4f9a1d67004c4a96573f614820d6966c8af00267de3e3c9")
	if !arid5.Equal(arid3) {
		t.Error("ARIDFromHex result does not match ARIDFromData")
	}
}

func TestUUID(t *testing.T) {
	RegisterTags()

	// Test random creation (type 4)
	uuid := NewUUID()
	data := uuid.Data()
	// Verify version 4 bits
	if data[6]&0xf0 != 0x40 {
		t.Errorf("UUID version bits = 0x%02x, want 0x40", data[6]&0xf0)
	}
	// Verify variant bits
	if data[8]&0xc0 != 0x80 {
		t.Errorf("UUID variant bits = 0x%02x, want 0x80", data[8]&0xc0)
	}

	// Test string format
	s := uuid.String()
	if len(s) != 36 {
		t.Errorf("UUID string length = %d, want 36", len(s))
	}
	// Check dash positions
	if s[8] != '-' || s[13] != '-' || s[18] != '-' || s[23] != '-' {
		t.Errorf("UUID string format invalid: %s", s)
	}

	// Test parse from string
	uuid2, err := UUIDFromString(s)
	if err != nil {
		t.Fatalf("UUIDFromString failed: %v", err)
	}
	if !uuid2.Equal(uuid) {
		t.Error("UUIDFromString roundtrip failed")
	}

	// Test from fixed data
	var fixedData [UUIDSize]byte
	for i := range fixedData {
		fixedData[i] = byte(i + 1)
	}
	fixedUUID := UUIDFromData(fixedData)
	if !bytes.Equal(fixedUUID.Bytes(), fixedData[:]) {
		t.Error("UUIDFromData did not preserve data")
	}

	// Test CBOR roundtrip
	cborData := uuid.TaggedCBOR().ToCBORData()
	cborVal, err := dcbor.TryFromData(cborData)
	if err != nil {
		t.Fatalf("TryFromData failed: %v", err)
	}
	decoded, err := DecodeTaggedUUID(cborVal)
	if err != nil {
		t.Fatalf("DecodeTaggedUUID failed: %v", err)
	}
	if !decoded.Equal(uuid) {
		t.Error("CBOR roundtrip failed")
	}

	// Test two random UUIDs are different
	uuid3 := NewUUID()
	if uuid.Equal(uuid3) {
		t.Error("two random UUIDs should not be equal")
	}

	// Test UUIDFromDataRef
	uuidRef, err := UUIDFromDataRef(fixedData[:])
	if err != nil {
		t.Fatalf("UUIDFromDataRef failed on valid data: %v", err)
	}
	if !uuidRef.Equal(fixedUUID) {
		t.Error("UUIDFromDataRef result does not match UUIDFromData")
	}

	// Test UUIDFromDataRef with invalid size
	_, err = UUIDFromDataRef([]byte{1, 2, 3})
	if err == nil {
		t.Error("UUIDFromDataRef should fail on short data")
	}
}

func TestURI(t *testing.T) {
	RegisterTags()

	// Test valid URI
	uri, err := NewURI("https://example.com")
	if err != nil {
		t.Fatalf("NewURI failed: %v", err)
	}
	if got := uri.URIString(); got != "https://example.com" {
		t.Errorf("URIString() = %s, want https://example.com", got)
	}
	if got := uri.String(); got != "https://example.com" {
		t.Errorf("String() = %s, want https://example.com", got)
	}

	// Test empty URI
	_, err = NewURI("")
	if err == nil {
		t.Error("NewURI should fail on empty string")
	}

	// Test equality
	uri2, err := NewURI("https://example.com")
	if err != nil {
		t.Fatal(err)
	}
	if !uri.Equal(uri2) {
		t.Error("equal URIs should be equal")
	}

	uri3, err := NewURI("https://other.com")
	if err != nil {
		t.Fatal(err)
	}
	if uri.Equal(uri3) {
		t.Error("different URIs should not be equal")
	}

	// Test CBOR roundtrip
	cborData := uri.TaggedCBOR().ToCBORData()
	cborVal, err := dcbor.TryFromData(cborData)
	if err != nil {
		t.Fatalf("TryFromData failed: %v", err)
	}
	decoded, err := DecodeTaggedURI(cborVal)
	if err != nil {
		t.Fatalf("DecodeTaggedURI failed: %v", err)
	}
	if !decoded.Equal(uri) {
		t.Error("CBOR roundtrip failed")
	}
}
