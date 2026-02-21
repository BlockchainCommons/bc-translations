package bctags

import (
	"fmt"
	"testing"

	"github.com/nickel-blockchaincommons/dcbor-go"
)

// tagEntry pairs a numeric tag value with its expected human-readable name.
type tagEntry struct {
	value uint64
	name  string
}

// expectedTags lists all 75 bc-tags in registration order.
var expectedTags = []tagEntry{
	{TagURI, TagNameURI},
	{TagUUID, TagNameUUID},
	{TagEncodedCBOR, TagNameEncodedCBOR},
	{TagEnvelope, TagNameEnvelope},
	{TagLeaf, TagNameLeaf},
	{TagJSON, TagNameJSON},
	{TagKnownValue, TagNameKnownValue},
	{TagDigest, TagNameDigest},
	{TagEncrypted, TagNameEncrypted},
	{TagCompressed, TagNameCompressed},
	{TagRequest, TagNameRequest},
	{TagResponse, TagNameResponse},
	{TagFunction, TagNameFunction},
	{TagParameter, TagNameParameter},
	{TagPlaceholder, TagNamePlaceholder},
	{TagReplacement, TagNameReplacement},
	{TagEvent, TagNameEvent},
	{TagSeedV1, TagNameSeedV1},
	{TagECKeyV1, TagNameECKeyV1},
	{TagSSKRShareV1, TagNameSSKRShareV1},
	{TagSeed, TagNameSeed},
	{TagECKey, TagNameECKey},
	{TagSSKRShare, TagNameSSKRShare},
	{TagX25519PrivateKey, TagNameX25519PrivateKey},
	{TagX25519PublicKey, TagNameX25519PublicKey},
	{TagARID, TagNameARID},
	{TagPrivateKeys, TagNamePrivateKeys},
	{TagNonce, TagNameNonce},
	{TagPassword, TagNamePassword},
	{TagPrivateKeyBase, TagNamePrivateKeyBase},
	{TagPublicKeys, TagNamePublicKeys},
	{TagSalt, TagNameSalt},
	{TagSealedMessage, TagNameSealedMessage},
	{TagSignature, TagNameSignature},
	{TagSigningPrivateKey, TagNameSigningPrivateKey},
	{TagSigningPublicKey, TagNameSigningPublicKey},
	{TagSymmetricKey, TagNameSymmetricKey},
	{TagXID, TagNameXID},
	{TagReference, TagNameReference},
	{TagEncryptedKey, TagNameEncryptedKey},
	{TagMLKEMPrivateKey, TagNameMLKEMPrivateKey},
	{TagMLKEMPublicKey, TagNameMLKEMPublicKey},
	{TagMLKEMCiphertext, TagNameMLKEMCiphertext},
	{TagMLDSAPrivateKey, TagNameMLDSAPrivateKey},
	{TagMLDSAPublicKey, TagNameMLDSAPublicKey},
	{TagMLDSASignature, TagNameMLDSASignature},
	{TagHDKeyV1, TagNameHDKeyV1},
	{TagDerivationPathV1, TagNameDerivationPathV1},
	{TagUseInfoV1, TagNameUseInfoV1},
	{TagOutputDescriptorV1, TagNameOutputDescriptorV1},
	{TagPSBTV1, TagNamePSBTV1},
	{TagAccountV1, TagNameAccountV1},
	{TagHDKey, TagNameHDKey},
	{TagDerivationPath, TagNameDerivationPath},
	{TagUseInfo, TagNameUseInfo},
	{TagAddress, TagNameAddress},
	{TagOutputDescriptor, TagNameOutputDescriptor},
	{TagPSBT, TagNamePSBT},
	{TagAccountDescriptor, TagNameAccountDescriptor},
	{TagSSHTextPrivateKey, TagNameSSHTextPrivateKey},
	{TagSSHTextPublicKey, TagNameSSHTextPublicKey},
	{TagSSHTextSignature, TagNameSSHTextSignature},
	{TagSSHTextCertificate, TagNameSSHTextCertificate},
	{TagOutputScriptHash, TagNameOutputScriptHash},
	{TagOutputWitnessScriptHash, TagNameOutputWitnessScriptHash},
	{TagOutputPublicKey, TagNameOutputPublicKey},
	{TagOutputPublicKeyHash, TagNameOutputPublicKeyHash},
	{TagOutputWitnessPublicKeyHash, TagNameOutputWitnessPublicKeyHash},
	{TagOutputCombo, TagNameOutputCombo},
	{TagOutputMultisig, TagNameOutputMultisig},
	{TagOutputSortedMultisig, TagNameOutputSortedMultisig},
	{TagOutputRawScript, TagNameOutputRawScript},
	{TagOutputTaproot, TagNameOutputTaproot},
	{TagOutputCosigner, TagNameOutputCosigner},
	{TagProvenanceMark, TagNameProvenanceMark},
}

func TestConstantValues(t *testing.T) {
	checks := []struct {
		name  string
		value uint64
		label string
	}{
		{"TagURI", TagURI, "url"},
		{"TagUUID", TagUUID, "uuid"},
		{"TagEnvelope", TagEnvelope, "envelope"},
		{"TagKnownValue", TagKnownValue, "known-value"},
		{"TagRequest", TagRequest, "request"},
		{"TagX25519PrivateKey", TagX25519PrivateKey, "agreement-private-key"},
		{"TagMLKEMPrivateKey", TagMLKEMPrivateKey, "mlkem-private-key"},
		{"TagSeed", TagSeed, "seed"},
		{"TagSSHTextPrivateKey", TagSSHTextPrivateKey, "ssh-private"},
		{"TagProvenanceMark", TagProvenanceMark, "provenance"},
		{"TagSeedV1", TagSeedV1, "crypto-seed"},
		{"TagOutputScriptHash", TagOutputScriptHash, "output-script-hash"},
	}
	for _, c := range checks {
		t.Run(c.name, func(t *testing.T) {
			found := false
			for _, e := range expectedTags {
				if e.value == c.value {
					if e.name != c.label {
						t.Errorf("expected name %q, got %q", c.label, e.name)
					}
					found = true
					break
				}
			}
			if !found {
				t.Errorf("value %d not found in expectedTags", c.value)
			}
		})
	}
}

