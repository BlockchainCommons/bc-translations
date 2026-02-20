package com.blockchaincommons.bccrypto

import com.blockchaincommons.bcrand.RandomNumberGenerator
import fr.acinq.secp256k1.Secp256k1

const val ECDSA_PRIVATE_KEY_SIZE = 32
const val ECDSA_PUBLIC_KEY_SIZE = 33
const val ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE = 65
const val ECDSA_MESSAGE_HASH_SIZE = 32
const val ECDSA_SIGNATURE_SIZE = 64
const val SCHNORR_PUBLIC_KEY_SIZE = 32

fun ecdsaNewPrivateKeyUsing(rng: RandomNumberGenerator): ByteArray =
    rng.randomData(ECDSA_PRIVATE_KEY_SIZE)

fun ecdsaPublicKeyFromPrivateKey(privateKey: ByteArray): ByteArray {
    val uncompressed = Secp256k1.pubkeyCreate(privateKey)
    return Secp256k1.pubKeyCompress(uncompressed)
}

fun ecdsaDecompressPublicKey(compressedPublicKey: ByteArray): ByteArray =
    Secp256k1.pubkeyParse(compressedPublicKey)

fun ecdsaCompressPublicKey(uncompressedPublicKey: ByteArray): ByteArray =
    Secp256k1.pubKeyCompress(Secp256k1.pubkeyParse(uncompressedPublicKey))

fun ecdsaDerivePrivateKey(keyMaterial: ByteArray): ByteArray =
    hkdfHmacSha256(keyMaterial, "signing".toByteArray(), 32)

fun schnorrPublicKeyFromPrivateKey(privateKey: ByteArray): ByteArray {
    val compressed = ecdsaPublicKeyFromPrivateKey(privateKey)
    return compressed.copyOfRange(1, 33)
}
