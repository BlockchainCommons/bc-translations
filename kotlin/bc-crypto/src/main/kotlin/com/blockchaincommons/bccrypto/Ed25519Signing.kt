package com.blockchaincommons.bccrypto

import com.blockchaincommons.bcrand.RandomNumberGenerator
import org.bouncycastle.crypto.params.Ed25519PrivateKeyParameters
import org.bouncycastle.crypto.params.Ed25519PublicKeyParameters
import org.bouncycastle.crypto.signers.Ed25519Signer

const val ED25519_PRIVATE_KEY_SIZE = 32
const val ED25519_PUBLIC_KEY_SIZE = 32
const val ED25519_SIGNATURE_SIZE = 64

fun ed25519NewPrivateKeyUsing(rng: RandomNumberGenerator): ByteArray =
    rng.randomData(ED25519_PRIVATE_KEY_SIZE)

fun ed25519PublicKeyFromPrivateKey(privateKey: ByteArray): ByteArray {
    val privParams = Ed25519PrivateKeyParameters(privateKey, 0)
    return privParams.generatePublicKey().encoded
}

fun ed25519Sign(privateKey: ByteArray, message: ByteArray): ByteArray {
    val privParams = Ed25519PrivateKeyParameters(privateKey, 0)
    val signer = Ed25519Signer()
    signer.init(true, privParams)
    signer.update(message, 0, message.size)
    return signer.generateSignature()
}

fun ed25519Verify(publicKey: ByteArray, message: ByteArray, signature: ByteArray): Boolean {
    val pubParams = Ed25519PublicKeyParameters(publicKey, 0)
    val verifier = Ed25519Signer()
    verifier.init(false, pubParams)
    verifier.update(message, 0, message.size)
    return verifier.verifySignature(signature)
}
