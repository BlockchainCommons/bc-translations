package bccomponents

// SignatureScheme identifies the digital signature algorithm used.
type SignatureScheme int

const (
	SchemeSchnorr     SignatureScheme = iota
	SchemeECDSA
	SchemeEd25519
	SchemeMLDSA44
	SchemeMLDSA65
	SchemeMLDSA87
	SchemeSSHEd25519
	SchemeSSHDSA
	SchemeSSHEcdsaP256
	SchemeSSHEcdsaP384
)

// String returns the scheme name.
func (s SignatureScheme) String() string {
	switch s {
	case SchemeSchnorr:
		return "Schnorr"
	case SchemeECDSA:
		return "ECDSA"
	case SchemeEd25519:
		return "Ed25519"
	case SchemeMLDSA44:
		return "MLDSA44"
	case SchemeMLDSA65:
		return "MLDSA65"
	case SchemeMLDSA87:
		return "MLDSA87"
	case SchemeSSHEd25519:
		return "SSH-Ed25519"
	case SchemeSSHDSA:
		return "SSH-DSA"
	case SchemeSSHEcdsaP256:
		return "SSH-ECDSA-P256"
	case SchemeSSHEcdsaP384:
		return "SSH-ECDSA-P384"
	default:
		return "Unknown"
	}
}
