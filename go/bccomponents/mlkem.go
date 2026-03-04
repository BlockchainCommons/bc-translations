package bccomponents

import (
	"bytes"
	"crypto/rand"
	"fmt"

	"github.com/cloudflare/circl/kem/mlkem/mlkem512"
	"github.com/cloudflare/circl/kem/mlkem/mlkem768"
	"github.com/cloudflare/circl/kem/mlkem/mlkem1024"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// MLKEM represents a Module-Lattice Key Encapsulation Mechanism security level.
type MLKEM int

const (
	MLKEM512  MLKEM = 512
	MLKEM768  MLKEM = 768
	MLKEM1024 MLKEM = 1024
)

const mlkemSharedSecretSize = 32

// Keypair generates a new ML-KEM keypair at this security level.
func (l MLKEM) Keypair() (MLKEMPrivateKey, MLKEMPublicKey, error) {
	switch l {
	case MLKEM512:
		pub, priv, err := mlkem512.GenerateKeyPair(rand.Reader)
		if err != nil {
			return MLKEMPrivateKey{}, MLKEMPublicKey{}, errPostQuantum(err.Error())
		}
		return MLKEMPrivateKey{level: l, data512: priv}, MLKEMPublicKey{level: l, data512: pub}, nil
	case MLKEM768:
		pub, priv, err := mlkem768.GenerateKeyPair(rand.Reader)
		if err != nil {
			return MLKEMPrivateKey{}, MLKEMPublicKey{}, errPostQuantum(err.Error())
		}
		return MLKEMPrivateKey{level: l, data768: priv}, MLKEMPublicKey{level: l, data768: pub}, nil
	case MLKEM1024:
		pub, priv, err := mlkem1024.GenerateKeyPair(rand.Reader)
		if err != nil {
			return MLKEMPrivateKey{}, MLKEMPublicKey{}, errPostQuantum(err.Error())
		}
		return MLKEMPrivateKey{level: l, data1024: priv}, MLKEMPublicKey{level: l, data1024: pub}, nil
	default:
		return MLKEMPrivateKey{}, MLKEMPublicKey{}, errPostQuantum("unsupported MLKEM level")
	}
}

// String returns the level name.
func (l MLKEM) String() string {
	switch l {
	case MLKEM512:
		return "MLKEM512"
	case MLKEM768:
		return "MLKEM768"
	case MLKEM1024:
		return "MLKEM1024"
	default:
		return fmt.Sprintf("MLKEM(%d)", int(l))
	}
}

// MLKEMFromLevel converts a numeric level to an MLKEM value.
func MLKEMFromLevel(level int) (MLKEM, error) {
	switch level {
	case 512:
		return MLKEM512, nil
	case 768:
		return MLKEM768, nil
	case 1024:
		return MLKEM1024, nil
	default:
		return 0, errPostQuantum(fmt.Sprintf("unsupported MLKEM level: %d", level))
	}
}

// --- MLKEMPrivateKey ---

// MLKEMPrivateKey is a Module-Lattice Key Encapsulation Mechanism private key.
type MLKEMPrivateKey struct {
	level    MLKEM
	data512  *mlkem512.PrivateKey
	data768  *mlkem768.PrivateKey
	data1024 *mlkem1024.PrivateKey
}

// Level returns the security level.
func (k *MLKEMPrivateKey) Level() MLKEM { return k.level }

// Bytes returns the private key as a byte slice.
func (k *MLKEMPrivateKey) Bytes() []byte {
	switch k.level {
	case MLKEM512:
		b, _ := k.data512.MarshalBinary()
		return b
	case MLKEM768:
		b, _ := k.data768.MarshalBinary()
		return b
	case MLKEM1024:
		b, _ := k.data1024.MarshalBinary()
		return b
	default:
		return nil
	}
}

// DecapsulateSharedSecret recovers a shared secret from a ciphertext.
func (k *MLKEMPrivateKey) DecapsulateSharedSecret(ct MLKEMCiphertext) (SymmetricKey, error) {
	if ct.level != k.level {
		return SymmetricKey{}, errPostQuantum("MLKEM level mismatch")
	}
	var ss []byte
	switch k.level {
	case MLKEM512:
		ss = make([]byte, mlkemSharedSecretSize)
		k.data512.DecapsulateTo(ss, ct.data)
	case MLKEM768:
		ss = make([]byte, mlkemSharedSecretSize)
		k.data768.DecapsulateTo(ss, ct.data)
	case MLKEM1024:
		ss = make([]byte, mlkemSharedSecretSize)
		k.data1024.DecapsulateTo(ss, ct.data)
	default:
		return SymmetricKey{}, errPostQuantum("unsupported MLKEM level")
	}
	key, err := SymmetricKeyFromDataRef(ss)
	if err != nil {
		return SymmetricKey{}, err
	}
	return key, nil
}

// MLKEMPrivateKeyFromBytes creates an MLKEMPrivateKey from raw bytes at the given level.
func MLKEMPrivateKeyFromBytes(level MLKEM, data []byte) (MLKEMPrivateKey, error) {
	switch level {
	case MLKEM512:
		var key mlkem512.PrivateKey
		if err := key.Unpack(data); err != nil {
			return MLKEMPrivateKey{}, errPostQuantum(err.Error())
		}
		return MLKEMPrivateKey{level: level, data512: &key}, nil
	case MLKEM768:
		var key mlkem768.PrivateKey
		if err := key.Unpack(data); err != nil {
			return MLKEMPrivateKey{}, errPostQuantum(err.Error())
		}
		return MLKEMPrivateKey{level: level, data768: &key}, nil
	case MLKEM1024:
		var key mlkem1024.PrivateKey
		if err := key.Unpack(data); err != nil {
			return MLKEMPrivateKey{}, errPostQuantum(err.Error())
		}
		return MLKEMPrivateKey{level: level, data1024: &key}, nil
	default:
		return MLKEMPrivateKey{}, errPostQuantum("unsupported MLKEM level")
	}
}

// String returns a human-readable representation.
func (k *MLKEMPrivateKey) String() string {
	return fmt.Sprintf("MLKEMPrivateKey(%s)", k.level)
}

// Reference implements ReferenceProvider.
func (k *MLKEMPrivateKey) Reference() Reference {
	digest := DigestFromImage(k.Bytes())
	return ReferenceFromDigest(digest)
}

// --- CBOR support ---

func MLKEMPrivateKeyCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagMLKEMPrivateKey})
}

