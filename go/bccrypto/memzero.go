package bccrypto

import "runtime"

// Memzero zeros all elements in-place.
func Memzero[T any](s []T) {
	var zero T
	for i := range s {
		s[i] = zero
	}
	runtime.KeepAlive(s)
}

// MemzeroByteSlices zeros each inner byte slice.
func MemzeroByteSlices(s [][]byte) {
	for i := range s {
		Memzero(s[i])
	}
	runtime.KeepAlive(s)
}
