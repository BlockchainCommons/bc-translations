package bctags

import (
	"github.com/nickel-blockchaincommons/dcbor-go"
)

// Standard IANA tags.
const (
	// TagURI is the CBOR tag value for URI (IANA tag 32).
	TagURI     uint64 = 32
	TagNameURI        = "url"

	// TagUUID is the CBOR tag value for UUID (IANA tag 37).
	TagUUID     uint64 = 37
	TagNameUUID        = "uuid"
)

// Core Envelope tags.
const (
	// TagEncodedCBOR is the CBOR tag value for encoded CBOR data items.
	TagEncodedCBOR     uint64 = 24
	TagNameEncodedCBOR        = "encoded-cbor"

	// TagEnvelope is the CBOR tag value for Gordian Envelope.
	TagEnvelope     uint64 = 200
	TagNameEnvelope        = "envelope"

	// TagLeaf is the CBOR tag value for dCBOR/Envelope leaf data items.
	TagLeaf     uint64 = 201
	TagNameLeaf        = "leaf"

	// TagJSON is the CBOR tag value for JSON text embedded in a byte string.
	TagJSON     uint64 = 262
	TagNameJSON        = "json"
)

// Envelope extension tags.
const (
	// TagKnownValue is the CBOR tag value for known values.
	TagKnownValue     uint64 = 40000
	TagNameKnownValue        = "known-value"

	// TagDigest is the CBOR tag value for cryptographic digests.
	TagDigest     uint64 = 40001
	TagNameDigest        = "digest"

	// TagEncrypted is the CBOR tag value for encrypted data.
	TagEncrypted     uint64 = 40002
	TagNameEncrypted        = "encrypted"

	// TagCompressed is the CBOR tag value for compressed data.
	TagCompressed     uint64 = 40003
	TagNameCompressed        = "compressed"
)

// Distributed Function Call tags.
const (
	// TagRequest is the CBOR tag value for function call requests.
	TagRequest     uint64 = 40004
	TagNameRequest        = "request"

	// TagResponse is the CBOR tag value for function call responses.
	TagResponse     uint64 = 40005
	TagNameResponse        = "response"

	// TagFunction is the CBOR tag value for function identifiers.
	TagFunction     uint64 = 40006
	TagNameFunction        = "function"

	// TagParameter is the CBOR tag value for function parameters.
	TagParameter     uint64 = 40007
	TagNameParameter        = "parameter"

	// TagPlaceholder is the CBOR tag value for response placeholders.
	TagPlaceholder     uint64 = 40008
	TagNamePlaceholder        = "placeholder"

	// TagReplacement is the CBOR tag value for response replacements.
	TagReplacement     uint64 = 40009
	TagNameReplacement        = "replacement"
)

