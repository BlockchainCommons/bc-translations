package provenancemark

import (
	"encoding/json"
	"testing"
)

func TestGeneratorJSONAndEnvelopeRoundTrip(t *testing.T) {
	generator := NewProvenanceMarkGeneratorWithPassphrase(ProvenanceMarkResolutionMedium, "Wolf")
	generator.Next(mustDate(t, 2023, 6, 20, 12, 0, 0), nil)
	generator.Next(mustDate(t, 2023, 6, 21, 12, 0, 0), "Lorem ipsum sit dolor amet.")

	payload, err := json.Marshal(generator)
	if err != nil {
		t.Fatalf("json.Marshal failed: %v", err)
	}
	var decodedJSON ProvenanceMarkGenerator
	if err := json.Unmarshal(payload, &decodedJSON); err != nil {
		t.Fatalf("json.Unmarshal failed: %v", err)
	}
	if !generator.Equal(decodedJSON) {
		t.Fatal("generator JSON round-trip mismatch")
	}

	envelope := generator.ToEnvelope()
	decodedEnvelope, err := ProvenanceMarkGeneratorFromEnvelope(envelope)
	if err != nil {
		t.Fatalf("ProvenanceMarkGeneratorFromEnvelope failed: %v", err)
	}
	if !generator.Equal(decodedEnvelope) {
		t.Fatal("generator envelope round-trip mismatch")
	}
}
