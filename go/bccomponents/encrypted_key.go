package bccomponents

import (
	"fmt"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// EncryptedKey wraps a SymmetricKey encrypted using a secret-derived key.
// The key derivation method and parameters are encoded as CBOR in the
// EncryptedMessage's AAD field.
type EncryptedKey struct {
	params           KeyDerivationParams
	encryptedMessage EncryptedMessage
}

// LockEncryptedKey creates an EncryptedKey using the given method with default parameters.
func LockEncryptedKey(method KeyDerivationMethod, secret []byte, contentKey SymmetricKey) (*EncryptedKey, error) {
	var params KeyDerivationParams
	switch method {
	case KDMethodHKDF:
		params = NewKDParamsHKDF(NewHKDFParams())
	case KDMethodPBKDF2:
		params = NewKDParamsPBKDF2(NewPBKDF2Params())
	case KDMethodScrypt:
		params = NewKDParamsScrypt(NewScryptParams())
	case KDMethodArgon2id:
		params = NewKDParamsArgon2id(NewArgon2idParams())
	default:
		return nil, errGeneral("unsupported key derivation method")
	}
	return LockEncryptedKeyOpt(params, secret, contentKey)
}

// LockEncryptedKeyOpt creates an EncryptedKey with custom parameters.
func LockEncryptedKeyOpt(params KeyDerivationParams, secret []byte, contentKey SymmetricKey) (*EncryptedKey, error) {
	msg, err := params.Lock(contentKey, secret)
	if err != nil {
		return nil, err
	}
	return &EncryptedKey{params: params, encryptedMessage: msg}, nil
}

// Unlock decrypts the encrypted key using the secret.
func (ek *EncryptedKey) Unlock(secret []byte) (SymmetricKey, error) {
	aadCBOR, err := ek.AADCBOR()
	if err != nil {
		return SymmetricKey{}, err
	}
	a, err := aadCBOR.TryIntoArray()
	if err != nil {
		return SymmetricKey{}, err
	}
	if len(a) == 0 {
		return SymmetricKey{}, errGeneral("Missing method in AAD")
	}
	method, err := kdMethodFromCBOR(a[0])
	if err != nil {
		return SymmetricKey{}, err
	}

	// Re-create the CBOR array to pass to the params decoder for unlock.
	paramsCBOR := dcbor.NewCBORArray(a)
	msg := &ek.encryptedMessage

	switch method {
	case KDMethodHKDF:
		p, err := hkdfParamsFromCBOR(paramsCBOR)
		if err != nil {
			return SymmetricKey{}, err
		}
		return p.Unlock(msg, secret)
	case KDMethodPBKDF2:
		p, err := pbkdf2ParamsFromCBOR(paramsCBOR)
		if err != nil {
			return SymmetricKey{}, err
		}
		return p.Unlock(msg, secret)
	case KDMethodScrypt:
		p, err := scryptParamsFromCBOR(paramsCBOR)
		if err != nil {
			return SymmetricKey{}, err
		}
		return p.Unlock(msg, secret)
	case KDMethodArgon2id:
		p, err := argon2idParamsFromCBOR(paramsCBOR)
		if err != nil {
			return SymmetricKey{}, err
		}
		return p.Unlock(msg, secret)
	default:
		return SymmetricKey{}, errGeneral("unsupported key derivation method")
	}
}

// EncryptedMessage returns the underlying encrypted message.
func (ek *EncryptedKey) EncryptedMessage() *EncryptedMessage { return &ek.encryptedMessage }

// AADCBOR parses the AAD of the encrypted message as CBOR.
func (ek *EncryptedKey) AADCBOR() (dcbor.CBOR, error) {
	c := ek.encryptedMessage.AADAsCBOR()
	if c == nil {
		return dcbor.CBOR{}, errGeneral("Missing AAD CBOR in EncryptedMessage")
	}
	return *c, nil
}

// IsPasswordBased returns true if the derivation method is password-based.
func (ek *EncryptedKey) IsPasswordBased() bool { return ek.params.IsPasswordBased() }

// IsSSHAgent returns true if the derivation method is SSH agent.
func (ek *EncryptedKey) IsSSHAgent() bool { return ek.params.IsSSHAgent() }

// String returns a human-readable representation.
func (ek *EncryptedKey) String() string {
	return fmt.Sprintf("EncryptedKey(%s)", ek.params.String())
}

// --- CBOR support ---

// EncryptedKeyCBORTags returns the CBOR tags for EncryptedKey.
func EncryptedKeyCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagEncryptedKey})
}

// CBORTags implements CBORTagged.
func (ek *EncryptedKey) CBORTags() []dcbor.Tag { return EncryptedKeyCBORTags() }

// UntaggedCBOR returns the EncryptedMessage as tagged CBOR (the untagged content
// of an EncryptedKey is a tagged EncryptedMessage).
func (ek *EncryptedKey) UntaggedCBOR() dcbor.CBOR {
	return ek.encryptedMessage.TaggedCBOR()
}

// TaggedCBOR returns the full tagged CBOR encoding.
func (ek *EncryptedKey) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(ek)
	return cbor
}

// ToCBOR implements dcbor.CBOREncodable.
func (ek *EncryptedKey) ToCBOR() dcbor.CBOR { return ek.TaggedCBOR() }

// DecodeEncryptedKey decodes an EncryptedKey from untagged CBOR.
// The untagged CBOR is expected to be a tagged EncryptedMessage.
func DecodeEncryptedKey(cbor dcbor.CBOR) (*EncryptedKey, error) {
	msg, err := DecodeTaggedEncryptedMessage(cbor)
	if err != nil {
		return nil, err
	}
	paramsCBOR, err := dcbor.TryFromData(msg.AAD())
	if err != nil {
		return nil, err
	}
	params, err := kdParamsFromCBOR(paramsCBOR)
	if err != nil {
		return nil, err
	}
	return &EncryptedKey{params: params, encryptedMessage: *msg}, nil
}

// DecodeTaggedEncryptedKey decodes an EncryptedKey from tagged CBOR.
func DecodeTaggedEncryptedKey(cbor dcbor.CBOR) (*EncryptedKey, error) {
	return dcbor.DecodeTagged(cbor, EncryptedKeyCBORTags(), DecodeEncryptedKey)
}
