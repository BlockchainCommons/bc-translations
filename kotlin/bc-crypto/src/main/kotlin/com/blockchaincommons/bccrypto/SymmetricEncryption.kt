package com.blockchaincommons.bccrypto

import javax.crypto.Cipher
import javax.crypto.spec.IvParameterSpec
import javax.crypto.spec.SecretKeySpec

const val SYMMETRIC_KEY_SIZE = 32
const val SYMMETRIC_NONCE_SIZE = 12
const val SYMMETRIC_AUTH_SIZE = 16

fun aeadChaCha20Poly1305Encrypt(
    plaintext: ByteArray,
    key: ByteArray,
    nonce: ByteArray,
    aad: ByteArray = ByteArray(0),
): Pair<ByteArray, ByteArray> {
    val cipher = Cipher.getInstance("ChaCha20-Poly1305")
    cipher.init(Cipher.ENCRYPT_MODE, SecretKeySpec(key, "ChaCha20"), IvParameterSpec(nonce))
    if (aad.isNotEmpty()) {
        cipher.updateAAD(aad)
    }
    val output = cipher.doFinal(plaintext)
    val ciphertext = output.copyOfRange(0, output.size - SYMMETRIC_AUTH_SIZE)
    val auth = output.copyOfRange(output.size - SYMMETRIC_AUTH_SIZE, output.size)
    return ciphertext to auth
}

fun aeadChaCha20Poly1305Decrypt(
    ciphertext: ByteArray,
    key: ByteArray,
    nonce: ByteArray,
    auth: ByteArray,
    aad: ByteArray = ByteArray(0),
): ByteArray {
    val cipher = Cipher.getInstance("ChaCha20-Poly1305")
    cipher.init(Cipher.DECRYPT_MODE, SecretKeySpec(key, "ChaCha20"), IvParameterSpec(nonce))
    if (aad.isNotEmpty()) {
        cipher.updateAAD(aad)
    }
    val input = ciphertext + auth
    return try {
        cipher.doFinal(input)
    } catch (e: Exception) {
        throw BcCryptoException("AEAD error", e)
    }
}
