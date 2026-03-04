package bccomponents

import (
	"fmt"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// SigningPublicKey is a public key that can verify digital signatures.
type SigningPublicKey struct {
	keyType  signingKeyType
	schnorr  *SchnorrPublicKey
	ecdsa    *ECPublicKey
	ed25519  *Ed25519PublicKey
	ssh      *SSHPublicKey
	mldsa    *MLDSAPublicKey
}

// NewSigningPublicKeySchnorr creates a Schnorr signing public key.
func NewSigningPublicKeySchnorr(key SchnorrPublicKey) SigningPublicKey {
	return SigningPublicKey{keyType: skSchnorr, schnorr: &key}
}

// NewSigningPublicKeyECDSA creates an ECDSA signing public key.
func NewSigningPublicKeyECDSA(key ECPublicKey) SigningPublicKey {
	return SigningPublicKey{keyType: skECDSA, ecdsa: &key}
}

// NewSigningPublicKeyEd25519 creates an Ed25519 signing public key.
func NewSigningPublicKeyEd25519(key Ed25519PublicKey) SigningPublicKey {
	return SigningPublicKey{keyType: skEd25519, ed25519: &key}
}

// NewSigningPublicKeySSH creates an SSH signing public key.
func NewSigningPublicKeySSH(key *SSHPublicKey) SigningPublicKey {
	return SigningPublicKey{keyType: skSSH, ssh: key}
}

// NewSigningPublicKeyMLDSA creates an ML-DSA signing public key.
func NewSigningPublicKeyMLDSA(key *MLDSAPublicKey) SigningPublicKey {
	return SigningPublicKey{keyType: skMLDSA, mldsa: key}
}

// Scheme returns the signature scheme.
func (k SigningPublicKey) Scheme() SignatureScheme {
	switch k.keyType {
	case skSchnorr:
		return SchemeSchnorr
	case skECDSA:
		return SchemeECDSA
	case skEd25519:
		return SchemeEd25519
	case skSSH:
		if k.ssh != nil {
			return k.ssh.sshScheme()
		}
		return SchemeSSHEd25519
	case skMLDSA:
		if k.mldsa != nil {
			switch k.mldsa.level {
			case MLDSA44:
				return SchemeMLDSA44
			case MLDSA65:
				return SchemeMLDSA65
			case MLDSA87:
				return SchemeMLDSA87
			}
		}
		return SchemeMLDSA65
	default:
		return SchemeSchnorr
	}
}

// Verify verifies a signature over a message.
func (k SigningPublicKey) Verify(sig Signature, message []byte) bool {
	switch k.keyType {
	case skSchnorr:
		if schnorrData, ok := sig.ToSchnorr(); ok {
			return k.schnorr.SchnorrVerify(schnorrData, message)
		}
		return false
	case skECDSA:
		if ecdsaData, ok := sig.ToECDSA(); ok {
			return k.ecdsa.Verify(ecdsaData, message)
		}
		return false
	case skEd25519:
		if ed25519Data, ok := sig.ToEd25519(); ok {
			return k.ed25519.Verify(ed25519Data, message)
		}
		return false
	case skSSH:
		if sshSig := sig.ToSSH(); sshSig != nil {
			return k.ssh.Verify(sshSig, message)
		}
		return false
	case skMLDSA:
		if mldsaSig := sig.ToMLDSA(); mldsaSig != nil {
			return k.mldsa.Verify(*mldsaSig, message)
		}
		return false
	default:
		return false
	}
}

// ToSchnorr returns the underlying Schnorr public key.
func (k SigningPublicKey) ToSchnorr() *SchnorrPublicKey { return k.schnorr }

// ToECDSA returns the underlying ECDSA public key.
func (k SigningPublicKey) ToECDSA() *ECPublicKey { return k.ecdsa }

// ToSSH returns the underlying SSH public key.
func (k SigningPublicKey) ToSSH() *SSHPublicKey { return k.ssh }

// String returns a human-readable representation.
func (k SigningPublicKey) String() string {
	return fmt.Sprintf("SigningPublicKey(%s)", k.Scheme())
}

// Reference implements ReferenceProvider.
func (k SigningPublicKey) Reference() Reference {
	return ReferenceForCBORTaggedEncodable(k)
}

// --- CBOR support ---

func SigningPublicKeyCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagSigningPublicKey})
}

func (k SigningPublicKey) CBORTags() []dcbor.Tag { return SigningPublicKeyCBORTags() }

