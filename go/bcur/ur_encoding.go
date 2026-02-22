package bcur

import (
	"fmt"
	"strconv"
	"strings"
)

// urEncode encodes data as a single-part UR string.
func urEncode(data []byte, urType string) string {
	body := BytewordsEncode(data, BytewordsMinimal)
	return fmt.Sprintf("ur:%s/%s", urType, body)
}

// urKind indicates whether a UR is single or multi-part.
type urKind int

const (
	urKindSinglePart urKind = iota
	urKindMultiPart
)

// urDecode decodes a UR string into its kind and data.
func urDecode(value string) (urKind, []byte, error) {
	lower := strings.ToLower(value)
	stripScheme, ok := strings.CutPrefix(lower, "ur:")
	if !ok {
		return 0, nil, ErrInvalidScheme
	}

	typePart, rest, found := strings.Cut(stripScheme, "/")
	if !found {
		return 0, nil, ErrTypeUnspecified
	}

	// Validate type characters
	for _, c := range typePart {
		if !isURTypeChar(c) {
			return 0, nil, ErrInvalidCharacters
		}
	}

	// Check for multi-part: look for another "/" in rest
	if idx := strings.LastIndex(rest, "/"); idx >= 0 {
		// Multi-part: indices/payload
		indices := rest[:idx]
		payload := rest[idx+1:]

		// Parse indices as "n-m"
		parts := strings.SplitN(indices, "-", 2)
		if len(parts) != 2 {
			return 0, nil, ErrInvalidIndices
		}
		if _, err := strconv.ParseUint(parts[0], 10, 16); err != nil {
			return 0, nil, ErrInvalidIndices
		}
		if _, err := strconv.ParseUint(parts[1], 10, 16); err != nil {
			return 0, nil, ErrInvalidIndices
		}

		decoded, err := BytewordsDecode(payload, BytewordsMinimal)
		if err != nil {
			return 0, nil, err
		}
		return urKindMultiPart, decoded, nil
	}

	// Single-part
	decoded, err := BytewordsDecode(rest, BytewordsMinimal)
	if err != nil {
		return 0, nil, err
	}
	return urKindSinglePart, decoded, nil
}

// isURTypeChar returns true if the character is valid in a UR type string.
func isURTypeChar(c rune) bool {
	return (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-'
}

// isValidURType returns true if the string is a valid UR type.
func isValidURType(s string) bool {
	if len(s) == 0 {
		return false
	}
	for _, c := range s {
		if !isURTypeChar(c) {
			return false
		}
	}
	return true
}
