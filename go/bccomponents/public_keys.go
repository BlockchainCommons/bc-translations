package bccomponents

import (
	"fmt"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// PublicKeys bundles a signing public key and an encapsulation public key.
type PublicKeys struct {
	signing      SigningPublicKey
	encapsulation EncapsulationPublicKey
}

// NewPublicKeys creates a new PublicKeys bundle.
func NewPublicKeys(signing SigningPublicKey, encapsulation EncapsulationPublicKey) PublicKeys {
	return PublicKeys{signing: signing, encapsulation: encapsulation}
}

// SigningPublicKey returns the signing public key.
func (k PublicKeys) SigningPublicKey() SigningPublicKey { return k.signing }

// EncapsulationPublicKey returns the encapsulation public key.
func (k PublicKeys) EncapsulationPublicKey() EncapsulationPublicKey { return k.encapsulation }

// Verify verifies a signature over a message.
func (k PublicKeys) Verify(sig Signature, message []byte) bool {
	return k.signing.Verify(sig, message)
}

// EncapsulateNewSharedSecret generates a shared secret and ciphertext.
func (k PublicKeys) EncapsulateNewSharedSecret() (SymmetricKey, EncapsulationCiphertext, error) {
	return k.encapsulation.EncapsulateNewSharedSecret()
}

// String returns a human-readable representation.
func (k PublicKeys) String() string {
	return fmt.Sprintf("PublicKeys(%s, %s)", k.signing.Scheme(), k.encapsulation.Scheme())
}

// Reference implements ReferenceProvider.
func (k PublicKeys) Reference() Reference {
	return ReferenceForCBORTaggedEncodable(k)
}

// --- CBOR support ---

func PublicKeysCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagPublicKeys})
}

func (k PublicKeys) CBORTags() []dcbor.Tag { return PublicKeysCBORTags() }

func (k PublicKeys) UntaggedCBOR() dcbor.CBOR {
	return dcbor.NewCBORArray([]dcbor.CBOR{
		k.signing.TaggedCBOR(),
		k.encapsulation.TaggedCBOR(),
	})
}

func (k PublicKeys) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k PublicKeys) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodePublicKeys(cbor dcbor.CBOR) (PublicKeys, error) {
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return PublicKeys{}, err
	}
	if len(elements) != 2 {
		return PublicKeys{}, dcbor.NewErrorf("PublicKeys must have 2 elements")
	}
	signing, err := DecodeTaggedSigningPublicKey(elements[0])
	if err != nil {
		return PublicKeys{}, err
	}
	encap, err := DecodeEncapsulationPublicKey(elements[1])
	if err != nil {
		return PublicKeys{}, err
	}
	return NewPublicKeys(signing, encap), nil
}

func DecodeTaggedPublicKeys(cbor dcbor.CBOR) (PublicKeys, error) {
	return dcbor.DecodeTagged(cbor, PublicKeysCBORTags(), DecodePublicKeys)
}

// --- UR support ---

func PublicKeysToURString(k PublicKeys) string { return bcur.ToURString(k) }

func PublicKeysFromURString(urString string) (PublicKeys, error) {
	return bcur.DecodeURString(urString, PublicKeysCBORTags(), DecodePublicKeys)
}
