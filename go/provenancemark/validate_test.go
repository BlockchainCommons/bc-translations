package provenancemark

import (
	"strings"
	"testing"
)

func TestValidateEmpty(t *testing.T) {
	report := Validate(nil)
	if got, want := report.Format(ValidationReportFormatJSONPretty), strings.TrimSpace(`{
  "marks": [],
  "chains": []
}`); got != want {
		t.Fatalf("pretty JSON mismatch:\n%s\nwant:\n%s", got, want)
	}
	if got, want := report.Format(ValidationReportFormatJSONCompact), `{"marks":[],"chains":[]}`; got != want {
		t.Fatalf("compact JSON mismatch: got=%q want=%q", got, want)
	}
	if got := report.Format(ValidationReportFormatText); got != "" {
		t.Fatalf("expected empty text output, got %q", got)
	}
}

func TestValidateSingleMark(t *testing.T) {
	marks := makeTestMarks(t, 1, ProvenanceMarkResolutionLow, "test", false)
	report := Validate(marks)
	if got, want := report.Format(ValidationReportFormatJSONPretty), strings.TrimSpace(`{
  "marks": [
    "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"
  ],
  "chains": [
    {
      "chain_id": "b16a7cbd",
      "has_genesis": true,
      "marks": [
        "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba"
      ],
      "sequences": [
        {
          "start_seq": 0,
          "end_seq": 0,
          "marks": [
            {
              "mark": "ur:provenance/lfaegdpaimkerydihsaedetiimmttpgdmocfdpbnhlasba",
              "issues": []
            }
          ]
        }
      ]
    }
  ]
}`); got != want {
		t.Fatalf("pretty JSON mismatch:\n%s\nwant:\n%s", got, want)
	}
	if got := report.Format(ValidationReportFormatText); got != "" {
		t.Fatalf("expected empty text output, got %q", got)
	}
}

func TestValidateMultipleChains(t *testing.T) {
	marks1 := makeTestMarks(t, 3, ProvenanceMarkResolutionLow, "alice", false)
	marks2 := makeTestMarks(t, 3, ProvenanceMarkResolutionLow, "bob", false)
	all := append(marks1, marks2...)
	report := Validate(all)
	if got, want := report.Format(ValidationReportFormatText), strings.TrimSpace(`Total marks: 6
Chains: 2

Chain 1: 7a9c3f5e
  0: 0d6e0afd (genesis mark)
  1: 6cd504e7
  2: dc07895c

Chain 2: a33e10de
  0: c2a985ff (genesis mark)
  1: 5567cd24
  2: f759ad4c`); got != want {
		t.Fatalf("text report mismatch:\n%s\nwant:\n%s", got, want)
	}
}

func TestValidateMissingGenesis(t *testing.T) {
	marks := makeTestMarks(t, 5, ProvenanceMarkResolutionLow, "test", false)
	report := Validate(marks[1:])
	if got, want := report.Format(ValidationReportFormatText), strings.TrimSpace(`Total marks: 4
Chains: 1

Chain 1: b16a7cbd
  Warning: No genesis mark found
  1: 1b806d6c
  2: b292f357
  3: 761a5e74
  4: 42d12de5`); got != want {
		t.Fatalf("text report mismatch:\n%s\nwant:\n%s", got, want)
	}
}

func TestValidateSequenceGap(t *testing.T) {
	marks := makeTestMarks(t, 5, ProvenanceMarkResolutionLow, "test", false)
	withGap := []ProvenanceMark{marks[0], marks[1], marks[3], marks[4]}
	report := Validate(withGap)
	if got, want := report.Format(ValidationReportFormatText), strings.TrimSpace(`Total marks: 4
Chains: 1

Chain 1: b16a7cbd
  0: f057c8c4 (genesis mark)
  1: 1b806d6c
  3: 761a5e74 (gap: 2 missing)
  4: 42d12de5`); got != want {
		t.Fatalf("text report mismatch:\n%s\nwant:\n%s", got, want)
	}
}

func TestValidateOutOfOrderSortsBySequence(t *testing.T) {
	marks := makeTestMarks(t, 5, ProvenanceMarkResolutionLow, "test", false)
	outOfOrder := []ProvenanceMark{marks[0], marks[1], marks[3], marks[2], marks[4]}
	report := Validate(outOfOrder)
	if got := report.Format(ValidationReportFormatText); got != "" {
		t.Fatalf("expected empty text output after sequence sort, got %q", got)
	}
}
