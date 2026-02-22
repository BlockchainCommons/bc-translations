package bcur

import (
	"fmt"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// MultipartEncoder wraps a fountain encoder for producing multipart UR strings.
type MultipartEncoder struct {
	fountain *fountainEncoder
	urType   string
}

// NewMultipartEncoder creates a new multipart encoder for the given UR.
func NewMultipartEncoder(ur *UR, maxFragmentLen int) (*MultipartEncoder, error) {
	data := dcbor.ToCBORData(ur.cbor)
	enc, err := newFountainEncoder(data, maxFragmentLen)
	if err != nil {
		return nil, err
	}
	return &MultipartEncoder{
		fountain: enc,
		urType:   ur.URTypeStr(),
	}, nil
}

// NextPart returns the next multipart UR string.
func (e *MultipartEncoder) NextPart() (string, error) {
	part := e.fountain.nextPart()
	cbor := part.toCBOR()
	body := BytewordsEncode(cbor, BytewordsMinimal)
	return fmt.Sprintf("ur:%s/%s/%s", e.urType, part.sequenceID(), body), nil
}

// CurrentIndex returns the number of parts emitted so far.
func (e *MultipartEncoder) CurrentIndex() int {
	return e.fountain.currentSequence
}

// PartsCount returns the number of original fragments.
func (e *MultipartEncoder) PartsCount() int {
	return e.fountain.fragmentCount()
}
