package provenancemark

import (
	"encoding/hex"
	"encoding/json"
	"errors"
	"fmt"
	"sort"
	"strings"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// ValidationReportFormat selects how validation reports are rendered.
type ValidationReportFormat string

const (
	ValidationReportFormatText        ValidationReportFormat = "Text"
	ValidationReportFormatJSONCompact ValidationReportFormat = "JsonCompact"
	ValidationReportFormatJSONPretty  ValidationReportFormat = "JsonPretty"
)

// ValidationIssueType identifies a validation issue category.
type ValidationIssueType string

const (
	ValidationIssueHashMismatch      ValidationIssueType = "HashMismatch"
	ValidationIssueKeyMismatch       ValidationIssueType = "KeyMismatch"
	ValidationIssueSequenceGap       ValidationIssueType = "SequenceGap"
	ValidationIssueDateOrdering      ValidationIssueType = "DateOrdering"
	ValidationIssueNonGenesisAtZero  ValidationIssueType = "NonGenesisAtZero"
	ValidationIssueInvalidGenesisKey ValidationIssueType = "InvalidGenesisKey"
)

// ValidationIssue is a concrete validation problem found in a mark sequence.
type ValidationIssue struct {
	Type        ValidationIssueType
	Expected    []byte
	Actual      []byte
	ExpectedSeq uint32
	ActualSeq   uint32
	Previous    dcbor.Date
	Next        dcbor.Date
}

// NewHashMismatchIssue constructs a hash mismatch issue.
func NewHashMismatchIssue(expected []byte, actual []byte) ValidationIssue {
	return ValidationIssue{
		Type:     ValidationIssueHashMismatch,
		Expected: cloneBytes(expected),
		Actual:   cloneBytes(actual),
	}
}

// NewKeyMismatchIssue constructs a generic key mismatch issue.
func NewKeyMismatchIssue() ValidationIssue {
	return ValidationIssue{Type: ValidationIssueKeyMismatch}
}

// NewSequenceGapIssue constructs a sequence gap issue.
func NewSequenceGapIssue(expected uint32, actual uint32) ValidationIssue {
	return ValidationIssue{
		Type:        ValidationIssueSequenceGap,
		ExpectedSeq: expected,
		ActualSeq:   actual,
	}
}

// NewDateOrderingIssue constructs a date ordering issue.
func NewDateOrderingIssue(previous dcbor.Date, next dcbor.Date) ValidationIssue {
	return ValidationIssue{
		Type:     ValidationIssueDateOrdering,
		Previous: previous,
		Next:     next,
	}
}

// NewNonGenesisAtZeroIssue constructs a non-genesis-at-zero issue.
func NewNonGenesisAtZeroIssue() ValidationIssue {
	return ValidationIssue{Type: ValidationIssueNonGenesisAtZero}
}

// NewInvalidGenesisKeyIssue constructs an invalid genesis key issue.
func NewInvalidGenesisKeyIssue() ValidationIssue {
	return ValidationIssue{Type: ValidationIssueInvalidGenesisKey}
}

// SequenceGapExpected returns the parsed expected sequence number.
func (i ValidationIssue) SequenceGapExpected() uint32 {
	return i.ExpectedSeq
}

// SequenceGapActual returns the parsed actual sequence number.
func (i ValidationIssue) SequenceGapActual() uint32 {
	return i.ActualSeq
}

// Error implements error.
func (i ValidationIssue) Error() string {
	switch i.Type {
	case ValidationIssueHashMismatch:
		return fmt.Sprintf("hash mismatch: expected %s, got %s", hex.EncodeToString(i.Expected), hex.EncodeToString(i.Actual))
	case ValidationIssueKeyMismatch:
		return "key mismatch: current hash was not generated from next key"
	case ValidationIssueSequenceGap:
		return fmt.Sprintf("sequence number gap: expected %d, got %d", i.SequenceGapExpected(), i.SequenceGapActual())
	case ValidationIssueDateOrdering:
		return fmt.Sprintf("date must be equal or later: previous is %s, next is %s", i.Previous.String(), i.Next.String())
	case ValidationIssueNonGenesisAtZero:
		return "non-genesis mark at sequence 0"
	case ValidationIssueInvalidGenesisKey:
		return "genesis mark must have key equal to chain_id"
	default:
		return string(i.Type)
	}
}

// MarshalJSON encodes the issue using the Rust enum-like JSON surface.
func (i ValidationIssue) MarshalJSON() ([]byte, error) {
	switch i.Type {
	case ValidationIssueHashMismatch:
		return json.Marshal(struct {
			Type string `json:"type"`
			Data any    `json:"data"`
		}{
			Type: string(i.Type),
			Data: struct {
				Expected string `json:"expected"`
				Actual   string `json:"actual"`
			}{
				Expected: hex.EncodeToString(i.Expected),
				Actual:   hex.EncodeToString(i.Actual),
			},
		})
	case ValidationIssueSequenceGap:
		return json.Marshal(struct {
			Type string `json:"type"`
			Data any    `json:"data"`
		}{
			Type: string(i.Type),
			Data: struct {
				Expected uint32 `json:"expected"`
				Actual   uint32 `json:"actual"`
			}{
				Expected: i.SequenceGapExpected(),
				Actual:   i.SequenceGapActual(),
			},
		})
	case ValidationIssueDateOrdering:
		return json.Marshal(struct {
			Type string `json:"type"`
			Data any    `json:"data"`
		}{
			Type: string(i.Type),
			Data: struct {
				Previous string `json:"previous"`
				Next     string `json:"next"`
			}{
				Previous: i.Previous.String(),
				Next:     i.Next.String(),
			},
		})
	default:
		return json.Marshal(struct {
			Type string `json:"type"`
		}{
			Type: string(i.Type),
		})
	}
}

// FlaggedMark pairs a mark with any issues found during validation.
type FlaggedMark struct {
	mark   ProvenanceMark
	issues []ValidationIssue
}

// NewFlaggedMark creates a flagged mark with no issues.
func NewFlaggedMark(mark ProvenanceMark) FlaggedMark {
	return FlaggedMark{mark: mark, issues: []ValidationIssue{}}
}

// NewFlaggedMarkWithIssue creates a flagged mark with a single issue.
func NewFlaggedMarkWithIssue(mark ProvenanceMark, issue ValidationIssue) FlaggedMark {
	return FlaggedMark{mark: mark, issues: []ValidationIssue{issue}}
}

// Mark returns the mark.
func (f FlaggedMark) Mark() ProvenanceMark {
	return f.mark
}

// Issues returns the validation issues.
func (f FlaggedMark) Issues() []ValidationIssue {
	cloned := make([]ValidationIssue, len(f.issues))
	copy(cloned, f.issues)
	return cloned
}

// MarshalJSON encodes the flagged mark in its public JSON form.
func (f FlaggedMark) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Mark   string            `json:"mark"`
		Issues []ValidationIssue `json:"issues"`
	}{
		Mark:   f.mark.URString(),
		Issues: f.issues,
	})
}