// Cryptographic key and identity tags.
const (
	// TagX25519PrivateKey is the CBOR tag value for X25519 agreement private keys.
	TagX25519PrivateKey     uint64 = 40010
	TagNameX25519PrivateKey        = "agreement-private-key"

	// TagX25519PublicKey is the CBOR tag value for X25519 agreement public keys.
	TagX25519PublicKey     uint64 = 40011
	TagNameX25519PublicKey        = "agreement-public-key"

	// TagARID is the CBOR tag value for Apparently Random Identifiers.
	TagARID     uint64 = 40012
	TagNameARID        = "arid"

	// TagPrivateKeys is the CBOR tag value for private key bundles.
	TagPrivateKeys     uint64 = 40013
	TagNamePrivateKeys        = "crypto-prvkeys"

	// TagNonce is the CBOR tag value for cryptographic nonces.
	TagNonce     uint64 = 40014
	TagNameNonce        = "nonce"

	// TagPassword is the CBOR tag value for passwords.
	TagPassword     uint64 = 40015
	TagNamePassword        = "password"

	// TagPrivateKeyBase is the CBOR tag value for private key base material.
	TagPrivateKeyBase     uint64 = 40016
	TagNamePrivateKeyBase        = "crypto-prvkey-base"

	// TagPublicKeys is the CBOR tag value for public key bundles.
	TagPublicKeys     uint64 = 40017
	TagNamePublicKeys        = "crypto-pubkeys"

	// TagSalt is the CBOR tag value for cryptographic salts.
	TagSalt     uint64 = 40018
	TagNameSalt        = "salt"

	// TagSealedMessage is the CBOR tag value for sealed (encrypted+signed) messages.
	TagSealedMessage     uint64 = 40019
	TagNameSealedMessage        = "crypto-sealed"

	// TagSignature is the CBOR tag value for cryptographic signatures.
	TagSignature     uint64 = 40020
	TagNameSignature        = "signature"

	// TagSigningPrivateKey is the CBOR tag value for signing private keys.
	TagSigningPrivateKey     uint64 = 40021
	TagNameSigningPrivateKey        = "signing-private-key"

	// TagSigningPublicKey is the CBOR tag value for signing public keys.
	TagSigningPublicKey     uint64 = 40022
	TagNameSigningPublicKey        = "signing-public-key"

	// TagSymmetricKey is the CBOR tag value for symmetric encryption keys.
	TagSymmetricKey     uint64 = 40023
	TagNameSymmetricKey        = "crypto-key"

	// TagXID is the CBOR tag value for Extended Identifiers.
	TagXID     uint64 = 40024
	TagNameXID        = "xid"

	// TagReference is the CBOR tag value for references.
	TagReference     uint64 = 40025
	TagNameReference        = "reference"

	// TagEvent is the CBOR tag value for events.
	TagEvent     uint64 = 40026
	TagNameEvent        = "event"

	// TagEncryptedKey is the CBOR tag value for encrypted keys.
	TagEncryptedKey     uint64 = 40027
	TagNameEncryptedKey        = "encrypted-key"
)

// Post-Quantum Cryptography tags.
const (
	// TagMLKEMPrivateKey is the CBOR tag value for ML-KEM private keys.
	TagMLKEMPrivateKey     uint64 = 40100
	TagNameMLKEMPrivateKey        = "mlkem-private-key"

	// TagMLKEMPublicKey is the CBOR tag value for ML-KEM public keys.
	TagMLKEMPublicKey     uint64 = 40101
	TagNameMLKEMPublicKey        = "mlkem-public-key"

	// TagMLKEMCiphertext is the CBOR tag value for ML-KEM ciphertexts.
	TagMLKEMCiphertext     uint64 = 40102
	TagNameMLKEMCiphertext        = "mlkem-ciphertext"

	// TagMLDSAPrivateKey is the CBOR tag value for ML-DSA private keys.
	TagMLDSAPrivateKey     uint64 = 40103
	TagNameMLDSAPrivateKey        = "mldsa-private-key"

	// TagMLDSAPublicKey is the CBOR tag value for ML-DSA public keys.
	TagMLDSAPublicKey     uint64 = 40104
	TagNameMLDSAPublicKey        = "mldsa-public-key"

	// TagMLDSASignature is the CBOR tag value for ML-DSA signatures.
	TagMLDSASignature     uint64 = 40105
	TagNameMLDSASignature        = "mldsa-signature"
)

// Key and descriptor tags.
const (
	// TagSeed is the CBOR tag value for cryptographic seeds.
	TagSeed     uint64 = 40300
	TagNameSeed        = "seed"

	// TagHDKey is the CBOR tag value for HD (hierarchical deterministic) keys.
	TagHDKey     uint64 = 40303
	TagNameHDKey        = "hdkey"

	// TagDerivationPath is the CBOR tag value for key derivation paths.
	TagDerivationPath     uint64 = 40304
	TagNameDerivationPath        = "keypath"

	// TagUseInfo is the CBOR tag value for coin/network use info.
	TagUseInfo     uint64 = 40305
	TagNameUseInfo        = "coin-info"

	// TagECKey is the CBOR tag value for elliptic curve keys.
	TagECKey     uint64 = 40306
	TagNameECKey        = "eckey"

	// TagAddress is the CBOR tag value for cryptocurrency addresses.
	TagAddress     uint64 = 40307
	TagNameAddress        = "address"

	// TagOutputDescriptor is the CBOR tag value for output descriptors.
	TagOutputDescriptor     uint64 = 40308
	TagNameOutputDescriptor        = "output-descriptor"

	// TagSSKRShare is the CBOR tag value for SSKR shares.
	TagSSKRShare     uint64 = 40309
	TagNameSSKRShare        = "sskr"

	// TagPSBT is the CBOR tag value for Partially Signed Bitcoin Transactions.
	TagPSBT     uint64 = 40310
	TagNamePSBT        = "psbt"

	// TagAccountDescriptor is the CBOR tag value for account descriptors.
	TagAccountDescriptor     uint64 = 40311
	TagNameAccountDescriptor        = "account-descriptor"
)

