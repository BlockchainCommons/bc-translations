package provenancemark

import (
	"crypto/sha256"
	"io"

	"golang.org/x/crypto/chacha20"
	"golang.org/x/crypto/hkdf"
)

// SHA256Size is the byte length of a SHA-256 digest.
const SHA256Size = 32

// SHA256 returns the SHA-256 digest of data.
func SHA256(data []byte) [SHA256Size]byte {
	return sha256.Sum256(data)
}

// SHA256Prefix returns the first prefix bytes of the SHA-256 digest of data.
func SHA256Prefix(data []byte, prefix int) []byte {
	digest := SHA256(data)
	if prefix > len(digest) {
		prefix = len(digest)
	}
	if prefix < 0 {
		prefix = 0
	}
	return append([]byte(nil), digest[:prefix]...)
}

// HKDFHMACSHA256 derives keyLen bytes using HKDF-HMAC-SHA-256.
func HKDFHMACSHA256(keyMaterial, salt []byte, keyLen int) []byte {
	reader := hkdf.New(sha256.New, keyMaterial, salt, nil)
	key := make([]byte, keyLen)
	if _, err := io.ReadFull(reader, key); err != nil {
		panic("provenancemark: hkdf expand failed: " + err.Error())
	}
	return key
}

// ExtendKey expands arbitrary key material to 32 bytes with HKDF-HMAC-SHA-256.
func ExtendKey(data []byte) [32]byte {
	derived := HKDFHMACSHA256(data, nil, 32)
	var result [32]byte
	copy(result[:], derived)
	return result
}

// Obfuscate XORs message with the ChaCha20 keystream derived from key.
func Obfuscate(key, message []byte) []byte {
	if len(message) == 0 {
		return append([]byte(nil), message...)
	}

	extendedKey := ExtendKey(key)
	var nonce [12]byte
	for i := range nonce {
		nonce[i] = extendedKey[len(extendedKey)-1-i]
	}

	cipher, err := chacha20.NewUnauthenticatedCipher(extendedKey[:], nonce[:])
	if err != nil {
		panic("provenancemark: chacha20 init failed: " + err.Error())
	}

	buffer := append([]byte(nil), message...)
	cipher.XORKeyStream(buffer, buffer)
	return buffer
}
