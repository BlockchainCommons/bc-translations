package com.blockchaincommons.bcshamir

internal fun bitslice(r: UIntArray, x: ByteArray) {
    require(r.size >= 8)
    require(x.size >= 32)
    r.fill(0u)
    for (arrIdx in 0 until 32) {
        val cur = x[arrIdx].toUByte().toUInt()
        for (bitIdx in 0 until 8) {
            r[bitIdx] = r[bitIdx] or (((cur and (1u shl bitIdx)) shr bitIdx) shl arrIdx)
        }
    }
}

internal fun unbitslice(r: ByteArray, x: UIntArray) {
    require(r.size >= 32)
    require(x.size >= 8)
    r.fill(0)
    for (bitIdx in 0 until 8) {
        val cur = x[bitIdx]
        for (arrIdx in 0 until 32) {
            r[arrIdx] = (
                r[arrIdx].toUByte().toUInt() or
                    (((cur and (1u shl arrIdx)) shr arrIdx) shl bitIdx)
                ).toByte()
        }
    }
}

internal fun bitsliceSetall(r: UIntArray, x: UByte) {
    for (idx in 0 until 8) {
        r[idx] = if (((x.toUInt() shr idx) and 1u) == 1u) UInt.MAX_VALUE else 0u
    }
}

internal fun gf256Add(r: UIntArray, x: UIntArray) {
    for (idx in 0 until 8) {
        r[idx] = r[idx] xor x[idx]
    }
}

internal fun gf256Mul(r: UIntArray, a: UIntArray, b: UIntArray) {
    val a2 = a.copyOf()

    r[0] = a2[0] and b[0]
    r[1] = a2[1] and b[0]
    r[2] = a2[2] and b[0]
    r[3] = a2[3] and b[0]
    r[4] = a2[4] and b[0]
    r[5] = a2[5] and b[0]
    r[6] = a2[6] and b[0]
    r[7] = a2[7] and b[0]
    a2[0] = a2[0] xor a2[7]
    a2[2] = a2[2] xor a2[7]
    a2[3] = a2[3] xor a2[7]

    r[0] = r[0] xor (a2[7] and b[1])
    r[1] = r[1] xor (a2[0] and b[1])
    r[2] = r[2] xor (a2[1] and b[1])
    r[3] = r[3] xor (a2[2] and b[1])
    r[4] = r[4] xor (a2[3] and b[1])
    r[5] = r[5] xor (a2[4] and b[1])
    r[6] = r[6] xor (a2[5] and b[1])
    r[7] = r[7] xor (a2[6] and b[1])
    a2[7] = a2[7] xor a2[6]
    a2[1] = a2[1] xor a2[6]
    a2[2] = a2[2] xor a2[6]

    r[0] = r[0] xor (a2[6] and b[2])
    r[1] = r[1] xor (a2[7] and b[2])
    r[2] = r[2] xor (a2[0] and b[2])
    r[3] = r[3] xor (a2[1] and b[2])
    r[4] = r[4] xor (a2[2] and b[2])
    r[5] = r[5] xor (a2[3] and b[2])
    r[6] = r[6] xor (a2[4] and b[2])
    r[7] = r[7] xor (a2[5] and b[2])
    a2[6] = a2[6] xor a2[5]
    a2[0] = a2[0] xor a2[5]
    a2[1] = a2[1] xor a2[5]

    r[0] = r[0] xor (a2[5] and b[3])
    r[1] = r[1] xor (a2[6] and b[3])
    r[2] = r[2] xor (a2[7] and b[3])
    r[3] = r[3] xor (a2[0] and b[3])
    r[4] = r[4] xor (a2[1] and b[3])
    r[5] = r[5] xor (a2[2] and b[3])
    r[6] = r[6] xor (a2[3] and b[3])
    r[7] = r[7] xor (a2[4] and b[3])
    a2[5] = a2[5] xor a2[4]
    a2[7] = a2[7] xor a2[4]
    a2[0] = a2[0] xor a2[4]

    r[0] = r[0] xor (a2[4] and b[4])
    r[1] = r[1] xor (a2[5] and b[4])
    r[2] = r[2] xor (a2[6] and b[4])
    r[3] = r[3] xor (a2[7] and b[4])
    r[4] = r[4] xor (a2[0] and b[4])
    r[5] = r[5] xor (a2[1] and b[4])
    r[6] = r[6] xor (a2[2] and b[4])
    r[7] = r[7] xor (a2[3] and b[4])
    a2[4] = a2[4] xor a2[3]
    a2[6] = a2[6] xor a2[3]
    a2[7] = a2[7] xor a2[3]

    r[0] = r[0] xor (a2[3] and b[5])
    r[1] = r[1] xor (a2[4] and b[5])
    r[2] = r[2] xor (a2[5] and b[5])
    r[3] = r[3] xor (a2[6] and b[5])
    r[4] = r[4] xor (a2[7] and b[5])
    r[5] = r[5] xor (a2[0] and b[5])
    r[6] = r[6] xor (a2[1] and b[5])
    r[7] = r[7] xor (a2[2] and b[5])
    a2[3] = a2[3] xor a2[2]
    a2[5] = a2[5] xor a2[2]
    a2[6] = a2[6] xor a2[2]

    r[0] = r[0] xor (a2[2] and b[6])
    r[1] = r[1] xor (a2[3] and b[6])
    r[2] = r[2] xor (a2[4] and b[6])
    r[3] = r[3] xor (a2[5] and b[6])
    r[4] = r[4] xor (a2[6] and b[6])
    r[5] = r[5] xor (a2[7] and b[6])
    r[6] = r[6] xor (a2[0] and b[6])
    r[7] = r[7] xor (a2[1] and b[6])
    a2[2] = a2[2] xor a2[1]
    a2[4] = a2[4] xor a2[1]
    a2[5] = a2[5] xor a2[1]

    r[0] = r[0] xor (a2[1] and b[7])
    r[1] = r[1] xor (a2[2] and b[7])
    r[2] = r[2] xor (a2[3] and b[7])
    r[3] = r[3] xor (a2[4] and b[7])
    r[4] = r[4] xor (a2[5] and b[7])
    r[5] = r[5] xor (a2[6] and b[7])
    r[6] = r[6] xor (a2[7] and b[7])
    r[7] = r[7] xor (a2[0] and b[7])
}

