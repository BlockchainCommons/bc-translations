package com.blockchaincommons.provenancemark

class Xoshiro256StarStar private constructor(private val s: ULongArray) {
    init {
        require(s.size == 4) { "state must have 4 elements" }
    }

    fun toState(): ULongArray = s.copyOf()

    fun toData(): ByteArray {
        val data = ByteArray(32)
        for (i in 0 until 4) {
            val value = s[i]
            val offset = i * 8
            for (j in 0 until 8) {
                data[offset + j] = ((value shr (j * 8)) and 0xFFu).toByte()
            }
        }
        return data
    }

    fun copy(): Xoshiro256StarStar = fromState(s)

    fun nextULong(): ULong {
        val result = (s[1] * 5u).rotateLeft(7) * 9u
        val t = s[1] shl 17

        s[2] = s[2] xor s[0]
        s[3] = s[3] xor s[1]
        s[1] = s[1] xor s[2]
        s[0] = s[0] xor s[3]

        s[2] = s[2] xor t
        s[3] = s[3].rotateLeft(45)

        return result
    }

    fun nextByte(): Byte = (nextULong() and 0xFFu).toByte()

    fun nextBytes(length: Int): ByteArray = ByteArray(length) { nextByte() }

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Xoshiro256StarStar) return false
        return s.contentEquals(other.s)
    }

    override fun hashCode(): Int = s.contentHashCode()

    companion object {
        fun fromState(state: ULongArray): Xoshiro256StarStar {
            require(state.size == 4) { "state must have 4 elements" }
            return Xoshiro256StarStar(state.copyOf())
        }

        fun fromData(data: ByteArray): Xoshiro256StarStar {
            require(data.size == 32) { "data must be 32 bytes" }
            val state = ULongArray(4)
            for (i in 0 until 4) {
                var value = 0uL
                val offset = i * 8
                for (j in 0 until 8) {
                    value = value or ((data[offset + j].toUByte().toULong()) shl (j * 8))
                }
                state[i] = value
            }
            return Xoshiro256StarStar(state)
        }
    }
}

private fun ULong.rotateLeft(bits: Int): ULong =
    (this shl bits) or (this shr (64 - bits))