func (k *MLKEMPrivateKey) CBORTags() []dcbor.Tag { return MLKEMPrivateKeyCBORTags() }

func (k *MLKEMPrivateKey) UntaggedCBOR() dcbor.CBOR {
	elements := []dcbor.CBOR{
		dcbor.MustFromAny(int64(k.level)),
		dcbor.ToByteString(k.Bytes()),
	}
	return dcbor.NewCBORArray(elements)
}

func (k *MLKEMPrivateKey) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k *MLKEMPrivateKey) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodeMLKEMPrivateKey(cbor dcbor.CBOR) (*MLKEMPrivateKey, error) {
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return nil, err
	}
	if len(elements) != 2 {
		return nil, dcbor.NewErrorf("MLKEMPrivateKey must have 2 elements")
	}
	levelVal, ok := elements[0].AsInt64()
	if !ok {
		return nil, dcbor.NewErrorf("MLKEMPrivateKey level must be integer")
	}
	level, err := MLKEMFromLevel(int(levelVal))
	if err != nil {
		return nil, err
	}
	data, err := elements[1].TryIntoByteString()
	if err != nil {
		return nil, err
	}
	key, err := MLKEMPrivateKeyFromBytes(level, data)
	if err != nil {
		return nil, err
	}
	return &key, nil
}

func DecodeTaggedMLKEMPrivateKey(cbor dcbor.CBOR) (*MLKEMPrivateKey, error) {
	return dcbor.DecodeTagged(cbor, MLKEMPrivateKeyCBORTags(), DecodeMLKEMPrivateKey)
}

// --- MLKEMPublicKey ---

// MLKEMPublicKey is a Module-Lattice Key Encapsulation Mechanism public key.
type MLKEMPublicKey struct {
	level    MLKEM
	data512  *mlkem512.PublicKey
	data768  *mlkem768.PublicKey
	data1024 *mlkem1024.PublicKey
}

// Level returns the security level.
func (k *MLKEMPublicKey) Level() MLKEM { return k.level }

// Bytes returns the public key as a byte slice.
func (k *MLKEMPublicKey) Bytes() []byte {
	switch k.level {
	case MLKEM512:
		b, _ := k.data512.MarshalBinary()
		return b
	case MLKEM768:
		b, _ := k.data768.MarshalBinary()
		return b
	case MLKEM1024:
		b, _ := k.data1024.MarshalBinary()
		return b
	default:
		return nil
	}
}

