package bccomponents

import (
	"encoding/hex"
	"fmt"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const X25519PrivateKeySize = bccrypto.X25519PrivateKeySize

// X25519PrivateKey is a 32-byte Curve25519 private key for key agreement (ECDH).
type X25519PrivateKey struct {
	data [X25519PrivateKeySize]byte
}

// NewX25519PrivateKey creates a new random X25519 private key.
func NewX25519PrivateKey() X25519PrivateKey {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return NewX25519PrivateKeyUsing(rng)
}

// NewX25519PrivateKeyUsing creates a new random X25519 private key using rng.
func NewX25519PrivateKeyUsing(rng bcrand.RandomNumberGenerator) X25519PrivateKey {
	return X25519PrivateKey{data: bccrypto.X25519NewPrivateKeyUsing(rng)}
}

// X25519PrivateKeyFromData creates an X25519PrivateKey from a 32-byte array.
func X25519PrivateKeyFromData(data [X25519PrivateKeySize]byte) X25519PrivateKey {
	return X25519PrivateKey{data: data}
}

// X25519PrivateKeyFromDataRef creates an X25519PrivateKey from a byte slice.
func X25519PrivateKeyFromDataRef(data []byte) (X25519PrivateKey, error) {
	if len(data) != X25519PrivateKeySize {
		return X25519PrivateKey{}, errInvalidSize("X25519 private key", X25519PrivateKeySize, len(data))
	}
	var k X25519PrivateKey
	copy(k.data[:], data)
	return k, nil
}

// X25519PrivateKeyFromHex creates an X25519PrivateKey from a hex string.
func X25519PrivateKeyFromHex(h string) (X25519PrivateKey, error) {
	data, err := hex.DecodeString(h)
	if err != nil {
		return X25519PrivateKey{}, fmt.Errorf("bccomponents: invalid X25519 private key hex: %w", err)
	}
	return X25519PrivateKeyFromDataRef(data)
}

// X25519Keypair generates a new X25519 keypair.
func X25519Keypair() (X25519PrivateKey, X25519PublicKey) {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return X25519KeypairUsing(rng)
}

// X25519KeypairUsing generates a new X25519 keypair using the provided RNG.
func X25519KeypairUsing(rng bcrand.RandomNumberGenerator) (X25519PrivateKey, X25519PublicKey) {
	priv := NewX25519PrivateKeyUsing(rng)
	pub := priv.PublicKey()
	return priv, pub
}

// DeriveX25519PrivateKey derives an X25519 private key from key material.
func DeriveX25519PrivateKey(keyMaterial []byte) X25519PrivateKey {
	derived := bccrypto.DeriveAgreementPrivateKey(keyMaterial)
	return X25519PrivateKey{data: derived}
}

// Data returns the 32-byte key data.
func (k X25519PrivateKey) Data() [X25519PrivateKeySize]byte { return k.data }

// Bytes returns the key as a byte slice.
func (k X25519PrivateKey) Bytes() []byte { return k.data[:] }

// Hex returns the key as a 64-character hex string.
func (k X25519PrivateKey) Hex() string { return hex.EncodeToString(k.data[:]) }

// PublicKey derives the corresponding X25519 public key.
func (k X25519PrivateKey) PublicKey() X25519PublicKey {
	pub := bccrypto.X25519PublicKeyFromPrivateKey(k.data)
	return X25519PublicKey{data: pub}
}

// SharedKeyWith computes a shared symmetric key from this private key and
// the recipient's public key via X25519 ECDH.
func (k X25519PrivateKey) SharedKeyWith(pub X25519PublicKey) SymmetricKey {
	shared := bccrypto.X25519SharedKey(k.data, pub.data)
	return SymmetricKeyFromData(shared)
}

// PrivateKeyData implements PrivateKeyDataProvider.
func (k X25519PrivateKey) PrivateKeyData() []byte { return k.Bytes() }

// String returns a human-readable representation.
func (k X25519PrivateKey) String() string {
	return fmt.Sprintf("X25519PrivateKey(%s)", k.Hex())
}

// Equal reports whether two keys are equal.
func (k X25519PrivateKey) Equal(other X25519PrivateKey) bool { return k.data == other.data }

// Reference implements ReferenceProvider.
func (k X25519PrivateKey) Reference() Reference {
	return ReferenceForCBORTaggedEncodable(k)
}

// --- CBOR support ---

func X25519PrivateKeyCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagX25519PrivateKey})
}

func (k X25519PrivateKey) CBORTags() []dcbor.Tag   { return X25519PrivateKeyCBORTags() }
func (k X25519PrivateKey) UntaggedCBOR() dcbor.CBOR { return dcbor.ToByteString(k.data[:]) }

func (k X25519PrivateKey) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k X25519PrivateKey) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodeX25519PrivateKey(cbor dcbor.CBOR) (X25519PrivateKey, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return X25519PrivateKey{}, err
	}
	return X25519PrivateKeyFromDataRef(data)
}

func DecodeTaggedX25519PrivateKey(cbor dcbor.CBOR) (X25519PrivateKey, error) {
	return dcbor.DecodeTagged(cbor, X25519PrivateKeyCBORTags(), DecodeX25519PrivateKey)
}

// --- UR support ---

func X25519PrivateKeyToURString(k X25519PrivateKey) string { return bcur.ToURString(k) }

func X25519PrivateKeyFromURString(urString string) (X25519PrivateKey, error) {
	return bcur.DecodeURString(urString, X25519PrivateKeyCBORTags(), DecodeX25519PrivateKey)
}
