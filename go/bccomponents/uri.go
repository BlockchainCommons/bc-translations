package bccomponents

import (
	"fmt"
	"net/url"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// URI is a Uniform Resource Identifier, validated against RFC 3986 via
// net/url.Parse.
type URI struct {
	uri string
}

// NewURI creates a new URI from a string, validating it via net/url.Parse.
func NewURI(uri string) (URI, error) {
	_, err := url.Parse(uri)
	if err != nil {
		return URI{}, errInvalidData("URI", "invalid URI format")
	}
	// url.Parse is very permissive; reject empty strings explicitly.
	if uri == "" {
		return URI{}, errInvalidData("URI", "empty URI")
	}
	return URI{uri: uri}, nil
}

// URIString returns the raw URI string.
func (u URI) URIString() string { return u.uri }

// String returns the URI string.
func (u URI) String() string { return u.uri }

// Equal reports whether two URIs are equal.
func (u URI) Equal(other URI) bool { return u.uri == other.uri }

// --- CBOR support ---

// URICBORTags returns the CBOR tags used for URI.
func URICBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagURI})
}

// CBORTags implements dcbor.CBORTagged.
func (u URI) CBORTags() []dcbor.Tag { return URICBORTags() }

// UntaggedCBOR implements dcbor.CBORTaggedEncodable.
func (u URI) UntaggedCBOR() dcbor.CBOR { return dcbor.MustFromAny(u.uri) }

// TaggedCBOR returns the tagged CBOR encoding of the URI.
func (u URI) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(u)
	return cbor
}

// ToCBOR implements dcbor.CBOREncodable.
func (u URI) ToCBOR() dcbor.CBOR { return u.TaggedCBOR() }

// DecodeURI decodes a URI from untagged CBOR.
func DecodeURI(cbor dcbor.CBOR) (URI, error) {
	s, err := cbor.TryIntoText()
	if err != nil {
		return URI{}, err
	}
	uri, err := NewURI(s)
	if err != nil {
		return URI{}, fmt.Errorf("bccomponents: %w", err)
	}
	return uri, nil
}

// DecodeTaggedURI decodes a URI from tagged CBOR.
func DecodeTaggedURI(cbor dcbor.CBOR) (URI, error) {
	return dcbor.DecodeTagged(cbor, URICBORTags(), DecodeURI)
}

// URIFromTaggedCBOR decodes a URI from tagged CBOR bytes.
func URIFromTaggedCBOR(data []byte) (URI, error) {
	return dcbor.DecodeTaggedData(data, URICBORTags(), DecodeURI)
}
