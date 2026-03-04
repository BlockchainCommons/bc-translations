package bccomponents

import (
	"bytes"
	"crypto/rand"
	"fmt"

	"github.com/cloudflare/circl/sign/mldsa/mldsa44"
	"github.com/cloudflare/circl/sign/mldsa/mldsa65"
	"github.com/cloudflare/circl/sign/mldsa/mldsa87"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// MLDSA represents a Module-Lattice Digital Signature Algorithm security level.
type MLDSA int

const (
	MLDSA44 MLDSA = 2
	MLDSA65 MLDSA = 3
	MLDSA87 MLDSA = 5
)

// Keypair generates a new ML-DSA keypair at this security level.
func (l MLDSA) Keypair() (MLDSAPrivateKey, MLDSAPublicKey, error) {
	switch l {
	case MLDSA44:
		pub, priv, err := mldsa44.GenerateKey(rand.Reader)
		if err != nil {
			return MLDSAPrivateKey{}, MLDSAPublicKey{}, errPostQuantum(err.Error())
		}
		return MLDSAPrivateKey{level: l, data44: priv}, MLDSAPublicKey{level: l, data44: pub}, nil
	case MLDSA65:
		pub, priv, err := mldsa65.GenerateKey(rand.Reader)
		if err != nil {
			return MLDSAPrivateKey{}, MLDSAPublicKey{}, errPostQuantum(err.Error())
		}
		return MLDSAPrivateKey{level: l, data65: priv}, MLDSAPublicKey{level: l, data65: pub}, nil
	case MLDSA87:
		pub, priv, err := mldsa87.GenerateKey(rand.Reader)
		if err != nil {
			return MLDSAPrivateKey{}, MLDSAPublicKey{}, errPostQuantum(err.Error())
		}
		return MLDSAPrivateKey{level: l, data87: priv}, MLDSAPublicKey{level: l, data87: pub}, nil
	default:
		return MLDSAPrivateKey{}, MLDSAPublicKey{}, errPostQuantum("unsupported MLDSA level")
	}
}

// String returns the level name.
func (l MLDSA) String() string {
	switch l {
	case MLDSA44:
		return "MLDSA44"
	case MLDSA65:
		return "MLDSA65"
	case MLDSA87:
		return "MLDSA87"
	default:
		return fmt.Sprintf("MLDSA(%d)", int(l))
	}
}

// MLDSAFromLevel converts a numeric level to an MLDSA value.
func MLDSAFromLevel(level int) (MLDSA, error) {
	switch level {
	case 2:
		return MLDSA44, nil
	case 3:
		return MLDSA65, nil
	case 5:
		return MLDSA87, nil
	default:
		return 0, errPostQuantum(fmt.Sprintf("unsupported MLDSA level: %d", level))
	}
}

// --- MLDSAPrivateKey ---

// MLDSAPrivateKey is a Module-Lattice Digital Signature Algorithm private key.
type MLDSAPrivateKey struct {
	level  MLDSA
	data44 *mldsa44.PrivateKey
	data65 *mldsa65.PrivateKey
	data87 *mldsa87.PrivateKey
}

// Level returns the security level.
func (k *MLDSAPrivateKey) Level() MLDSA { return k.level }

// Bytes returns the private key as a byte slice.
func (k *MLDSAPrivateKey) Bytes() []byte {
	switch k.level {
	case MLDSA44:
		b, _ := k.data44.MarshalBinary()
		return b
	case MLDSA65:
		b, _ := k.data65.MarshalBinary()
		return b
	case MLDSA87:
		b, _ := k.data87.MarshalBinary()
		return b
	default:
		return nil
	}
}

// Sign signs a message and returns an MLDSASignature.
func (k *MLDSAPrivateKey) Sign(message []byte) (MLDSASignature, error) {
	switch k.level {
	case MLDSA44:
		sig := make([]byte, mldsa44.SignatureSize)
		if err := mldsa44.SignTo(k.data44, message, nil, false, sig); err != nil {
			return MLDSASignature{}, errPostQuantum(err.Error())
		}
		return MLDSASignature{level: k.level, data: sig}, nil
	case MLDSA65:
		sig := make([]byte, mldsa65.SignatureSize)
		if err := mldsa65.SignTo(k.data65, message, nil, false, sig); err != nil {
			return MLDSASignature{}, errPostQuantum(err.Error())
		}
		return MLDSASignature{level: k.level, data: sig}, nil
	case MLDSA87:
		sig := make([]byte, mldsa87.SignatureSize)
		if err := mldsa87.SignTo(k.data87, message, nil, false, sig); err != nil {
			return MLDSASignature{}, errPostQuantum(err.Error())
		}
		return MLDSASignature{level: k.level, data: sig}, nil
	default:
		return MLDSASignature{}, errPostQuantum("unsupported MLDSA level")
	}
}

// MLDSAPrivateKeyFromBytes creates an MLDSAPrivateKey from raw bytes at the given level.
func MLDSAPrivateKeyFromBytes(level MLDSA, data []byte) (MLDSAPrivateKey, error) {
	switch level {
	case MLDSA44:
		var key mldsa44.PrivateKey
		if err := key.UnmarshalBinary(data); err != nil {
			return MLDSAPrivateKey{}, errPostQuantum(err.Error())
		}
		return MLDSAPrivateKey{level: level, data44: &key}, nil
	case MLDSA65:
		var key mldsa65.PrivateKey
		if err := key.UnmarshalBinary(data); err != nil {
			return MLDSAPrivateKey{}, errPostQuantum(err.Error())
		}
		return MLDSAPrivateKey{level: level, data65: &key}, nil
	case MLDSA87:
		var key mldsa87.PrivateKey
		if err := key.UnmarshalBinary(data); err != nil {
			return MLDSAPrivateKey{}, errPostQuantum(err.Error())
		}
		return MLDSAPrivateKey{level: level, data87: &key}, nil
	default:
		return MLDSAPrivateKey{}, errPostQuantum("unsupported MLDSA level")
	}
}

// String returns a human-readable representation.
func (k *MLDSAPrivateKey) String() string {
	return fmt.Sprintf("MLDSAPrivateKey(%s)", k.level)
}

// Reference implements ReferenceProvider.
func (k *MLDSAPrivateKey) Reference() Reference {
	digest := DigestFromImage(k.Bytes())
	return ReferenceFromDigest(digest)
}

// --- CBOR support ---

func MLDSAPrivateKeyCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagMLDSAPrivateKey})
}

