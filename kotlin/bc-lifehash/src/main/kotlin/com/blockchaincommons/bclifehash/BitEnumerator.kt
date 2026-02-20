package com.blockchaincommons.bclifehash

class BitEnumerator(
    private val data: ByteArray,
) {
    private var index: Int = 0
    private var mask: Int = 0x80

    fun hasNext(): Boolean = mask != 0 || index != data.size - 1

    fun next(): Boolean {
        require(hasNext()) { "BitEnumerator underflow" }

        if (mask == 0) {
            mask = 0x80
            index += 1
        }

        val bit = (data[index].toInt() and mask) != 0
        mask = mask ushr 1
        return bit
    }

    fun nextUInt2(): Int {
        var bitMask = 0x02
        var value = 0
        repeat(2) {
            if (next()) {
                value = value or bitMask
            }
            bitMask = bitMask ushr 1
        }
        return value
    }

    fun nextUInt8(): Int {
        var bitMask = 0x80
        var value = 0
        repeat(8) {
            if (next()) {
                value = value or bitMask
            }
            bitMask = bitMask ushr 1
        }
        return value
    }

    fun nextUInt16(): Int {
        var bitMask = 0x8000
        var value = 0
        repeat(16) {
            if (next()) {
                value = value or bitMask
            }
            bitMask = bitMask ushr 1
        }
        return value
    }

    fun nextFrac(): Double = nextUInt16().toDouble() / 65535.0

    fun forAll(block: (Boolean) -> Unit) {
        while (hasNext()) {
            block(next())
        }
    }
}

class BitAggregator {
    private val data: MutableList<Byte> = mutableListOf()
    private var bitMask: Int = 0

    fun append(bit: Boolean) {
        if (bitMask == 0) {
            bitMask = 0x80
            data.add(0)
        }

        if (bit) {
            val last = data.lastIndex
            data[last] = (data[last].toInt() or bitMask).toByte()
        }

        bitMask = bitMask ushr 1
    }

    fun data(): ByteArray = data.toByteArray()
}
