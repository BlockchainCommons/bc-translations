package com.blockchaincommons.bccrypto

import org.bouncycastle.crypto.digests.SHA256Digest
import org.bouncycastle.crypto.digests.SHA512Digest
import org.bouncycastle.crypto.generators.HKDFBytesGenerator
import org.bouncycastle.crypto.generators.PKCS5S2ParametersGenerator
import org.bouncycastle.crypto.params.HKDFParameters
import org.bouncycastle.crypto.params.KeyParameter
import java.security.MessageDigest
import javax.crypto.Mac
import javax.crypto.spec.SecretKeySpec

const val CRC32_SIZE = 4
const val SHA256_SIZE = 32
const val SHA512_SIZE = 64

fun crc32(data: ByteArray): UInt {
    val crc = java.util.zip.CRC32()
    crc.update(data)
    return crc.value.toUInt()
}

fun crc32Data(data: ByteArray, littleEndian: Boolean = false): ByteArray {
    val checksum = crc32(data).toInt()
    return if (littleEndian) {
        byteArrayOf(
            (checksum and 0xFF).toByte(),
            ((checksum shr 8) and 0xFF).toByte(),
            ((checksum shr 16) and 0xFF).toByte(),
            ((checksum shr 24) and 0xFF).toByte(),
        )
    } else {
        byteArrayOf(
            ((checksum shr 24) and 0xFF).toByte(),
            ((checksum shr 16) and 0xFF).toByte(),
            ((checksum shr 8) and 0xFF).toByte(),
            (checksum and 0xFF).toByte(),
        )
    }
}

fun sha256(data: ByteArray): ByteArray {
    val digest = MessageDigest.getInstance("SHA-256")
    return digest.digest(data)
}

fun doubleSha256(data: ByteArray): ByteArray = sha256(sha256(data))

fun sha512(data: ByteArray): ByteArray {
    val digest = MessageDigest.getInstance("SHA-512")
    return digest.digest(data)
}

fun hmacSha256(key: ByteArray, message: ByteArray): ByteArray {
    val mac = Mac.getInstance("HmacSHA256")
    mac.init(SecretKeySpec(key, "HmacSHA256"))
    return mac.doFinal(message)
}

fun hmacSha512(key: ByteArray, message: ByteArray): ByteArray {
    val mac = Mac.getInstance("HmacSHA512")
    mac.init(SecretKeySpec(key, "HmacSHA512"))
    return mac.doFinal(message)
}

fun pbkdf2HmacSha256(
    pass: ByteArray,
    salt: ByteArray,
    iterations: Int,
    keyLen: Int,
): ByteArray {
    val generator = PKCS5S2ParametersGenerator(SHA256Digest())
    generator.init(pass, salt, iterations)
    val params = generator.generateDerivedParameters(keyLen * 8) as KeyParameter
    return params.key
}

fun pbkdf2HmacSha512(
    pass: ByteArray,
    salt: ByteArray,
    iterations: Int,
    keyLen: Int,
): ByteArray {
    val generator = PKCS5S2ParametersGenerator(SHA512Digest())
    generator.init(pass, salt, iterations)
    val params = generator.generateDerivedParameters(keyLen * 8) as KeyParameter
    return params.key
}

fun hkdfHmacSha256(
    keyMaterial: ByteArray,
    salt: ByteArray,
    keyLen: Int,
): ByteArray {
    val hkdf = HKDFBytesGenerator(SHA256Digest())
    hkdf.init(HKDFParameters(keyMaterial, salt, ByteArray(0)))
    val output = ByteArray(keyLen)
    hkdf.generateBytes(output, 0, keyLen)
    return output
}

fun hkdfHmacSha512(
    keyMaterial: ByteArray,
    salt: ByteArray,
    keyLen: Int,
): ByteArray {
    val hkdf = HKDFBytesGenerator(SHA512Digest())
    hkdf.init(HKDFParameters(keyMaterial, salt, ByteArray(0)))
    val output = ByteArray(keyLen)
    hkdf.generateBytes(output, 0, keyLen)
    return output
}
