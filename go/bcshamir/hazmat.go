package bcshamir

import (
	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
)

func bitslice(r *[8]uint32, x []byte) {
	bccrypto.Memzero(r[:])
	for arrIdx := 0; arrIdx < 32; arrIdx++ {
		cur := uint32(x[arrIdx])
		for bitIdx := 0; bitIdx < 8; bitIdx++ {
			r[bitIdx] |= ((cur & (1 << (uint(bitIdx) & 31))) >> (uint(bitIdx) & 31)) << (uint(arrIdx) & 31)
		}
	}
}

func unbitslice(r []byte, x *[8]uint32) {
	bccrypto.Memzero(r[:32])
	for bitIdx := 0; bitIdx < 8; bitIdx++ {
		cur := x[bitIdx]
		for arrIdx := 0; arrIdx < 32; arrIdx++ {
			r[arrIdx] |= byte(((cur & (1 << (uint(arrIdx) & 31))) >> (uint(arrIdx) & 31)) << (uint(bitIdx) & 31))
		}
	}
}

func bitsliceSetall(r *[8]uint32, x byte) {
	for idx := 0; idx < 8; idx++ {
		r[idx] = uint32(int32((uint32(x)&(1<<(uint(idx)&31)))<<(uint(31-idx)&31)) >> 31)
	}
}

// gf256Add adds (XOR) r with x and stores the result in r.
func gf256Add(r *[8]uint32, x *[8]uint32) {
	for i := 0; i < 8; i++ {
		r[i] ^= x[i]
	}
}

// gf256Mul safely multiplies two bitsliced polynomials in GF(2^8) reduced by
// x^8 + x^4 + x^3 + x + 1. r and a may overlap, but overlapping of r
// and b will produce an incorrect result. If you need to square a polynomial
// use gf256Square instead.
func gf256Mul(r *[8]uint32, a *[8]uint32, b *[8]uint32) {
	// This function implements Russian Peasant multiplication on two
	// bitsliced polynomials.
	var a2 [8]uint32
	a2 = *a

	r[0] = a2[0] & b[0]
	r[1] = a2[1] & b[0]
	r[2] = a2[2] & b[0]
	r[3] = a2[3] & b[0]
	r[4] = a2[4] & b[0]
	r[5] = a2[5] & b[0]
	r[6] = a2[6] & b[0]
	r[7] = a2[7] & b[0]
	a2[0] ^= a2[7] // reduce
	a2[2] ^= a2[7]
	a2[3] ^= a2[7]

	r[0] ^= a2[7] & b[1] // add
	r[1] ^= a2[0] & b[1]
	r[2] ^= a2[1] & b[1]
	r[3] ^= a2[2] & b[1]
	r[4] ^= a2[3] & b[1]
	r[5] ^= a2[4] & b[1]
	r[6] ^= a2[5] & b[1]
	r[7] ^= a2[6] & b[1]
	a2[7] ^= a2[6] // reduce
	a2[1] ^= a2[6]
	a2[2] ^= a2[6]

	r[0] ^= a2[6] & b[2] // add
	r[1] ^= a2[7] & b[2]
	r[2] ^= a2[0] & b[2]
	r[3] ^= a2[1] & b[2]
	r[4] ^= a2[2] & b[2]
	r[5] ^= a2[3] & b[2]
	r[6] ^= a2[4] & b[2]
	r[7] ^= a2[5] & b[2]
	a2[6] ^= a2[5] // reduce
	a2[0] ^= a2[5]
	a2[1] ^= a2[5]

	r[0] ^= a2[5] & b[3] // add
	r[1] ^= a2[6] & b[3]
	r[2] ^= a2[7] & b[3]
	r[3] ^= a2[0] & b[3]
	r[4] ^= a2[1] & b[3]
	r[5] ^= a2[2] & b[3]
	r[6] ^= a2[3] & b[3]
	r[7] ^= a2[4] & b[3]
	a2[5] ^= a2[4] // reduce
	a2[7] ^= a2[4]
	a2[0] ^= a2[4]

	r[0] ^= a2[4] & b[4] // add
	r[1] ^= a2[5] & b[4]
	r[2] ^= a2[6] & b[4]
	r[3] ^= a2[7] & b[4]
	r[4] ^= a2[0] & b[4]
	r[5] ^= a2[1] & b[4]
	r[6] ^= a2[2] & b[4]
	r[7] ^= a2[3] & b[4]
	a2[4] ^= a2[3] // reduce
	a2[6] ^= a2[3]
	a2[7] ^= a2[3]

	r[0] ^= a2[3] & b[5] // add
	r[1] ^= a2[4] & b[5]
	r[2] ^= a2[5] & b[5]
	r[3] ^= a2[6] & b[5]
	r[4] ^= a2[7] & b[5]
	r[5] ^= a2[0] & b[5]
	r[6] ^= a2[1] & b[5]
	r[7] ^= a2[2] & b[5]
	a2[3] ^= a2[2] // reduce
	a2[5] ^= a2[2]
	a2[6] ^= a2[2]

	r[0] ^= a2[2] & b[6] // add
	r[1] ^= a2[3] & b[6]
	r[2] ^= a2[4] & b[6]
	r[3] ^= a2[5] & b[6]
	r[4] ^= a2[6] & b[6]
	r[5] ^= a2[7] & b[6]
	r[6] ^= a2[0] & b[6]
	r[7] ^= a2[1] & b[6]
	a2[2] ^= a2[1] // reduce
	a2[4] ^= a2[1]
	a2[5] ^= a2[1]

	r[0] ^= a2[1] & b[7] // add
	r[1] ^= a2[2] & b[7]
	r[2] ^= a2[3] & b[7]
	r[3] ^= a2[4] & b[7]
	r[4] ^= a2[5] & b[7]
	r[5] ^= a2[6] & b[7]
	r[6] ^= a2[7] & b[7]
	r[7] ^= a2[0] & b[7]
}