// EncapsulateNewSharedSecret generates a shared secret and ciphertext.
func (k *MLKEMPublicKey) EncapsulateNewSharedSecret() (SymmetricKey, MLKEMCiphertext, error) {
	switch k.level {
	case MLKEM512:
		ct := make([]byte, mlkem512.CiphertextSize)
		ss := make([]byte, mlkemSharedSecretSize)
		seed := make([]byte, mlkem512.EncapsulationSeedSize)
		if _, err := rand.Read(seed); err != nil {
			return SymmetricKey{}, MLKEMCiphertext{}, errPostQuantum(err.Error())
		}
		k.data512.EncapsulateTo(ct, ss, seed)
		key, _ := SymmetricKeyFromDataRef(ss)
		return key, MLKEMCiphertext{level: k.level, data: ct}, nil
	case MLKEM768:
		ct := make([]byte, mlkem768.CiphertextSize)
		ss := make([]byte, mlkemSharedSecretSize)
		seed := make([]byte, mlkem768.EncapsulationSeedSize)
		if _, err := rand.Read(seed); err != nil {
			return SymmetricKey{}, MLKEMCiphertext{}, errPostQuantum(err.Error())
		}
		k.data768.EncapsulateTo(ct, ss, seed)
		key, _ := SymmetricKeyFromDataRef(ss)
		return key, MLKEMCiphertext{level: k.level, data: ct}, nil
	case MLKEM1024:
		ct := make([]byte, mlkem1024.CiphertextSize)
		ss := make([]byte, mlkemSharedSecretSize)
		seed := make([]byte, mlkem1024.EncapsulationSeedSize)
		if _, err := rand.Read(seed); err != nil {
			return SymmetricKey{}, MLKEMCiphertext{}, errPostQuantum(err.Error())
		}
		k.data1024.EncapsulateTo(ct, ss, seed)
		key, _ := SymmetricKeyFromDataRef(ss)
		return key, MLKEMCiphertext{level: k.level, data: ct}, nil
	default:
		return SymmetricKey{}, MLKEMCiphertext{}, errPostQuantum("unsupported MLKEM level")
	}
}

// MLKEMPublicKeyFromBytes creates an MLKEMPublicKey from raw bytes at the given level.
func MLKEMPublicKeyFromBytes(level MLKEM, data []byte) (MLKEMPublicKey, error) {
	switch level {
	case MLKEM512:
		var key mlkem512.PublicKey
		if err := key.Unpack(data); err != nil {
			return MLKEMPublicKey{}, errPostQuantum(err.Error())
		}
		return MLKEMPublicKey{level: level, data512: &key}, nil
	case MLKEM768:
		var key mlkem768.PublicKey
		if err := key.Unpack(data); err != nil {
			return MLKEMPublicKey{}, errPostQuantum(err.Error())
		}
		return MLKEMPublicKey{level: level, data768: &key}, nil
	case MLKEM1024:
		var key mlkem1024.PublicKey
		if err := key.Unpack(data); err != nil {
			return MLKEMPublicKey{}, errPostQuantum(err.Error())
		}
		return MLKEMPublicKey{level: level, data1024: &key}, nil
	default:
		return MLKEMPublicKey{}, errPostQuantum("unsupported MLKEM level")
	}
}

// String returns a human-readable representation.
func (k *MLKEMPublicKey) String() string {
	return fmt.Sprintf("MLKEMPublicKey(%s)", k.level)
}

// Equal reports whether two keys are equal.
func (k *MLKEMPublicKey) Equal(other *MLKEMPublicKey) bool {
	if k == nil || other == nil {
		return k == other
	}
	if k.level != other.level {
		return false
	}
	return bytes.Equal(k.Bytes(), other.Bytes())
}

// Reference implements ReferenceProvider.
func (k *MLKEMPublicKey) Reference() Reference {
	digest := DigestFromImage(k.Bytes())
	return ReferenceFromDigest(digest)
}

// --- CBOR support ---

func MLKEMPublicKeyCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagMLKEMPublicKey})
}

func (k *MLKEMPublicKey) CBORTags() []dcbor.Tag { return MLKEMPublicKeyCBORTags() }

