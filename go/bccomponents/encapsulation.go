package bccomponents

import (
	"fmt"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// EncapsulationScheme identifies the key encapsulation mechanism.
type EncapsulationScheme int

const (
	EncapsulationX25519   EncapsulationScheme = iota
	EncapsulationMLKEM512
	EncapsulationMLKEM768
	EncapsulationMLKEM1024
)

// String returns the scheme name.
func (s EncapsulationScheme) String() string {
	switch s {
	case EncapsulationX25519:
		return "X25519"
	case EncapsulationMLKEM512:
		return "MLKEM512"
	case EncapsulationMLKEM768:
		return "MLKEM768"
	case EncapsulationMLKEM1024:
		return "MLKEM1024"
	default:
		return "Unknown"
	}
}

// EncapsulationKeypair generates a new encapsulation keypair for the given scheme.
func EncapsulationKeypair(scheme EncapsulationScheme) (EncapsulationPrivateKey, EncapsulationPublicKey, error) {
	switch scheme {
	case EncapsulationX25519:
		priv, pub := X25519Keypair()
		return EncapsulationPrivateKeyFromX25519(priv), EncapsulationPublicKeyFromX25519(pub), nil
	case EncapsulationMLKEM512:
		priv, pub, err := MLKEM512.Keypair()
		if err != nil {
			return EncapsulationPrivateKey{}, EncapsulationPublicKey{}, err
		}
		return EncapsulationPrivateKeyFromMLKEM(&priv), EncapsulationPublicKeyFromMLKEM(&pub), nil
	case EncapsulationMLKEM768:
		priv, pub, err := MLKEM768.Keypair()
		if err != nil {
			return EncapsulationPrivateKey{}, EncapsulationPublicKey{}, err
		}
		return EncapsulationPrivateKeyFromMLKEM(&priv), EncapsulationPublicKeyFromMLKEM(&pub), nil
	case EncapsulationMLKEM1024:
		priv, pub, err := MLKEM1024.Keypair()
		if err != nil {
			return EncapsulationPrivateKey{}, EncapsulationPublicKey{}, err
		}
		return EncapsulationPrivateKeyFromMLKEM(&priv), EncapsulationPublicKeyFromMLKEM(&pub), nil
	default:
		return EncapsulationPrivateKey{}, EncapsulationPublicKey{}, errGeneral("unsupported encapsulation scheme")
	}
}

// --- EncapsulationCiphertext ---

// encapCTType discriminates ciphertext variants.
type encapCTType int

const (
	encapCTX25519 encapCTType = iota
	encapCTMLKEM
)

// EncapsulationCiphertext holds the result of key encapsulation.
type EncapsulationCiphertext struct {
	ctType  encapCTType
	x25519  *X25519PublicKey
	mlkem   *MLKEMCiphertext
}

// EncapsulationCiphertextFromX25519 creates an X25519 encapsulation ciphertext.
func EncapsulationCiphertextFromX25519(ephemeralPub X25519PublicKey) EncapsulationCiphertext {
	return EncapsulationCiphertext{ctType: encapCTX25519, x25519: &ephemeralPub}
}

// EncapsulationCiphertextFromMLKEM creates an ML-KEM encapsulation ciphertext.
func EncapsulationCiphertextFromMLKEM(ct MLKEMCiphertext) EncapsulationCiphertext {
	return EncapsulationCiphertext{ctType: encapCTMLKEM, mlkem: &ct}
}

// IsX25519 reports whether this is an X25519 ciphertext.
func (c EncapsulationCiphertext) IsX25519() bool { return c.ctType == encapCTX25519 }

// IsMLKEM reports whether this is an ML-KEM ciphertext.
func (c EncapsulationCiphertext) IsMLKEM() bool { return c.ctType == encapCTMLKEM }

// X25519PublicKey returns the ephemeral X25519 public key, or nil.
func (c EncapsulationCiphertext) X25519Key() *X25519PublicKey { return c.x25519 }

// MLKEMCiphertext returns the ML-KEM ciphertext, or nil.
func (c EncapsulationCiphertext) MLKEMCipher() *MLKEMCiphertext { return c.mlkem }

// EncapsulationScheme returns the encapsulation scheme used.
func (c EncapsulationCiphertext) EncapsulationScheme() EncapsulationScheme {
	if c.ctType == encapCTX25519 {
		return EncapsulationX25519
	}
	if c.mlkem != nil {
		switch c.mlkem.level {
		case MLKEM512:
			return EncapsulationMLKEM512
		case MLKEM768:
			return EncapsulationMLKEM768
		case MLKEM1024:
			return EncapsulationMLKEM1024
		}
	}
	return EncapsulationX25519
}

// String returns a human-readable representation.
func (c EncapsulationCiphertext) String() string {
	return fmt.Sprintf("EncapsulationCiphertext(%s)", c.EncapsulationScheme())
}

// ToCBOR encodes the ciphertext as CBOR.
func (c EncapsulationCiphertext) ToCBOR() dcbor.CBOR {
	if c.ctType == encapCTX25519 && c.x25519 != nil {
		return c.x25519.TaggedCBOR()
	}
	if c.ctType == encapCTMLKEM && c.mlkem != nil {
		return c.mlkem.TaggedCBOR()
	}
	return dcbor.ToByteString(nil)
}

// DecodeEncapsulationCiphertext decodes an encapsulation ciphertext from CBOR.
func DecodeEncapsulationCiphertext(cbor dcbor.CBOR) (EncapsulationCiphertext, error) {
	tag, _, ok := cbor.AsTaggedValue()
	if !ok {
		return EncapsulationCiphertext{}, dcbor.NewErrorf("expected tagged CBOR for encapsulation ciphertext")
	}
	if tag.Value() == bctags.TagX25519PublicKey {
		key, err := DecodeTaggedX25519PublicKey(cbor)
		if err != nil {
			return EncapsulationCiphertext{}, err
		}
		return EncapsulationCiphertextFromX25519(key), nil
	}
	if tag.Value() == bctags.TagMLKEMCiphertext {
		ct, err := DecodeTaggedMLKEMCiphertext(cbor)
		if err != nil {
			return EncapsulationCiphertext{}, err
		}
		return EncapsulationCiphertextFromMLKEM(ct), nil
	}
	return EncapsulationCiphertext{}, dcbor.NewErrorf("unknown encapsulation ciphertext tag: %d", tag.Value())
}

// --- EncapsulationPrivateKey ---

// encapKeyType discriminates encapsulation key variants.
type encapKeyType int

const (
	encapKeyX25519 encapKeyType = iota
	encapKeyMLKEM
)

// EncapsulationPrivateKey is a private key for key decapsulation.
type EncapsulationPrivateKey struct {
	ekType  encapKeyType
	x25519  *X25519PrivateKey
	mlkem   *MLKEMPrivateKey
}

// EncapsulationPrivateKeyFromX25519 creates an X25519 encapsulation private key.
func EncapsulationPrivateKeyFromX25519(key X25519PrivateKey) EncapsulationPrivateKey {
	return EncapsulationPrivateKey{ekType: encapKeyX25519, x25519: &key}
}

// EncapsulationPrivateKeyFromMLKEM creates an ML-KEM encapsulation private key.
func EncapsulationPrivateKeyFromMLKEM(key *MLKEMPrivateKey) EncapsulationPrivateKey {
	return EncapsulationPrivateKey{ekType: encapKeyMLKEM, mlkem: key}
}

// DecapsulateSharedSecret recovers a shared secret from a ciphertext.
func (k EncapsulationPrivateKey) DecapsulateSharedSecret(ct EncapsulationCiphertext) (SymmetricKey, error) {
	switch k.ekType {
	case encapKeyX25519:
		if ct.ctType != encapCTX25519 || ct.x25519 == nil {
			return SymmetricKey{}, errGeneral("X25519 key cannot decapsulate non-X25519 ciphertext")
		}
		shared := k.x25519.SharedKeyWith(*ct.x25519)
		return shared, nil
	case encapKeyMLKEM:
		if ct.ctType != encapCTMLKEM || ct.mlkem == nil {
			return SymmetricKey{}, errGeneral("MLKEM key cannot decapsulate non-MLKEM ciphertext")
		}
		return k.mlkem.DecapsulateSharedSecret(*ct.mlkem)
	default:
		return SymmetricKey{}, errGeneral("unsupported encapsulation key type")
	}
}

// PublicKey derives the corresponding encapsulation public key.
func (k EncapsulationPrivateKey) PublicKey() EncapsulationPublicKey {
	switch k.ekType {
	case encapKeyX25519:
		pub := k.x25519.PublicKey()
		return EncapsulationPublicKeyFromX25519(pub)
	case encapKeyMLKEM:
		// ML-KEM public key must be derived during keypair generation.
		return EncapsulationPublicKey{}
	default:
		return EncapsulationPublicKey{}
	}
}

// Scheme returns the encapsulation scheme.
func (k EncapsulationPrivateKey) Scheme() EncapsulationScheme {
	switch k.ekType {
	case encapKeyX25519:
		return EncapsulationX25519
	case encapKeyMLKEM:
		if k.mlkem != nil {
			switch k.mlkem.level {
			case MLKEM512:
				return EncapsulationMLKEM512
			case MLKEM768:
				return EncapsulationMLKEM768
			case MLKEM1024:
				return EncapsulationMLKEM1024
			}
		}
	}
	return EncapsulationX25519
}

// String returns a human-readable representation.
func (k EncapsulationPrivateKey) String() string {
	return fmt.Sprintf("EncapsulationPrivateKey(%s)", k.Scheme())
}

// Reference implements ReferenceProvider.
func (k EncapsulationPrivateKey) Reference() Reference {
	return ReferenceForCBORTaggedEncodable(k)
}

// --- CBOR support for EncapsulationPrivateKey ---

func (k EncapsulationPrivateKey) CBORTags() []dcbor.Tag {
	switch k.ekType {
	case encapKeyX25519:
		return X25519PrivateKeyCBORTags()
	case encapKeyMLKEM:
		return MLKEMPrivateKeyCBORTags()
	default:
		return X25519PrivateKeyCBORTags()
	}
}

func (k EncapsulationPrivateKey) UntaggedCBOR() dcbor.CBOR {
	switch k.ekType {
	case encapKeyX25519:
		return k.x25519.UntaggedCBOR()
	case encapKeyMLKEM:
		return k.mlkem.UntaggedCBOR()
	default:
		return dcbor.ToByteString(nil)
	}
}

func (k EncapsulationPrivateKey) TaggedCBOR() dcbor.CBOR {
	switch k.ekType {
	case encapKeyX25519:
		return k.x25519.TaggedCBOR()
	case encapKeyMLKEM:
		return k.mlkem.TaggedCBOR()
	default:
		return dcbor.ToByteString(nil)
	}
}

func (k EncapsulationPrivateKey) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodeEncapsulationPrivateKey(cbor dcbor.CBOR) (EncapsulationPrivateKey, error) {
	tag, _, ok := cbor.AsTaggedValue()
	if !ok {
		return EncapsulationPrivateKey{}, dcbor.NewErrorf("expected tagged CBOR for encapsulation private key")
	}
	if tag.Value() == bctags.TagX25519PrivateKey {
		key, err := DecodeTaggedX25519PrivateKey(cbor)
		if err != nil {
			return EncapsulationPrivateKey{}, err
		}
		return EncapsulationPrivateKeyFromX25519(key), nil
	}
	if tag.Value() == bctags.TagMLKEMPrivateKey {
		key, err := DecodeTaggedMLKEMPrivateKey(cbor)
		if err != nil {
			return EncapsulationPrivateKey{}, err
		}
		return EncapsulationPrivateKeyFromMLKEM(key), nil
	}
	return EncapsulationPrivateKey{}, dcbor.NewErrorf("unknown encapsulation private key tag: %d", tag.Value())
}

// --- EncapsulationPublicKey ---

// EncapsulationPublicKey is a public key for key encapsulation.
type EncapsulationPublicKey struct {
	ekType  encapKeyType
	x25519  *X25519PublicKey
	mlkem   *MLKEMPublicKey
}

// EncapsulationPublicKeyFromX25519 creates an X25519 encapsulation public key.
func EncapsulationPublicKeyFromX25519(key X25519PublicKey) EncapsulationPublicKey {
	return EncapsulationPublicKey{ekType: encapKeyX25519, x25519: &key}
}

// EncapsulationPublicKeyFromMLKEM creates an ML-KEM encapsulation public key.
func EncapsulationPublicKeyFromMLKEM(key *MLKEMPublicKey) EncapsulationPublicKey {
	return EncapsulationPublicKey{ekType: encapKeyMLKEM, mlkem: key}
}

// EncapsulateNewSharedSecret generates a new shared secret and ciphertext.
func (k EncapsulationPublicKey) EncapsulateNewSharedSecret() (SymmetricKey, EncapsulationCiphertext, error) {
	switch k.ekType {
	case encapKeyX25519:
		ephPriv, ephPub := X25519Keypair()
		shared := ephPriv.SharedKeyWith(*k.x25519)
		ct := EncapsulationCiphertextFromX25519(ephPub)
		return shared, ct, nil
	case encapKeyMLKEM:
		ss, mlkemCT, err := k.mlkem.EncapsulateNewSharedSecret()
		if err != nil {
			return SymmetricKey{}, EncapsulationCiphertext{}, err
		}
		ct := EncapsulationCiphertextFromMLKEM(mlkemCT)
		return ss, ct, nil
	default:
		return SymmetricKey{}, EncapsulationCiphertext{}, errGeneral("unsupported encapsulation key type")
	}
}

// Scheme returns the encapsulation scheme.
func (k EncapsulationPublicKey) Scheme() EncapsulationScheme {
	switch k.ekType {
	case encapKeyX25519:
		return EncapsulationX25519
	case encapKeyMLKEM:
		if k.mlkem != nil {
			switch k.mlkem.level {
			case MLKEM512:
				return EncapsulationMLKEM512
			case MLKEM768:
				return EncapsulationMLKEM768
			case MLKEM1024:
				return EncapsulationMLKEM1024
			}
		}
	}
	return EncapsulationX25519
}

// String returns a human-readable representation.
func (k EncapsulationPublicKey) String() string {
	return fmt.Sprintf("EncapsulationPublicKey(%s)", k.Scheme())
}

// Reference implements ReferenceProvider.
func (k EncapsulationPublicKey) Reference() Reference {
	return ReferenceForCBORTaggedEncodable(k)
}

// --- CBOR support for EncapsulationPublicKey ---

func (k EncapsulationPublicKey) CBORTags() []dcbor.Tag {
	switch k.ekType {
	case encapKeyX25519:
		return X25519PublicKeyCBORTags()
	case encapKeyMLKEM:
		return MLKEMPublicKeyCBORTags()
	default:
		return X25519PublicKeyCBORTags()
	}
}

func (k EncapsulationPublicKey) UntaggedCBOR() dcbor.CBOR {
	switch k.ekType {
	case encapKeyX25519:
		return k.x25519.UntaggedCBOR()
	case encapKeyMLKEM:
		return k.mlkem.UntaggedCBOR()
	default:
		return dcbor.ToByteString(nil)
	}
}

func (k EncapsulationPublicKey) TaggedCBOR() dcbor.CBOR {
	switch k.ekType {
	case encapKeyX25519:
		return k.x25519.TaggedCBOR()
	case encapKeyMLKEM:
		return k.mlkem.TaggedCBOR()
	default:
		return dcbor.ToByteString(nil)
	}
}

func (k EncapsulationPublicKey) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodeEncapsulationPublicKey(cbor dcbor.CBOR) (EncapsulationPublicKey, error) {
	tag, _, ok := cbor.AsTaggedValue()
	if !ok {
		return EncapsulationPublicKey{}, dcbor.NewErrorf("expected tagged CBOR for encapsulation public key")
	}
	if tag.Value() == bctags.TagX25519PublicKey {
		key, err := DecodeTaggedX25519PublicKey(cbor)
		if err != nil {
			return EncapsulationPublicKey{}, err
		}
		return EncapsulationPublicKeyFromX25519(key), nil
	}
	if tag.Value() == bctags.TagMLKEMPublicKey {
		key, err := DecodeTaggedMLKEMPublicKey(cbor)
		if err != nil {
			return EncapsulationPublicKey{}, err
		}
		return EncapsulationPublicKeyFromMLKEM(key), nil
	}
	return EncapsulationPublicKey{}, dcbor.NewErrorf("unknown encapsulation public key tag: %d", tag.Value())
}

// --- SealedMessage ---

// SealedMessage combines an encrypted message with key encapsulation.
type SealedMessage struct {
	message  *EncryptedMessage
	ciphertext EncapsulationCiphertext
}

// NewSealedMessage encrypts plaintext for a recipient using their public key.
func NewSealedMessage(plaintext []byte, recipient EncapsulationPublicKey) (*SealedMessage, error) {
	return NewSealedMessageOpt(plaintext, recipient, nil, nil)
}

// NewSealedMessageWithAAD encrypts plaintext with additional authenticated data.
func NewSealedMessageWithAAD(plaintext []byte, recipient EncapsulationPublicKey, aad []byte) (*SealedMessage, error) {
	return NewSealedMessageOpt(plaintext, recipient, aad, nil)
}

// NewSealedMessageOpt encrypts plaintext with optional AAD and nonce (for testing).
func NewSealedMessageOpt(plaintext []byte, recipient EncapsulationPublicKey, aad []byte, testNonce *Nonce) (*SealedMessage, error) {
	sharedKey, ct, err := recipient.EncapsulateNewSharedSecret()
	if err != nil {
		return nil, err
	}
	encrypted := sharedKey.Encrypt(plaintext, aad, testNonce)
	return &SealedMessage{message: &encrypted, ciphertext: ct}, nil
}

// Decrypt decrypts the sealed message using the recipient's private key.
func (m *SealedMessage) Decrypt(privKey EncapsulationPrivateKey) ([]byte, error) {
	sharedKey, err := privKey.DecapsulateSharedSecret(m.ciphertext)
	if err != nil {
		return nil, err
	}
	return sharedKey.Decrypt(m.message)
}

// EncryptedMessage returns the encrypted message.
func (m *SealedMessage) EncryptedMessage() *EncryptedMessage { return m.message }

// Ciphertext returns the encapsulation ciphertext.
func (m *SealedMessage) Ciphertext() EncapsulationCiphertext { return m.ciphertext }

// EncapsulationScheme returns the scheme used.
func (m *SealedMessage) EncapsulationScheme() EncapsulationScheme {
	return m.ciphertext.EncapsulationScheme()
}

// String returns a human-readable representation.
func (m *SealedMessage) String() string {
	return fmt.Sprintf("SealedMessage(%s)", m.EncapsulationScheme())
}

// --- CBOR support for SealedMessage ---

func SealedMessageCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagSealedMessage})
}

