package bccomponents

import (
	"encoding/hex"
	"fmt"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const (
	ECPublicKeySize             = bccrypto.ECDSAPublicKeySize
	ECUncompressedPublicKeySize = bccrypto.ECDSAUncompressedPublicKeySize
)

// ECPublicKey is a 33-byte compressed secp256k1 ECDSA public key.
type ECPublicKey struct {
	data [ECPublicKeySize]byte
}

// ECPublicKeyFromData creates an ECPublicKey from a 33-byte array.
func ECPublicKeyFromData(data [ECPublicKeySize]byte) ECPublicKey {
	return ECPublicKey{data: data}
}

// ECPublicKeyFromDataRef creates an ECPublicKey from a byte slice.
func ECPublicKeyFromDataRef(data []byte) (ECPublicKey, error) {
	if len(data) != ECPublicKeySize {
		return ECPublicKey{}, errInvalidSize("EC public key", ECPublicKeySize, len(data))
	}
	var k ECPublicKey
	copy(k.data[:], data)
	return k, nil
}

// Data returns the 33-byte compressed key data.
func (k ECPublicKey) Data() [ECPublicKeySize]byte { return k.data }

// Bytes returns the key as a byte slice.
func (k ECPublicKey) Bytes() []byte { return k.data[:] }

// Hex returns the key as a hex string.
func (k ECPublicKey) Hex() string { return hex.EncodeToString(k.data[:]) }

// UncompressedPublicKey returns the 65-byte uncompressed form.
func (k ECPublicKey) UncompressedPublicKey() ECUncompressedPublicKey {
	uncompressed := bccrypto.ECDSADecompressPublicKey(k.data)
	return ECUncompressedPublicKey{data: uncompressed}
}

// Verify verifies an ECDSA signature over a message.
func (k ECPublicKey) Verify(signature [bccrypto.ECDSASignatureSize]byte, message []byte) bool {
	return bccrypto.ECDSAVerify(k.data, signature, message)
}

// String returns a human-readable representation.
func (k ECPublicKey) String() string {
	return fmt.Sprintf("ECPublicKey(%s)", k.Hex())
}

// Equal reports whether two keys are equal.
func (k ECPublicKey) Equal(other ECPublicKey) bool { return k.data == other.data }

// Reference implements ReferenceProvider.
func (k ECPublicKey) Reference() Reference {
	return ReferenceForCBORTaggedEncodable(k)
}

// --- CBOR support ---

func ECPublicKeyCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagECKey, bctags.TagECKeyV1})
}

func (k ECPublicKey) CBORTags() []dcbor.Tag { return ECPublicKeyCBORTags() }

func (k ECPublicKey) UntaggedCBOR() dcbor.CBOR {
	m := dcbor.NewMap()
	m.InsertAny(3, dcbor.ToByteString(k.data[:]))
	return dcbor.NewCBORMap(m)
}

func (k ECPublicKey) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k ECPublicKey) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodeECPublicKey(cbor dcbor.CBOR) (ECPublicKey, error) {
	m, err := cbor.TryIntoMap()
	if err != nil {
		return ECPublicKey{}, err
	}

	// If key 2 is present, this is a private key, not a public key.
	if _, ok := m.Get(dcbor.MustFromAny(int64(2))); ok {
		return ECPublicKey{}, dcbor.NewErrorf("expected EC public key (no key 2)")
	}

	dataCBOR, err := m.Extract(dcbor.MustFromAny(int64(3)))
	if err != nil {
		return ECPublicKey{}, err
	}
	data, err := dataCBOR.TryIntoByteString()
	if err != nil {
		return ECPublicKey{}, err
	}
	return ECPublicKeyFromDataRef(data)
}

func DecodeTaggedECPublicKey(cbor dcbor.CBOR) (ECPublicKey, error) {
	return dcbor.DecodeTagged(cbor, ECPublicKeyCBORTags(), DecodeECPublicKey)
}

// --- ECUncompressedPublicKey ---

// ECUncompressedPublicKey is a 65-byte uncompressed secp256k1 ECDSA public key
// (0x04 prefix + 32 bytes x + 32 bytes y). This is a legacy format.
type ECUncompressedPublicKey struct {
	data [ECUncompressedPublicKeySize]byte
}

// ECUncompressedPublicKeyFromData creates an ECUncompressedPublicKey from a 65-byte array.
func ECUncompressedPublicKeyFromData(data [ECUncompressedPublicKeySize]byte) ECUncompressedPublicKey {
	return ECUncompressedPublicKey{data: data}
}

// ECUncompressedPublicKeyFromDataRef creates an ECUncompressedPublicKey from a byte slice.
func ECUncompressedPublicKeyFromDataRef(data []byte) (ECUncompressedPublicKey, error) {
	if len(data) != ECUncompressedPublicKeySize {
		return ECUncompressedPublicKey{}, errInvalidSize("EC uncompressed public key", ECUncompressedPublicKeySize, len(data))
	}
	var k ECUncompressedPublicKey
	copy(k.data[:], data)
	return k, nil
}

// Data returns the 65-byte uncompressed key data.
func (k ECUncompressedPublicKey) Data() [ECUncompressedPublicKeySize]byte { return k.data }

// Bytes returns the key as a byte slice.
func (k ECUncompressedPublicKey) Bytes() []byte { return k.data[:] }

// Hex returns the key as a hex string.
func (k ECUncompressedPublicKey) Hex() string { return hex.EncodeToString(k.data[:]) }

// CompressedPublicKey returns the compressed 33-byte form.
func (k ECUncompressedPublicKey) CompressedPublicKey() ECPublicKey {
	compressed := bccrypto.ECDSACompressPublicKey(k.data)
	return ECPublicKey{data: compressed}
}

// String returns a human-readable representation.
func (k ECUncompressedPublicKey) String() string {
	return fmt.Sprintf("ECUncompressedPublicKey(%s)", k.Hex())
}

// Equal reports whether two keys are equal.
func (k ECUncompressedPublicKey) Equal(other ECUncompressedPublicKey) bool {
	return k.data == other.data
}
