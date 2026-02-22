package bcur

import (
	"bytes"
	"testing"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

func TestEncode(t *testing.T) {
	cbor := dcbor.NewCBORArray([]dcbor.CBOR{
		dcbor.NewCBORUnsigned(1),
		dcbor.NewCBORUnsigned(2),
		dcbor.NewCBORUnsigned(3),
	})
	ur, err := NewUR("test", cbor)
	if err != nil {
		t.Fatal(err)
	}
	got := ur.URString()
	if got != "ur:test/lsadaoaxjygonesw" {
		t.Errorf("URString = %q, want %q", got, "ur:test/lsadaoaxjygonesw")
	}
}

func TestDecode(t *testing.T) {
	urString := "ur:test/lsadaoaxjygonesw"
	ur, err := FromURString(urString)
	if err != nil {
		t.Fatal(err)
	}
	if ur.URTypeStr() != "test" {
		t.Errorf("URTypeStr = %q, want %q", ur.URTypeStr(), "test")
	}

	expected := dcbor.NewCBORArray([]dcbor.CBOR{
		dcbor.NewCBORUnsigned(1),
		dcbor.NewCBORUnsigned(2),
		dcbor.NewCBORUnsigned(3),
	})
	if !ur.CBOR().Equal(expected) {
		t.Errorf("CBOR mismatch")
	}
}

func TestUR(t *testing.T) {
	cbor := dcbor.NewCBORArray([]dcbor.CBOR{
		dcbor.NewCBORUnsigned(1),
		dcbor.NewCBORUnsigned(2),
		dcbor.NewCBORUnsigned(3),
	})
	ur, err := NewUR("test", cbor)
	if err != nil {
		t.Fatal(err)
	}
	urString := ur.URString()
	if urString != "ur:test/lsadaoaxjygonesw" {
		t.Errorf("URString = %q", urString)
	}

	ur2, err := FromURString(urString)
	if err != nil {
		t.Fatal(err)
	}
	if ur2.URTypeStr() != "test" {
		t.Errorf("URTypeStr = %q", ur2.URTypeStr())
	}
	if !ur2.CBOR().Equal(ur.CBOR()) {
		t.Error("CBOR mismatch")
	}

	// Case-insensitive
	capsURString := "UR:TEST/LSADAOAXJYGONESW"
	ur3, err := FromURString(capsURString)
	if err != nil {
		t.Fatal(err)
	}
	if ur3.URTypeStr() != "test" {
		t.Errorf("URTypeStr = %q", ur3.URTypeStr())
	}
	if !ur3.CBOR().Equal(ur.CBOR()) {
		t.Error("CBOR mismatch for uppercase")
	}
}

func runFountainTest(t *testing.T, startPart int) int {
	t.Helper()
	message := "The only thing we have to fear is fear itself."
	cbor := dcbor.ToByteString([]byte(message))
	ur, err := NewUR("bytes", cbor)
	if err != nil {
		t.Fatal(err)
	}

	encoder, err := NewMultipartEncoder(ur, 10)
	if err != nil {
		t.Fatal(err)
	}
	decoder := NewMultipartDecoder()
	for i := 0; i < 1000; i++ {
		part, err := encoder.NextPart()
		if err != nil {
			t.Fatal(err)
		}
		if encoder.CurrentIndex() >= startPart {
			if err := decoder.Receive(part); err != nil {
				t.Fatal(err)
			}
		}
		if decoder.IsComplete() {
			break
		}
	}
	receivedUR, err := decoder.Message()
	if err != nil {
		t.Fatal(err)
	}
	if receivedUR == nil {
		t.Fatal("received nil UR")
	}
	if !receivedUR.Equal(ur) {
		t.Error("received UR does not match original")
	}
	return encoder.CurrentIndex()
}

func TestFountain(t *testing.T) {
	if got := runFountainTest(t, 1); got != 5 {
		t.Errorf("runFountainTest(1) = %d, want 5", got)
	}
	if got := runFountainTest(t, 51); got != 61 {
		t.Errorf("runFountainTest(51) = %d, want 61", got)
	}
	if got := runFountainTest(t, 101); got != 110 {
		t.Errorf("runFountainTest(101) = %d, want 110", got)
	}
	if got := runFountainTest(t, 501); got != 507 {
		t.Errorf("runFountainTest(501) = %d, want 507", got)
	}
}

func TestSinglePartUR(t *testing.T) {
	// Make test UR data (CBOR-encoded byte string of pseudo-random bytes)
	msg := makeMessage("Wolf", 50)
	cbor := dcbor.ToByteString(msg)
	data := dcbor.ToCBORData(cbor)

	encoded := urEncode(data, "bytes")
	expected := "ur:bytes/hdeymejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtgwdpfnsboxgwlbaawzuefywkdplrsrjynbvygabwjldapfcsdwkbrkch"
	if encoded != expected {
		t.Errorf("SinglePartUR encode = %q, want %q", encoded, expected)
	}

	kind, decoded, err := urDecode(encoded)
	if err != nil {
		t.Fatal(err)
	}
	if kind != urKindSinglePart {
		t.Error("expected SinglePart")
	}
	if !bytes.Equal(decoded, data) {
		t.Error("decoded data mismatch")
	}
}

func TestUREncoder(t *testing.T) {
	msg := makeMessage("Wolf", 256)
	cbor := dcbor.ToByteString(msg)
	_ = dcbor.ToCBORData(cbor)

	ur, err := NewUR("bytes", dcbor.ToByteString(msg))
	if err != nil {
		t.Fatal(err)
	}

	encoder, err := NewMultipartEncoder(ur, 30)
	if err != nil {
		t.Fatal(err)
	}

	expected := []string{
		"ur:bytes/1-9/lpadascfadaxcywenbpljkhdcahkadaemejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtdkgslpgh",
		"ur:bytes/2-9/lpaoascfadaxcywenbpljkhdcagwdpfnsboxgwlbaawzuefywkdplrsrjynbvygabwjldapfcsgmghhkhstlrdcxaefz",
		"ur:bytes/3-9/lpaxascfadaxcywenbpljkhdcahelbknlkuejnbadmssfhfrdpsbiegecpasvssovlgeykssjykklronvsjksopdzmol",
		"ur:bytes/4-9/lpaaascfadaxcywenbpljkhdcasotkhemthydawydtaxneurlkosgwcekonertkbrlwmplssjtammdplolsbrdzcrtas",
		"ur:bytes/5-9/lpahascfadaxcywenbpljkhdcatbbdfmssrkzmcwnezelennjpfzbgmuktrhtejscktelgfpdlrkfyfwdajldejokbwf",
		"ur:bytes/6-9/lpamascfadaxcywenbpljkhdcackjlhkhybssklbwefectpfnbbectrljectpavyrolkzczcpkmwidmwoxkilghdsowp",
		"ur:bytes/7-9/lpatascfadaxcywenbpljkhdcavszmwnjkwtclrtvaynhpahrtoxmwvwatmedibkaegdosftvandiodagdhthtrlnnhy",
		"ur:bytes/8-9/lpayascfadaxcywenbpljkhdcadmsponkkbbhgsoltjntegepmttmoonftnbuoiyrehfrtsabzsttorodklubbuyaetk",
		"ur:bytes/9-9/lpasascfadaxcywenbpljkhdcajskecpmdckihdyhphfotjojtfmlnwmadspaxrkytbztpbauotbgtgtaeaevtgavtny",
		"ur:bytes/10-9/lpbkascfadaxcywenbpljkhdcahkadaemejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtwdkiplzs",
		"ur:bytes/11-9/lpbdascfadaxcywenbpljkhdcahelbknlkuejnbadmssfhfrdpsbiegecpasvssovlgeykssjykklronvsjkvetiiapk",
		"ur:bytes/12-9/lpbnascfadaxcywenbpljkhdcarllaluzmdmgstospeyiefmwejlwtpedamktksrvlcygmzemovovllarodtmtbnptrs",
		"ur:bytes/13-9/lpbtascfadaxcywenbpljkhdcamtkgtpknghchchyketwsvwgwfdhpgmgtylctotzopdrpayoschcmhplffziachrfgd",
		"ur:bytes/14-9/lpbaascfadaxcywenbpljkhdcapazewnvonnvdnsbyleynwtnsjkjndeoldydkbkdslgjkbbkortbelomueekgvstegt",
		"ur:bytes/15-9/lpbsascfadaxcywenbpljkhdcaynmhpddpzmversbdqdfyrehnqzlugmjzmnmtwmrouohtstgsbsahpawkditkckynwt",
		"ur:bytes/16-9/lpbeascfadaxcywenbpljkhdcawygekobamwtlihsnpalnsghenskkiynthdzotsimtojetprsttmukirlrsbtamjtpd",
		"ur:bytes/17-9/lpbyascfadaxcywenbpljkhdcamklgftaxykpewyrtqzhydntpnytyisincxmhtbceaykolduortotiaiaiafhiaoyce",
		"ur:bytes/18-9/lpbgascfadaxcywenbpljkhdcahkadaemejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtntwkbkwy",
		"ur:bytes/19-9/lpbwascfadaxcywenbpljkhdcadekicpaajootjzpsdrbalpeywllbdsnbinaerkurspbncxgslgftvtsrjtksplcpeo",
		"ur:bytes/20-9/lpbbascfadaxcywenbpljkhdcayapmrleeleaxpasfrtrdkncffwjyjzgyetdmlewtkpktgllepfrltataztksmhkbot",
	}

	if encoder.PartsCount() != 9 {
		t.Errorf("PartsCount = %d, want 9", encoder.PartsCount())
	}

	for i, e := range expected {
		if encoder.CurrentIndex() != i {
			t.Errorf("CurrentIndex = %d, want %d", encoder.CurrentIndex(), i)
		}
		part, err := encoder.NextPart()
		if err != nil {
			t.Fatal(err)
		}
		if part != e {
			t.Errorf("part[%d] = %q, want %q", i, part, e)
		}
	}
}

func TestMultipartUR(t *testing.T) {
	msg := makeMessage("Wolf", 32767)
	cbor := dcbor.ToByteString(msg)
	_ = dcbor.ToCBORData(cbor)

	ur, err := NewUR("bytes", dcbor.ToByteString(msg))
	if err != nil {
		t.Fatal(err)
	}

	encoder, err := NewMultipartEncoder(ur, 1000)
	if err != nil {
		t.Fatal(err)
	}
	decoder := NewMultipartDecoder()
	for !decoder.IsComplete() {
		msg, _ := decoder.Message()
		if msg != nil {
			t.Error("message should be nil before complete")
		}
		part, err := encoder.NextPart()
		if err != nil {
			t.Fatal(err)
		}
		decoder.Receive(part)
	}
	received, err := decoder.Message()
	if err != nil {
		t.Fatal(err)
	}
	if !received.Equal(ur) {
		t.Error("received UR mismatch")
	}
}

func TestURDecoder(t *testing.T) {
	// Invalid scheme
	_, _, err := urDecode("uhr:bytes/aeadaolazmjendeoti")
	if err != ErrInvalidScheme {
		t.Errorf("expected ErrInvalidScheme, got %v", err)
	}

	// No type
	_, _, err = urDecode("ur:aeadaolazmjendeoti")
	if err != ErrTypeUnspecified {
		t.Errorf("expected ErrTypeUnspecified, got %v", err)
	}

	// Invalid characters
	_, _, err = urDecode("ur:bytes#4/aeadaolazmjendeoti")
	if err != ErrInvalidCharacters {
		t.Errorf("expected ErrInvalidCharacters, got %v", err)
	}

	// Invalid indices
	_, _, err = urDecode("ur:bytes/1-1a/aeadaolazmjendeoti")
	if err != ErrInvalidIndices {
		t.Errorf("expected ErrInvalidIndices, got %v", err)
	}

	// Too many slashes
	_, _, err = urDecode("ur:bytes/1-1/toomuch/aeadaolazmjendeoti")
	if err != ErrInvalidIndices {
		t.Errorf("expected ErrInvalidIndices for too many slashes, got %v", err)
	}

	// Valid
	_, _, err = urDecode("ur:bytes/aeadaolazmjendeoti")
	if err != nil {
		t.Errorf("unexpected error for valid UR: %v", err)
	}

	// Valid with custom type
	_, _, err = urDecode("ur:whatever-12/aeadaolazmjendeoti")
	if err != nil {
		t.Errorf("unexpected error for custom type: %v", err)
	}
}

func TestCustomEncoder(t *testing.T) {
	data := []byte("Ten chars!")
	cbor := dcbor.ToByteString(data)
	ur, err := NewUR("my-scheme", cbor)
	if err != nil {
		t.Fatal(err)
	}
	encoder, err := NewMultipartEncoder(ur, 5)
	if err != nil {
		t.Fatal(err)
	}
	part, err := encoder.NextPart()
	if err != nil {
		t.Fatal(err)
	}
	expected := "ur:my-scheme/1-3/lpadaxbdcyfdadtoaefygeghihjttnvleoba"
	if part != expected {
		t.Errorf("custom encoder part = %q, want %q", part, expected)
	}
}

// URCodable test
type testLeaf struct {
	s string
}

func (l *testLeaf) CBORTags() []dcbor.Tag {
	return []dcbor.Tag{dcbor.NewTag(24, "leaf")}
}

func (l *testLeaf) UntaggedCBOR() dcbor.CBOR {
	return dcbor.NewCBORText(l.s)
}

func (l *testLeaf) ToCBOR() dcbor.CBOR {
	tagged, _ := dcbor.TaggedCBOR(l)
	return tagged
}

func decodeLeaf(cbor dcbor.CBOR) (*testLeaf, error) {
	s, err := dcbor.DecodeText(cbor)
	if err != nil {
		return nil, err
	}
	return &testLeaf{s: s}, nil
}

func TestURCodable(t *testing.T) {
	leaf := &testLeaf{s: "test"}
	ur := ToUR(leaf)
	urString := ur.URString()
	if urString != "ur:leaf/iejyihjkjygupyltla" {
		t.Errorf("URString = %q, want %q", urString, "ur:leaf/iejyihjkjygupyltla")
	}

	leaf2, err := DecodeURString(urString, leaf.CBORTags(), decodeLeaf)
	if err != nil {
		t.Fatal(err)
	}
	if leaf2.s != leaf.s {
		t.Errorf("decoded leaf.s = %q, want %q", leaf2.s, leaf.s)
	}
}
