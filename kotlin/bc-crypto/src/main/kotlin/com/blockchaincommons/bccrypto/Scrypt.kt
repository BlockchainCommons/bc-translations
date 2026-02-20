package com.blockchaincommons.bccrypto

import org.bouncycastle.crypto.generators.SCrypt

fun scrypt(
    pass: ByteArray,
    salt: ByteArray,
    outputLen: Int,
    logN: Int = 15,
    r: Int = 8,
    p: Int = 1,
): ByteArray {
    val n = 1 shl logN
    return SCrypt.generate(pass, salt, n, r, p, outputLen)
}
