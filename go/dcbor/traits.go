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
