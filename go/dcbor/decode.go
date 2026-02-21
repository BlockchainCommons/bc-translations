package dcbor

// DecodeCBOR decodes deterministic CBOR bytes into symbolic CBOR.
func DecodeCBOR(data []byte) (CBOR, error) {
	return TryFromData(data)
}
