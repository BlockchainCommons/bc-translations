package bccrypto

import (
	"github.com/btcsuite/btcd/btcec/v2/ecdsa"
)

// ECDSASign signs message using secp256k1 ECDSA over double-SHA256(message).
func ECDSASign(
	privateKey [ECDSAPrivateKeySize]byte,
	message []byte,
) [ECDSASignatureSize]byte {
	sk := privateKeyFromBytesStrict(privateKey)
	hash := DoubleSHA256(message)
	compact := ecdsa.SignCompact(sk, hash[:], true)

	var out [ECDSASignatureSize]byte
	copy(out[:], compact[1:])
	return out
}

// ECDSAVerify verifies compact 64-byte ECDSA signature over double-SHA256(message).
func ECDSAVerify(
	publicKey [ECDSAPublicKeySize]byte,
	signature [ECDSASignatureSize]byte,
	message []byte,
) bool {
	pk := parseCompressedPublicKeyStrict(publicKey)
	sig := ecdsaSignatureFromCompact(signature)
	hash := DoubleSHA256(message)
	return sig.Verify(hash[:], pk)
}
