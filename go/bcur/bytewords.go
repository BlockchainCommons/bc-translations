package bcur

import (
	"bytes"
	"encoding/binary"
	"strings"
)

// BytewordsStyle represents the encoding style for bytewords.
type BytewordsStyle int

const (
	// BytewordsStandard uses full 4-letter words separated by spaces.
	BytewordsStandard BytewordsStyle = iota
	// BytewordsURI uses full 4-letter words separated by dashes.
	BytewordsURI
	// BytewordsMinimal uses 2-letter abbreviations concatenated without separators.
	BytewordsMinimal
)

// BytewordsEncode encodes data as a bytewords string with CRC32 appended.
func BytewordsEncode(data []byte, style BytewordsStyle) string {
	checksum := crc32Checksum(data)
	var checksumBytes [4]byte
	binary.BigEndian.PutUint32(checksumBytes[:], checksum)

	allBytes := make([]byte, len(data)+4)
	copy(allBytes, data)
	copy(allBytes[len(data):], checksumBytes[:])

	var words []string
	switch style {
	case BytewordsStandard, BytewordsURI:
		for _, b := range allBytes {
			words = append(words, Bytewords[b])
		}
	case BytewordsMinimal:
		for _, b := range allBytes {
			words = append(words, minimals[b])
		}
	}

	switch style {
	case BytewordsStandard:
		return strings.Join(words, " ")
	case BytewordsURI:
		return strings.Join(words, "-")
	default: // Minimal
		return strings.Join(words, "")
	}
}

// BytewordsDecode decodes a bytewords string back to bytes, verifying the CRC32 checksum.
func BytewordsDecode(encoded string, style BytewordsStyle) ([]byte, error) {
	for _, r := range encoded {
		if r > 127 {
			return nil, ErrNonASCII
		}
	}

	switch style {
	case BytewordsMinimal:
		return decodeMinimal(encoded)
	default:
		var separator byte
		if style == BytewordsStandard {
			separator = ' '
		} else {
			separator = '-'
		}
		return decodeFromWords(encoded, separator)
	}
}

func decodeFromWords(encoded string, separator byte) ([]byte, error) {
	if len(encoded) == 0 {
		return nil, ErrInvalidWord
	}
	words := strings.Split(encoded, string(separator))
	decoded := make([]byte, len(words))
	for i, word := range words {
		idx, ok := wordToIndex[word]
		if !ok {
			return nil, ErrInvalidWord
		}
		decoded[i] = idx
	}
	return stripChecksum(decoded)
}

func decodeMinimal(encoded string) ([]byte, error) {
	if len(encoded)%2 != 0 {
		return nil, ErrInvalidLength
	}
	count := len(encoded) / 2
	decoded := make([]byte, count)
	for i := 0; i < count; i++ {
		code := encoded[i*2 : i*2+2]
		idx, ok := minimalToIndex[code]
		if !ok {
			return nil, ErrInvalidWord
		}
		decoded[i] = idx
	}
	return stripChecksum(decoded)
}

func stripChecksum(data []byte) ([]byte, error) {
	if len(data) < 4 {
		return nil, ErrInvalidChecksum
	}
	payload := data[:len(data)-4]
	checksumBytes := data[len(data)-4:]

	expected := crc32Checksum(payload)
	var expectedBytes [4]byte
	binary.BigEndian.PutUint32(expectedBytes[:], expected)

	if !bytes.Equal(checksumBytes, expectedBytes[:]) {
		return nil, ErrInvalidChecksum
	}

	result := make([]byte, len(payload))
	copy(result, payload)
	return result, nil
}

// EncodeToWords encodes raw bytes as space-separated bytewords without a CRC suffix.
func EncodeToWords(data []byte) string {
	words := make([]string, len(data))
	for i, b := range data {
		words[i] = Bytewords[b]
	}
	return strings.Join(words, " ")
}

// EncodeToBytemojis encodes raw bytes as space-separated bytemojis without a CRC suffix.
func EncodeToBytemojis(data []byte) string {
	words := make([]string, len(data))
	for i, b := range data {
		words[i] = Bytemojis[b]
	}
	return strings.Join(words, " ")
}

// EncodeToMinimalBytewords encodes raw bytes as concatenated minimal bytewords without a CRC suffix.
func EncodeToMinimalBytewords(data []byte) string {
	var builder strings.Builder
	builder.Grow(len(data) * 2)
	for _, b := range data {
		builder.WriteString(minimals[b])
	}
	return builder.String()
}

// BytewordsIdentifier returns a 4-byte identifier as space-separated bytewords.
func BytewordsIdentifier(data [4]byte) string {
	return EncodeToWords(data[:])
}

// BytemojiIdentifier returns a 4-byte identifier as space-separated bytemojis.
func BytemojiIdentifier(data [4]byte) string {
	return EncodeToBytemojis(data[:])
}
