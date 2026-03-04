package bccomponents

import (
	"encoding/hex"
	"fmt"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const SymmetricKeySize = 32

// SymmetricKey is a 32-byte symmetric encryption key for ChaCha20-Poly1305 AEAD.
type SymmetricKey struct {
	data [SymmetricKeySize]byte
}

// NewSymmetricKey creates a new random symmetric key using the system CSPRNG.
func NewSymmetricKey() SymmetricKey {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return NewSymmetricKeyUsing(rng)
}

// NewSymmetricKeyUsing creates a new random symmetric key using the provided RNG.
func NewSymmetricKeyUsing(rng bcrand.RandomNumberGenerator) SymmetricKey {
	var k SymmetricKey
	rng.FillRandomData(k.data[:])
	return k
}

// SymmetricKeyFromData creates a SymmetricKey from a 32-byte array.
func SymmetricKeyFromData(data [SymmetricKeySize]byte) SymmetricKey {
	return SymmetricKey{data: data}
}

// SymmetricKeyFromDataRef creates a SymmetricKey from a byte slice.
func SymmetricKeyFromDataRef(data []byte) (SymmetricKey, error) {
	if len(data) != SymmetricKeySize {
		return SymmetricKey{}, errInvalidSize("symmetric key", SymmetricKeySize, len(data))
	}
	var k SymmetricKey
	copy(k.data[:], data)
	return k, nil
}

// SymmetricKeyFromHex creates a SymmetricKey from a hex string.
func SymmetricKeyFromHex(h string) (SymmetricKey, error) {
	data, err := hex.DecodeString(h)
	if err != nil {
		return SymmetricKey{}, fmt.Errorf("bccomponents: invalid symmetric key hex: %w", err)
	}
	return SymmetricKeyFromDataRef(data)
}

// Data returns the 32-byte key data.
func (k SymmetricKey) Data() [SymmetricKeySize]byte { return k.data }

// Bytes returns the key as a byte slice.
func (k SymmetricKey) Bytes() []byte { return k.data[:] }

// Hex returns the key as a 64-character hex string.
func (k SymmetricKey) Hex() string { return hex.EncodeToString(k.data[:]) }

// Encrypt encrypts plaintext with this key using ChaCha20-Poly1305.
func (k SymmetricKey) Encrypt(plaintext, aad []byte, nonce *Nonce) EncryptedMessage {
	n := NonceFromData([NonceSize]byte{})
	if nonce != nil {
		n = *nonce
	} else {
		n = NewNonce()
	}
	nonceData := n.Data()

	var ciphertext []byte
	var auth [bccrypto.SymmetricAuthSize]byte
	if len(aad) > 0 {
		ciphertext, auth = bccrypto.AEADChaCha20Poly1305EncryptWithAAD(plaintext, k.data, nonceData, aad)
	} else {
		ciphertext, auth = bccrypto.AEADChaCha20Poly1305Encrypt(plaintext, k.data, nonceData)
	}

	return NewEncryptedMessage(ciphertext, aad, n, AuthenticationTagFromData(auth))
}

// EncryptWithDigest encrypts plaintext using the digest's tagged CBOR as AAD.
func (k SymmetricKey) EncryptWithDigest(plaintext []byte, digest Digest, nonce *Nonce) EncryptedMessage {
	aad := digest.TaggedCBOR().ToCBORData()
	return k.Encrypt(plaintext, aad, nonce)
}

// Decrypt decrypts an EncryptedMessage with this key.
func (k SymmetricKey) Decrypt(msg *EncryptedMessage) ([]byte, error) {
	nonceData := msg.Nonce().Data()
	authData := msg.AuthenticationTag().Data()
	var plaintext []byte
	var err error
	if len(msg.AAD()) > 0 {
		plaintext, err = bccrypto.AEADChaCha20Poly1305DecryptWithAAD(msg.Ciphertext(), k.data, nonceData, msg.AAD(), authData)
	} else {
		plaintext, err = bccrypto.AEADChaCha20Poly1305Decrypt(msg.Ciphertext(), k.data, nonceData, authData)
	}
	if err != nil {
		return nil, errCrypto(err.Error())
	}
	return plaintext, nil
}

// String returns a human-readable representation.
func (k SymmetricKey) String() string {
	return fmt.Sprintf("SymmetricKey(%s)", k.Hex())
}

// Equal reports whether two keys are equal.
func (k SymmetricKey) Equal(other SymmetricKey) bool { return k.data == other.data }

// Reference implements ReferenceProvider.
func (k SymmetricKey) Reference() Reference {
	return ReferenceForCBORTaggedEncodable(k)
}

// --- CBOR support ---

func SymmetricKeyCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagSymmetricKey})
}

func (k SymmetricKey) CBORTags() []dcbor.Tag   { return SymmetricKeyCBORTags() }
func (k SymmetricKey) UntaggedCBOR() dcbor.CBOR { return dcbor.ToByteString(k.data[:]) }

func (k SymmetricKey) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k SymmetricKey) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodeSymmetricKey(cbor dcbor.CBOR) (SymmetricKey, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return SymmetricKey{}, err
	}
	return SymmetricKeyFromDataRef(data)
}

func DecodeTaggedSymmetricKey(cbor dcbor.CBOR) (SymmetricKey, error) {
	return dcbor.DecodeTagged(cbor, SymmetricKeyCBORTags(), DecodeSymmetricKey)
}