// SequenceReport describes one contiguous sequence within a chain.
type SequenceReport struct {
	startSeq uint32
	endSeq   uint32
	marks    []FlaggedMark
}

// StartSeq returns the starting sequence number.
func (r SequenceReport) StartSeq() uint32 {
	return r.startSeq
}

// EndSeq returns the ending sequence number.
func (r SequenceReport) EndSeq() uint32 {
	return r.endSeq
}

// Marks returns the flagged marks in the sequence.
func (r SequenceReport) Marks() []FlaggedMark {
	cloned := make([]FlaggedMark, len(r.marks))
	copy(cloned, r.marks)
	return cloned
}

// MarshalJSON encodes the sequence report in its public JSON form.
func (r SequenceReport) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		StartSeq uint32        `json:"start_seq"`
		EndSeq   uint32        `json:"end_seq"`
		Marks    []FlaggedMark `json:"marks"`
	}{
		StartSeq: r.startSeq,
		EndSeq:   r.endSeq,
		Marks:    r.marks,
	})
}

// ChainReport groups marks that share a chain ID.
type ChainReport struct {
	chainID    []byte
	hasGenesis bool
	marks      []ProvenanceMark
	sequences  []SequenceReport
}

// ChainID returns the chain identifier.
func (r ChainReport) ChainID() []byte {
	return cloneBytes(r.chainID)
}

// HasGenesis reports whether the chain has a valid genesis mark.
func (r ChainReport) HasGenesis() bool {
	return r.hasGenesis
}

// Marks returns the chain's marks.
func (r ChainReport) Marks() []ProvenanceMark {
	return cloneMarks(r.marks)
}

