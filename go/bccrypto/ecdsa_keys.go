package bccrypto

import (
	"github.com/btcsuite/btcd/btcec/v2"
	btecdsa "github.com/btcsuite/btcd/btcec/v2/ecdsa"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

const (
	ECDSAPrivateKeySize            = 32
	ECDSAPublicKeySize             = 33
	ECDSAUncompressedPublicKeySize = 65
	ECDSAMessageHashSize           = 32
	ECDSASignatureSize             = 64
	SchnorrPublicKeySize           = 32
)

func privateKeyFromBytesStrict(privateKey [ECDSAPrivateKeySize]byte) *btcec.PrivateKey {
	var scalar btcec.ModNScalar
	overflow := scalar.SetByteSlice(privateKey[:])
	if overflow || scalar.IsZero() {
		panic("32 bytes, within curve order")
	}
	return btcec.PrivKeyFromScalar(&scalar)
}

func parseCompressedPublicKeyStrict(
	publicKey [ECDSAPublicKeySize]byte,
) *btcec.PublicKey {
	pk, err := btcec.ParsePubKey(publicKey[:])
	if err != nil {
		panic("33 or 65 bytes, serialized according to the spec")
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
		panic("65 bytes, serialized according to the spec")
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
		panic("33 bytes, serialized according to the spec")
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
		panic("64 bytes, signature according to the spec")
	}
	if r.IsZero() || s.IsZero() {
		panic("64 bytes, signature according to the spec")
	}
	return btecdsa.NewSignature(&r, &s)
}
