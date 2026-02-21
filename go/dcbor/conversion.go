package dcbor

// CBORDecodeFunc decodes a CBOR value into a concrete Go type.
type CBORDecodeFunc[T any] func(CBOR) (T, error)

func DecodeBool(c CBOR) (bool, error) {
	return c.TryIntoBool()
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

func DecodeInt64(c CBOR) (int64, error) {
	return c.TryIntoInt64()
}

func DecodeFloat64(c CBOR) (float64, error) {
	return c.TryIntoFloat64()
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