func (k *MLDSAPrivateKey) CBORTags() []dcbor.Tag { return MLDSAPrivateKeyCBORTags() }

func (k *MLDSAPrivateKey) UntaggedCBOR() dcbor.CBOR {
	elements := []dcbor.CBOR{
		dcbor.MustFromAny(int64(k.level)),
		dcbor.ToByteString(k.Bytes()),
	}
	return dcbor.NewCBORArray(elements)
}

func (k *MLDSAPrivateKey) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k *MLDSAPrivateKey) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodeMLDSAPrivateKey(cbor dcbor.CBOR) (*MLDSAPrivateKey, error) {
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return nil, err
	}
	if len(elements) != 2 {
		return nil, dcbor.NewErrorf("MLDSAPrivateKey must have 2 elements")
	}
	levelVal, ok := elements[0].AsInt64()
	if !ok {
		return nil, dcbor.NewErrorf("MLDSAPrivateKey level must be integer")
	}
	level, err := MLDSAFromLevel(int(levelVal))
	if err != nil {
		return nil, err
	}
	data, err := elements[1].TryIntoByteString()
	if err != nil {
		return nil, err
	}
	key, err := MLDSAPrivateKeyFromBytes(level, data)
	if err != nil {
		return nil, err
	}
	return &key, nil
}

