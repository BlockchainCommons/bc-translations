package com.blockchaincommons.bcur

/**
 * Bytewords encoding and decoding with CRC32 checksums.
 *
 * Bytewords maps each byte (0–255) to a unique four-letter English word,
 * enabling human-friendly representations of binary data with error detection.
 */
object Bytewords {
    /**
     * Encodes a byte payload into a bytewords string.
     *
     * Appends a CRC32 checksum and encodes each byte according to the given [style].
     *
     * @throws URException.BytewordsError if encoding fails
     */
    fun encode(data: ByteArray, style: BytewordsStyle): String {
        val checksumBytes = Crc32.checksum(data).toBytesBigEndian()
        val allBytes = data + checksumBytes
        val words = when (style) {
            BytewordsStyle.Standard, BytewordsStyle.Uri ->
                allBytes.map { BytewordsConstants.WORDS[it.toInt() and 0xFF] }
            BytewordsStyle.Minimal ->
                allBytes.map { BytewordsConstants.MINIMALS[it.toInt() and 0xFF] }
        }
        val separator = when (style) {
            BytewordsStyle.Standard -> " "
            BytewordsStyle.Uri -> "-"
            BytewordsStyle.Minimal -> ""
        }
        return words.joinToString(separator)
    }

    /**
     * Decodes a bytewords string back into a byte payload.
     *
     * Verifies the CRC32 checksum and strips it from the result.
     *
     * @throws URException.BytewordsError if the string is invalid or the checksum does not match
     */
    fun decode(encoded: String, style: BytewordsStyle): ByteArray {
        if (!encoded.all { it.code < 128 }) {
            throw URException.BytewordsError("bytewords string contains non-ASCII characters")
        }

        val bytes = when (style) {
            BytewordsStyle.Minimal -> decodeMinimal(encoded)
            else -> {
                val separator = if (style == BytewordsStyle.Standard) " " else "-"
                val parts = encoded.split(separator)
                parts.map { word ->
                    BytewordsConstants.WORD_INDEXES[word]
                        ?: throw URException.BytewordsError("invalid word")
                }.map { it.toByte() }.toByteArray()
            }
        }

        return stripChecksum(bytes)
    }

    /**
     * Encodes a 4-byte identifier as space-separated bytewords.
     *
     * @param data exactly 4 bytes
     * @throws IllegalArgumentException if the data is not exactly 4 bytes
     */
    fun identifier(data: ByteArray): String {
        require(data.size == 4) { "Expected 4 bytes, got ${data.size}" }
        return data.map { BytewordsConstants.WORDS[it.toInt() and 0xFF] }
            .joinToString(" ")
    }

    /**
     * Encodes a 4-byte identifier as space-separated bytemojis.
     *
     * @param data exactly 4 bytes
     * @throws IllegalArgumentException if the data is not exactly 4 bytes
     */
    fun bytemojiIdentifier(data: ByteArray): String {
        require(data.size == 4) { "Expected 4 bytes, got ${data.size}" }
        return data.map { BytewordsConstants.BYTEMOJIS[it.toInt() and 0xFF] }
            .joinToString(" ")
    }

    /** Returns `true` if [word] (lowercase) is a valid byteword. */
    fun isValidWord(word: String): Boolean =
        BytewordsConstants.WORD_INDEXES.containsKey(word)

    private fun decodeMinimal(encoded: String): ByteArray {
        if (encoded.length % 2 != 0) {
            throw URException.BytewordsError("invalid length")
        }
        val result = ByteArray(encoded.length / 2)
        for (i in result.indices) {
            val pair = encoded.substring(i * 2, i * 2 + 2)
            result[i] = (BytewordsConstants.MINIMAL_INDEXES[pair]
                ?: throw URException.BytewordsError("invalid word")).toByte()
        }
        return result
    }

    private fun stripChecksum(data: ByteArray): ByteArray {
        if (data.size < 4) {
            throw URException.BytewordsError("invalid checksum")
        }
        val payloadLen = data.size - 4
        val payload = data.copyOfRange(0, payloadLen)
        val checksum = data.copyOfRange(payloadLen, data.size)
        val expectedBytes = Crc32.checksum(payload).toBytesBigEndian()
        if (!checksum.contentEquals(expectedBytes)) {
            throw URException.BytewordsError("invalid checksum")
        }
        return payload
    }
}
