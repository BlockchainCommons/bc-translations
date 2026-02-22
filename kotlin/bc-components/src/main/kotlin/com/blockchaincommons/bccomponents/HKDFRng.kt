package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.hkdfHmacSha256
import com.blockchaincommons.bcrand.RandomNumberGenerator

/**
 * A deterministic random number generator based on HKDF-HMAC-SHA256.
 *
 * Generates deterministic pseudorandom output from key material and a salt
 * using HKDF key derivation. Manages an internal buffer (page) and refills
 * it as needed.
 *
 * @param keyMaterial The seed material to derive random numbers from.
 * @param salt A salt value to mix with the key material.
 * @param pageLength The number of bytes to generate in each HKDF call.
 */
class HKDFRng(
    keyMaterial: ByteArray,
    private val salt: String,
    private val pageLength: Int = DEFAULT_PAGE_LENGTH,
) : RandomNumberGenerator() {

    private val keyMaterial: ByteArray = keyMaterial.copyOf()
    private var pageIndex: Int = 0
    private var buffer: ByteArray = ByteArray(0)
    private var offset: Int = 0

    override fun nextU32(): UInt {
        val bytes = randomData(4)
        return ((bytes[0].toInt() and 0xFF) or
            ((bytes[1].toInt() and 0xFF) shl 8) or
            ((bytes[2].toInt() and 0xFF) shl 16) or
            ((bytes[3].toInt() and 0xFF) shl 24)).toUInt()
    }

    override fun nextU64(): ULong {
        val bytes = randomData(8)
        var result = 0UL
        for (i in 0 until 8) {
            result = result or ((bytes[i].toLong() and 0xFF).toULong() shl (i * 8))
        }
        return result
    }

    private fun refillIfNeeded() {
        if (offset >= buffer.size) {
            val saltString = "$salt-$pageIndex".toByteArray()
            buffer = hkdfHmacSha256(keyMaterial, saltString, pageLength)
            pageIndex++
            offset = 0
        }
    }

    override fun randomData(size: Int): ByteArray {
        val result = ByteArray(size)
        var remaining = size
        var pos = 0
        while (remaining > 0) {
            refillIfNeeded()
            val available = buffer.size - offset
            val toCopy = minOf(remaining, available)
            System.arraycopy(buffer, offset, result, pos, toCopy)
            offset += toCopy
            pos += toCopy
            remaining -= toCopy
        }
        return result
    }

    companion object {
        const val DEFAULT_PAGE_LENGTH = 32
    }
}
