package bccrypto

import (
	"crypto/hmac"
	"crypto/sha256"
	"crypto/sha512"
	"encoding/binary"
	"hash"
	"hash/crc32"
	"io"

	"golang.org/x/crypto/hkdf"
	"golang.org/x/crypto/pbkdf2"
)

const (
	// CRC32Size is the byte length of a CRC-32 checksum.
	CRC32Size = 4
	// SHA256Size is the byte length of a SHA-256 digest.
	SHA256Size = 32
	// SHA512Size is the byte length of a SHA-512 digest.
	SHA512Size = 64
)

// CRC32 computes the CRC-32 checksum of data.
func CRC32(data []byte) uint32 {
	return crc32.ChecksumIEEE(data)
}

// CRC32DataWithEndian computes CRC-32 and returns bytes in the selected endianness.
func CRC32DataWithEndian(data []byte, littleEndian bool) [CRC32Size]byte {
	checksum := CRC32(data)
	var result [CRC32Size]byte
	if littleEndian {
		binary.LittleEndian.PutUint32(result[:], checksum)
	} else {
		binary.BigEndian.PutUint32(result[:], checksum)
	}
	return result
}

// CRC32Data computes CRC-32 and returns bytes in big-endian format.
func CRC32Data(data []byte) [CRC32Size]byte {
	return CRC32DataWithEndian(data, false)
}

// SHA256 computes the SHA-256 digest of data.
func SHA256(data []byte) [SHA256Size]byte {
	return sha256.Sum256(data)
}

// DoubleSHA256 computes SHA-256(SHA-256(message)).
func DoubleSHA256(message []byte) [SHA256Size]byte {
	first := SHA256(message)
	return SHA256(first[:])
}

// SHA512 computes the SHA-512 digest of data.
func SHA512(data []byte) [SHA512Size]byte {
	return sha512.Sum512(data)
}

// HMACSHA256 computes HMAC-SHA-256(key, message).
func HMACSHA256(key, message []byte) [SHA256Size]byte {
	mac := hmac.New(sha256.New, key)
	_, _ = mac.Write(message)
	var out [SHA256Size]byte
	copy(out[:], mac.Sum(nil))
	return out
}

// HMACSHA512 computes HMAC-SHA-512(key, message).
func HMACSHA512(key, message []byte) [SHA512Size]byte {
	mac := hmac.New(sha512.New, key)
	_, _ = mac.Write(message)
	var out [SHA512Size]byte
	copy(out[:], mac.Sum(nil))
	return out
}

// PBKDF2HMACSHA256 computes PBKDF2-HMAC-SHA-256.
func PBKDF2HMACSHA256(pass, salt []byte, iterations uint32, keyLen int) []byte {
	return pbkdf2.Key(pass, salt, int(iterations), keyLen, sha256.New)
}

// PBKDF2HMACSHA512 computes PBKDF2-HMAC-SHA-512.
func PBKDF2HMACSHA512(pass, salt []byte, iterations uint32, keyLen int) []byte {
	return pbkdf2.Key(pass, salt, int(iterations), keyLen, sha512.New)
}

func hkdfDerive(h func() hash.Hash, keyMaterial, salt []byte, keyLen int) []byte {
	reader := hkdf.New(h, keyMaterial, salt, nil)
	key := make([]byte, keyLen)
	if _, err := io.ReadFull(reader, key); err != nil {
		panic("bccrypto: hkdf derivation failed")
	}
	return key
}

// HKDFHMACSHA256 computes HKDF-HMAC-SHA-256.
func HKDFHMACSHA256(keyMaterial, salt []byte, keyLen int) []byte {
	return hkdfDerive(sha256.New, keyMaterial, salt, keyLen)
}

// HKDFHMACSHA512 computes HKDF-HMAC-SHA-512.
func HKDFHMACSHA512(keyMaterial, salt []byte, keyLen int) []byte {
	return hkdfDerive(sha512.New, keyMaterial, salt, keyLen)
}
