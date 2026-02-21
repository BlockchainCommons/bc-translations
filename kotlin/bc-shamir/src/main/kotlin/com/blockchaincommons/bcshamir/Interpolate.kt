package com.blockchaincommons.bcshamir

import com.blockchaincommons.bccrypto.memzero
import com.blockchaincommons.bccrypto.memzeroAll

private fun hazmatLagrangeBasis(values: ByteArray, n: Int, xc: ByteArray, x: UByte) {
    val xx = ByteArray(32 + 16)
    val xSlice = UIntArray(8)
    val lxi = MutableList(n) { UIntArray(8) }
    val numerator = UIntArray(8)
    val denominator = UIntArray(8)
    val temp = UIntArray(8)

    xc.copyInto(xx, endIndex = n)

    for (i in 0 until n) {
        bitslice(lxi[i], xx.copyOfRange(i, i + 32))
        xx[i + n] = xx[i]
    }

    bitsliceSetall(xSlice, x)
    bitsliceSetall(numerator, 1u.toUByte())
    bitsliceSetall(denominator, 1u.toUByte())

    for (i in 1 until n) {
        xSlice.copyInto(temp)
        gf256Add(temp, lxi[i])
        gf256Mul(numerator, numerator.copyOf(), temp)

        lxi[0].copyInto(temp)
        gf256Add(temp, lxi[i])
        gf256Mul(denominator, denominator.copyOf(), temp)
    }

    gf256Inv(temp, denominator)
    gf256Mul(numerator, numerator.copyOf(), temp)

    unbitslice(xx, numerator)
    xx.copyInto(values, endIndex = n)
}

/**
 * Evaluates the Lagrange interpolation polynomial at the given x-coordinate
 * over GF(256) using bitsliced arithmetic.
 *
 * @param n Number of data points (shares).
 * @param xi x-coordinates of the data points.
 * @param yl Length of each y-value (the secret/share length in bytes).
 * @param yij y-values (share data) corresponding to each x-coordinate.
 * @param x The x-coordinate at which to evaluate the polynomial.
 * @return The interpolated y-value at [x].
 */
internal fun interpolate(n: Int, xi: ByteArray, yl: Int, yij: List<ByteArray>, x: UByte): ByteArray {
    val y = MutableList(n) { ByteArray(MAX_SECRET_LEN) }
    val values = ByteArray(MAX_SECRET_LEN)

    for (i in 0 until n) {
        yij[i].copyInto(y[i], endIndex = yl)
    }

    val lagrange = ByteArray(n)
    val ySlice = UIntArray(8)
    val resultSlice = UIntArray(8)
    val temp = UIntArray(8)

    hazmatLagrangeBasis(lagrange, n, xi, x)
    bitsliceSetall(resultSlice, 0u.toUByte())

    for (i in 0 until n) {
        bitslice(ySlice, y[i])
        bitsliceSetall(temp, lagrange[i].toUByte())
        gf256Mul(temp, temp.copyOf(), ySlice)
        gf256Add(resultSlice, temp)
    }

    unbitslice(values, resultSlice)
    val result = values.copyOfRange(0, yl)

    memzero(lagrange)
    memzeroWords(ySlice)
    memzeroWords(resultSlice)
    memzeroWords(temp)
    memzeroAll(y)
    memzero(values)

    return result
}
