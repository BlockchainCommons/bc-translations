package com.blockchaincommons.dcbor

import java.util.TreeMap

/**
 * Comparator for byte arrays that sorts by lexicographic order of unsigned byte values.
 */
private object ByteArrayComparator : Comparator<ByteArray> {
    override fun compare(a: ByteArray, b: ByteArray): Int {
        val minLen = minOf(a.size, b.size)
        for (i in 0 until minLen) {
            val cmp = (a[i].toInt() and 0xFF) - (b[i].toInt() and 0xFF)
            if (cmp != 0) return cmp
        }
        return a.size - b.size
    }
}

/**
 * Deterministic CBOR map with keys sorted by encoded CBOR byte order.
 *
 * Keys are maintained in lexicographic order of their CBOR-encoded byte
 * representations, as required by the dCBOR specification.
 */
class CborMap : Iterable<Pair<Cbor, Cbor>> {
    // Keys are the CBOR-encoded bytes of the key.
    // Values are pairs of (original_key_cbor, value_cbor).
    @PublishedApi
    internal val entries: TreeMap<ByteArray, Pair<Cbor, Cbor>> = TreeMap(ByteArrayComparator)

    val size: Int get() = entries.size

    fun isEmpty(): Boolean = entries.isEmpty()

    fun insert(key: Cbor, value: Cbor) {
        val keyData = key.toCborData()
        entries[keyData] = key to value
    }

    /**
     * Insert during decode — validates ordering and uniqueness.
     */
    internal fun insertNext(key: Cbor, value: Cbor) {
        val keyData = key.toCborData()
        if (entries.isNotEmpty()) {
            val lastKey = entries.lastKey()
            val cmp = ByteArrayComparator.compare(lastKey, keyData)
            if (cmp > 0) throw CborException.MisorderedMapKey()
            if (cmp == 0) throw CborException.DuplicateMapKey()
        }
        entries[keyData] = key to value
    }

    inline fun <reified K, reified V> get(key: K): V? where K : Any {
        val keyCbor = Cbor.from(key)
        val keyData = keyCbor.toCborData()
        val pair = entries[keyData] ?: return null
        return Cbor.to(pair.second)
    }

    inline fun <reified K, reified V> extract(key: K): V where K : Any {
        return get<K, V>(key) ?: throw CborException.MissingMapKey()
    }

    fun containsKey(key: Cbor): Boolean {
        val keyData = key.toCborData()
        return entries.containsKey(keyData)
    }

    override fun iterator(): Iterator<Pair<Cbor, Cbor>> = entries.values.iterator()

    fun toList(): List<Pair<Cbor, Cbor>> = entries.values.toList()

    fun toCborData(): ByteArray {
        val buf = mutableListOf<Byte>()
        buf.addAll(Varint.encode(entries.size.toULong(), MajorType.Map).toList())
        for ((_, pair) in entries) {
            buf.addAll(pair.first.toCborData().toList())
            buf.addAll(pair.second.toCborData().toList())
        }
        return buf.toByteArray()
    }

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is CborMap) return false
        if (size != other.size) return false
        val myEntries = toList()
        val otherEntries = other.toList()
        return myEntries.zip(otherEntries).all { (a, b) -> a.first == b.first && a.second == b.second }
    }

    override fun hashCode(): Int {
        var hash = 0
        for ((_, pair) in entries) {
            hash = hash * 31 + pair.first.hashCode()
            hash = hash * 31 + pair.second.hashCode()
        }
        return hash
    }

    override fun toString(): String {
        val pairs = toList().joinToString(", ") { "${it.first}: ${it.second}" }
        return "{$pairs}"
    }
}
