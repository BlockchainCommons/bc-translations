package bccrypto

import (
	"github.com/btcsuite/btcd/btcec/v2"
	btecdsa "github.com/btcsuite/btcd/btcec/v2/ecdsa"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

const (
	// ECDSAPrivateKeySize is the byte length of a secp256k1 private key.
	ECDSAPrivateKeySize = 32
	// ECDSAPublicKeySize is the byte length of a compressed secp256k1 public key.
	ECDSAPublicKeySize = 33
	// ECDSAUncompressedPublicKeySize is the byte length of an uncompressed secp256k1 public key.
	ECDSAUncompressedPublicKeySize = 65
	// ECDSAMessageHashSize is the byte length of the message hash used in ECDSA signing.
	ECDSAMessageHashSize = 32
	// ECDSASignatureSize is the byte length of a compact (r||s) ECDSA signature.
	ECDSASignatureSize = 64
	// SchnorrPublicKeySize is the byte length of an x-only Schnorr public key.
	SchnorrPublicKeySize = 32
)

func privateKeyFromBytesStrict(privateKey [ECDSAPrivateKeySize]byte) *btcec.PrivateKey {
	var scalar btcec.ModNScalar
	overflow := scalar.SetByteSlice(privateKey[:])
	if overflow || scalar.IsZero() {
		panic("bccrypto: private key must be non-zero and within curve order")
	}
	return btcec.PrivKeyFromScalar(&scalar)
}

func parseCompressedPublicKeyStrict(
	publicKey [ECDSAPublicKeySize]byte,
) *btcec.PublicKey {
	pk, err := btcec.ParsePubKey(publicKey[:])
	if err != nil {
		panic("bccrypto: invalid compressed public key")
	}
	return pk
}

// ECDSANewPrivateKeyUsing generates a new ECDSA private key using rng.
func ECDSANewPrivateKeyUsing(
	rng bcrand.RandomNumberGenerator,
) [ECDSAPrivateKeySize]byte {
	data := rng.RandomData(ECDSAPrivateKeySize)
	var out [ECDSAPrivateKeySize]byte
	copy(out[:], data)
	return out
}

// ECDSAPublicKeyFromPrivateKey derives compressed secp256k1 public key bytes.
func ECDSAPublicKeyFromPrivateKey(
	privateKey [ECDSAPrivateKeySize]byte,
) [ECDSAPublicKeySize]byte {
	pk := privateKeyFromBytesStrict(privateKey).PubKey().SerializeCompressed()
	var out [ECDSAPublicKeySize]byte
	copy(out[:], pk)
	return out
}

// ECDSADecompressPublicKey decompresses a compressed secp256k1 public key.
func ECDSADecompressPublicKey(
	compressedPublicKey [ECDSAPublicKeySize]byte,
) [ECDSAUncompressedPublicKeySize]byte {
	pk, err := btcec.ParsePubKey(compressedPublicKey[:])
	if err != nil {
		panic("bccrypto: invalid compressed public key")
	}
	var out [ECDSAUncompressedPublicKeySize]byte
	copy(out[:], pk.SerializeUncompressed())
	return out
}

// ECDSACompressPublicKey compresses an uncompressed secp256k1 public key.
func ECDSACompressPublicKey(
	uncompressedPublicKey [ECDSAUncompressedPublicKeySize]byte,
) [ECDSAPublicKeySize]byte {
	pk, err := btcec.ParsePubKey(uncompressedPublicKey[:])
	if err != nil {
		panic("bccrypto: invalid uncompressed public key")
	}
	var out [ECDSAPublicKeySize]byte
	copy(out[:], pk.SerializeCompressed())
	return out
}

// ECDSADerivePrivateKey derives private key bytes from key material.
func ECDSADerivePrivateKey(keyMaterial []byte) []byte {
	return HKDFHMACSHA256(keyMaterial, []byte("signing"), 32)
}

// SchnorrPublicKeyFromPrivateKey derives x-only Schnorr public key bytes.
func SchnorrPublicKeyFromPrivateKey(
	privateKey [ECDSAPrivateKeySize]byte,
) [SchnorrPublicKeySize]byte {
	compressed := privateKeyFromBytesStrict(privateKey).PubKey().SerializeCompressed()
	var out [SchnorrPublicKeySize]byte
	copy(out[:], compressed[1:])
	return out
}

func ecdsaSignatureFromCompact(signature [ECDSASignatureSize]byte) *btecdsa.Signature {
	var r, s btcec.ModNScalar
	if r.SetByteSlice(signature[:32]) || s.SetByteSlice(signature[32:]) {
		panic("bccrypto: signature components overflow curve order")
	}
	if r.IsZero() || s.IsZero() {
		panic("bccrypto: signature components must be non-zero")
	}
	return btecdsa.NewSignature(&r, &s)
}
