package dcbor

import "bytes"

// CBOREncodable is implemented by values that can convert themselves to CBOR.
type CBOREncodable interface {
	ToCBOR() CBOR
}

// CBORDecodable is a marker interface for values decodable from CBOR.
type CBORDecodable interface{}

// CBORCodable is a marker for values that are both encodable and decodable.
type CBORCodable interface {
	CBOREncodable
	CBORDecodable
}

// CBORTagged is implemented by types that provide one or more accepted tags.
type CBORTagged interface {
	CBORTags() []Tag
}

// CBORTaggedEncodable is implemented by types that encode tagged CBOR payloads.
type CBORTaggedEncodable interface {
	CBORTagged
	UntaggedCBOR() CBOR
}

// CBORTaggedDecodable is implemented by types that decode tagged CBOR payloads.
type CBORTaggedDecodable interface {
	CBORTagged
}

// CBORTaggedCodable combines tagged encode and decode marker contracts.
type CBORTaggedCodable interface {
	CBORTaggedEncodable
	CBORTaggedDecodable
}

// SortArrayByCBOREncoding sorts values by their deterministic CBOR encoding.
func SortArrayByCBOREncoding(values []CBOR) []CBOR {
	copied := make([]CBOR, len(values))
	for i, value := range values {
		copied[i] = value.Clone()
	}
	for i := 0; i < len(copied); i++ {
		for j := i + 1; j < len(copied); j++ {
			if bytes.Compare(copied[i].ToCBORData(), copied[j].ToCBORData()) > 0 {
				copied[i], copied[j] = copied[j], copied[i]
			}
		}
	}
	return copied
}

// CBORSortable is implemented by collections that can sort by CBOR byte order.
type CBORSortable interface {
	SortByCBOREncoding() []CBOR
}

// ToCBORData converts an encodable value directly to deterministic CBOR bytes.
func ToCBORData(value CBOREncodable) []byte {
	return value.ToCBOR().ToCBORData()
}

// TaggedCBOR builds a tagged CBOR value using the first preferred tag.
func TaggedCBOR(value CBORTaggedEncodable) (CBOR, error) {
	tags := value.CBORTags()
	if len(tags) == 0 {
		return CBOR{}, Errorf("no tags are available for tagged encoding")
	}
	return NewCBORTagged(tags[0], value.UntaggedCBOR()), nil
}

// TaggedCBORData returns deterministic CBOR bytes for tagged encoding.
func TaggedCBORData(value CBORTaggedEncodable) ([]byte, error) {
	cbor, err := TaggedCBOR(value)
	if err != nil {
		return nil, err
	}
	return cbor.ToCBORData(), nil
}

// DecodeTagged decodes a tagged CBOR value using accepted tags and an untagged decoder.
func DecodeTagged[T any](cbor CBOR, tags []Tag, decodeUntagged CBORDecodeFunc[T]) (T, error) {
	var zero T
	tag, value, err := cbor.TryIntoTaggedValue()
	if err != nil {
		return zero, err
	}
	if len(tags) == 0 {
		return zero, Errorf("no tags are available for tagged decoding")
	}
	for _, expected := range tags {
		if tag.Equal(expected) {
			return decodeUntagged(value)
		}
	}
	return zero, WrongTagError{Expected: tags[0], Actual: tag}
}

// DecodeTaggedData decodes binary tagged CBOR using accepted tags and an untagged decoder.
func DecodeTaggedData[T any](data []byte, tags []Tag, decodeUntagged CBORDecodeFunc[T]) (T, error) {
	var zero T
	cbor, err := TryFromData(data)
	if err != nil {
		return zero, err
	}
	return DecodeTagged(cbor, tags, decodeUntagged)
}

// DecodeUntaggedData decodes binary CBOR and then applies the provided decoder.
func DecodeUntaggedData[T any](data []byte, decode CBORDecodeFunc[T]) (T, error) {
	var zero T
	cbor, err := TryFromData(data)
	if err != nil {
		return zero, err
	}
	return decode(cbor)
}

// TryFromCBOR decodes a value from an existing CBOR instance.
func TryFromCBOR[T any](cbor CBOR, decode CBORDecodeFunc[T]) (T, error) {
	return decode(cbor.Clone())
}

// TryFromCBORData decodes a value from CBOR bytes.
func TryFromCBORData[T any](data []byte, decode CBORDecodeFunc[T]) (T, error) {
	return DecodeUntaggedData(data, decode)
}

// DecodeTaggedFor decodes tagged CBOR using tag values supplied by a CBORTagged provider.
func DecodeTaggedFor[T any](cbor CBOR, tagged CBORTagged, decodeUntagged CBORDecodeFunc[T]) (T, error) {
	return DecodeTagged(cbor, tagged.CBORTags(), decodeUntagged)
}

// DecodeTaggedDataFor decodes tagged CBOR bytes using tags supplied by a CBORTagged provider.
func DecodeTaggedDataFor[T any](data []byte, tagged CBORTagged, decodeUntagged CBORDecodeFunc[T]) (T, error) {
	return DecodeTaggedData(data, tagged.CBORTags(), decodeUntagged)
}