// gf256Square squares x in GF(2^8) and writes the result to r. r and x may overlap.
func gf256Square(r *[8]uint32, x *[8]uint32) {
	var r8, r10 uint32
	// Use the Freshman's Dream rule to square the polynomial.
	// Assignments are done from 7 downto 0, because this allows the user
	// to execute this function in-place (e.g. gf256Square(r, r)).
	r14 := x[7]
	r12 := x[6]
	r10 = x[5]
	r8 = x[4]
	r[6] = x[3]
	r[4] = x[2]
	r[2] = x[1]
	r[0] = x[0]

	// Reduce with x^8 + x^4 + x^3 + x + 1 until order is less than 8
	r[7] = r14 // r[7] was 0
	r[6] ^= r14
	r10 ^= r14
	// Skip, because r13 is always 0
	r[4] ^= r12
	r[5] = r12 // r[5] was 0
	r[7] ^= r12
	r8 ^= r12
	// Skip, because r11 is always 0
	r[2] ^= r10
	r[3] = r10 // r[3] was 0
	r[5] ^= r10
	r[6] ^= r10
	r[1] = r14  // r[1] was 0
	r[2] ^= r14 // Substitute r9 by r14 because they will always be equal
	r[4] ^= r14
	r[5] ^= r14
	r[0] ^= r8
	r[1] ^= r8
	r[3] ^= r8
	r[4] ^= r8
}

// gf256Inv inverts x in GF(2^8) and writes the result to r.
func gf256Inv(r *[8]uint32, x *[8]uint32) {
	var y, z [8]uint32

	gf256Square(&y, x) // y = x^2
	y2 := y
	gf256Square(&y, &y2) // y = x^4
	gf256Square(r, &y)   // r = x^8
	gf256Mul(&z, r, x)   // z = x^9
	r2 := *r
	gf256Square(r, &r2) // r = x^16
	r2 = *r
	gf256Mul(r, &r2, &z) // r = x^25
	r2 = *r
	gf256Square(r, &r2) // r = x^50
	gf256Square(&z, r)  // z = x^100
	z2 := z
	gf256Square(&z, &z2) // z = x^200
	r2 = *r
	gf256Mul(r, &r2, &z) // r = x^250
	r2 = *r
	gf256Mul(r, &r2, &y) // r = x^254
}
