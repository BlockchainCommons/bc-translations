package com.blockchaincommons.provenancemark

internal class ChaCha20(key: ByteArray, nonce: ByteArray) {
    private val state = UIntArray(16)
    private val keystream = ByteArray(64)
    private var position = 64

    init {
        require(key.size == 32) { "ChaCha20 key must be 32 bytes" }
        require(nonce.size == 12) { "ChaCha20 nonce must be 12 bytes" }

        state[0] = 0x61707865u
        state[1] = 0x3320646eu
        state[2] = 0x79622d32u
        state[3] = 0x6b206574u

        for (i in 0 until 8) {
            val o = i * 4
            state[4 + i] =
                key[o].toUByte().toUInt() or
                    (key[o + 1].toUByte().toUInt() shl 8) or
                    (key[o + 2].toUByte().toUInt() shl 16) or
                    (key[o + 3].toUByte().toUInt() shl 24)
        }

        state[12] = 0u

        for (i in 0 until 3) {
            val o = i * 4
            state[13 + i] =
                nonce[o].toUByte().toUInt() or
                    (nonce[o + 1].toUByte().toUInt() shl 8) or
                    (nonce[o + 2].toUByte().toUInt() shl 16) or
                    (nonce[o + 3].toUByte().toUInt() shl 24)
        }
    }

    fun process(data: ByteArray): ByteArray {
        val out = data.copyOf()
        processInPlace(out)
        return out
    }

    fun processInPlace(data: ByteArray) {
        for (i in data.indices) {
            if (position >= 64) {
                generateBlock()
                position = 0
            }
            data[i] = (data[i].toInt() xor keystream[position].toInt()).toByte()
            position += 1
        }
    }

    private fun generateBlock() {
        val working = state.copyOf()

        repeat(10) {
            quarterRound(working, 0, 4, 8, 12)
            quarterRound(working, 1, 5, 9, 13)
            quarterRound(working, 2, 6, 10, 14)
            quarterRound(working, 3, 7, 11, 15)

            quarterRound(working, 0, 5, 10, 15)
            quarterRound(working, 1, 6, 11, 12)
            quarterRound(working, 2, 7, 8, 13)
            quarterRound(working, 3, 4, 9, 14)
        }

        for (i in 0 until 16) {
            val value = working[i] + state[i]
            val o = i * 4
            keystream[o] = value.toByte()
            keystream[o + 1] = (value shr 8).toByte()
            keystream[o + 2] = (value shr 16).toByte()
            keystream[o + 3] = (value shr 24).toByte()
        }

        state[12] = state[12] + 1u
    }

    private fun quarterRound(s: UIntArray, a: Int, b: Int, c: Int, d: Int) {
        s[a] += s[b]; s[d] = (s[d] xor s[a]).rotateLeft(16)
        s[c] += s[d]; s[b] = (s[b] xor s[c]).rotateLeft(12)
        s[a] += s[b]; s[d] = (s[d] xor s[a]).rotateLeft(8)
        s[c] += s[d]; s[b] = (s[b] xor s[c]).rotateLeft(7)
    }
}
