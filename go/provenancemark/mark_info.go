package provenancemark

import (
	"encoding/json"
	"strings"

	bcur "github.com/nickel-blockchaincommons/bcur-go"
)

// ProvenanceMarkInfo provides derived display information for a mark.
type ProvenanceMarkInfo struct {
	ur        *bcur.UR
	bytewords string
	bytemoji  string
	comment   string
	mark      ProvenanceMark
}

// NewProvenanceMarkInfo builds display information for the given mark.
func NewProvenanceMarkInfo(mark ProvenanceMark, comment string) ProvenanceMarkInfo {
	return ProvenanceMarkInfo{
		ur:        mark.ToUR(),
		bytewords: mark.IDBytewords(4, true),
		bytemoji:  mark.IDBytemoji(4, true),
		comment:   comment,
		mark:      mark,
	}
}

// Mark returns the underlying mark.
func (i ProvenanceMarkInfo) Mark() ProvenanceMark {
	return i.mark
}

// UR returns the UR representation.
func (i ProvenanceMarkInfo) UR() *bcur.UR {
	if i.ur == nil {
		return nil
	}
	return i.mark.ToUR()
}

// Bytewords returns the mark's prefixed bytewords identifier.
func (i ProvenanceMarkInfo) Bytewords() string {
	return i.bytewords
}

// Bytemoji returns the mark's prefixed bytemoji identifier.
func (i ProvenanceMarkInfo) Bytemoji() string {
	return i.bytemoji
}

// Comment returns the optional comment.
func (i ProvenanceMarkInfo) Comment() string {
	return i.comment
}

// MarkdownSummary returns the markdown summary block.
func (i ProvenanceMarkInfo) MarkdownSummary() string {
	lines := []string{
		"---",
		"",
		i.mark.Date().String(),
		"",
		"#### " + i.ur.String(),
		"",
		"#### `" + i.bytewords + "`",
		"",
		i.bytemoji,
		"",
	}
	if i.comment != "" {
		lines = append(lines, i.comment, "")
	}
	return strings.Join(lines, "\n")
}

type provenanceMarkInfoJSON struct {
	UR        string         `json:"ur"`
	Bytewords string         `json:"bytewords"`
	Bytemoji  string         `json:"bytemoji"`
	Comment   string         `json:"comment,omitempty"`
	Mark      ProvenanceMark `json:"mark"`
}

// MarshalJSON encodes the mark info in its public JSON form.
func (i ProvenanceMarkInfo) MarshalJSON() ([]byte, error) {
	return json.Marshal(provenanceMarkInfoJSON{
		UR:        i.ur.String(),
		Bytewords: i.bytewords,
		Bytemoji:  i.bytemoji,
		Comment:   i.comment,
		Mark:      i.mark,
	})
}

// UnmarshalJSON decodes the mark info from its public JSON form.
func (i *ProvenanceMarkInfo) UnmarshalJSON(data []byte) error {
	var payload provenanceMarkInfoJSON
	if err := json.Unmarshal(data, &payload); err != nil {
		return err
	}
	ur, err := DeserializeUR(payload.UR)
	if err != nil {
		return err
	}
	mark, err := ProvenanceMarkFromUR(ur)
	if err != nil {
		return err
	}
	*i = ProvenanceMarkInfo{
		ur:        ur,
		bytewords: payload.Bytewords,
		bytemoji:  payload.Bytemoji,
		comment:   payload.Comment,
		mark:      mark,
	}
	return nil
}

var _ json.Marshaler = ProvenanceMarkInfo{}
var _ json.Unmarshaler = (*ProvenanceMarkInfo)(nil)
