package bccomponents

import (
	"bytes"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// EncryptedMessage is a message encrypted with ChaCha20-Poly1305 AEAD (RFC 8439).
// It contains ciphertext, optional AAD, a nonce, and an authentication tag.
type EncryptedMessage struct {
	ciphertext []byte
	aad        []byte
	nonce      Nonce
	auth       AuthenticationTag
}

// NewEncryptedMessage creates an EncryptedMessage from its components.
func NewEncryptedMessage(ciphertext, aad []byte, nonce Nonce, auth AuthenticationTag) EncryptedMessage {
	ct := make([]byte, len(ciphertext))
	copy(ct, ciphertext)
	ad := make([]byte, len(aad))
	copy(ad, aad)
	return EncryptedMessage{ciphertext: ct, aad: ad, nonce: nonce, auth: auth}
}

// Ciphertext returns the encrypted data.
func (m *EncryptedMessage) Ciphertext() []byte { return m.ciphertext }

// AAD returns the additional authenticated data.
func (m *EncryptedMessage) AAD() []byte { return m.aad }

// Nonce returns the nonce used for encryption.
func (m *EncryptedMessage) Nonce() Nonce { return m.nonce }

// AuthenticationTag returns the Poly1305 authentication tag.
func (m *EncryptedMessage) AuthenticationTag() AuthenticationTag { return m.auth }

// AADAsCBOR returns the AAD parsed as CBOR, or nil if empty or invalid.
func (m *EncryptedMessage) AADAsCBOR() *dcbor.CBOR {
	if len(m.aad) == 0 {
		return nil
	}
	cbor, err := dcbor.TryFromData(m.aad)
	if err != nil {
		return nil
	}
	return &cbor
}

// AADDigest extracts a Digest from the AAD if it can be parsed as tagged CBOR.
func (m *EncryptedMessage) AADDigest() *Digest {
	aadCBOR := m.AADAsCBOR()
	if aadCBOR == nil {
		return nil
	}
	d, err := DecodeTaggedDigest(*aadCBOR)
	if err != nil {
		return nil
	}
	return &d
}

// HasDigest returns true if the AAD contains a valid Digest.
func (m *EncryptedMessage) HasDigest() bool { return m.AADDigest() != nil }

// Digest implements DigestProvider. Panics if no digest is available.
func (m *EncryptedMessage) Digest() Digest {
	d := m.AADDigest()
	if d == nil {
		panic("bccomponents: EncryptedMessage has no digest in AAD")
	}
	return *d
}

// Equal reports whether two encrypted messages are equal.
func (m *EncryptedMessage) Equal(other *EncryptedMessage) bool {
	if m == nil || other == nil {
		return m == other
	}
	if !bytes.Equal(m.ciphertext, other.ciphertext) {
		return false
	}
	if !bytes.Equal(m.aad, other.aad) {
		return false
	}
	return m.nonce.Equal(other.nonce) && m.auth.Equal(other.auth)
}

// --- CBOR support ---

// EncryptedMessageCBORTags returns the CBOR tags used for EncryptedMessage.
func EncryptedMessageCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagEncrypted})
}

// CBORTags implements dcbor.CBORTagged.
func (m *EncryptedMessage) CBORTags() []dcbor.Tag { return EncryptedMessageCBORTags() }

// UntaggedCBOR implements dcbor.CBORTaggedEncodable.
func (m *EncryptedMessage) UntaggedCBOR() dcbor.CBOR {
	elements := []dcbor.CBOR{
		dcbor.ToByteString(m.ciphertext),
		dcbor.ToByteString(m.nonce.Bytes()),
		dcbor.ToByteString(m.auth.Bytes()),
	}
	if len(m.aad) > 0 {
		elements = append(elements, dcbor.ToByteString(m.aad))
	}
	return dcbor.NewCBORArray(elements)
}

// TaggedCBOR returns the tagged CBOR encoding of the EncryptedMessage.
func (m *EncryptedMessage) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(m)
	return cbor
}

// ToCBOR implements dcbor.CBOREncodable.
func (m *EncryptedMessage) ToCBOR() dcbor.CBOR { return m.TaggedCBOR() }

// DecodeEncryptedMessage decodes an EncryptedMessage from untagged CBOR.
func DecodeEncryptedMessage(cbor dcbor.CBOR) (*EncryptedMessage, error) {
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return nil, dcbor.NewErrorf("EncryptedMessage must be an array")
	}
	if len(elements) < 3 {
		return nil, dcbor.NewErrorf("EncryptedMessage must have at least 3 elements")
	}

	ciphertext, err := elements[0].TryIntoByteString()
	if err != nil {
		return nil, err
	}
	nonceData, err := elements[1].TryIntoByteString()
	if err != nil {
		return nil, err
	}
	nonce, err := NonceFromDataRef(nonceData)
	if err != nil {
		return nil, err
	}
	authData, err := elements[2].TryIntoByteString()
	if err != nil {
		return nil, err
	}
	auth, err := AuthenticationTagFromDataRef(authData)
	if err != nil {
		return nil, err
	}

	var aad []byte
	if len(elements) > 3 {
		aad, err = elements[3].TryIntoByteString()
		if err != nil {
			return nil, err
		}
	}

	msg := NewEncryptedMessage(ciphertext, aad, nonce, auth)
	return &msg, nil
}

// DecodeTaggedEncryptedMessage decodes an EncryptedMessage from tagged CBOR.
func DecodeTaggedEncryptedMessage(cbor dcbor.CBOR) (*EncryptedMessage, error) {
	return dcbor.DecodeTagged(cbor, EncryptedMessageCBORTags(), DecodeEncryptedMessage)
}

// --- UR support ---

// EncryptedMessageToURString encodes an EncryptedMessage as a UR string.
func EncryptedMessageToURString(m *EncryptedMessage) string { return bcur.ToURString(m) }

// EncryptedMessageFromURString decodes an EncryptedMessage from a UR string.
func EncryptedMessageFromURString(urString string) (*EncryptedMessage, error) {
	return bcur.DecodeURString(urString, EncryptedMessageCBORTags(), DecodeEncryptedMessage)
}
