package bccomponents

import (
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

// SigningOptions holds algorithm-specific options for signing operations.
type SigningOptions struct {
	// For Schnorr signing: custom RNG for auxiliary randomness.
	SchnorrRNG bcrand.RandomNumberGenerator

	// For SSH signing: namespace and hash algorithm.
	SSHNamespace string
	SSHHashAlg   SSHHashAlg
}

// SSHHashAlg identifies the hash algorithm used for SSH signing.
type SSHHashAlg int

const (
	SSHHashSHA256 SSHHashAlg = iota
	SSHHashSHA512
)

// String returns the hash algorithm name.
func (h SSHHashAlg) String() string {
	switch h {
	case SSHHashSHA256:
		return "sha256"
	case SSHHashSHA512:
		return "sha512"
	default:
		return "unknown"
	}
}
