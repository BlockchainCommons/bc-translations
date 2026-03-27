package bccomponents

import (
	"fmt"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// RegisterTagsIn registers all bc-components CBOR tag summarizers into the
// provided tag store.
func RegisterTagsIn(tagsStore *dcbor.TagsStore) {
	bctags.RegisterTagsIn(tagsStore)

	tagsStore.SetSummarizer(bctags.TagDigest, func(untagged dcbor.CBOR, _ bool) (string, error) {
		d, err := DecodeDigest(untagged)
		if err != nil {
			return "", err
		}
		return fmt.Sprintf("Digest(%s)", d.ShortDescription()), nil
	})

	tagsStore.SetSummarizer(bctags.TagARID, func(untagged dcbor.CBOR, _ bool) (string, error) {
		a, err := DecodeARID(untagged)
		if err != nil {
			return "", err
		}
		return fmt.Sprintf("ARID(%s)", a.ShortDescription()), nil
	})

	tagsStore.SetSummarizer(bctags.TagXID, func(untagged dcbor.CBOR, _ bool) (string, error) {
		x, err := DecodeXID(untagged)
		if err != nil {
			return "", err
		}
		return fmt.Sprintf("XID(%s)", x.ShortDescription()), nil
	})

	tagsStore.SetSummarizer(bctags.TagURI, func(untagged dcbor.CBOR, _ bool) (string, error) {
		u, err := DecodeURI(untagged)
		if err != nil {
			return "", err
		}
		return fmt.Sprintf("URI(%s)", u.String()), nil
	})

	tagsStore.SetSummarizer(bctags.TagUUID, func(untagged dcbor.CBOR, _ bool) (string, error) {
		u, err := DecodeUUID(untagged)
		if err != nil {
			return "", err
		}
		return fmt.Sprintf("UUID(%s)", u.String()), nil
	})

	tagsStore.SetSummarizer(bctags.TagNonce, func(untagged dcbor.CBOR, _ bool) (string, error) {
		_, err := DecodeNonce(untagged)
		if err != nil {
			return "", err
		}
		return "Nonce", nil
	})

	tagsStore.SetSummarizer(bctags.TagSalt, func(untagged dcbor.CBOR, _ bool) (string, error) {
		_, err := DecodeSalt(untagged)
		if err != nil {
			return "", err
		}
		return "Salt", nil
	})

	tagsStore.SetSummarizer(bctags.TagJSON, func(untagged dcbor.CBOR, _ bool) (string, error) {
		j, err := DecodeJSON(untagged)
		if err != nil {
			return "", err
		}
		return fmt.Sprintf("JSON(%s)", j.Str()), nil
	})

	tagsStore.SetSummarizer(bctags.TagSeed, func(untagged dcbor.CBOR, _ bool) (string, error) {
		_, err := DecodeSeed(untagged)
		if err != nil {
			return "", err
		}
		return "Seed", nil
	})

	tagsStore.SetSummarizer(bctags.TagPrivateKeys, func(untagged dcbor.CBOR, _ bool) (string, error) {
		k, err := DecodePrivateKeys(untagged)
		if err != nil {
			return "", err
		}
		return k.String(), nil
	})

	tagsStore.SetSummarizer(bctags.TagPublicKeys, func(untagged dcbor.CBOR, _ bool) (string, error) {
		k, err := DecodePublicKeys(untagged)
		if err != nil {
			return "", err
		}
		return k.String(), nil
	})

	tagsStore.SetSummarizer(bctags.TagReference, func(untagged dcbor.CBOR, _ bool) (string, error) {
		r, err := DecodeReference(untagged)
		if err != nil {
			return "", err
		}
		return r.String(), nil
	})

	tagsStore.SetSummarizer(bctags.TagEncryptedKey, func(untagged dcbor.CBOR, _ bool) (string, error) {
		ek, err := DecodeEncryptedKey(untagged)
		if err != nil {
			return "", err
		}
		return ek.String(), nil
	})

	tagsStore.SetSummarizer(bctags.TagPrivateKeyBase, func(untagged dcbor.CBOR, _ bool) (string, error) {
		k, err := DecodePrivateKeyBase(untagged)
		if err != nil {
			return "", err
		}
		return k.String(), nil
	})

	tagsStore.SetSummarizer(bctags.TagSigningPrivateKey, func(untagged dcbor.CBOR, _ bool) (string, error) {
		k, err := DecodeSigningPrivateKey(untagged)
		if err != nil {
			return "", err
		}
		return k.String(), nil
	})

	tagsStore.SetSummarizer(bctags.TagSigningPublicKey, func(untagged dcbor.CBOR, _ bool) (string, error) {
		k, err := DecodeSigningPublicKey(untagged)
		if err != nil {
			return "", err
		}
		return k.String(), nil
	})

	tagsStore.SetSummarizer(bctags.TagSignature, func(untagged dcbor.CBOR, _ bool) (string, error) {
		sig, err := DecodeSignature(untagged)
		if err != nil {
			return "", err
		}
		scheme := sig.Scheme()
		if scheme == SchemeSchnorr {
			return "Signature", nil
		}
		return fmt.Sprintf("Signature(%s)", scheme), nil
	})

	tagsStore.SetSummarizer(bctags.TagSealedMessage, func(untagged dcbor.CBOR, _ bool) (string, error) {
		m, err := DecodeSealedMessage(untagged)
		if err != nil {
			return "", err
		}
		scheme := m.EncapsulationScheme()
		if scheme == EncapsulationX25519 {
			return "SealedMessage", nil
		}
		return fmt.Sprintf("SealedMessage(%s)", scheme), nil
	})

	tagsStore.SetSummarizer(bctags.TagSSKRShare, func(untagged dcbor.CBOR, _ bool) (string, error) {
		_, err := DecodeSSKRShare(untagged)
		if err != nil {
			return "", err
		}
		return "SSKRShare", nil
	})

	tagsStore.SetSummarizer(bctags.TagSSHTextSignature, func(untagged dcbor.CBOR, _ bool) (string, error) {
		text, ok := untagged.AsText()
		if !ok {
			return "", dcbor.NewErrorf("SSH signature must be text")
		}
		_, err := SSHSignatureFromPEM([]byte(text))
		if err != nil {
			return "", err
		}
		return "SSHSignature", nil
	})

	tagsStore.SetSummarizer(bctags.TagSSHTextCertificate, func(_ dcbor.CBOR, _ bool) (string, error) {
		return "SSHCertificate", nil
	})
}

// RegisterTags registers all bc-components CBOR tag summarizers in the
// process-global tag store.
func RegisterTags() {
	dcbor.WithTags(func(tagsStore *dcbor.TagsStore) struct{} {
		RegisterTagsIn(tagsStore)
		return struct{}{}
	})
}
