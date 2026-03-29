package provenancemark

import (
	"encoding/base64"
	"encoding/json"
	"fmt"

	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// SerializeBase64 encodes bytes as a base64 string.
func SerializeBase64(bytes []byte) string {
	return base64.StdEncoding.EncodeToString(bytes)
}

// DeserializeBase64 decodes a base64 string into bytes.
func DeserializeBase64(value string) ([]byte, error) {
	bytes, err := base64.StdEncoding.DecodeString(value)
	if err != nil {
		return nil, wrapBase64Error(err)
	}
	return bytes, nil
}

// ParseSeed parses a base64-encoded provenance seed.
func ParseSeed(value string) (ProvenanceSeed, error) {
	return ProvenanceSeedFromBase64(value)
}

// ParseDate parses an ISO-8601 date string.
func ParseDate(value string) (dcbor.Date, error) {
	date, err := dcbor.DateFromString(value)
	if err != nil {
		return dcbor.Date{}, err
	}
	return date, nil
}

// SerializeCBOR validates and base64-encodes CBOR bytes.
func SerializeCBOR(bytes []byte) (string, error) {
	if _, err := dcbor.TryFromData(bytes); err != nil {
		return "", wrapCBORError(err)
	}
	return SerializeBase64(bytes), nil
}

// DeserializeCBOR decodes base64 and validates the result as CBOR bytes.
func DeserializeCBOR(value string) ([]byte, error) {
	bytes, err := DeserializeBase64(value)
	if err != nil {
		return nil, err
	}
	if _, err := dcbor.TryFromData(bytes); err != nil {
		return nil, wrapCBORError(err)
	}
	return bytes, nil
}

// SerializeBlock base64-encodes a fixed 32-byte block.
func SerializeBlock(block [32]byte) string {
	return SerializeBase64(block[:])
}

// DeserializeBlock decodes a fixed 32-byte block from base64.
func DeserializeBlock(value string) ([32]byte, error) {
	bytes, err := DeserializeBase64(value)
	if err != nil {
		return [32]byte{}, err
	}
	if len(bytes) != 32 {
		return [32]byte{}, fmt.Errorf("seed length is %d, expected 32", len(bytes))
	}
	var result [32]byte
	copy(result[:], bytes)
	return result, nil
}

// SerializeISO8601 formats a dCBOR date as text.
func SerializeISO8601(date dcbor.Date) string {
	return date.String()
}

// DeserializeISO8601 parses a dCBOR date from text.
func DeserializeISO8601(value string) (dcbor.Date, error) {
	return ParseDate(value)
}

// SerializeUR returns the canonical UR string.
func SerializeUR(ur *bcur.UR) string {
	if ur == nil {
		return ""
	}
	return ur.String()
}

// DeserializeUR parses a UR string.
func DeserializeUR(value string) (*bcur.UR, error) {
	ur, err := bcur.FromURString(value)
	if err != nil {
		return nil, wrapBytewordsError(err)
	}
	return ur, nil
}

func marshalString(value string) ([]byte, error) {
	return json.Marshal(value)
}

func unmarshalString(data []byte) (string, error) {
	var value string
	if err := json.Unmarshal(data, &value); err != nil {
		return "", err
	}
	return value, nil
}