func TestBcTagsSliceMatchesExpected(t *testing.T) {
	if len(bcTags) != 75 {
		t.Fatalf("bcTags slice: expected 75 entries, got %d", len(bcTags))
	}
	if len(expectedTags) != 75 {
		t.Fatalf("expectedTags: expected 75 entries, got %d", len(expectedTags))
	}
	for i, tag := range bcTags {
		expected := expectedTags[i]
		if tag.Value() != expected.value {
			t.Errorf("bcTags[%d]: expected value %d, got %d", i, expected.value, tag.Value())
		}
		name, ok := tag.Name()
		if !ok {
			t.Errorf("bcTags[%d]: expected named tag", i)
			continue
		}
		if name != expected.name {
			t.Errorf("bcTags[%d]: expected name %q, got %q", i, expected.name, name)
		}
	}
}

func TestRegisterTagsIn(t *testing.T) {
	store := dcbor.NewTagsStore(nil)
	RegisterTagsIn(store)

	t.Run("dcbor_base_tags", func(t *testing.T) {
		dcborTags := []struct {
			value uint64
			name  string
		}{
			{dcbor.TagDate, dcbor.TagNameDate},
			{dcbor.TagPositiveBignum, dcbor.TagNamePositiveBignum},
			{dcbor.TagNegativeBignum, dcbor.TagNameNegativeBignum},
		}
		for _, dt := range dcborTags {
			tag, ok := store.TagForValue(dt.value)
			if !ok {
				t.Errorf("dcbor tag %d (%s) not registered", dt.value, dt.name)
				continue
			}
			name, hasName := tag.Name()
			if !hasName || name != dt.name {
				t.Errorf("dcbor tag %d: expected name %q, got %q", dt.value, dt.name, name)
			}
		}
	})

	t.Run("forward_lookup", func(t *testing.T) {
		for _, e := range expectedTags {
			tag, ok := store.TagForValue(e.value)
			if !ok {
				t.Errorf("bc-tag %d (%s) not registered", e.value, e.name)
				continue
			}
			name, hasName := tag.Name()
			if !hasName || name != e.name {
				t.Errorf("bc-tag %d: expected name %q, got %q", e.value, e.name, name)
			}
		}
	})

	t.Run("reverse_lookup", func(t *testing.T) {
		for _, e := range expectedTags {
			tag, ok := store.TagForName(e.name)
			if !ok {
				t.Errorf("tag name %q not found in store", e.name)
				continue
			}
			if tag.Value() != e.value {
				t.Errorf("tag name %q: expected value %d, got %d", e.name, e.value, tag.Value())
			}
		}
	})
}

func TestRegisterTagsInIdempotent(t *testing.T) {
	store := dcbor.NewTagsStore(nil)
	RegisterTagsIn(store)
	RegisterTagsIn(store)

	tag, ok := store.TagForValue(TagEnvelope)
	if !ok {
		t.Fatal("TagEnvelope not registered after idempotent call")
	}
	name, _ := tag.Name()
	if name != TagNameEnvelope {
		t.Errorf("expected %q, got %q", TagNameEnvelope, name)
	}
}

func TestRegisterTagsGlobal(t *testing.T) {
	RegisterTags()

	name := dcbor.WithTags(func(store *dcbor.TagsStore) string {
		return store.NameForValue(TagEnvelope)
	})
	if name != TagNameEnvelope {
		t.Errorf("global store: expected %q for %d, got %q", TagNameEnvelope, TagEnvelope, name)
	}

	dateName := dcbor.WithTags(func(store *dcbor.TagsStore) string {
		return store.NameForValue(dcbor.TagDate)
	})
	if dateName != dcbor.TagNameDate {
		t.Errorf("global store: expected %q for date tag, got %q", dcbor.TagNameDate, dateName)
	}
}

func TestUniqueValues(t *testing.T) {
	seen := make(map[uint64]struct{})
	for _, e := range expectedTags {
		if _, dup := seen[e.value]; dup {
			t.Errorf("duplicate tag value: %d (%s)", e.value, e.name)
		}
		seen[e.value] = struct{}{}
	}
}

func TestUniqueNames(t *testing.T) {
	seen := make(map[string]struct{})
	for _, e := range expectedTags {
		if _, dup := seen[e.name]; dup {
			t.Errorf("duplicate tag name: %s (%d)", e.name, e.value)
		}
		seen[e.name] = struct{}{}
	}
}

func TestTagNameForValue(t *testing.T) {
	store := dcbor.NewTagsStore(nil)
	RegisterTagsIn(store)

	checks := []struct {
		value uint64
		name  string
	}{
		{40023, "crypto-key"},
		{1347571542, "provenance"},
	}
	for _, c := range checks {
		t.Run(fmt.Sprintf("tag_%d", c.value), func(t *testing.T) {
			got := store.NameForValue(c.value)
			if got != c.name {
				t.Errorf("expected %q, got %q", c.name, got)
			}
		})
	}
}