// SSH tags.
const (
	// TagSSHTextPrivateKey is the CBOR tag value for SSH text-format private keys.
	TagSSHTextPrivateKey     uint64 = 40800
	TagNameSSHTextPrivateKey        = "ssh-private"

	// TagSSHTextPublicKey is the CBOR tag value for SSH text-format public keys.
	TagSSHTextPublicKey     uint64 = 40801
	TagNameSSHTextPublicKey        = "ssh-public"

	// TagSSHTextSignature is the CBOR tag value for SSH text-format signatures.
	TagSSHTextSignature     uint64 = 40802
	TagNameSSHTextSignature        = "ssh-signature"

	// TagSSHTextCertificate is the CBOR tag value for SSH text-format certificates.
	TagSSHTextCertificate     uint64 = 40803
	TagNameSSHTextCertificate        = "ssh-certificate"
)

// Provenance tag.
const (
	// TagProvenanceMark is the CBOR tag value for provenance marks.
	TagProvenanceMark     uint64 = 1347571542
	TagNameProvenanceMark        = "provenance"
)

// Deprecated tags (V1).
//
// These tags are deprecated and should not be used in new code.
// They remain registered for backward compatibility with external
// implementations that may still use them.
const (
	// TagSeedV1 is the deprecated V1 CBOR tag value for cryptographic seeds.
	TagSeedV1     uint64 = 300
	TagNameSeedV1        = "crypto-seed"

	// TagECKeyV1 is the deprecated V1 CBOR tag value for elliptic curve keys.
	TagECKeyV1     uint64 = 306
	TagNameECKeyV1        = "crypto-eckey"

	// TagSSKRShareV1 is the deprecated V1 CBOR tag value for SSKR shares.
	TagSSKRShareV1     uint64 = 309
	TagNameSSKRShareV1        = "crypto-sskr"

	// TagHDKeyV1 is the deprecated V1 CBOR tag value for HD keys.
	TagHDKeyV1     uint64 = 303
	TagNameHDKeyV1        = "crypto-hdkey"

	// TagDerivationPathV1 is the deprecated V1 CBOR tag value for key derivation paths.
	TagDerivationPathV1     uint64 = 304
	TagNameDerivationPathV1        = "crypto-keypath"

	// TagUseInfoV1 is the deprecated V1 CBOR tag value for coin/network use info.
	TagUseInfoV1     uint64 = 305
	TagNameUseInfoV1        = "crypto-coin-info"

	// TagOutputDescriptorV1 is the deprecated V1 CBOR tag value for output descriptors.
	TagOutputDescriptorV1     uint64 = 307
	TagNameOutputDescriptorV1        = "crypto-output"

	// TagPSBTV1 is the deprecated V1 CBOR tag value for PSBTs.
	TagPSBTV1     uint64 = 310
	TagNamePSBTV1        = "crypto-psbt"

	// TagAccountV1 is the deprecated V1 CBOR tag value for account descriptors.
	TagAccountV1     uint64 = 311
	TagNameAccountV1        = "crypto-account"
)

