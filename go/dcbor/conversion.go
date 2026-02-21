package dcbor

import "math/big"

// CBORDecodeFunc decodes a CBOR value into a concrete Go type.
type CBORDecodeFunc[T any] func(CBOR) (T, error)

// DecodeBool decodes a CBOR boolean.
func DecodeBool(c CBOR) (bool, error) {
	return c.TryIntoBool()
}

// DecodeSimpleValue decodes a CBOR simple value.
func DecodeSimpleValue(c CBOR) (Simple, error) {
	return c.TryIntoSimpleValue()
}

// DecodeText decodes a CBOR text value.
func DecodeText(c CBOR) (string, error) {
	return c.TryIntoText()
}

// DecodeByteString decodes a CBOR byte string.
func DecodeByteString(c CBOR) ([]byte, error) {
	return c.TryIntoByteString()
}

// DecodeUInt64 decodes an unsigned 64-bit integer.
func DecodeUInt64(c CBOR) (uint64, error) {
	return c.TryIntoUInt64()
}

// DecodeUInt32 decodes an unsigned 32-bit integer.
func DecodeUInt32(c CBOR) (uint32, error) {
	return c.TryIntoUInt32()
}

// DecodeUInt16 decodes an unsigned 16-bit integer.
func DecodeUInt16(c CBOR) (uint16, error) {
	return c.TryIntoUInt16()
}

// DecodeUInt8 decodes an unsigned 8-bit integer.
func DecodeUInt8(c CBOR) (uint8, error) {
	return c.TryIntoUInt8()
}

// DecodeUInt decodes an unsigned native-width integer.
func DecodeUInt(c CBOR) (uint, error) {
	return c.TryIntoUInt()
}

// DecodeInt64 decodes a signed 64-bit integer.
func DecodeInt64(c CBOR) (int64, error) {
	return c.TryIntoInt64()
}

// DecodeInt32 decodes a signed 32-bit integer.
func DecodeInt32(c CBOR) (int32, error) {
	return c.TryIntoInt32()
}

// DecodeInt16 decodes a signed 16-bit integer.
func DecodeInt16(c CBOR) (int16, error) {
	return c.TryIntoInt16()
}

// DecodeInt8 decodes a signed 8-bit integer.
func DecodeInt8(c CBOR) (int8, error) {
	return c.TryIntoInt8()
}

// DecodeInt decodes a signed native-width integer.
func DecodeInt(c CBOR) (int, error) {
	return c.TryIntoInt()
}

// DecodeFloat64 decodes an exact float64-representable numeric value.
func DecodeFloat64(c CBOR) (float64, error) {
	return c.TryIntoFloat64()
}

// DecodeFloat32 decodes an exact float32-representable numeric value.
func DecodeFloat32(c CBOR) (float32, error) {
	return c.TryIntoFloat32()
}

// DecodeFloat16 decodes an exact float16-representable numeric value.
func DecodeFloat16(c CBOR) (Float16, error) {
	return c.TryIntoFloat16()
}

// DecodeBigInt decodes an arbitrary-precision signed integer.
func DecodeBigInt(c CBOR) (*big.Int, error) {
	return c.TryIntoBigInt()
}

// DecodeBigUint decodes an arbitrary-precision unsigned integer.
func DecodeBigUint(c CBOR) (*big.Int, error) {
	return c.TryIntoBigUint()
}

// DecodeTaggedValue decodes a CBOR tagged value as tag/content pair.
func DecodeTaggedValue(c CBOR) (Tag, CBOR, error) {
	return c.TryIntoTaggedValue()
}

// DecodeExpectedTaggedValue decodes tagged CBOR content with an expected tag.
func DecodeExpectedTaggedValue(c CBOR, expected Tag) (CBOR, error) {
	return c.TryIntoExpectedTaggedValue(expected)
}

// DecodeDate decodes a tag-1 date value.
func DecodeDate(c CBOR) (Date, error) {
	return c.TryIntoDate()
}

// DecodeArray decodes a CBOR array into a typed slice using an item decoder.
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

// DecodeMap decodes a CBOR map into a typed Go map using key/value decoders.
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

// DecodeSet decodes a CBOR set-like array into a typed membership map.
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

// DecodeSetSlice decodes a CBOR set into an ordered typed slice.
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