func DecodeTaggedMLDSAPrivateKey(cbor dcbor.CBOR) (*MLDSAPrivateKey, error) {
	return dcbor.DecodeTagged(cbor, MLDSAPrivateKeyCBORTags(), DecodeMLDSAPrivateKey)
}

// --- MLDSAPublicKey ---

// MLDSAPublicKey is a Module-Lattice Digital Signature Algorithm public key.
type MLDSAPublicKey struct {
	level  MLDSA
	data44 *mldsa44.PublicKey
	data65 *mldsa65.PublicKey
	data87 *mldsa87.PublicKey
}

// Level returns the security level.
func (k *MLDSAPublicKey) Level() MLDSA { return k.level }

// Bytes returns the public key as a byte slice.
func (k *MLDSAPublicKey) Bytes() []byte {
	switch k.level {
	case MLDSA44:
		b, _ := k.data44.MarshalBinary()
		return b
	case MLDSA65:
		b, _ := k.data65.MarshalBinary()
		return b
	case MLDSA87:
		b, _ := k.data87.MarshalBinary()
		return b
	default:
		return nil
	}
}

// Verify verifies an ML-DSA signature over a message.
func (k *MLDSAPublicKey) Verify(sig MLDSASignature, message []byte) bool {
	if sig.level != k.level {
		return false
	}
	switch k.level {
	case MLDSA44:
		return mldsa44.Verify(k.data44, message, nil, sig.data)
	case MLDSA65:
		return mldsa65.Verify(k.data65, message, nil, sig.data)
	case MLDSA87:
		return mldsa87.Verify(k.data87, message, nil, sig.data)
	default:
		return false
	}
}

// MLDSAPublicKeyFromBytes creates an MLDSAPublicKey from raw bytes at the given level.
func MLDSAPublicKeyFromBytes(level MLDSA, data []byte) (MLDSAPublicKey, error) {
	switch level {
	case MLDSA44:
		var key mldsa44.PublicKey
		if err := key.UnmarshalBinary(data); err != nil {
			return MLDSAPublicKey{}, errPostQuantum(err.Error())
		}
		return MLDSAPublicKey{level: level, data44: &key}, nil
	case MLDSA65:
		var key mldsa65.PublicKey
		if err := key.UnmarshalBinary(data); err != nil {
			return MLDSAPublicKey{}, errPostQuantum(err.Error())
		}
		return MLDSAPublicKey{level: level, data65: &key}, nil
	case MLDSA87:
		var key mldsa87.PublicKey
		if err := key.UnmarshalBinary(data); err != nil {
			return MLDSAPublicKey{}, errPostQuantum(err.Error())
		}
		return MLDSAPublicKey{level: level, data87: &key}, nil
	default:
		return MLDSAPublicKey{}, errPostQuantum("unsupported MLDSA level")
	}
}

// String returns a human-readable representation.
func (k *MLDSAPublicKey) String() string {
	return fmt.Sprintf("MLDSAPublicKey(%s)", k.level)
}

// Equal reports whether two keys are equal.
func (k *MLDSAPublicKey) Equal(other *MLDSAPublicKey) bool {
	if k == nil || other == nil {
		return k == other
	}
	if k.level != other.level {
		return false
	}
	return bytes.Equal(k.Bytes(), other.Bytes())
}

// Reference implements ReferenceProvider.
func (k *MLDSAPublicKey) Reference() Reference {
	digest := DigestFromImage(k.Bytes())
	return ReferenceFromDigest(digest)
}

// --- CBOR support ---

func MLDSAPublicKeyCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagMLDSAPublicKey})
}

func (k *MLDSAPublicKey) CBORTags() []dcbor.Tag { return MLDSAPublicKeyCBORTags() }

func (k *MLDSAPublicKey) UntaggedCBOR() dcbor.CBOR {
	elements := []dcbor.CBOR{
		dcbor.MustFromAny(int64(k.level)),
		dcbor.ToByteString(k.Bytes()),
	}
	return dcbor.NewCBORArray(elements)
}