// Output descriptor sub-tags (for AccountBundle).
const (
	// TagOutputScriptHash is the CBOR tag value for output script hash descriptors.
	TagOutputScriptHash     uint64 = 400
	TagNameOutputScriptHash        = "output-script-hash"

	// TagOutputWitnessScriptHash is the CBOR tag value for output witness script hash descriptors.
	TagOutputWitnessScriptHash     uint64 = 401
	TagNameOutputWitnessScriptHash        = "output-witness-script-hash"

	// TagOutputPublicKey is the CBOR tag value for output public key descriptors.
	TagOutputPublicKey     uint64 = 402
	TagNameOutputPublicKey        = "output-public-key"

	// TagOutputPublicKeyHash is the CBOR tag value for output public key hash descriptors.
	TagOutputPublicKeyHash     uint64 = 403
	TagNameOutputPublicKeyHash        = "output-public-key-hash"

	// TagOutputWitnessPublicKeyHash is the CBOR tag value for output witness public key hash descriptors.
	TagOutputWitnessPublicKeyHash     uint64 = 404
	TagNameOutputWitnessPublicKeyHash        = "output-witness-public-key-hash"

	// TagOutputCombo is the CBOR tag value for output combo descriptors.
	TagOutputCombo     uint64 = 405
	TagNameOutputCombo        = "output-combo"

	// TagOutputMultisig is the CBOR tag value for output multisig descriptors.
	TagOutputMultisig     uint64 = 406
	TagNameOutputMultisig        = "output-multisig"

	// TagOutputSortedMultisig is the CBOR tag value for output sorted multisig descriptors.
	TagOutputSortedMultisig     uint64 = 407
	TagNameOutputSortedMultisig        = "output-sorted-multisig"

	// TagOutputRawScript is the CBOR tag value for output raw script descriptors.
	TagOutputRawScript     uint64 = 408
	TagNameOutputRawScript        = "output-raw-script"

	// TagOutputTaproot is the CBOR tag value for output taproot descriptors.
	TagOutputTaproot     uint64 = 409
	TagNameOutputTaproot        = "output-taproot"

	// TagOutputCosigner is the CBOR tag value for output cosigner descriptors.
	TagOutputCosigner     uint64 = 410
	TagNameOutputCosigner        = "output-cosigner"
)