func (k *MLKEMPublicKey) UntaggedCBOR() dcbor.CBOR {
	elements := []dcbor.CBOR{
		dcbor.MustFromAny(int64(k.level)),
		dcbor.ToByteString(k.Bytes()),
	}
	return dcbor.NewCBORArray(elements)
}

func (k *MLKEMPublicKey) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k *MLKEMPublicKey) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodeMLKEMPublicKey(cbor dcbor.CBOR) (*MLKEMPublicKey, error) {
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return nil, err
	}
	if len(elements) != 2 {
		return nil, dcbor.NewErrorf("MLKEMPublicKey must have 2 elements")
	}
	levelVal, ok := elements[0].AsInt64()
	if !ok {
		return nil, dcbor.NewErrorf("MLKEMPublicKey level must be integer")
	}
	level, err := MLKEMFromLevel(int(levelVal))
	if err != nil {
		return nil, err
	}
	data, err := elements[1].TryIntoByteString()
	if err != nil {
		return nil, err
	}
	key, err := MLKEMPublicKeyFromBytes(level, data)
	if err != nil {
		return nil, err
	}
	return &key, nil
}

func DecodeTaggedMLKEMPublicKey(cbor dcbor.CBOR) (*MLKEMPublicKey, error) {
	return dcbor.DecodeTagged(cbor, MLKEMPublicKeyCBORTags(), DecodeMLKEMPublicKey)
}

// --- MLKEMCiphertext ---

// MLKEMCiphertext is an ML-KEM encapsulated ciphertext.
type MLKEMCiphertext struct {
	level MLKEM
	data  []byte
}

// Level returns the security level.
func (c MLKEMCiphertext) Level() MLKEM { return c.level }

// Bytes returns the ciphertext data.
func (c MLKEMCiphertext) Bytes() []byte {
	cp := make([]byte, len(c.data))
	copy(cp, c.data)
	return cp
}

// Equal reports whether two ciphertexts are equal.
func (c MLKEMCiphertext) Equal(other MLKEMCiphertext) bool {
	return c.level == other.level && bytes.Equal(c.data, other.data)
}

// String returns a human-readable representation.
func (c MLKEMCiphertext) String() string {
	return fmt.Sprintf("MLKEMCiphertext(%s, %d bytes)", c.level, len(c.data))
}

// MLKEMCiphertextFromBytes creates an MLKEMCiphertext from raw bytes at the given level.
func MLKEMCiphertextFromBytes(level MLKEM, data []byte) MLKEMCiphertext {
	cp := make([]byte, len(data))
	copy(cp, data)
	return MLKEMCiphertext{level: level, data: cp}
}

// --- CBOR support ---

func MLKEMCiphertextCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagMLKEMCiphertext})
}

func (c MLKEMCiphertext) CBORTags() []dcbor.Tag { return MLKEMCiphertextCBORTags() }

func (c MLKEMCiphertext) UntaggedCBOR() dcbor.CBOR {
	elements := []dcbor.CBOR{
		dcbor.MustFromAny(int64(c.level)),
		dcbor.ToByteString(c.data),
	}
	return dcbor.NewCBORArray(elements)
}

func (c MLKEMCiphertext) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(c)
	return cbor
}

func (c MLKEMCiphertext) ToCBOR() dcbor.CBOR { return c.TaggedCBOR() }

func DecodeMLKEMCiphertext(cbor dcbor.CBOR) (MLKEMCiphertext, error) {
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return MLKEMCiphertext{}, err
	}
	if len(elements) != 2 {
		return MLKEMCiphertext{}, dcbor.NewErrorf("MLKEMCiphertext must have 2 elements")
	}
	levelVal, ok := elements[0].AsInt64()
	if !ok {
		return MLKEMCiphertext{}, dcbor.NewErrorf("MLKEMCiphertext level must be integer")
	}
	level, err := MLKEMFromLevel(int(levelVal))
	if err != nil {
		return MLKEMCiphertext{}, err
	}
	data, err := elements[1].TryIntoByteString()
	if err != nil {
		return MLKEMCiphertext{}, err
	}
	return MLKEMCiphertextFromBytes(level, data), nil
}

func DecodeTaggedMLKEMCiphertext(cbor dcbor.CBOR) (MLKEMCiphertext, error) {
	return dcbor.DecodeTagged(cbor, MLKEMCiphertextCBORTags(), DecodeMLKEMCiphertext)
}
