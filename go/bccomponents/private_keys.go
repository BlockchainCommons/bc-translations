package bccomponents

import (
	"fmt"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// PrivateKeys bundles a signing private key and an encapsulation private key.
type PrivateKeys struct {
	signing      SigningPrivateKey
	encapsulation EncapsulationPrivateKey
}

// NewPrivateKeys creates a new PrivateKeys bundle.
func NewPrivateKeys(signing SigningPrivateKey, encapsulation EncapsulationPrivateKey) PrivateKeys {
	return PrivateKeys{signing: signing, encapsulation: encapsulation}
}

// SigningPrivateKey returns the signing private key.
func (k PrivateKeys) SigningPrivateKey() SigningPrivateKey { return k.signing }

// EncapsulationPrivateKey returns the encapsulation private key.
func (k PrivateKeys) EncapsulationPrivateKey() EncapsulationPrivateKey { return k.encapsulation }

// PublicKeys derives the corresponding public keys.
func (k PrivateKeys) PublicKeys() PublicKeys {
	sigPub := k.signing.PublicKey()
	encapPub := k.encapsulation.PublicKey()
	return NewPublicKeys(sigPub, encapPub)
}

// Sign signs a message with the signing key.
func (k PrivateKeys) Sign(message []byte) (Signature, error) {
	return k.signing.Sign(message)
}

// SignWithOptions signs a message with options.
func (k PrivateKeys) SignWithOptions(message []byte, options *SigningOptions) (Signature, error) {
	return k.signing.SignWithOptions(message, options)
}

// DecapsulateSharedSecret decapsulates a shared secret.
func (k PrivateKeys) DecapsulateSharedSecret(ct EncapsulationCiphertext) (SymmetricKey, error) {
	return k.encapsulation.DecapsulateSharedSecret(ct)
}

// String returns a human-readable representation.
func (k PrivateKeys) String() string {
	return fmt.Sprintf("PrivateKeys(%s, %s)", k.signing.Scheme(), k.encapsulation.Scheme())
}

// Reference implements ReferenceProvider.
func (k PrivateKeys) Reference() Reference {
	return ReferenceForCBORTaggedEncodable(k)
}

// --- CBOR support ---

func PrivateKeysCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagPrivateKeys})
}

func (k PrivateKeys) CBORTags() []dcbor.Tag { return PrivateKeysCBORTags() }

func (k PrivateKeys) UntaggedCBOR() dcbor.CBOR {
	return dcbor.NewCBORArray([]dcbor.CBOR{
		k.signing.TaggedCBOR(),
		k.encapsulation.TaggedCBOR(),
	})
}

func (k PrivateKeys) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k PrivateKeys) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodePrivateKeys(cbor dcbor.CBOR) (PrivateKeys, error) {
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return PrivateKeys{}, err
	}
	if len(elements) != 2 {
		return PrivateKeys{}, dcbor.NewErrorf("PrivateKeys must have 2 elements")
	}
	signing, err := DecodeTaggedSigningPrivateKey(elements[0])
	if err != nil {
		return PrivateKeys{}, err
	}
	encap, err := DecodeEncapsulationPrivateKey(elements[1])
	if err != nil {
		return PrivateKeys{}, err
	}
	return NewPrivateKeys(signing, encap), nil
}

func DecodeTaggedPrivateKeys(cbor dcbor.CBOR) (PrivateKeys, error) {
	return dcbor.DecodeTagged(cbor, PrivateKeysCBORTags(), DecodePrivateKeys)
}

// --- UR support ---

func PrivateKeysToURString(k PrivateKeys) string { return bcur.ToURString(k) }

func PrivateKeysFromURString(urString string) (PrivateKeys, error) {
	return bcur.DecodeURString(urString, PrivateKeysCBORTags(), DecodePrivateKeys)
}
