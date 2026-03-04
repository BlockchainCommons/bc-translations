package bccomponents

import (
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

// Keypair generates a key pair using the default signature and encapsulation
// schemes (Schnorr for signing, X25519 for encapsulation).
func Keypair() (PrivateKeys, PublicKeys, error) {
	return KeypairOpt(SchemeSchnorr, EncapsulationX25519)
}

// KeypairUsing generates a key pair using the default schemes and a custom
// random number generator.
func KeypairUsing(rng bcrand.RandomNumberGenerator) (PrivateKeys, PublicKeys, error) {
	return KeypairOptUsing(SchemeSchnorr, EncapsulationX25519, rng)
}

// KeypairOpt generates a key pair with specified signature and encapsulation
// schemes.
func KeypairOpt(sigScheme SignatureScheme, encapScheme EncapsulationScheme) (PrivateKeys, PublicKeys, error) {
	sigPriv, sigPub, err := SigningKeypair(sigScheme)
	if err != nil {
		return PrivateKeys{}, PublicKeys{}, err
	}
	encapPriv, encapPub, err := EncapsulationKeypair(encapScheme)
	if err != nil {
		return PrivateKeys{}, PublicKeys{}, err
	}
	return NewPrivateKeys(sigPriv, encapPriv), NewPublicKeys(sigPub, encapPub), nil
}

// KeypairOptUsing generates a key pair with specified schemes using a custom
// random number generator.
func KeypairOptUsing(sigScheme SignatureScheme, encapScheme EncapsulationScheme, rng bcrand.RandomNumberGenerator) (PrivateKeys, PublicKeys, error) {
	sigPriv, sigPub, err := SigningKeypairUsing(sigScheme, rng, "")
	if err != nil {
		return PrivateKeys{}, PublicKeys{}, err
	}
	// EncapsulationKeypair does not currently accept an RNG parameter,
	// so we use the standard EncapsulationKeypair function.
	encapPriv, encapPub, err := EncapsulationKeypair(encapScheme)
	if err != nil {
		return PrivateKeys{}, PublicKeys{}, err
	}
	return NewPrivateKeys(sigPriv, encapPriv), NewPublicKeys(sigPub, encapPub), nil
}
