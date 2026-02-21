package com.blockchaincommons.dcbor

/**
 * Represents a CBOR byte string (major type 2).
 *
 * Wrapper around a byte array providing a richer API for CBOR operations.
 */
class ByteString(private val data: ByteArray) : Comparable<ByteString>, Iterable<Byte> {

    val size: Int get() = data.size

    fun isEmpty(): Boolean = data.isEmpty()

    fun toByteArray(): ByteArray = data.copyOf()

    operator fun get(index: Int): Byte = data[index]

    override fun iterator(): Iterator<Byte> = data.iterator()

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is ByteString) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    override fun compareTo(other: ByteString): Int {
        val minLen = minOf(data.size, other.data.size)
        for (i in 0 until minLen) {
            val cmp = (data[i].toInt() and 0xFF) - (other.data[i].toInt() and 0xFF)
            if (cmp != 0) return cmp
        }
        return data.size - other.data.size
    }

    fun toHexString(): String = data.toHexString()

    override fun toString(): String = "ByteString(${toHexString()})"

    companion object {
        fun fromHex(hex: String): ByteString = ByteString(hex.hexToByteArray())
    }
}
