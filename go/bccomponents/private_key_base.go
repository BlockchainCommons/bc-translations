package bccomponents

import (
	"bytes"
	"fmt"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// PrivateKeyBase holds root key material from which multiple cryptographic
// keys can be derived deterministically.
type PrivateKeyBase struct {
	data []byte
}

// NewPrivateKeyBase creates a new random private key base.
func NewPrivateKeyBase() *PrivateKeyBase {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return NewPrivateKeyBaseUsing(rng)
}

// NewPrivateKeyBaseUsing creates a new random private key base using the provided RNG.
func NewPrivateKeyBaseUsing(rng bcrand.RandomNumberGenerator) *PrivateKeyBase {
	data := rng.RandomData(32)
	return PrivateKeyBaseFromData(data)
}

// PrivateKeyBaseFromData creates a PrivateKeyBase from raw key material.
func PrivateKeyBaseFromData(data []byte) *PrivateKeyBase {
	cp := make([]byte, len(data))
	copy(cp, data)
	return &PrivateKeyBase{data: cp}
}

// PrivateKeyBaseFromOptionalData creates a PrivateKeyBase from optional data,
// generating random data if nil.
func PrivateKeyBaseFromOptionalData(data []byte) *PrivateKeyBase {
	if data == nil {
		return NewPrivateKeyBase()
	}
	return PrivateKeyBaseFromData(data)
}

// Data returns a copy of the raw key material.
func (k *PrivateKeyBase) Data() []byte {
	cp := make([]byte, len(k.data))
	copy(cp, k.data)
	return cp
}

// PrivateKeyData implements PrivateKeyDataProvider.
func (k *PrivateKeyBase) PrivateKeyData() []byte { return k.Data() }

// --- Key derivation methods ---

// ECDSASigningPrivateKey derives an ECDSA signing private key.
func (k *PrivateKeyBase) ECDSASigningPrivateKey() SigningPrivateKey {
	ecKey := DeriveECPrivateKey(k.data)
	return NewSigningPrivateKeyECDSA(ecKey)
}

// SchnorrSigningPrivateKey derives a Schnorr signing private key.
func (k *PrivateKeyBase) SchnorrSigningPrivateKey() SigningPrivateKey {
	ecKey := DeriveECPrivateKey(k.data)
	return NewSigningPrivateKeySchnorr(ecKey)
}

// Ed25519SigningPrivateKey derives an Ed25519 signing private key.
func (k *PrivateKeyBase) Ed25519SigningPrivateKey() SigningPrivateKey {
	edKey := DeriveEd25519PrivateKey(k.data)
	return NewSigningPrivateKeyEd25519(edKey)
}

// X25519PrivateKey derives an X25519 agreement private key.
func (k *PrivateKeyBase) X25519PrivateKey() X25519PrivateKey {
	return DeriveX25519PrivateKey(k.data)
}

// SSHSigningPrivateKey derives an SSH signing private key for the given algorithm.
func (k *PrivateKeyBase) SSHSigningPrivateKey(scheme SignatureScheme, comment string) (SigningPrivateKey, error) {
	rng := NewHKDFRng(k.data, scheme.String())
	switch scheme {
	case SchemeSSHEd25519:
		seed := rng.NextBytes(32)
		sshKey, err := generateSSHEd25519Key(seed, comment)
		if err != nil {
			return SigningPrivateKey{}, err
		}
		return NewSigningPrivateKeySSH(sshKey), nil
	default:
		return SigningPrivateKey{}, errSSH("unsupported SSH scheme")
	}
}

// SchnorrPrivateKeys derives a Schnorr signing + X25519 encapsulation keypair.
func (k *PrivateKeyBase) SchnorrPrivateKeys() PrivateKeys {
	signing := k.SchnorrSigningPrivateKey()
	x25519Key := k.X25519PrivateKey()
	encap := EncapsulationPrivateKeyFromX25519(x25519Key)
	return NewPrivateKeys(signing, encap)
}

// ECDSAPrivateKeys derives an ECDSA signing + X25519 encapsulation keypair.
func (k *PrivateKeyBase) ECDSAPrivateKeys() PrivateKeys {
	signing := k.ECDSASigningPrivateKey()
	x25519Key := k.X25519PrivateKey()
	encap := EncapsulationPrivateKeyFromX25519(x25519Key)
	return NewPrivateKeys(signing, encap)
}

// SchnorrPublicKeys derives the public keys for Schnorr signing + X25519.
func (k *PrivateKeyBase) SchnorrPublicKeys() PublicKeys {
	return k.SchnorrPrivateKeys().PublicKeys()
}

// ECDSAPublicKeys derives the public keys for ECDSA signing + X25519.
func (k *PrivateKeyBase) ECDSAPublicKeys() PublicKeys {
	return k.ECDSAPrivateKeys().PublicKeys()
}

// PrivateKeys returns the default private key bundle (Schnorr + X25519).
func (k *PrivateKeyBase) PrivateKeys() PrivateKeys {
	return k.SchnorrPrivateKeys()
}

// PublicKeys returns the default public key bundle (Schnorr + X25519).
func (k *PrivateKeyBase) PublicKeys() PublicKeys {
	return k.SchnorrPublicKeys()
}

// String returns a human-readable representation.
func (k *PrivateKeyBase) String() string {
	ref := ReferenceForCBORTaggedEncodable(k)
	return fmt.Sprintf("PrivateKeyBase(%s)", ref.RefHexShort())
}

// Equal reports whether two key bases are equal.
func (k *PrivateKeyBase) Equal(other *PrivateKeyBase) bool {
	if k == nil || other == nil {
		return k == other
	}
	return bytes.Equal(k.data, other.data)
}

// Reference implements ReferenceProvider.
func (k *PrivateKeyBase) Reference() Reference {
	return ReferenceForCBORTaggedEncodable(k)
}

// --- CBOR support ---

func PrivateKeyBaseCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagPrivateKeyBase})
}

func (k *PrivateKeyBase) CBORTags() []dcbor.Tag    { return PrivateKeyBaseCBORTags() }
func (k *PrivateKeyBase) UntaggedCBOR() dcbor.CBOR { return dcbor.ToByteString(k.data) }

func (k *PrivateKeyBase) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k *PrivateKeyBase) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodePrivateKeyBase(cbor dcbor.CBOR) (*PrivateKeyBase, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return nil, err
	}
	return PrivateKeyBaseFromData(data), nil
}

func DecodeTaggedPrivateKeyBase(cbor dcbor.CBOR) (*PrivateKeyBase, error) {
	return dcbor.DecodeTagged(cbor, PrivateKeyBaseCBORTags(), DecodePrivateKeyBase)
}

// --- UR support ---

func PrivateKeyBaseToURString(k *PrivateKeyBase) string { return bcur.ToURString(k) }

func PrivateKeyBaseFromURString(urString string) (*PrivateKeyBase, error) {
	return bcur.DecodeURString(urString, PrivateKeyBaseCBORTags(), DecodePrivateKeyBase)
}
