package bccomponents

import (
	"encoding/hex"
	"fmt"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const ECPrivateKeySize = bccrypto.ECDSAPrivateKeySize

// ECPrivateKey is a 32-byte secp256k1 ECDSA private key.
type ECPrivateKey struct {
	data [ECPrivateKeySize]byte
}

// NewECPrivateKey creates a new random EC private key.
func NewECPrivateKey() ECPrivateKey {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return NewECPrivateKeyUsing(rng)
}

// NewECPrivateKeyUsing creates a new random EC private key using rng.
func NewECPrivateKeyUsing(rng bcrand.RandomNumberGenerator) ECPrivateKey {
	return ECPrivateKey{data: bccrypto.ECDSANewPrivateKeyUsing(rng)}
}

// ECPrivateKeyFromData creates an ECPrivateKey from a 32-byte array.
func ECPrivateKeyFromData(data [ECPrivateKeySize]byte) ECPrivateKey {
	return ECPrivateKey{data: data}
}

// ECPrivateKeyFromDataRef creates an ECPrivateKey from a byte slice.
func ECPrivateKeyFromDataRef(data []byte) (ECPrivateKey, error) {
	if len(data) != ECPrivateKeySize {
		return ECPrivateKey{}, errInvalidSize("EC private key", ECPrivateKeySize, len(data))
	}
	var k ECPrivateKey
	copy(k.data[:], data)
	return k, nil
}

// DeriveECPrivateKey derives an EC private key from key material.
func DeriveECPrivateKey(keyMaterial []byte) ECPrivateKey {
	derived := bccrypto.ECDSADerivePrivateKey(keyMaterial)
	var data [ECPrivateKeySize]byte
	copy(data[:], derived)
	return ECPrivateKey{data: data}
}

// Data returns the 32-byte key data.
func (k ECPrivateKey) Data() [ECPrivateKeySize]byte { return k.data }

// Bytes returns the key as a byte slice.
func (k ECPrivateKey) Bytes() []byte { return k.data[:] }

// Hex returns the key as a hex string.
func (k ECPrivateKey) Hex() string { return hex.EncodeToString(k.data[:]) }

// ECDSAPublicKey derives the corresponding compressed ECDSA public key.
func (k ECPrivateKey) ECDSAPublicKey() ECPublicKey {
	pub := bccrypto.ECDSAPublicKeyFromPrivateKey(k.data)
	return ECPublicKey{data: pub}
}

// PublicKey returns the corresponding compressed ECDSA public key.
func (k ECPrivateKey) PublicKey() ECPublicKey {
	return k.ECDSAPublicKey()
}

// SchnorrPublicKey derives the corresponding x-only Schnorr public key.
func (k ECPrivateKey) SchnorrPublicKey() SchnorrPublicKey {
	pub := bccrypto.SchnorrPublicKeyFromPrivateKey(k.data)
	return SchnorrPublicKey{data: pub}
}

// ECDSASign signs a message with ECDSA and returns a 64-byte compact signature.
func (k ECPrivateKey) ECDSASign(message []byte) [bccrypto.ECDSASignatureSize]byte {
	return bccrypto.ECDSASign(k.data, message)
}

// SchnorrSign signs a message with BIP340 Schnorr using secure randomness.
func (k ECPrivateKey) SchnorrSign(message []byte) [bccrypto.SchnorrSignatureSize]byte {
	return bccrypto.SchnorrSign(k.data, message)
}

// SchnorrSignUsing signs a message with BIP340 Schnorr using the provided RNG.
func (k ECPrivateKey) SchnorrSignUsing(message []byte, rng bcrand.RandomNumberGenerator) [bccrypto.SchnorrSignatureSize]byte {
	return bccrypto.SchnorrSignUsing(k.data, message, rng)
}

// PrivateKeyData implements PrivateKeyDataProvider.
func (k ECPrivateKey) PrivateKeyData() []byte { return k.Bytes() }

// String returns a human-readable representation.
func (k ECPrivateKey) String() string {
	return fmt.Sprintf("ECPrivateKey(%s)", k.Hex())
}

// Equal reports whether two keys are equal.
func (k ECPrivateKey) Equal(other ECPrivateKey) bool { return k.data == other.data }

// Reference implements ReferenceProvider.
func (k ECPrivateKey) Reference() Reference {
	return ReferenceForCBORTaggedEncodable(k)
}

// --- CBOR support ---

func ECPrivateKeyCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagECKey, bctags.TagECKeyV1})
}

func (k ECPrivateKey) CBORTags() []dcbor.Tag { return ECPrivateKeyCBORTags() }

func (k ECPrivateKey) UntaggedCBOR() dcbor.CBOR {
	m := dcbor.NewMap()
	m.InsertAny(2, dcbor.MustFromAny(true))
	m.InsertAny(3, dcbor.ToByteString(k.data[:]))
	return dcbor.NewCBORMap(m)
}

func (k ECPrivateKey) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k ECPrivateKey) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodeECPrivateKey(cbor dcbor.CBOR) (ECPrivateKey, error) {
	m, err := cbor.TryIntoMap()
	if err != nil {
		return ECPrivateKey{}, err
	}

	isPrivate, ok := m.Get(dcbor.MustFromAny(int64(2)))
	if !ok {
		return ECPrivateKey{}, dcbor.NewErrorf("EC key map must have key 2 for private key")
	}
	if b, ok := isPrivate.AsBool(); !ok || !b {
		return ECPrivateKey{}, dcbor.NewErrorf("EC key private key flag must be true")
	}

	dataCBOR, err := m.Extract(dcbor.MustFromAny(int64(3)))
	if err != nil {
		return ECPrivateKey{}, err
	}
	data, err := dataCBOR.TryIntoByteString()
	if err != nil {
		return ECPrivateKey{}, err
	}
	return ECPrivateKeyFromDataRef(data)
}

func DecodeTaggedECPrivateKey(cbor dcbor.CBOR) (ECPrivateKey, error) {
	return dcbor.DecodeTagged(cbor, ECPrivateKeyCBORTags(), DecodeECPrivateKey)
}