internal fun gf256Square(r: UIntArray, x: UIntArray) {
    val inX = x.copyOf()

    val r14 = inX[7]
    val r12 = inX[6]
    var r10 = inX[5]
    var r8 = inX[4]
    r[6] = inX[3]
    r[4] = inX[2]
    r[2] = inX[1]
    r[0] = inX[0]

    r[7] = r14
    r[6] = r[6] xor r14
    r10 = r10 xor r14

    r[4] = r[4] xor r12
    r[5] = r12
    r[7] = r[7] xor r12
    r8 = r8 xor r12

    r[2] = r[2] xor r10
    r[3] = r10
    r[5] = r[5] xor r10
    r[6] = r[6] xor r10

    r[1] = r14
    r[2] = r[2] xor r14
    r[4] = r[4] xor r14
    r[5] = r[5] xor r14

    r[0] = r[0] xor r8
    r[1] = r[1] xor r8
    r[3] = r[3] xor r8
    r[4] = r[4] xor r8
}

internal fun gf256Inv(r: UIntArray, x: UIntArray) {
    val y = UIntArray(8)
    val z = UIntArray(8)

    gf256Square(y, x)
    gf256Square(y, y.copyOf())
    gf256Square(r, y)
    gf256Mul(z, r, x)
    gf256Square(r, r.copyOf())
    gf256Mul(r, r.copyOf(), z)
    gf256Square(r, r.copyOf())
    gf256Square(z, r)
    gf256Square(z, z.copyOf())
    gf256Mul(r, r.copyOf(), z)
    gf256Mul(r, r.copyOf(), y)
}

internal fun memzeroWords(data: UIntArray) {
    data.fill(0u)
}
