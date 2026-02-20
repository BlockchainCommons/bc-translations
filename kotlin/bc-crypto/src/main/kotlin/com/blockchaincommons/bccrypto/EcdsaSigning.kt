package com.blockchaincommons.bccrypto

import fr.acinq.secp256k1.Secp256k1

fun ecdsaSign(privateKey: ByteArray, message: ByteArray): ByteArray {
    val hash = doubleSha256(message)
    return Secp256k1.sign(hash, privateKey)
}

fun ecdsaVerify(publicKey: ByteArray, signature: ByteArray, message: ByteArray): Boolean {
    val hash = doubleSha256(message)
    val pub = Secp256k1.pubkeyParse(publicKey)
    return Secp256k1.verify(signature, hash, pub)
}
