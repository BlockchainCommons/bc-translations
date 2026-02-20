package bccrypto

import (
	"encoding/hex"
	"strings"
)

func mustHex(s string) []byte {
	decoded, err := hex.DecodeString(strings.ReplaceAll(s, " ", ""))
	if err != nil {
		panic(err)
	}
	return decoded
}

func mustLen(s string, n int) []byte {
	decoded := mustHex(s)
	if len(decoded) != n {
		panic("invalid hex length")
	}
	return decoded
}

func must12(s string) [12]byte {
	bytes := mustLen(s, 12)
	var out [12]byte
	copy(out[:], bytes)
	return out
}

func must4(s string) [4]byte {
	bytes := mustLen(s, 4)
	var out [4]byte
	copy(out[:], bytes)
	return out
}

func must16(s string) [16]byte {
	bytes := mustLen(s, 16)
	var out [16]byte
	copy(out[:], bytes)
	return out
}

func must32(s string) [32]byte {
	bytes := mustLen(s, 32)
	var out [32]byte
	copy(out[:], bytes)
	return out
}

func must33(s string) [33]byte {
	bytes := mustLen(s, 33)
	var out [33]byte
	copy(out[:], bytes)
	return out
}

func must64(s string) [64]byte {
	bytes := mustLen(s, 64)
	var out [64]byte
	copy(out[:], bytes)
	return out
}

func must65(s string) [65]byte {
	bytes := mustLen(s, 65)
	var out [65]byte
	copy(out[:], bytes)
	return out
}