func (m *SealedMessage) CBORTags() []dcbor.Tag { return SealedMessageCBORTags() }

func (m *SealedMessage) UntaggedCBOR() dcbor.CBOR {
	elements := []dcbor.CBOR{
		m.message.TaggedCBOR(),
		m.ciphertext.ToCBOR(),
	}
	return dcbor.NewCBORArray(elements)
}

func (m *SealedMessage) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(m)
	return cbor
}

func (m *SealedMessage) ToCBOR() dcbor.CBOR { return m.TaggedCBOR() }

func DecodeSealedMessage(cbor dcbor.CBOR) (*SealedMessage, error) {
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return nil, err
	}
	if len(elements) != 2 {
		return nil, dcbor.NewErrorf("SealedMessage must have 2 elements")
	}
	msg, err := DecodeTaggedEncryptedMessage(elements[0])
	if err != nil {
		return nil, err
	}
	ct, err := DecodeEncapsulationCiphertext(elements[1])
	if err != nil {
		return nil, err
	}
	return &SealedMessage{message: msg, ciphertext: ct}, nil
}

func DecodeTaggedSealedMessage(cbor dcbor.CBOR) (*SealedMessage, error) {
	return dcbor.DecodeTagged(cbor, SealedMessageCBORTags(), DecodeSealedMessage)
}

// --- UR support for SealedMessage ---

func SealedMessageToURString(m *SealedMessage) string { return bcur.ToURString(m) }

func SealedMessageFromURString(urString string) (*SealedMessage, error) {
	return bcur.DecodeURString(urString, SealedMessageCBORTags(), DecodeSealedMessage)
}
