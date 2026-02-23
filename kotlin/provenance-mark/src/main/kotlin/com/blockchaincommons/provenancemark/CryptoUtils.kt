package com.blockchaincommons.provenancemark

import java.security.MessageDigest
import javax.crypto.Mac
import javax.crypto.spec.SecretKeySpec

object CryptoUtils {
    const val SHA256_SIZE: Int = 32

    fun sha256(data: ByteArray): ByteArray {
        val digest = MessageDigest.getInstance("SHA-256")
        return digest.digest(data)
    }

    fun sha256Prefix(data: ByteArray, prefix: Int): ByteArray {
        return sha256(data).copyOfRange(0, prefix)
    }

    fun extendKey(data: ByteArray): ByteArray {
        return hkdfHmacSha256(data, byteArrayOf(), 32)
    }

    fun hkdfHmacSha256(keyMaterial: ByteArray, salt: ByteArray, keyLen: Int): ByteArray {
        return HkdfSha256.derive(
            keyMaterial = keyMaterial,
            salt = salt,
            keyLen = keyLen,
            info = byteArrayOf(),
        )
    }

    fun obfuscate(key: ByteArray, message: ByteArray): ByteArray {
        if (message.isEmpty()) return message.copyOf()

        val extendedKey = extendKey(key)
        val iv = extendedKey.reversedArray().copyOfRange(0, 12)

        val cipher = ChaCha20(extendedKey, iv)
        return cipher.process(message)
    }

    private fun hmacSha256(key: ByteArray, message: ByteArray): ByteArray {
        val mac = Mac.getInstance("HmacSHA256")
        mac.init(SecretKeySpec(key, "HmacSHA256"))
        return mac.doFinal(message)
    }

    private object HkdfSha256 {
        private const val HASH_LEN = SHA256_SIZE
        private const val MAX_OUTPUT_LENGTH = 255 * HASH_LEN

        fun derive(
            keyMaterial: ByteArray,
            salt: ByteArray,
            keyLen: Int,
            info: ByteArray,
        ): ByteArray {
            require(keyLen >= 0) { "keyLen must be non-negative" }
            if (keyLen == 0) return byteArrayOf()
            require(keyLen <= MAX_OUTPUT_LENGTH) {
                "keyLen too large for HKDF-SHA256: $keyLen > $MAX_OUTPUT_LENGTH"
            }

            val prk = extract(keyMaterial, salt)
            return expand(prk, info, keyLen)
        }

        private fun extract(keyMaterial: ByteArray, salt: ByteArray): ByteArray {
            val effectiveSalt = if (salt.isEmpty()) ByteArray(HASH_LEN) else salt
            return hmacSha256(effectiveSalt, keyMaterial)
        }

        private fun expand(prk: ByteArray, info: ByteArray, keyLen: Int): ByteArray {
            val output = ByteArray(keyLen)
            var generated = 0
            var previous = byteArrayOf()
            var counter = 1

            while (generated < keyLen) {
                val input = ByteArray(previous.size + info.size + 1)
                var cursor = 0
                if (previous.isNotEmpty()) {
                    System.arraycopy(previous, 0, input, cursor, previous.size)
                    cursor += previous.size
                }
                if (info.isNotEmpty()) {
                    System.arraycopy(info, 0, input, cursor, info.size)
                    cursor += info.size
                }
                input[cursor] = counter.toByte()

                previous = hmacSha256(prk, input)
                val copyLength = minOf(previous.size, keyLen - generated)
                System.arraycopy(previous, 0, output, generated, copyLength)
                generated += copyLength
                counter += 1
            }

            return output
        }
    }
}
