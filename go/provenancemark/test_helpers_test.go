package provenancemark

import (
	"net/url"
	"testing"
	"time"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

func mustDate(t *testing.T, year, month, day, hour, minute, second int) dcbor.Date {
	t.Helper()
	return dcbor.DateFromDatetime(time.Date(year, time.Month(month), day, hour, minute, second, 0, time.UTC))
}

func mustParseURL(t *testing.T, raw string) *url.URL {
	t.Helper()
	parsed, err := url.Parse(raw)
	if err != nil {
		t.Fatalf("url.Parse failed: %v", err)
	}
	return parsed
}

func makeTestMarks(t *testing.T, count int, res ProvenanceMarkResolution, passphrase string, withInfo bool) []ProvenanceMark {
	t.Helper()
	generator := NewProvenanceMarkGeneratorWithPassphrase(res, passphrase)
	marks := make([]ProvenanceMark, 0, count)
	for i := 0; i < count; i++ {
		var info any
		if withInfo {
			info = "Lorem ipsum sit dolor amet."
		}
		mark := generator.Next(mustDate(t, 2023, 6, 20+i, 12, 0, 0), info)
		marks = append(marks, mark)
	}
	return marks
}
