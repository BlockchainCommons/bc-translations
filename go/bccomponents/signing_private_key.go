package bccomponents

import (
	"fmt"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// signingKeyType is the internal discriminator for SigningPrivateKey variants.
type signingKeyType int

const (
	skSchnorr signingKeyType = iota
	skECDSA
	skEd25519
	skSSH
	skMLDSA
)

// SigningPrivateKey is a private key that can produce digital signatures.
type SigningPrivateKey struct {
	keyType signingKeyType
	schnorr *ECPrivateKey
	ecdsa   *ECPrivateKey
	ed25519 *Ed25519PrivateKey
	ssh     *SSHPrivateKey
	mldsa   *MLDSAPrivateKey
}

// NewSigningPrivateKeySchnorr creates a Schnorr signing private key.
func NewSigningPrivateKeySchnorr(key ECPrivateKey) SigningPrivateKey {
	return SigningPrivateKey{keyType: skSchnorr, schnorr: &key}
}

// NewSigningPrivateKeyECDSA creates an ECDSA signing private key.
func NewSigningPrivateKeyECDSA(key ECPrivateKey) SigningPrivateKey {
	return SigningPrivateKey{keyType: skECDSA, ecdsa: &key}
}

// NewSigningPrivateKeyEd25519 creates an Ed25519 signing private key.
func NewSigningPrivateKeyEd25519(key Ed25519PrivateKey) SigningPrivateKey {
	return SigningPrivateKey{keyType: skEd25519, ed25519: &key}
}

// NewSigningPrivateKeySSH creates an SSH signing private key.
func NewSigningPrivateKeySSH(key *SSHPrivateKey) SigningPrivateKey {
	return SigningPrivateKey{keyType: skSSH, ssh: key}
}

// NewSigningPrivateKeyMLDSA creates an ML-DSA signing private key.
func NewSigningPrivateKeyMLDSA(key *MLDSAPrivateKey) SigningPrivateKey {
	return SigningPrivateKey{keyType: skMLDSA, mldsa: key}
}

// Scheme returns the signature scheme.
func (k SigningPrivateKey) Scheme() SignatureScheme {
	switch k.keyType {
	case skSchnorr:
		return SchemeSchnorr
	case skECDSA:
		return SchemeECDSA
	case skEd25519:
		return SchemeEd25519
	case skSSH:
		if k.ssh != nil {
			return k.ssh.PublicKey().sshScheme()
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

// PublicKey derives the corresponding signing public key.
func (k SigningPrivateKey) PublicKey() SigningPublicKey {
	switch k.keyType {
	case skSchnorr:
		pub := k.schnorr.SchnorrPublicKey()
		return NewSigningPublicKeySchnorr(pub)
	case skECDSA:
		pub := k.ecdsa.ECDSAPublicKey()
		return NewSigningPublicKeyECDSA(pub)
	case skEd25519:
		pub := k.ed25519.PublicKey()
		return NewSigningPublicKeyEd25519(pub)
	case skSSH:
		return NewSigningPublicKeySSH(k.ssh.PublicKey())
	case skMLDSA:
		// ML-DSA public key derivation not directly available from private key alone.
		// It should be stored alongside. For now, return empty.
		return SigningPublicKey{}
	default:
		return SigningPublicKey{}
	}
}

// Sign signs a message with default options.
func (k SigningPrivateKey) Sign(message []byte) (Signature, error) {
	return k.SignWithOptions(message, nil)
}

// SignWithOptions signs a message with the provided options.
func (k SigningPrivateKey) SignWithOptions(message []byte, options *SigningOptions) (Signature, error) {
	switch k.keyType {
	case skSchnorr:
		var sig [bccrypto.SchnorrSignatureSize]byte
		if options != nil && options.SchnorrRNG != nil {
			sig = k.schnorr.SchnorrSignUsing(message, options.SchnorrRNG)
		} else {
			sig = k.schnorr.SchnorrSign(message)
		}
		return SchnorrSignatureFromData(sig), nil
	case skECDSA:
		sig := k.ecdsa.ECDSASign(message)
		return ECDSASignatureFromData(sig), nil
	case skEd25519:
		sig := k.ed25519.Sign(message)
		return Ed25519SignatureFromData(sig), nil
	case skSSH:
		if options == nil {
			return Signature{}, errSSH("SSH signing requires SigningOptions")
		}
		sshSig, err := k.ssh.SignSSH(message, options.SSHNamespace, options.SSHHashAlg)
		if err != nil {
			return Signature{}, err
		}
		return SSHSignatureFrom(sshSig), nil
	case skMLDSA:
		mldsaSig, err := k.mldsa.Sign(message)
		if err != nil {
			return Signature{}, err
		}
		return MLDSASignatureFrom(mldsaSig), nil
	default:
		return Signature{}, errGeneral("unsupported signing key type")
	}
}

// ToSchnorr returns the underlying EC private key if this is a Schnorr key.
func (k SigningPrivateKey) ToSchnorr() *ECPrivateKey { return k.schnorr }

// ToECDSA returns the underlying EC private key if this is an ECDSA key.
func (k SigningPrivateKey) ToECDSA() *ECPrivateKey { return k.ecdsa }

// ToSSH returns the underlying SSH private key if this is an SSH key.
func (k SigningPrivateKey) ToSSH() *SSHPrivateKey { return k.ssh }

// IsSchnorr reports whether this is a Schnorr key.
func (k SigningPrivateKey) IsSchnorr() bool { return k.keyType == skSchnorr }

// IsECDSA reports whether this is an ECDSA key.
func (k SigningPrivateKey) IsECDSA() bool { return k.keyType == skECDSA }

// IsEd25519 reports whether this is an Ed25519 key.
func (k SigningPrivateKey) IsEd25519() bool { return k.keyType == skEd25519 }

// IsSSH reports whether this is an SSH key.
func (k SigningPrivateKey) IsSSH() bool { return k.keyType == skSSH }

// String returns a human-readable representation.
func (k SigningPrivateKey) String() string {
	return fmt.Sprintf("SigningPrivateKey(%s)", k.Scheme())
}

// Reference implements ReferenceProvider.
func (k SigningPrivateKey) Reference() Reference {
	return ReferenceForCBORTaggedEncodable(k)
}

// --- CBOR support ---

func SigningPrivateKeyCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagSigningPrivateKey})
}

func (k SigningPrivateKey) CBORTags() []dcbor.Tag { return SigningPrivateKeyCBORTags() }

func (k SigningPrivateKey) UntaggedCBOR() dcbor.CBOR {
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
		if k.ssh != nil && k.ssh.OpenSSHBytes() != nil {
			return dcbor.NewCBORTagged(dcbor.TagWithValue(bctags.TagSSHTextPrivateKey),
				dcbor.MustFromAny(string(k.ssh.OpenSSHBytes())))
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

func (k SigningPrivateKey) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(k)
	return cbor
}

func (k SigningPrivateKey) ToCBOR() dcbor.CBOR { return k.TaggedCBOR() }

func DecodeSigningPrivateKey(cbor dcbor.CBOR) (SigningPrivateKey, error) {
	// Try as byte string (Schnorr)
	if data, err := cbor.TryIntoByteString(); err == nil {
		key, err := ECPrivateKeyFromDataRef(data)
		if err != nil {
			return SigningPrivateKey{}, err
		}
		return NewSigningPrivateKeySchnorr(key), nil
	}

	// Try as tagged CBOR (SSH or MLDSA)
	if tag, inner, ok := cbor.AsTaggedValue(); ok {
		if tag.Value() == bctags.TagSSHTextPrivateKey {
			text, ok := inner.AsText()
			if !ok {
				return SigningPrivateKey{}, dcbor.NewErrorf("SSH private key must be text")
			}
			sshKey, err := SSHPrivateKeyFromOpenSSH([]byte(text))
			if err != nil {
				return SigningPrivateKey{}, err
			}
			return NewSigningPrivateKeySSH(sshKey), nil
		}
		if tag.Value() == bctags.TagMLDSAPrivateKey {
			mldsaKey, err := DecodeTaggedMLDSAPrivateKey(cbor)
			if err != nil {
				return SigningPrivateKey{}, err
			}
			return NewSigningPrivateKeyMLDSA(mldsaKey), nil
		}
	}

	// Try as array (ECDSA or Ed25519)
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return SigningPrivateKey{}, dcbor.NewErrorf("invalid SigningPrivateKey encoding")
	}
	if len(elements) != 2 {
		return SigningPrivateKey{}, dcbor.NewErrorf("SigningPrivateKey array must have 2 elements")
	}
	disc, ok := elements[0].AsInt64()
	if !ok {
		return SigningPrivateKey{}, dcbor.NewErrorf("SigningPrivateKey discriminator must be integer")
	}
	data, err := elements[1].TryIntoByteString()
	if err != nil {
		return SigningPrivateKey{}, err
	}
	switch disc {
	case 1:
		key, err := ECPrivateKeyFromDataRef(data)
		if err != nil {
			return SigningPrivateKey{}, err
		}
		return NewSigningPrivateKeyECDSA(key), nil
	case 2:
		key, err := Ed25519PrivateKeyFromDataRef(data)
		if err != nil {
			return SigningPrivateKey{}, err
		}
		return NewSigningPrivateKeyEd25519(key), nil
	default:
		return SigningPrivateKey{}, dcbor.NewErrorf("unknown SigningPrivateKey discriminator: %d", disc)
	}
}

func DecodeTaggedSigningPrivateKey(cbor dcbor.CBOR) (SigningPrivateKey, error) {
	return dcbor.DecodeTagged(cbor, SigningPrivateKeyCBORTags(), DecodeSigningPrivateKey)
}

// Keypair generates a new signing keypair for the given scheme.
func SigningKeypair(scheme SignatureScheme) (SigningPrivateKey, SigningPublicKey, error) {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return SigningKeypairUsing(scheme, rng, "")
}

// SigningKeypairUsing generates a new signing keypair using the provided RNG.
func SigningKeypairUsing(scheme SignatureScheme, rng bcrand.RandomNumberGenerator, comment string) (SigningPrivateKey, SigningPublicKey, error) {
	switch scheme {
	case SchemeSchnorr:
		ecKey := NewECPrivateKeyUsing(rng)
		priv := NewSigningPrivateKeySchnorr(ecKey)
		pub := priv.PublicKey()
		return priv, pub, nil
	case SchemeECDSA:
		ecKey := NewECPrivateKeyUsing(rng)
		priv := NewSigningPrivateKeyECDSA(ecKey)
		pub := priv.PublicKey()
		return priv, pub, nil
	case SchemeEd25519:
		edKey := NewEd25519PrivateKeyUsing(rng)
		priv := NewSigningPrivateKeyEd25519(edKey)
		pub := priv.PublicKey()
		return priv, pub, nil
	case SchemeMLDSA44:
		mldsaPriv, mldsaPub, err := MLDSA44.Keypair()
		if err != nil {
			return SigningPrivateKey{}, SigningPublicKey{}, err
		}
		priv := NewSigningPrivateKeyMLDSA(&mldsaPriv)
		pub := NewSigningPublicKeyMLDSA(&mldsaPub)
		return priv, pub, nil
	case SchemeMLDSA65:
		mldsaPriv, mldsaPub, err := MLDSA65.Keypair()
		if err != nil {
			return SigningPrivateKey{}, SigningPublicKey{}, err
		}
		priv := NewSigningPrivateKeyMLDSA(&mldsaPriv)
		pub := NewSigningPublicKeyMLDSA(&mldsaPub)
		return priv, pub, nil
	case SchemeMLDSA87:
		mldsaPriv, mldsaPub, err := MLDSA87.Keypair()
		if err != nil {
			return SigningPrivateKey{}, SigningPublicKey{}, err
		}
		priv := NewSigningPrivateKeyMLDSA(&mldsaPriv)
		pub := NewSigningPublicKeyMLDSA(&mldsaPub)
		return priv, pub, nil
	case SchemeSSHEd25519:
		seed := rng.RandomData(32)
		sshKey, err := generateSSHEd25519Key(seed, comment)
		if err != nil {
			return SigningPrivateKey{}, SigningPublicKey{}, err
		}
		priv := NewSigningPrivateKeySSH(sshKey)
		pub := NewSigningPublicKeySSH(sshKey.PublicKey())
		return priv, pub, nil
	default:
		return SigningPrivateKey{}, SigningPublicKey{}, errGeneral("unsupported signature scheme")
	}
}