// bcTags contains all 75 bc-tags Tag objects in registration order.
var bcTags = []dcbor.Tag{
	dcbor.NewTag(TagURI, TagNameURI),
	dcbor.NewTag(TagUUID, TagNameUUID),
	dcbor.NewTag(TagEncodedCBOR, TagNameEncodedCBOR),
	dcbor.NewTag(TagEnvelope, TagNameEnvelope),
	dcbor.NewTag(TagLeaf, TagNameLeaf),
	dcbor.NewTag(TagJSON, TagNameJSON),
	dcbor.NewTag(TagKnownValue, TagNameKnownValue),
	dcbor.NewTag(TagDigest, TagNameDigest),
	dcbor.NewTag(TagEncrypted, TagNameEncrypted),
	dcbor.NewTag(TagCompressed, TagNameCompressed),
	dcbor.NewTag(TagRequest, TagNameRequest),
	dcbor.NewTag(TagResponse, TagNameResponse),
	dcbor.NewTag(TagFunction, TagNameFunction),
	dcbor.NewTag(TagParameter, TagNameParameter),
	dcbor.NewTag(TagPlaceholder, TagNamePlaceholder),
	dcbor.NewTag(TagReplacement, TagNameReplacement),
	dcbor.NewTag(TagEvent, TagNameEvent),
	dcbor.NewTag(TagSeedV1, TagNameSeedV1),
	dcbor.NewTag(TagECKeyV1, TagNameECKeyV1),
	dcbor.NewTag(TagSSKRShareV1, TagNameSSKRShareV1),
	dcbor.NewTag(TagSeed, TagNameSeed),
	dcbor.NewTag(TagECKey, TagNameECKey),
	dcbor.NewTag(TagSSKRShare, TagNameSSKRShare),
	dcbor.NewTag(TagX25519PrivateKey, TagNameX25519PrivateKey),
	dcbor.NewTag(TagX25519PublicKey, TagNameX25519PublicKey),
	dcbor.NewTag(TagARID, TagNameARID),
	dcbor.NewTag(TagPrivateKeys, TagNamePrivateKeys),
	dcbor.NewTag(TagNonce, TagNameNonce),
	dcbor.NewTag(TagPassword, TagNamePassword),
	dcbor.NewTag(TagPrivateKeyBase, TagNamePrivateKeyBase),
	dcbor.NewTag(TagPublicKeys, TagNamePublicKeys),
	dcbor.NewTag(TagSalt, TagNameSalt),
	dcbor.NewTag(TagSealedMessage, TagNameSealedMessage),
	dcbor.NewTag(TagSignature, TagNameSignature),
	dcbor.NewTag(TagSigningPrivateKey, TagNameSigningPrivateKey),
	dcbor.NewTag(TagSigningPublicKey, TagNameSigningPublicKey),
	dcbor.NewTag(TagSymmetricKey, TagNameSymmetricKey),
	dcbor.NewTag(TagXID, TagNameXID),
	dcbor.NewTag(TagReference, TagNameReference),
	dcbor.NewTag(TagEncryptedKey, TagNameEncryptedKey),
	dcbor.NewTag(TagMLKEMPrivateKey, TagNameMLKEMPrivateKey),
	dcbor.NewTag(TagMLKEMPublicKey, TagNameMLKEMPublicKey),
	dcbor.NewTag(TagMLKEMCiphertext, TagNameMLKEMCiphertext),
	dcbor.NewTag(TagMLDSAPrivateKey, TagNameMLDSAPrivateKey),
	dcbor.NewTag(TagMLDSAPublicKey, TagNameMLDSAPublicKey),
	dcbor.NewTag(TagMLDSASignature, TagNameMLDSASignature),
	dcbor.NewTag(TagHDKeyV1, TagNameHDKeyV1),
	dcbor.NewTag(TagDerivationPathV1, TagNameDerivationPathV1),
	dcbor.NewTag(TagUseInfoV1, TagNameUseInfoV1),
	dcbor.NewTag(TagOutputDescriptorV1, TagNameOutputDescriptorV1),
	dcbor.NewTag(TagPSBTV1, TagNamePSBTV1),
	dcbor.NewTag(TagAccountV1, TagNameAccountV1),
	dcbor.NewTag(TagHDKey, TagNameHDKey),
	dcbor.NewTag(TagDerivationPath, TagNameDerivationPath),
	dcbor.NewTag(TagUseInfo, TagNameUseInfo),
	dcbor.NewTag(TagAddress, TagNameAddress),
	dcbor.NewTag(TagOutputDescriptor, TagNameOutputDescriptor),
	dcbor.NewTag(TagPSBT, TagNamePSBT),
	dcbor.NewTag(TagAccountDescriptor, TagNameAccountDescriptor),
	dcbor.NewTag(TagSSHTextPrivateKey, TagNameSSHTextPrivateKey),
	dcbor.NewTag(TagSSHTextPublicKey, TagNameSSHTextPublicKey),
	dcbor.NewTag(TagSSHTextSignature, TagNameSSHTextSignature),
	dcbor.NewTag(TagSSHTextCertificate, TagNameSSHTextCertificate),
	dcbor.NewTag(TagOutputScriptHash, TagNameOutputScriptHash),
	dcbor.NewTag(TagOutputWitnessScriptHash, TagNameOutputWitnessScriptHash),
	dcbor.NewTag(TagOutputPublicKey, TagNameOutputPublicKey),
	dcbor.NewTag(TagOutputPublicKeyHash, TagNameOutputPublicKeyHash),
	dcbor.NewTag(TagOutputWitnessPublicKeyHash, TagNameOutputWitnessPublicKeyHash),
	dcbor.NewTag(TagOutputCombo, TagNameOutputCombo),
	dcbor.NewTag(TagOutputMultisig, TagNameOutputMultisig),
	dcbor.NewTag(TagOutputSortedMultisig, TagNameOutputSortedMultisig),
	dcbor.NewTag(TagOutputRawScript, TagNameOutputRawScript),
	dcbor.NewTag(TagOutputTaproot, TagNameOutputTaproot),
	dcbor.NewTag(TagOutputCosigner, TagNameOutputCosigner),
	dcbor.NewTag(TagProvenanceMark, TagNameProvenanceMark),
}

// RegisterTagsIn registers all bc-tags (plus dcbor base tags) into the
// provided tag store.
func RegisterTagsIn(tagsStore *dcbor.TagsStore) {
	dcbor.RegisterTagsIn(tagsStore)
	tagsStore.InsertAll(bcTags)
}

// RegisterTags registers all bc-tags (plus dcbor base tags) in the
// process-global tag store.
func RegisterTags() {
	dcbor.WithTags(func(tagsStore *dcbor.TagsStore) struct{} {
		RegisterTagsIn(tagsStore)
		return struct{}{}
	})
}
