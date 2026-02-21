import BCCrypto

/// Calculate the Lagrange basis coefficients for the Lagrange polynomial
/// defined by the x coordinates `xc` at the value `x`.
private func hazmatLagrangeBasis(_ values: inout [UInt8], n: Int, xc: [UInt8], x: UInt8) {
    var xx = [UInt8](repeating: 0, count: 32 + 16)
    var xSlice = [UInt32](repeating: 0, count: 8)
    var lxi = [[UInt32]](repeating: [UInt32](repeating: 0, count: 8), count: n)
    var numerator = [UInt32](repeating: 0, count: 8)
    var denominator = [UInt32](repeating: 0, count: 8)
    var temp = [UInt32](repeating: 0, count: 8)
    xx.replaceSubrange(0..<n, with: xc[0..<n])

    for i in 0..<n {
        bitslice(&lxi[i], Array(xx[i...]))
        xx[i + n] = xx[i]
    }

    bitsliceSetall(&xSlice, x)
    bitsliceSetall(&numerator, 1)
    bitsliceSetall(&denominator, 1)

    for i in 1..<n {
        temp = xSlice
        gf256Add(&temp, lxi[i])
        let numerator2 = numerator
        gf256Mul(&numerator, numerator2, temp)

        temp = lxi[0]
        gf256Add(&temp, lxi[i])
        let denominator2 = denominator
        gf256Mul(&denominator, denominator2, temp)
    }

    gf256Inv(&temp, &denominator)

    let numerator2 = numerator
    gf256Mul(&numerator, numerator2, temp)

    unbitslice(&xx, numerator)

    values.replaceSubrange(0..<n, with: xx[0..<n])
}

/// Interpolates the polynomial going through the given points.
func interpolate(
    n: Int,
    xi: [UInt8],
    yl: Int,
    yij: [[UInt8]],
    x: UInt8
) throws(ShamirError) -> [UInt8] {
    var y = [[UInt8]](repeating: [UInt8](repeating: 0, count: maxSecretLen), count: n)
    var values = [UInt8](repeating: 0, count: maxSecretLen)

    for i in 0..<n {
        y[i].replaceSubrange(0..<yl, with: yij[i][0..<yl])
    }

    var lagrange = [UInt8](repeating: 0, count: n)
    var ySlice = [UInt32](repeating: 0, count: 8)
    var resultSlice = [UInt32](repeating: 0, count: 8)
    var temp = [UInt32](repeating: 0, count: 8)

    hazmatLagrangeBasis(&lagrange, n: n, xc: xi, x: x)

    bitsliceSetall(&resultSlice, 0)

    for i in 0..<n {
        bitslice(&ySlice, y[i])
        bitsliceSetall(&temp, lagrange[i])
        let temp2 = temp
        gf256Mul(&temp, temp2, ySlice)
        gf256Add(&resultSlice, temp)
    }

    unbitslice(&values, resultSlice)
    var result = [UInt8](repeating: 0, count: yl)
    result.replaceSubrange(0..<yl, with: values[0..<yl])

    // Zero sensitive temporaries.
    memzero(&lagrange)
    memzero(&ySlice)
    memzero(&resultSlice)
    memzero(&temp)
    memzero(&y)
    memzero(&values)

    return result
}
