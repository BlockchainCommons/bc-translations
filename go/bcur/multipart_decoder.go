package bcur

import (
	"strings"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// MultipartDecoder receives and decodes multipart UR strings.
type MultipartDecoder struct {
	urType  *URType
	decoder *fountainDecoder
}

// NewMultipartDecoder creates a new multipart decoder.
func NewMultipartDecoder() *MultipartDecoder {
	return &MultipartDecoder{
		decoder: newFountainDecoder(),
	}
}

// Receive processes a multipart UR string.
func (d *MultipartDecoder) Receive(value string) error {
	decodedType, err := decodeURType(value)
	if err != nil {
		return err
	}

	if d.urType != nil {
		if d.urType.value != decodedType.value {
			return &UnexpectedTypeError{Expected: d.urType.value, Found: decodedType.value}
		}
	} else {
		d.urType = &decodedType
	}

	// Decode the UR string as multi-part
	kind, data, err := urDecode(value)
	if err != nil {
		return err
	}
	if kind != urKindMultiPart {
		return ErrNotMultiPart
	}

	// Decode the fountain part from CBOR
	part, err := fountainPartFromCBOR(data)
	if err != nil {
		return err
	}

	_, err = d.decoder.receive(part)
	return err
}

// IsComplete returns true if the message has been fully decoded.
func (d *MultipartDecoder) IsComplete() bool {
	return d.decoder.complete()
}

// Message returns the decoded UR if complete.
func (d *MultipartDecoder) Message() (*UR, error) {
	data, err := d.decoder.message()
	if err != nil {
		return nil, err
	}
	if data == nil {
		return nil, nil
	}

	cbor, err := dcbor.TryFromData(data)
	if err != nil {
		return nil, err
	}
	if d.urType == nil {
		return nil, ErrTypeUnspecified
	}

	return NewUR(d.urType.String(), cbor)
}

// decodeURType extracts the UR type from a UR string without fully decoding it.
func decodeURType(urString string) (URType, error) {
	lower := strings.ToLower(urString)
	withoutScheme, ok := strings.CutPrefix(lower, "ur:")
	if !ok {
		return URType{}, ErrInvalidScheme
	}
	firstComponent, _, found := strings.Cut(withoutScheme, "/")
	if !found {
		return URType{}, ErrTypeUnspecified
	}
	return NewURType(firstComponent)
}
