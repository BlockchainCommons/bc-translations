package com.blockchaincommons.bccrypto

import org.bouncycastle.crypto.generators.Argon2BytesGenerator
import org.bouncycastle.crypto.params.Argon2Parameters

fun argon2id(pass: ByteArray, salt: ByteArray, outputLen: Int): ByteArray {
    val params = Argon2Parameters.Builder(Argon2Parameters.ARGON2_id)
        .withMemoryAsKB(19456)
        .withIterations(2)
        .withParallelism(1)
        .withVersion(Argon2Parameters.ARGON2_VERSION_13)
        .withSalt(salt)
        .build()
    val generator = Argon2BytesGenerator()
    generator.init(params)
    val output = ByteArray(outputLen)
    generator.generateBytes(pass, output)
    return output
}
