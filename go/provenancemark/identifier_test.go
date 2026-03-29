package provenancemark

import (
	"encoding/hex"
	"strings"
	"testing"
)

func TestIDReturns32Bytes(t *testing.T) {
	for _, res := range []ProvenanceMarkResolution{
		ProvenanceMarkResolutionLow,
		ProvenanceMarkResolutionMedium,
		ProvenanceMarkResolutionQuartile,
		ProvenanceMarkResolutionHigh,
	} {
		marks := makeTestMarks(t, 3, res, "Wolf", false)
		for _, mark := range marks {
			if got := len(mark.ID()); got != 32 {
				t.Fatalf("ID length mismatch for %v: got=%d want=32", res, got)
			}
		}
	}
}

func TestIDPreservesHashPrefix(t *testing.T) {
	for _, res := range []ProvenanceMarkResolution{
		ProvenanceMarkResolutionLow,
		ProvenanceMarkResolutionMedium,
		ProvenanceMarkResolutionQuartile,
		ProvenanceMarkResolutionHigh,
	} {
		marks := makeTestMarks(t, 3, res, "Wolf", false)
		for _, mark := range marks {
			id := mark.ID()
			hash := mark.Hash()
			if got := string(id[:len(hash)]); got != string(hash) {
				t.Fatalf("ID prefix mismatch for %v: got=%x want=%x", res, id[:len(hash)], hash)
			}
		}
	}
}

func TestIDHexEncodesFullID(t *testing.T) {
	mark := makeTestMarks(t, 1, ProvenanceMarkResolutionLow, "Wolf", false)[0]
	id := mark.ID()
	if got, want := mark.IDHex(), hex.EncodeToString(id[:]); got != want {
		t.Fatalf("IDHex mismatch: got=%q want=%q", got, want)
	}
}

func TestIDBytewordsPrefixFlag(t *testing.T) {
	mark := makeTestMarks(t, 1, ProvenanceMarkResolutionLow, "Wolf", false)[0]
	without := mark.IDBytewords(4, false)
	with := mark.IDBytewords(4, true)
	if len(with) < 5 || with[:5] != ProvenanceMarkPrefix+" " {
		t.Fatalf("prefixed IDBytewords missing prefix: %q", with)
	}
	if got := with[5:]; got != without {
		t.Fatalf("prefixed IDBytewords body mismatch: got=%q want=%q", got, without)
	}
}

func TestIDBytewordsAndBytemojiCounts(t *testing.T) {
	mark := makeTestMarks(t, 1, ProvenanceMarkResolutionLow, "Wolf", false)[0]
	for n := 4; n <= 32; n++ {
		if got := len(splitWords(mark.IDBytewords(n, false))); got != n {
			t.Fatalf("IDBytewords count mismatch for n=%d: got=%d", n, got)
		}
		if got := len(splitWords(mark.IDBytemoji(n, false))); got != n {
			t.Fatalf("IDBytemoji count mismatch for n=%d: got=%d", n, got)
		}
		if got := len(mark.IDBytewordsMinimal(n, false)); got != n*2 {
			t.Fatalf("IDBytewordsMinimal length mismatch for n=%d: got=%d", n, got)
		}
	}
}

func TestIDBytewordsMinimalProperties(t *testing.T) {
	mark := makeTestMarks(t, 1, ProvenanceMarkResolutionLow, "Wolf", false)[0]
	short := mark.IDBytewordsMinimal(4, false)
	long := mark.IDBytewordsMinimal(8, false)
	if long[:len(short)] != short {
		t.Fatalf("minimal bytewords prefix mismatch: short=%q long=%q", short, long)
	}
	if got := short; got != strings.ToUpper(short) {
		t.Fatalf("minimal bytewords should be upper-case: got=%q", got)
	}
}

func TestIDWordCountPanics(t *testing.T) {
	mark := makeTestMarks(t, 1, ProvenanceMarkResolutionLow, "Wolf", false)[0]
	assertPanics(t, func() { mark.IDBytewords(3, false) })
	assertPanics(t, func() { mark.IDBytewords(33, false) })
	assertPanics(t, func() { mark.IDBytemoji(33, false) })
	assertPanics(t, func() { mark.IDBytewordsMinimal(3, false) })
}