func (k SigningPublicKey) UntaggedCBOR() dcbor.CBOR {
	switch k.keyType {
	case skSchnorr:
		return dcbor.ToByteString(k.schnorr.Bytes())
	case skECDSA:
		return dcbor.NewCBORArray([]dcbor.CBOR{
			dcbor.MustFromAny(int64(1)),
			dcbor.ToByteString(k.ecdsa.Bytes()),
		})
	case skEd25519:
		return dcbor.NewCBORArray([]dcbor.CBOR{
			dcbor.MustFromAny(int64(2)),
			dcbor.ToByteString(k.ed25519.Bytes()),
		})
	case skSSH:
		if k.ssh != nil {
			return dcbor.NewCBORTagged(dcbor.TagWithValue(bctags.TagSSHTextPublicKey),
				dcbor.MustFromAny(string(k.ssh.AuthorizedKeyBytes())))
		}
		return dcbor.ToByteString(nil)
	case skMLDSA:
		if k.mldsa != nil {
			return k.mldsa.TaggedCBOR()
		}
		return dcbor.ToByteString(nil)
	default:
		return dcbor.ToByteString(nil)
	}
}

func (k SigningPublicKey) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k SigningPublicKey) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodeSigningPublicKey(cbor dcbor.CBOR) (SigningPublicKey, error) {
	// Try as byte string (Schnorr - 32 bytes)
	if data, err := cbor.TryIntoByteString(); err == nil {
		if len(data) == bccrypto.SchnorrPublicKeySize {
			key, err := SchnorrPublicKeyFromDataRef(data)
			if err != nil {
				return SigningPublicKey{}, err
			}
			return NewSigningPublicKeySchnorr(key), nil
		}
		return SigningPublicKey{}, dcbor.NewErrorf("invalid Schnorr public key size")
	}

	// Try as tagged CBOR (SSH or MLDSA)
	if tag, inner, ok := cbor.AsTaggedValue(); ok {
		if tag.Value() == bctags.TagSSHTextPublicKey {
			text, ok := inner.AsText()
			if !ok {
				return SigningPublicKey{}, dcbor.NewErrorf("SSH public key must be text")
			}
			sshKey, err := SSHPublicKeyFromAuthorizedKey([]byte(text))
			if err != nil {
				return SigningPublicKey{}, err
			}
			return NewSigningPublicKeySSH(sshKey), nil
		}
		if tag.Value() == bctags.TagMLDSAPublicKey {
			mldsaKey, err := DecodeTaggedMLDSAPublicKey(cbor)
			if err != nil {
				return SigningPublicKey{}, err
			}
			return NewSigningPublicKeyMLDSA(mldsaKey), nil
		}
	}

	// Try as array (ECDSA or Ed25519)
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return SigningPublicKey{}, dcbor.NewErrorf("invalid SigningPublicKey encoding")
	}
	if len(elements) != 2 {
		return SigningPublicKey{}, dcbor.NewErrorf("SigningPublicKey array must have 2 elements")
	}
	disc, ok := elements[0].AsInt64()
	if !ok {
		return SigningPublicKey{}, dcbor.NewErrorf("SigningPublicKey discriminator must be integer")
	}
	data, err := elements[1].TryIntoByteString()
	if err != nil {
		return SigningPublicKey{}, err
	}
	switch disc {
	case 1:
		key, err := ECPublicKeyFromDataRef(data)
		if err != nil {
			return SigningPublicKey{}, err
		}
		return NewSigningPublicKeyECDSA(key), nil
	case 2:
		key, err := Ed25519PublicKeyFromDataRef(data)
		if err != nil {
			return SigningPublicKey{}, err
		}
		return NewSigningPublicKeyEd25519(key), nil
	default:
		return SigningPublicKey{}, dcbor.NewErrorf("unknown SigningPublicKey discriminator: %d", disc)
	}
}

func DecodeTaggedSigningPublicKey(cbor dcbor.CBOR) (SigningPublicKey, error) {
	return dcbor.DecodeTagged(cbor, SigningPublicKeyCBORTags(), DecodeSigningPublicKey)
}

// --- UR support ---

func SigningPublicKeyToURString(k SigningPublicKey) string { return bcur.ToURString(k) }

func SigningPublicKeyFromURString(urString string) (SigningPublicKey, error) {
	return bcur.DecodeURString(urString, SigningPublicKeyCBORTags(), DecodeSigningPublicKey)
}

// sshScheme returns the signature scheme for an SSH public key.
func (k *SSHPublicKey) sshScheme() SignatureScheme {
	switch k.key.Type() {
	case "ssh-ed25519":
		return SchemeSSHEd25519
	case "ssh-dss":
		return SchemeSSHDSA
	case "ecdsa-sha2-nistp256":
		return SchemeSSHEcdsaP256
	case "ecdsa-sha2-nistp384":
		return SchemeSSHEcdsaP384
	default:
		return SchemeSSHEd25519
	}
}