// Sequences returns the chain's contiguous sequences.
func (r ChainReport) Sequences() []SequenceReport {
	cloned := make([]SequenceReport, len(r.sequences))
	copy(cloned, r.sequences)
	return cloned
}

// ChainIDHex returns the chain ID as hex.
func (r ChainReport) ChainIDHex() string {
	return hex.EncodeToString(r.chainID)
}

// MarshalJSON encodes the chain report in its public JSON form.
func (r ChainReport) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		ChainID    string           `json:"chain_id"`
		HasGenesis bool             `json:"has_genesis"`
		Marks      []string         `json:"marks"`
		Sequences  []SequenceReport `json:"sequences"`
	}{
		ChainID:    hex.EncodeToString(r.chainID),
		HasGenesis: r.hasGenesis,
		Marks:      provenanceMarksToURStrings(r.marks),
		Sequences:  r.sequences,
	})
}

// ValidationReport is the full validation result for a set of marks.
type ValidationReport struct {
	marks  []ProvenanceMark
	chains []ChainReport
}

// Validate validates a collection of provenance marks.
func Validate(marks []ProvenanceMark) ValidationReport {
	seen := make(map[string]struct{})
	deduplicated := make([]ProvenanceMark, 0, len(marks))
	for _, mark := range marks {
		key := fmt.Sprintf("%d:%x", mark.Res(), mark.Message())
		if _, ok := seen[key]; ok {
			continue
		}
		seen[key] = struct{}{}
		deduplicated = append(deduplicated, mark)
	}

	chainBins := make(map[string][]ProvenanceMark)
	for _, mark := range deduplicated {
		key := hex.EncodeToString(mark.ChainID())
		chainBins[key] = append(chainBins[key], mark)
	}

	chains := make([]ChainReport, 0, len(chainBins))
	for _, chainMarks := range chainBins {
		sort.Slice(chainMarks, func(i, j int) bool {
			return chainMarks[i].Seq() < chainMarks[j].Seq()
		})
		hasGenesis := len(chainMarks) > 0 && chainMarks[0].Seq() == 0 && chainMarks[0].IsGenesis()
		chains = append(chains, ChainReport{
			chainID:    chainMarks[0].ChainID(),
			hasGenesis: hasGenesis,
			marks:      cloneMarks(chainMarks),
			sequences:  buildSequenceBins(chainMarks),
		})
	}

	sort.Slice(chains, func(i, j int) bool {
		return compareBytes(chains[i].chainID, chains[j].chainID) < 0
	})

	return ValidationReport{
		marks:  deduplicated,
		chains: chains,
	}
}

func buildSequenceBins(marks []ProvenanceMark) []SequenceReport {
	sequences := make([]SequenceReport, 0)
	current := make([]FlaggedMark, 0)

	for i, mark := range marks {
		if i == 0 {
			current = append(current, NewFlaggedMark(mark))
			continue
		}

		prev := marks[i-1]
		if err := prev.PrecedesOpt(mark); err == nil {
			current = append(current, NewFlaggedMark(mark))
			continue
		} else {
			if len(current) > 0 {
				sequences = append(sequences, createSequenceReport(current))
			}
			issue := NewKeyMismatchIssue()
			var validationIssue ValidationIssue
			if errors.As(err, &validationIssue) {
				issue = validationIssue
			}
			current = []FlaggedMark{NewFlaggedMarkWithIssue(mark, issue)}
		}
	}

	if len(current) > 0 {
		sequences = append(sequences, createSequenceReport(current))
	}

	return sequences
}

func createSequenceReport(marks []FlaggedMark) SequenceReport {
	startSeq := uint32(0)
	endSeq := uint32(0)
	if len(marks) > 0 {
		startSeq = marks[0].mark.Seq()
		endSeq = marks[len(marks)-1].mark.Seq()
	}
	cloned := make([]FlaggedMark, len(marks))
	copy(cloned, marks)
	return SequenceReport{
		startSeq: startSeq,
		endSeq:   endSeq,
		marks:    cloned,
	}
}

// Marks returns the deduplicated marks included in the report.
func (r ValidationReport) Marks() []ProvenanceMark {
	return cloneMarks(r.marks)
}

// Chains returns the chain reports.
func (r ValidationReport) Chains() []ChainReport {
	cloned := make([]ChainReport, len(r.chains))
	copy(cloned, r.chains)
	return cloned
}

