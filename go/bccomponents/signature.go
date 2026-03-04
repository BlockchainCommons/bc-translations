package bccomponents

import (
	"fmt"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// signatureType is the internal discriminator for Signature variants.
type signatureType int

const (
	sigSchnorr signatureType = iota
	sigECDSA
	sigEd25519
	sigSSH
	sigMLDSA
)

// Signature is a digital signature produced by one of the supported schemes.
type Signature struct {
	sigType signatureType
	schnorr [bccrypto.SchnorrSignatureSize]byte
	ecdsa   [bccrypto.ECDSASignatureSize]byte
	ed25519 [bccrypto.Ed25519SignatureSize]byte
	ssh     *SSHSignature
	mldsa   *MLDSASignature
}

// SchnorrSignatureFromData creates a Schnorr Signature.
func SchnorrSignatureFromData(data [bccrypto.SchnorrSignatureSize]byte) Signature {
	return Signature{sigType: sigSchnorr, schnorr: data}
}

// SchnorrSignatureFromDataRef creates a Schnorr Signature from a byte slice.
func SchnorrSignatureFromDataRef(data []byte) (Signature, error) {
	if len(data) != bccrypto.SchnorrSignatureSize {
		return Signature{}, errInvalidSize("Schnorr signature", bccrypto.SchnorrSignatureSize, len(data))
	}
	var sig Signature
	sig.sigType = sigSchnorr
	copy(sig.schnorr[:], data)
	return sig, nil
}

// ECDSASignatureFromData creates an ECDSA Signature.
func ECDSASignatureFromData(data [bccrypto.ECDSASignatureSize]byte) Signature {
	return Signature{sigType: sigECDSA, ecdsa: data}
}

// ECDSASignatureFromDataRef creates an ECDSA Signature from a byte slice.
func ECDSASignatureFromDataRef(data []byte) (Signature, error) {
	if len(data) != bccrypto.ECDSASignatureSize {
		return Signature{}, errInvalidSize("ECDSA signature", bccrypto.ECDSASignatureSize, len(data))
	}
	var sig Signature
	sig.sigType = sigECDSA
	copy(sig.ecdsa[:], data)
	return sig, nil
}

// Ed25519SignatureFromData creates an Ed25519 Signature.
func Ed25519SignatureFromData(data [bccrypto.Ed25519SignatureSize]byte) Signature {
	return Signature{sigType: sigEd25519, ed25519: data}
}

// Ed25519SignatureFromDataRef creates an Ed25519 Signature from a byte slice.
func Ed25519SignatureFromDataRef(data []byte) (Signature, error) {
	if len(data) != bccrypto.Ed25519SignatureSize {
		return Signature{}, errInvalidSize("Ed25519 signature", bccrypto.Ed25519SignatureSize, len(data))
	}
	var sig Signature
	sig.sigType = sigEd25519
	copy(sig.ed25519[:], data)
	return sig, nil
}

// SSHSignatureFrom creates an SSH Signature.
func SSHSignatureFrom(sig *SSHSignature) Signature {
	return Signature{sigType: sigSSH, ssh: sig}
}

// MLDSASignatureFrom creates an ML-DSA Signature.
func MLDSASignatureFrom(sig MLDSASignature) Signature {
	return Signature{sigType: sigMLDSA, mldsa: &sig}
}

// Scheme returns the signature's scheme.
func (s Signature) Scheme() SignatureScheme {
	switch s.sigType {
	case sigSchnorr:
		return SchemeSchnorr
	case sigECDSA:
		return SchemeECDSA
	case sigEd25519:
		return SchemeEd25519
	case sigSSH:
		if s.ssh != nil {
			return s.ssh.Scheme()
		}
		return SchemeSSHEd25519
	case sigMLDSA:
		if s.mldsa != nil {
			switch s.mldsa.level {
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

// ToSchnorr returns the Schnorr signature data, or false.
func (s Signature) ToSchnorr() ([bccrypto.SchnorrSignatureSize]byte, bool) {
	if s.sigType == sigSchnorr {
		return s.schnorr, true
	}
	return [bccrypto.SchnorrSignatureSize]byte{}, false
}

// ToECDSA returns the ECDSA signature data, or false.
func (s Signature) ToECDSA() ([bccrypto.ECDSASignatureSize]byte, bool) {
	if s.sigType == sigECDSA {
		return s.ecdsa, true
	}
	return [bccrypto.ECDSASignatureSize]byte{}, false
}

// ToEd25519 returns the Ed25519 signature data, or false.
func (s Signature) ToEd25519() ([bccrypto.Ed25519SignatureSize]byte, bool) {
	if s.sigType == sigEd25519 {
		return s.ed25519, true
	}
	return [bccrypto.Ed25519SignatureSize]byte{}, false
}

// ToSSH returns the SSH signature, or nil.
func (s Signature) ToSSH() *SSHSignature {
	if s.sigType == sigSSH {
		return s.ssh
	}
	return nil
}

// ToMLDSA returns the ML-DSA signature, or nil.
func (s Signature) ToMLDSA() *MLDSASignature {
	if s.sigType == sigMLDSA {
		return s.mldsa
	}
	return nil
}

// String returns a human-readable representation.
func (s Signature) String() string {
	return fmt.Sprintf("Signature(%s)", s.Scheme())
}

// --- CBOR support ---

func SignatureCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagSignature})
}

func (s Signature) CBORTags() []dcbor.Tag { return SignatureCBORTags() }

func (s Signature) UntaggedCBOR() dcbor.CBOR {
	switch s.sigType {
	case sigSchnorr:
		return dcbor.ToByteString(s.schnorr[:])
	case sigECDSA:
		return dcbor.NewCBORArray([]dcbor.CBOR{
			dcbor.MustFromAny(int64(1)),
			dcbor.ToByteString(s.ecdsa[:]),
		})
	case sigEd25519:
		return dcbor.NewCBORArray([]dcbor.CBOR{
			dcbor.MustFromAny(int64(2)),
			dcbor.ToByteString(s.ed25519[:]),
		})
	case sigSSH:
		if s.ssh != nil {
			return s.ssh.ToCBOR()
		}
		return dcbor.ToByteString(nil)
	case sigMLDSA:
		if s.mldsa != nil {
			return s.mldsa.TaggedCBOR()
		}
		return dcbor.ToByteString(nil)
	default:
		return dcbor.ToByteString(nil)
	}
}

func (s Signature) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(s)
	return cbor
}

func (s Signature) ToCBOR() dcbor.CBOR { return s.TaggedCBOR() }

func DecodeSignature(cbor dcbor.CBOR) (Signature, error) {
	// Try as byte string first (Schnorr)
	if data, err := cbor.TryIntoByteString(); err == nil {
		if len(data) == bccrypto.SchnorrSignatureSize {
			sig, _ := SchnorrSignatureFromDataRef(data)
			return sig, nil
		}
		return Signature{}, dcbor.NewErrorf("invalid Schnorr signature size")
	}

	// Try as tagged CBOR (SSH or MLDSA)
	if tag, _, ok := cbor.AsTaggedValue(); ok {
		if tag.Value() == bctags.TagSSHTextSignature {
			sshSig, err := DecodeSSHSignature(cbor)
			if err != nil {
				return Signature{}, err
			}
			return SSHSignatureFrom(sshSig), nil
		}
		if tag.Value() == bctags.TagMLDSASignature {
			mldsaSig, err := DecodeTaggedMLDSASignature(cbor)
			if err != nil {
				return Signature{}, err
			}
			return MLDSASignatureFrom(mldsaSig), nil
		}
	}

	// Try as array (ECDSA or Ed25519)
	elements, err := cbor.TryIntoArray()
	if err != nil {
		return Signature{}, dcbor.NewErrorf("invalid Signature encoding")
	}
	if len(elements) != 2 {
		return Signature{}, dcbor.NewErrorf("Signature array must have 2 elements")
	}
	disc, ok := elements[0].AsInt64()
	if !ok {
		return Signature{}, dcbor.NewErrorf("Signature discriminator must be integer")
	}
	data, err := elements[1].TryIntoByteString()
	if err != nil {
		return Signature{}, err
	}
	switch disc {
	case 1:
		return ECDSASignatureFromDataRef(data)
	case 2:
		return Ed25519SignatureFromDataRef(data)
	default:
		return Signature{}, dcbor.NewErrorf("unknown Signature discriminator: %d", disc)
	}
}

func DecodeTaggedSignature(cbor dcbor.CBOR) (Signature, error) {
	return dcbor.DecodeTagged(cbor, SignatureCBORTags(), DecodeSignature)
}