func (k *MLDSAPublicKey) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k *MLDSAPublicKey) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodeMLDSAPublicKey(cbor dcbor.CBOR) (*MLDSAPublicKey, error) {
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return nil, err
	}
	if len(elements) != 2 {
		return nil, dcbor.NewErrorf("MLDSAPublicKey must have 2 elements")
	}
	levelVal, ok := elements[0].AsInt64()
	if !ok {
		return nil, dcbor.NewErrorf("MLDSAPublicKey level must be integer")
	}
	level, err := MLDSAFromLevel(int(levelVal))
	if err != nil {
		return nil, err
	}
	data, err := elements[1].TryIntoByteString()
	if err != nil {
		return nil, err
	}
	key, err := MLDSAPublicKeyFromBytes(level, data)
	if err != nil {
		return nil, err
	}
	return &key, nil
}

func DecodeTaggedMLDSAPublicKey(cbor dcbor.CBOR) (*MLDSAPublicKey, error) {
	return dcbor.DecodeTagged(cbor, MLDSAPublicKeyCBORTags(), DecodeMLDSAPublicKey)
}

// --- MLDSASignature ---

// MLDSASignature is a Module-Lattice Digital Signature Algorithm signature.
type MLDSASignature struct {
	level MLDSA
	data  []byte
}

// Level returns the security level.
func (s MLDSASignature) Level() MLDSA { return s.level }

// Bytes returns the signature data.
func (s MLDSASignature) Bytes() []byte {
	cp := make([]byte, len(s.data))
	copy(cp, s.data)
	return cp
}

// Equal reports whether two signatures are equal.
func (s MLDSASignature) Equal(other MLDSASignature) bool {
	return bytes.Equal(s.data, other.data)
}

// String returns a human-readable representation.
func (s MLDSASignature) String() string {
	return fmt.Sprintf("MLDSASignature(%s, %d bytes)", s.level, len(s.data))
}

// MLDSASignatureFromBytes creates an MLDSASignature from raw bytes at the given level.
func MLDSASignatureFromBytes(level MLDSA, data []byte) MLDSASignature {
	cp := make([]byte, len(data))
	copy(cp, data)
	return MLDSASignature{level: level, data: cp}
}

// --- CBOR support ---

func MLDSASignatureCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagMLDSASignature})
}

func (s MLDSASignature) CBORTags() []dcbor.Tag { return MLDSASignatureCBORTags() }

func (s MLDSASignature) UntaggedCBOR() dcbor.CBOR {
	elements := []dcbor.CBOR{
		dcbor.MustFromAny(int64(s.level)),
		dcbor.ToByteString(s.data),
	}
	return dcbor.NewCBORArray(elements)
}

func (s MLDSASignature) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(s)
	return cbor
}

func (s MLDSASignature) ToCBOR() dcbor.CBOR { return s.TaggedCBOR() }

func DecodeMLDSASignature(cbor dcbor.CBOR) (MLDSASignature, error) {
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return MLDSASignature{}, err
	}
	if len(elements) != 2 {
		return MLDSASignature{}, dcbor.NewErrorf("MLDSASignature must have 2 elements")
	}
	levelVal, ok := elements[0].AsInt64()
	if !ok {
		return MLDSASignature{}, dcbor.NewErrorf("MLDSASignature level must be integer")
	}
	level, err := MLDSAFromLevel(int(levelVal))
	if err != nil {
		return MLDSASignature{}, err
	}
	data, err := elements[1].TryIntoByteString()
	if err != nil {
		return MLDSASignature{}, err
	}
	return MLDSASignatureFromBytes(level, data), nil
}

func DecodeTaggedMLDSASignature(cbor dcbor.CBOR) (MLDSASignature, error) {
	return dcbor.DecodeTagged(cbor, MLDSASignatureCBORTags(), DecodeMLDSASignature)
}
