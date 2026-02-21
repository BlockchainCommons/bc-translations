package com.blockchaincommons.dcbor

typealias TagValue = ULong

/**
 * Represents a CBOR tag (major type 6) with optional associated name.
 *
 * Tags are considered equal if their numeric values are equal,
 * regardless of their names.
 */
class Tag private constructor(
    val value: TagValue,
    val name: String?
) {
    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Tag) return false
        return value == other.value
    }

    override fun hashCode(): Int = value.hashCode()

    override fun toString(): String = name ?: value.toString()

    companion object {
        fun withValue(value: TagValue): Tag = Tag(value, null)

        fun withStaticName(value: TagValue, name: String): Tag = Tag(value, name)

        operator fun invoke(value: TagValue, name: String): Tag = Tag(value, name)

        operator fun invoke(value: Int, name: String): Tag = Tag(value.toULong(), name)

        operator fun invoke(value: TagValue): Tag = Tag(value, null)
    }
}

fun TagValue.toTag(): Tag = Tag.withValue(this)
fun Int.toTag(): Tag = Tag.withValue(this.toULong())
fun Long.toTag(): Tag = Tag.withValue(this.toULong())
