package dcbor

import "math/big"

// CBORDecodeFunc decodes a CBOR value into a concrete Go type.
type CBORDecodeFunc[T any] func(CBOR) (T, error)

func DecodeBool(c CBOR) (bool, error) {
	return c.TryIntoBool()
}

func DecodeSimpleValue(c CBOR) (Simple, error) {
	return c.TryIntoSimpleValue()
}

func DecodeText(c CBOR) (string, error) {
	return c.TryIntoText()
}

func DecodeByteString(c CBOR) ([]byte, error) {
	return c.TryIntoByteString()
}

func DecodeUInt64(c CBOR) (uint64, error) {
	return c.TryIntoUInt64()
}

func DecodeUInt32(c CBOR) (uint32, error) {
	return c.TryIntoUInt32()
}

func DecodeUInt16(c CBOR) (uint16, error) {
	return c.TryIntoUInt16()
}

func DecodeUInt8(c CBOR) (uint8, error) {
	return c.TryIntoUInt8()
}

func DecodeUInt(c CBOR) (uint, error) {
	return c.TryIntoUInt()
}

func DecodeInt64(c CBOR) (int64, error) {
	return c.TryIntoInt64()
}

func DecodeInt32(c CBOR) (int32, error) {
	return c.TryIntoInt32()
}

func DecodeInt16(c CBOR) (int16, error) {
	return c.TryIntoInt16()
}

func DecodeInt8(c CBOR) (int8, error) {
	return c.TryIntoInt8()
}

func DecodeInt(c CBOR) (int, error) {
	return c.TryIntoInt()
}

func DecodeFloat64(c CBOR) (float64, error) {
	return c.TryIntoFloat64()
}

func DecodeFloat32(c CBOR) (float32, error) {
	return c.TryIntoFloat32()
}

func DecodeFloat16(c CBOR) (Float16, error) {
	return c.TryIntoFloat16()
}

func DecodeBigInt(c CBOR) (*big.Int, error) {
	return c.TryIntoBigInt()
}

func DecodeBigUint(c CBOR) (*big.Int, error) {
	return c.TryIntoBigUint()
}

func DecodeTaggedValue(c CBOR) (Tag, CBOR, error) {
	return c.TryIntoTaggedValue()
}

func DecodeExpectedTaggedValue(c CBOR, expected Tag) (CBOR, error) {
	return c.TryIntoExpectedTaggedValue(expected)
}

func DecodeDate(c CBOR) (Date, error) {
	return c.TryIntoDate()
}

func DecodeArray[T any](c CBOR, decodeItem CBORDecodeFunc[T]) ([]T, error) {
	items, err := c.TryIntoArray()
	if err != nil {
		return nil, err
	}

	out := make([]T, 0, len(items))
	for _, item := range items {
		decoded, err := decodeItem(item)
		if err != nil {
			return nil, err
		}
		out = append(out, decoded)
	}
	return out, nil
}

func DecodeMap[K comparable, V any](
	c CBOR,
	decodeKey CBORDecodeFunc[K],
	decodeValue CBORDecodeFunc[V],
) (map[K]V, error) {
	m, err := c.TryIntoMap()
	if err != nil {
		return nil, err
	}

	out := make(map[K]V, m.Len())
	iter := m.Iter()
	for {
		keyCBOR, valueCBOR, ok := iter.Next()
		if !ok {
			break
		}
		key, err := decodeKey(keyCBOR)
		if err != nil {
			return nil, err
		}
		value, err := decodeValue(valueCBOR)
		if err != nil {
			return nil, err
		}
		out[key] = value
	}
	return out, nil
}

func DecodeSet[T comparable](c CBOR, decodeItem CBORDecodeFunc[T]) (map[T]struct{}, error) {
	items, err := DecodeArray(c, decodeItem)
	if err != nil {
		return nil, err
	}
	out := make(map[T]struct{}, len(items))
	for _, item := range items {
		out[item] = struct{}{}
	}
	return out, nil
}

func DecodeSetSlice[T any](c CBOR, decodeItem CBORDecodeFunc[T]) ([]T, error) {
	set, err := c.TryIntoSet()
	if err != nil {
		return nil, err
	}
	values := set.AsVec()
	out := make([]T, 0, len(values))
	for _, value := range values {
		decoded, err := decodeItem(value)
		if err != nil {
			return nil, err
		}
		out = append(out, decoded)
	}
	return out, nil
}
