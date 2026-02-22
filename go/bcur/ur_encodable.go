package bcur

import dcbor "github.com/nickel-blockchaincommons/dcbor-go"

// UREncodable is the interface for types that can be encoded to a UR.
type UREncodable interface {
	dcbor.CBORTaggedEncodable
}

// ToUR converts a UREncodable to a UR.
func ToUR(obj UREncodable) *UR {
	tags := obj.CBORTags()
	if len(tags) == 0 {
		panic("UREncodable must have at least one CBOR tag")
	}
	tag := tags[0]
	name, ok := tag.Name()
	if !ok {
		panic("CBOR tag must have a name. Did you call RegisterTags()?")
	}
	ur, err := NewUR(name, obj.UntaggedCBOR())
	if err != nil {
		panic("failed to create UR: " + err.Error())
	}
	return ur
}

// ToURString converts a UREncodable to a UR string.
func ToURString(obj UREncodable) string {
	return ToUR(obj).URString()
}

// DecodeUR decodes a UR into a value of type T, verifying the UR type against the provided tags.
func DecodeUR[T any](ur *UR, tags []dcbor.Tag, decode dcbor.CBORDecodeFunc[T]) (T, error) {
	if len(tags) > 0 {
		name, ok := tags[0].Name()
		if ok {
			if err := ur.CheckType(name); err != nil {
				var zero T
				return zero, err
			}
		}
	}
	return decode(ur.CBOR())
}

// DecodeURString decodes a UR string into a value of type T.
func DecodeURString[T any](urString string, tags []dcbor.Tag, decode dcbor.CBORDecodeFunc[T]) (T, error) {
	ur, err := FromURString(urString)
	if err != nil {
		var zero T
		return zero, err
	}
	return DecodeUR(ur, tags, decode)
}