func TestDisambiguatedNoCollisions(t *testing.T) {
	marks := makeTestMarks(t, 5, ProvenanceMarkResolutionLow, "Wolf", false)
	ids := DisambiguatedIDBytewords(marks, false)
	if len(ids) != 5 {
		t.Fatalf("identifier count mismatch: got=%d want=5", len(ids))
	}
	for _, id := range ids {
		if got := len(splitWords(id)); got != 4 {
			t.Fatalf("non-colliding mark should stay at 4 words: got=%d id=%q", got, id)
		}
	}
}

func TestDisambiguatedSelectiveExtension(t *testing.T) {
	marks := makeTestMarks(t, 5, ProvenanceMarkResolutionLow, "Wolf", false)
	withDuplicate := []ProvenanceMark{marks[0], marks[1], marks[2], marks[0]}
	ids := DisambiguatedIDBytewords(withDuplicate, false)
	if len(ids) != 4 {
		t.Fatalf("identifier count mismatch: got=%d want=4", len(ids))
	}
	if got := len(splitWords(ids[1])); got != 4 {
		t.Fatalf("non-colliding mark should stay at 4 words: got=%d id=%q", got, ids[1])
	}
	if got := len(splitWords(ids[2])); got != 4 {
		t.Fatalf("non-colliding mark should stay at 4 words: got=%d id=%q", got, ids[2])
	}
	if got := len(splitWords(ids[0])); got != 32 {
		t.Fatalf("duplicate mark should extend to 32 words: got=%d", got)
	}
	if got := len(splitWords(ids[3])); got != 32 {
		t.Fatalf("duplicate mark should extend to 32 words: got=%d", got)
	}
	if ids[0] != ids[3] {
		t.Fatalf("identical marks should produce identical identifiers: %q vs %q", ids[0], ids[3])
	}
}

func TestDisambiguatedIdentifiersAreUnique(t *testing.T) {
	marks := makeTestMarks(t, 10, ProvenanceMarkResolutionLow, "Wolf", false)
	ids := DisambiguatedIDBytewords(marks, false)
	seen := make(map[string]struct{}, len(ids))
	for _, id := range ids {
		seen[id] = struct{}{}
	}
	if len(seen) != len(ids) {
		t.Fatalf("expected all disambiguated identifiers to be unique: got=%d want=%d", len(seen), len(ids))
	}
}

func TestDisambiguatedBytemojiUsesSamePrefixLengths(t *testing.T) {
	marks := makeTestMarks(t, 3, ProvenanceMarkResolutionLow, "Wolf", false)
	withDuplicate := []ProvenanceMark{marks[0], marks[1], marks[0]}
	wordIDs := DisambiguatedIDBytewords(withDuplicate, false)
	emojiIDs := DisambiguatedIDBytemoji(withDuplicate, false)
	if len(wordIDs) != len(emojiIDs) {
		t.Fatalf("disambiguated identifier count mismatch: words=%d emojis=%d", len(wordIDs), len(emojiIDs))
	}
	for i := range wordIDs {
		if got, want := len(splitWords(emojiIDs[i])), len(splitWords(wordIDs[i])); got != want {
			t.Fatalf("prefix length mismatch at index %d: got=%d want=%d", i, got, want)
		}
	}
}

func TestDisambiguatedPrefixFlag(t *testing.T) {
	marks := makeTestMarks(t, 3, ProvenanceMarkResolutionLow, "Wolf", false)
	without := DisambiguatedIDBytewords(marks, false)
	with := DisambiguatedIDBytewords(marks, true)
	for i := range without {
		if len(with[i]) < 5 || with[i][:5] != ProvenanceMarkPrefix+" " {
			t.Fatalf("prefixed identifier missing prefix: %q", with[i])
		}
		if got := with[i][5:]; got != without[i] {
			t.Fatalf("prefixed identifier body mismatch: got=%q want=%q", got, without[i])
		}
	}
}

func splitWords(value string) []string {
	if value == "" {
		return nil
	}
	return strings.Split(value, " ")
}

func assertPanics(t *testing.T, fn func()) {
	t.Helper()
	defer func() {
		if recover() == nil {
			t.Fatal("expected panic")
		}
	}()
	fn()
}
