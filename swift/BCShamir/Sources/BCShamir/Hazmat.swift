import BCCrypto

func bitslice(_ r: inout [UInt32], _ x: [UInt8]) {
    precondition(x.count >= 32)
    memzero(&r)
    for arrIdx in 0..<32 {
        let cur = UInt32(x[arrIdx])
        for bitIdx in 0..<8 {
            r[bitIdx] |= ((cur & (1 &<< UInt32(bitIdx))) &>> UInt32(bitIdx)) &<< UInt32(arrIdx)
        }
    }
}

func unbitslice(_ r: inout [UInt8], _ x: [UInt32]) {
    precondition(r.count >= 32)
    memzero(&r)
    for bitIdx in 0..<8 {
        let cur = x[bitIdx]
        for arrIdx in 0..<32 {
            r[arrIdx] |= UInt8(((cur & (1 &<< UInt32(arrIdx))) &>> UInt32(arrIdx)) &<< UInt32(bitIdx))
        }
    }
}

func bitsliceSetall(_ r: inout [UInt32], _ x: UInt8) {
    for idx in 0..<8 {
        r[idx] = UInt32(bitPattern: Int32(bitPattern: ((UInt32(x) & (1 &<< UInt32(idx))) &<< (31 &- UInt32(idx)))) &>> 31)
    }
}

/// Add (XOR) `r` with `x` and store the result in `r`.
func gf256Add(_ r: inout [UInt32], _ x: [UInt32]) {
    for i in 0..<8 {
        r[i] ^= x[i]
    }
}

/// Safely multiply two bitsliced polynomials in GF(2^8) reduced by
/// x^8 + x^4 + x^3 + x + 1.
func gf256Mul(_ r: inout [UInt32], _ a: [UInt32], _ b: [UInt32]) {
    var a2 = a

    r[0] = a2[0] & b[0]
    r[1] = a2[1] & b[0]
    r[2] = a2[2] & b[0]
    r[3] = a2[3] & b[0]
    r[4] = a2[4] & b[0]
    r[5] = a2[5] & b[0]
    r[6] = a2[6] & b[0]
    r[7] = a2[7] & b[0]
    a2[0] ^= a2[7]
    a2[2] ^= a2[7]
    a2[3] ^= a2[7]

    r[0] ^= a2[7] & b[1]
    r[1] ^= a2[0] & b[1]
    r[2] ^= a2[1] & b[1]
    r[3] ^= a2[2] & b[1]
    r[4] ^= a2[3] & b[1]
    r[5] ^= a2[4] & b[1]
    r[6] ^= a2[5] & b[1]
    r[7] ^= a2[6] & b[1]
    a2[7] ^= a2[6]
    a2[1] ^= a2[6]
    a2[2] ^= a2[6]

    r[0] ^= a2[6] & b[2]
    r[1] ^= a2[7] & b[2]
    r[2] ^= a2[0] & b[2]
    r[3] ^= a2[1] & b[2]
    r[4] ^= a2[2] & b[2]
    r[5] ^= a2[3] & b[2]
    r[6] ^= a2[4] & b[2]
    r[7] ^= a2[5] & b[2]
    a2[6] ^= a2[5]
    a2[0] ^= a2[5]
    a2[1] ^= a2[5]

    r[0] ^= a2[5] & b[3]
    r[1] ^= a2[6] & b[3]
    r[2] ^= a2[7] & b[3]
    r[3] ^= a2[0] & b[3]
    r[4] ^= a2[1] & b[3]
    r[5] ^= a2[2] & b[3]
    r[6] ^= a2[3] & b[3]
    r[7] ^= a2[4] & b[3]
    a2[5] ^= a2[4]
    a2[7] ^= a2[4]
    a2[0] ^= a2[4]

    r[0] ^= a2[4] & b[4]
    r[1] ^= a2[5] & b[4]
    r[2] ^= a2[6] & b[4]
    r[3] ^= a2[7] & b[4]
    r[4] ^= a2[0] & b[4]
    r[5] ^= a2[1] & b[4]
    r[6] ^= a2[2] & b[4]
    r[7] ^= a2[3] & b[4]
    a2[4] ^= a2[3]
    a2[6] ^= a2[3]
    a2[7] ^= a2[3]

    r[0] ^= a2[3] & b[5]
    r[1] ^= a2[4] & b[5]
    r[2] ^= a2[5] & b[5]
    r[3] ^= a2[6] & b[5]
    r[4] ^= a2[7] & b[5]
    r[5] ^= a2[0] & b[5]
    r[6] ^= a2[1] & b[5]
    r[7] ^= a2[2] & b[5]
    a2[3] ^= a2[2]
    a2[5] ^= a2[2]
    a2[6] ^= a2[2]

    r[0] ^= a2[2] & b[6]
    r[1] ^= a2[3] & b[6]
    r[2] ^= a2[4] & b[6]
    r[3] ^= a2[5] & b[6]
    r[4] ^= a2[6] & b[6]
    r[5] ^= a2[7] & b[6]
    r[6] ^= a2[0] & b[6]
    r[7] ^= a2[1] & b[6]
    a2[2] ^= a2[1]
    a2[4] ^= a2[1]
    a2[5] ^= a2[1]

    r[0] ^= a2[1] & b[7]
    r[1] ^= a2[2] & b[7]
    r[2] ^= a2[3] & b[7]
    r[3] ^= a2[4] & b[7]
    r[4] ^= a2[5] & b[7]
    r[5] ^= a2[6] & b[7]
    r[6] ^= a2[7] & b[7]
    r[7] ^= a2[0] & b[7]
}

/// Square `x` in GF(2^8) and write the result to `r`. `r` and `x` may overlap.
func gf256Square(_ r: inout [UInt32], _ x: [UInt32]) {
    var r8: UInt32
    var r10: UInt32

    let r14 = x[7]
    let r12 = x[6]
    r10 = x[5]
    r8 = x[4]
    r[6] = x[3]
    r[4] = x[2]
    r[2] = x[1]
    r[0] = x[0]

    r[7] = r14
    r[6] ^= r14
    r10 ^= r14

    r[4] ^= r12
    r[5] = r12
    r[7] ^= r12
    r8 ^= r12

    r[2] ^= r10
    r[3] = r10
    r[5] ^= r10
    r[6] ^= r10
    r[1] = r14
    r[2] ^= r14
    r[4] ^= r14
    r[5] ^= r14
    r[0] ^= r8
    r[1] ^= r8
    r[3] ^= r8
    r[4] ^= r8
}

/// Invert `x` in GF(2^8) and write the result to `r`.
func gf256Inv(_ r: inout [UInt32], _ x: inout [UInt32]) {
    var y = [UInt32](repeating: 0, count: 8)
    var z = [UInt32](repeating: 0, count: 8)

    gf256Square(&y, x)          // y = x^2
    let y2 = y
    gf256Square(&y, y2)         // y = x^4
    gf256Square(&r, y)          // r = x^8
    gf256Mul(&z, r, x)          // z = x^9
    var r2 = r
    gf256Square(&r, r2)         // r = x^16
    r2 = r
    gf256Mul(&r, r2, z)         // r = x^25
    r2 = r
    gf256Square(&r, r2)         // r = x^50
    gf256Square(&z, r)          // z = x^100
    let z2 = z
    gf256Square(&z, z2)         // z = x^200
    r2 = r
    gf256Mul(&r, r2, z)         // r = x^250
    r2 = r
    gf256Mul(&r, r2, y)         // r = x^254
}
