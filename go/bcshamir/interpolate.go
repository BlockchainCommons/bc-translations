package bcshamir

import (
	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
)

// hazmatLagrangeBasis calculates the Lagrange basis coefficients for the
// Lagrange polynomial defined by the x coordinates xc at the value x.
//
// After the function runs, the values slice holds data satisfying:
//
//	              ---     (x-xc[j])
//	values[i] =   | |   -------------
//	            j != i  (xc[i]-xc[j])
func hazmatLagrangeBasis(values []byte, n int, xc []byte, x byte) {
	xx := make([]byte, 32+16)
	var xSlice [8]uint32
	lxi := make([][8]uint32, n)
	var numerator, denominator, temp [8]uint32
	copy(xx[:n], xc[:n])

	// xx now contains bitsliced [ x0 x1 x2 ... xn-1 0 0 0 ... ]
	for i := 0; i < n; i++ {
		bitslice(&lxi[i], xx[i:])
		xx[i+n] = xx[i]
	}

	bitsliceSetall(&xSlice, x)
	bitsliceSetall(&numerator, 1)
	bitsliceSetall(&denominator, 1)

	for i := 1; i < n; i++ {
		temp = xSlice
		gf256Add(&temp, &lxi[i])
		numerator2 := numerator
		gf256Mul(&numerator, &numerator2, &temp)

		temp = lxi[0]
		gf256Add(&temp, &lxi[i])
		denominator2 := denominator
		gf256Mul(&denominator, &denominator2, &temp)
	}

	gf256Inv(&temp, &denominator)

	numerator2 := numerator
	gf256Mul(&numerator, &numerator2, &temp)

	unbitslice(xx, &numerator)

	copy(values[:n], xx[:n])
}

// interpolate safely interpolates the polynomial going through the given
// points at coordinate x.
//
// Parameters:
//   - n: number of points to interpolate
//   - xi: x coordinates for points (slice of length n)
//   - yl: length of y coordinate arrays
//   - yij: slice of n byte slices, each of length yl
//   - x: coordinate to interpolate at
func interpolate(n int, xi []byte, yl int, yij [][]byte, x byte) ([]byte, error) {
	// The hazmat gf256 implementation needs the y-coordinate data
	// to be in 32-byte blocks
	y := make([][]byte, n)
	for i := range y {
		y[i] = make([]byte, MaxSecretLen)
	}
	values := make([]byte, MaxSecretLen)

	for i := 0; i < n; i++ {
		copy(y[i][:yl], yij[i])
	}

	lagrange := make([]byte, n)
	var ySlice, resultSlice, temp [8]uint32

	hazmatLagrangeBasis(lagrange, n, xi, x)

	bitsliceSetall(&resultSlice, 0)

	for i := 0; i < n; i++ {
		bitslice(&ySlice, y[i])
		bitsliceSetall(&temp, lagrange[i])
		temp2 := temp
		gf256Mul(&temp, &temp2, &ySlice)
		gf256Add(&resultSlice, &temp)
	}

	unbitslice(values, &resultSlice)
	result := make([]byte, yl)
	copy(result[:yl], values[:yl])

	// clean up
	bccrypto.Memzero(lagrange)
	bccrypto.Memzero(ySlice[:])
	bccrypto.Memzero(resultSlice[:])
	bccrypto.Memzero(temp[:])
	bccrypto.MemzeroByteSlices(y)
	bccrypto.Memzero(values)

	return result, nil
}