// Format formats the report as text or JSON.
func (r ValidationReport) Format(format ValidationReportFormat) string {
	switch format {
	case ValidationReportFormatText, "":
		return r.formatText()
	case ValidationReportFormatJSONCompact:
		data, err := json.Marshal(r)
		if err != nil {
			return ""
		}
		return string(data)
	case ValidationReportFormatJSONPretty:
		data, err := json.MarshalIndent(r, "", "  ")
		if err != nil {
			return ""
		}
		return string(data)
	default:
		return r.formatText()
	}
}

func (r ValidationReport) formatText() string {
	if !r.isInteresting() {
		return ""
	}

	lines := []string{
		fmt.Sprintf("Total marks: %d", len(r.marks)),
		fmt.Sprintf("Chains: %d", len(r.chains)),
		"",
	}

	for chainIndex, chain := range r.chains {
		chainIDHex := chain.ChainIDHex()
		shortChainID := chainIDHex
		if len(shortChainID) > 8 {
			shortChainID = shortChainID[:8]
		}
		lines = append(lines, fmt.Sprintf("Chain %d: %s", chainIndex+1, shortChainID))
		if !chain.hasGenesis {
			lines = append(lines, "  Warning: No genesis mark found")
		}
		for _, sequence := range chain.sequences {
			for _, flagged := range sequence.marks {
				mark := flagged.mark
				shortID := mark.IDHex()[:8]
				annotations := make([]string, 0)
				if mark.IsGenesis() {
					annotations = append(annotations, "genesis mark")
				}
				for _, issue := range flagged.issues {
					switch issue.Type {
					case ValidationIssueSequenceGap:
						annotations = append(annotations, fmt.Sprintf("gap: %d missing", issue.SequenceGapExpected()))
					case ValidationIssueDateOrdering:
						annotations = append(annotations, fmt.Sprintf("date %s < %s", issue.Previous.String(), issue.Next.String()))
					case ValidationIssueHashMismatch:
						annotations = append(annotations, "hash mismatch")
					case ValidationIssueKeyMismatch:
						annotations = append(annotations, "key mismatch")
					case ValidationIssueNonGenesisAtZero:
						annotations = append(annotations, "non-genesis at seq 0")
					case ValidationIssueInvalidGenesisKey:
						annotations = append(annotations, "invalid genesis key")
					}
				}
				if len(annotations) == 0 {
					lines = append(lines, fmt.Sprintf("  %d: %s", mark.Seq(), shortID))
				} else {
					lines = append(lines, fmt.Sprintf("  %d: %s (%s)", mark.Seq(), shortID, strings.Join(annotations, ", ")))
				}
			}
		}
		lines = append(lines, "")
	}

	return strings.TrimRight(strings.Join(lines, "\n"), "\n")
}

func (r ValidationReport) isInteresting() bool {
	if len(r.chains) == 0 {
		return false
	}
	for _, chain := range r.chains {
		if !chain.hasGenesis {
			return true
		}
	}
	if len(r.chains) == 1 && len(r.chains[0].sequences) == 1 {
		sequence := r.chains[0].sequences[0]
		allClear := true
		for _, mark := range sequence.marks {
			if len(mark.issues) != 0 {
				allClear = false
				break
			}
		}
		if allClear {
			return false
		}
	}
	return true
}

// HasIssues reports whether the validation report contains any issues.
func (r ValidationReport) HasIssues() bool {
	for _, chain := range r.chains {
		if !chain.hasGenesis {
			return true
		}
		for _, sequence := range chain.sequences {
			for _, mark := range sequence.marks {
				if len(mark.issues) > 0 {
					return true
				}
			}
		}
	}
	if len(r.chains) > 1 {
		return true
	}
	return len(r.chains) == 1 && len(r.chains[0].sequences) > 1
}

// MarshalJSON encodes the validation report in its public JSON form.
func (r ValidationReport) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Marks  []string      `json:"marks"`
		Chains []ChainReport `json:"chains"`
	}{
		Marks:  provenanceMarksToURStrings(r.marks),
		Chains: r.chains,
	})
}

func provenanceMarksToURStrings(marks []ProvenanceMark) []string {
	result := make([]string, len(marks))
	for i, mark := range marks {
		result[i] = mark.URString()
	}
	return result
}
