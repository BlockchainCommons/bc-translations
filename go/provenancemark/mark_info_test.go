package provenancemark

import (
	"encoding/json"
	"strings"
	"testing"
)

func TestMarkInfoJSONAndMarkdown(t *testing.T) {
	mark := makeTestMarks(t, 1, ProvenanceMarkResolutionLow, "Wolf", false)[0]
	info := NewProvenanceMarkInfo(mark, "Genesis mark.")

	if got, want := info.Bytewords(), mark.IDBytewords(4, true); got != want {
		t.Fatalf("bytewords mismatch: got=%q want=%q", got, want)
	}
	if got, want := info.Bytemoji(), mark.IDBytemoji(4, true); got != want {
		t.Fatalf("bytemoji mismatch: got=%q want=%q", got, want)
	}
	if got, want := info.UR().String(), mark.URString(); got != want {
		t.Fatalf("UR mismatch: got=%q want=%q", got, want)
	}

	markdown := info.MarkdownSummary()
	for _, expected := range []string{
		"---",
		mark.Date().String(),
		mark.URString(),
		info.Bytewords(),
		info.Bytemoji(),
		"Genesis mark.",
	} {
		if !strings.Contains(markdown, expected) {
			t.Fatalf("markdown summary missing %q:\n%s", expected, markdown)
		}
	}

	payload, err := json.Marshal(info)
	if err != nil {
		t.Fatalf("json.Marshal failed: %v", err)
	}
	var decoded ProvenanceMarkInfo
	if err := json.Unmarshal(payload, &decoded); err != nil {
		t.Fatalf("json.Unmarshal failed: %v", err)
	}
	if !decoded.Mark().Equal(mark) {
		t.Fatal("mark info JSON round-trip mismatch")
	}
}
