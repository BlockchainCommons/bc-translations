package com.blockchaincommons.bccrypto

import com.blockchaincommons.bcrand.RandomNumberGenerator
import org.bouncycastle.crypto.agreement.X25519Agreement
import org.bouncycastle.crypto.params.X25519PrivateKeyParameters
import org.bouncycastle.crypto.params.X25519PublicKeyParameters

const val GENERIC_PRIVATE_KEY_SIZE = 32
const val GENERIC_PUBLIC_KEY_SIZE = 32
const val X25519_PRIVATE_KEY_SIZE = 32
const val X25519_PUBLIC_KEY_SIZE = 32

fun deriveAgreementPrivateKey(keyMaterial: ByteArray): ByteArray =
    hkdfHmacSha256(keyMaterial, "agreement".toByteArray(), GENERIC_PRIVATE_KEY_SIZE)

fun deriveSigningPrivateKey(keyMaterial: ByteArray): ByteArray =
    hkdfHmacSha256(keyMaterial, "signing".toByteArray(), GENERIC_PUBLIC_KEY_SIZE)

fun x25519NewPrivateKeyUsing(rng: RandomNumberGenerator): ByteArray =
    rng.randomData(X25519_PRIVATE_KEY_SIZE)

fun x25519PublicKeyFromPrivateKey(privateKey: ByteArray): ByteArray {
    val privParams = X25519PrivateKeyParameters(privateKey, 0)
    return privParams.generatePublicKey().encoded
}

fun x25519SharedKey(privateKey: ByteArray, publicKey: ByteArray): ByteArray {
    val privParams = X25519PrivateKeyParameters(privateKey, 0)
    val pubParams = X25519PublicKeyParameters(publicKey, 0)
    val agreement = X25519Agreement()
    agreement.init(privParams)
    val sharedSecret = ByteArray(agreement.agreementSize)
    agreement.calculateAgreement(pubParams, sharedSecret, 0)
    return hkdfHmacSha256(sharedSecret, "agreement".toByteArray(), SYMMETRIC_KEY_SIZE)
}
