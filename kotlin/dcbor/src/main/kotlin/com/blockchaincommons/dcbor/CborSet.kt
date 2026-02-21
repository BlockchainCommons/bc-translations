package com.blockchaincommons.dcbor

/**
 * Deterministic CBOR set. Wraps [CborMap] where each element is both key and value.
 */
class CborSet : Iterable<Cbor> {
    private val map = CborMap()

    val size: Int get() = map.size

    fun isEmpty(): Boolean = map.isEmpty()

    fun insert(value: Cbor) {
        map.insert(value, value)
    }

    fun contains(value: Cbor): Boolean = map.containsKey(value)

    override fun iterator(): Iterator<Cbor> = map.toList().map { it.first }.iterator()

    fun toList(): List<Cbor> = map.toList().map { it.first }

    fun toCborData(): ByteArray = map.toCborData()

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is CborSet) return false
        return map == other.map
    }

    override fun hashCode(): Int = map.hashCode()

    override fun toString(): String {
        val items = toList().joinToString(", ") { it.toString() }
        return "Set([$items])"
    }

    companion object {
        fun fromList(items: List<Cbor>): CborSet {
            val set = CborSet()
            for (item in items.sortedWith(Cbor.cborComparator())) {
                set.insert(item)
            }
            return set
        }

        fun tryFromList(items: List<Cbor>): CborSet {
            val set = CborSet()
            val sorted = items.sortedWith(Cbor.cborComparator())
            for (item in sorted) {
                if (set.contains(item)) throw CborException.DuplicateMapKey()
                set.insert(item)
            }
            return set
        }
    }
}
