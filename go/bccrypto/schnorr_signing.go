package bccrypto

import (
	"bytes"
	"crypto/sha256"

	"github.com/btcsuite/btcd/btcec/v2"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

const SchnorrSignatureSize = 64

func taggedHash(tag string, parts ...[]byte) [32]byte {
	tagDigest := sha256.Sum256([]byte(tag))
	h := sha256.New()
	_, _ = h.Write(tagDigest[:])
	_, _ = h.Write(tagDigest[:])
	for _, part := range parts {
		_, _ = h.Write(part)
	}

	sum := h.Sum(nil)
	var out [32]byte
	copy(out[:], sum)
	return out
}

func parseXOnlyPublicKeyStrict(publicKey [SchnorrPublicKeySize]byte) *btcec.PublicKey {
	var x btcec.FieldVal
	if x.SetByteSlice(publicKey[:]) {
		panic("32 bytes, serialized according to the spec")
	}
	x.Normalize()

	var y btcec.FieldVal
	if !btcec.DecompressY(&x, false, &y) {
		panic("32 bytes, serialized according to the spec")
	}
	return btcec.NewPublicKey(&x, &y)
}

// SchnorrSign signs message with secure auxiliary randomness.
func SchnorrSign(
	ecdsaPrivateKey [ECDSAPrivateKeySize]byte,
	message []byte,
) [SchnorrSignatureSize]byte {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return SchnorrSignUsing(ecdsaPrivateKey, message, rng)
}

// SchnorrSignUsing signs message with RNG-provided auxiliary randomness.
func SchnorrSignUsing(
	ecdsaPrivateKey [ECDSAPrivateKeySize]byte,
	message []byte,
	rng bcrand.RandomNumberGenerator,
) [SchnorrSignatureSize]byte {
	aux := rng.RandomData(32)
	var auxRand [32]byte
	copy(auxRand[:], aux)
	return SchnorrSignWithAuxRand(ecdsaPrivateKey, message, auxRand)
}

// SchnorrSignWithAuxRand signs message using explicit 32-byte aux randomness.
func SchnorrSignWithAuxRand(
	ecdsaPrivateKey [ECDSAPrivateKeySize]byte,
	message []byte,
	auxRand [32]byte,
) [SchnorrSignatureSize]byte {
	var d btcec.ModNScalar
	if d.SetByteSlice(ecdsaPrivateKey[:]) || d.IsZero() {
		panic("32 bytes, within curve order")
	}

	sk := btcec.PrivKeyFromScalar(&d)
	compressed := sk.PubKey().SerializeCompressed()

	var px [32]byte
	copy(px[:], compressed[1:])

	if compressed[0] == 0x03 {
		d.Negate()
	}

	dBytes := d.Bytes()
	auxHash := taggedHash("BIP0340/aux", auxRand[:])
	var t [32]byte
	for i := 0; i < 32; i++ {
		t[i] = dBytes[i] ^ auxHash[i]
	}

	nonceHash := taggedHash("BIP0340/nonce", t[:], px[:], message)
	var k btcec.ModNScalar
	k.SetByteSlice(nonceHash[:])
	if k.IsZero() {
		panic("schnorr signing failed")
	}

	var rJac btcec.JacobianPoint
	btcec.ScalarBaseMultNonConst(&k, &rJac)
	rJac.ToAffine()

	if rJac.Y.Normalize().IsOdd() {
		k.Negate()
	}

	var rx [32]byte
	rJac.X.Normalize().PutBytes(&rx)

	eHash := taggedHash("BIP0340/challenge", rx[:], px[:], message)
	var e btcec.ModNScalar
	e.SetByteSlice(eHash[:])

	var ed btcec.ModNScalar
	ed.Mul2(&e, &d)

	var s btcec.ModNScalar
	s.Add2(&k, &ed)

	var sig [SchnorrSignatureSize]byte
	copy(sig[:32], rx[:])
	sBytes := s.Bytes()
	copy(sig[32:], sBytes[:])
	return sig
}

// SchnorrVerify verifies BIP340 signature over arbitrary message bytes.
func SchnorrVerify(
	schnorrPublicKey [SchnorrPublicKeySize]byte,
	schnorrSignature [SchnorrSignatureSize]byte,
	message []byte,
) bool {
	pk := parseXOnlyPublicKeyStrict(schnorrPublicKey)

	var r btcec.FieldVal
	if r.SetByteSlice(schnorrSignature[:32]) {
		return false
	}
	r.Normalize()

	var s btcec.ModNScalar
	if s.SetByteSlice(schnorrSignature[32:]) {
		return false
	}

	eHash := taggedHash(
		"BIP0340/challenge",
		schnorrSignature[:32],
		schnorrPublicKey[:],
		message,
	)
	var e btcec.ModNScalar
	e.SetByteSlice(eHash[:])

	var pJac btcec.JacobianPoint
	pk.AsJacobian(&pJac)

	var eP btcec.JacobianPoint
	btcec.ScalarMultNonConst(&e, &pJac, &eP)
	eP.Y.Normalize().Negate(1)

	var sG btcec.JacobianPoint
	btcec.ScalarBaseMultNonConst(&s, &sG)

	var rPoint btcec.JacobianPoint
	btcec.AddNonConst(&sG, &eP, &rPoint)
	if rPoint.Z.Normalize().IsZero() {
		return false
	}

	rPoint.ToAffine()
	if rPoint.Y.Normalize().IsOdd() {
		return false
	}

	var rx [32]byte
	rPoint.X.Normalize().PutBytes(&rx)
	return bytes.Equal(rx[:], schnorrSignature[:32])
}
