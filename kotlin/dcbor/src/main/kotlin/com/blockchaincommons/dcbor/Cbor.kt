package com.blockchaincommons.dcbor

import java.math.BigInteger

/**
 * Central type representing any CBOR data item.
 *
 * Wraps a [CborCase] and provides encoding, decoding, formatting, and
 * convenience accessors. Immutable — operations that appear to modify
 * create new instances.
 */
class Cbor(private val case: CborCase) {
    val cborCase: CborCase get() = case

    // ---- Encoding ----

    fun toCborData(): ByteArray = when (case) {
        is CborCase.Unsigned -> Varint.encode(case.value, MajorType.Unsigned)
        is CborCase.Negative -> Varint.encode(case.value, MajorType.Negative)
        is CborCase.CborByteString -> {
            val data = case.value.toByteArray()
            Varint.encode(data.size.toULong(), MajorType.ByteString) + data
        }
        is CborCase.Text -> {
            val nfc = StringUtil.toNfc(case.value)
            val bytes = nfc.toByteArray(Charsets.UTF_8)
            Varint.encode(bytes.size.toULong(), MajorType.Text) + bytes
        }
        is CborCase.Array -> {
            val buf = Varint.encode(case.value.size.toULong(), MajorType.Array).toMutableList()
            for (item in case.value) {
                buf.addAll(item.toCborData().toList())
            }
            buf.toByteArray()
        }
        is CborCase.CborMap -> case.value.toCborData()
        is CborCase.Tagged -> {
            Varint.encode(case.tag.value, MajorType.Tagged) + case.item.toCborData()
        }
        is CborCase.CborSimple -> case.value.toCborData()
    }

    val hex: String get() = toCborData().toHexString()

    // ---- Decoding ----

    companion object {
        fun tryFromData(data: ByteArray): Cbor = Decode.decodeCbor(data)

        fun tryFromHex(hex: String): Cbor {
            val data = hex.hexToByteArray()
            return tryFromData(data)
        }

        // ---- Factory methods ----

        fun fromUnsigned(value: ULong): Cbor = Cbor(CborCase.Unsigned(value))
        fun fromNegative(value: ULong): Cbor = Cbor(CborCase.Negative(value))

        fun fromInt(value: Int): Cbor = if (value >= 0) {
            Cbor(CborCase.Unsigned(value.toULong()))
        } else {
            Cbor(CborCase.Negative((-1L - value.toLong()).toULong()))
        }

        fun fromLong(value: Long): Cbor = if (value >= 0) {
            Cbor(CborCase.Unsigned(value.toULong()))
        } else {
            Cbor(CborCase.Negative((-1L - value).toULong()))
        }

        fun fromUInt(value: UInt): Cbor = Cbor(CborCase.Unsigned(value.toULong()))
        fun fromUShort(value: UShort): Cbor = Cbor(CborCase.Unsigned(value.toULong()))
        fun fromUByte(value: UByte): Cbor = Cbor(CborCase.Unsigned(value.toULong()))

        fun fromDouble(value: Double): Cbor {
            val n = value
            if (n < 0.0) {
                val i = Exact.longExactFromDouble(n)
                if (i != null && i < 0) {
                    val neg = -1L - i
                    if (neg >= 0) return Cbor(CborCase.Negative(neg.toULong()))
                }
            }
            val u = Exact.ulongFromDouble(n)
            if (u != null) return Cbor(CborCase.Unsigned(u))
            return Cbor(CborCase.CborSimple(Simple.Float(n)))
        }

        fun fromFloat(value: Float): Cbor {
            val n = value
            if (n < 0.0f) {
                val neg = Exact.ulongFromFloat(-1.0f - n)
                if (neg != null) return Cbor(CborCase.Negative(neg))
            }
            val u = Exact.uintFromFloat(n)
            if (u != null) return Cbor(CborCase.Unsigned(u.toULong()))
            return Cbor(CborCase.CborSimple(Simple.Float(n.toDouble())))
        }

        fun fromString(value: String): Cbor = Cbor(CborCase.Text(value))
        fun fromByteString(value: ByteArray): Cbor = Cbor(CborCase.CborByteString(ByteString(value)))
        fun fromByteString(value: ByteString): Cbor = Cbor(CborCase.CborByteString(value))
        fun fromByteStringHex(hex: String): Cbor = Cbor(CborCase.CborByteString(ByteString.fromHex(hex)))

        fun fromArray(items: List<Cbor>): Cbor = Cbor(CborCase.Array(items))
        fun fromMap(map: CborMap): Cbor = Cbor(CborCase.CborMap(map))
        fun fromSet(set: CborSet): Cbor = fromMap(set.let {
            // Set is backed by a map
            val m = CborMap()
            for (item in it.toList()) m.insert(item, item)
            m
        })

        fun tagged(tag: Tag, item: Cbor): Cbor = Cbor(CborCase.Tagged(tag, item))
        fun taggedValue(tagValue: ULong, item: Cbor): Cbor = Cbor(CborCase.Tagged(Tag.withValue(tagValue), item))

        val TRUE: Cbor = Cbor(CborCase.CborSimple(Simple.True))
        val FALSE: Cbor = Cbor(CborCase.CborSimple(Simple.False))
        val NULL: Cbor = Cbor(CborCase.CborSimple(Simple.Null))

        fun `true`(): Cbor = TRUE
        fun `false`(): Cbor = FALSE
        fun `null`(): Cbor = NULL
        fun nan(): Cbor = Cbor(CborCase.CborSimple(Simple.Float(Double.NaN)))

        fun fromBoolean(value: Boolean): Cbor = if (value) `true`() else `false`()

        // ---- Generic conversion ----

        @Suppress("UNCHECKED_CAST")
        inline fun <reified T> from(value: T): Cbor where T : Any = when (T::class) {
            Int::class -> fromInt(value as Int)
            Long::class -> fromLong(value as Long)
            UInt::class -> fromUInt(value as UInt)
            ULong::class -> fromUnsigned(value as ULong)
            UShort::class -> fromUShort(value as UShort)
            UByte::class -> fromUByte(value as UByte)
            Double::class -> fromDouble(value as Double)
            Float::class -> fromFloat(value as Float)
            String::class -> fromString(value as String)
            Boolean::class -> fromBoolean(value as Boolean)
            ByteArray::class -> fromByteString(value as ByteArray)
            ByteString::class -> fromByteString(value as ByteString)
            Cbor::class -> value as Cbor
            else -> throw CborException.WrongType()
        }

        @Suppress("UNCHECKED_CAST")
        inline fun <reified T> to(cbor: Cbor): T where T : Any = when (T::class) {
            Int::class -> cbor.tryInt() as T
            Long::class -> cbor.tryLong() as T
            UInt::class -> cbor.tryUInt() as T
            ULong::class -> cbor.tryULong() as T
            Double::class -> cbor.tryDouble() as T
            Float::class -> cbor.tryFloat() as T
            String::class -> cbor.tryText() as T
            Boolean::class -> cbor.tryBool() as T
            ByteArray::class -> cbor.tryByteStringData() as T
            ByteString::class -> cbor.tryByteString() as T
            Cbor::class -> cbor as T
            else -> throw CborException.WrongType()
        }

        /** Comparator that sorts CBOR values by their encoded byte representation. */
        fun cborComparator(): Comparator<Cbor> = Comparator { a, b ->
            val aData = a.toCborData()
            val bData = b.toCborData()
            val minLen = minOf(aData.size, bData.size)
            for (i in 0 until minLen) {
                val cmp = (aData[i].toInt() and 0xFF) - (bData[i].toInt() and 0xFF)
                if (cmp != 0) return@Comparator cmp
            }
            aData.size - bData.size
        }
    }

    // ---- Convenience accessors ----

    // Byte string
    fun isByteString(): Boolean = case is CborCase.CborByteString
    fun tryByteStringData(): ByteArray = when (case) {
        is CborCase.CborByteString -> case.value.toByteArray()
        else -> throw CborException.WrongType()
    }
    fun tryByteString(): ByteString = when (case) {
        is CborCase.CborByteString -> case.value
        else -> throw CborException.WrongType()
    }

    // Tagged
    fun isTagged(): Boolean = case is CborCase.Tagged
    fun tryTagged(): Pair<Tag, Cbor> = when (case) {
        is CborCase.Tagged -> case.tag to case.item
        else -> throw CborException.WrongType()
    }
    fun tryTaggedValue(): Pair<ULong, Cbor> = when (case) {
        is CborCase.Tagged -> case.tag.value to case.item
        else -> throw CborException.WrongType()
    }
    fun tryExpectedTag(expectedTag: Tag): Cbor {
        val (tag, item) = tryTagged()
        if (tag != expectedTag) throw CborException.WrongTag(expectedTag, tag)
        return item
    }

    // Text
    fun isText(): Boolean = case is CborCase.Text
    fun tryText(): String = when (case) {
        is CborCase.Text -> case.value
        else -> throw CborException.WrongType()
    }

    // Array
    fun isArray(): Boolean = case is CborCase.Array
    fun tryArray(): List<Cbor> = when (case) {
        is CborCase.Array -> case.value
        else -> throw CborException.WrongType()
    }

    // Map
    fun isMap(): Boolean = case is CborCase.CborMap
    fun tryMap(): CborMap = when (case) {
        is CborCase.CborMap -> case.value
        else -> throw CborException.WrongType()
    }

    // Boolean
    fun isTrue(): Boolean = case == CborCase.CborSimple(Simple.True)
    fun isFalse(): Boolean = case == CborCase.CborSimple(Simple.False)
    fun tryBool(): Boolean = when (case) {
        is CborCase.CborSimple -> when (case.value) {
            is Simple.True -> true
            is Simple.False -> false
            else -> throw CborException.WrongType()
        }
        else -> throw CborException.WrongType()
    }

    // Null
    fun isNull(): Boolean = case == CborCase.CborSimple(Simple.Null)

    // NaN
    fun isNaN(): Boolean = case is CborCase.CborSimple && case.value is Simple.Float && case.value.value.isNaN()

    // Number
    fun isNumber(): Boolean = when (case) {
        is CborCase.Unsigned, is CborCase.Negative -> true
        is CborCase.CborSimple -> case.value is Simple.Float
        else -> false
    }

    // Integer conversions
    fun tryInt(): Int {
        val l = tryLong()
        if (l < Int.MIN_VALUE || l > Int.MAX_VALUE) throw CborException.OutOfRange()
        return l.toInt()
    }

    fun tryLong(): Long = when (case) {
        is CborCase.Unsigned -> {
            if (case.value > Long.MAX_VALUE.toULong()) throw CborException.OutOfRange()
            case.value.toLong()
        }
        is CborCase.Negative -> {
            val n = case.value
            if (n > Long.MAX_VALUE.toULong()) throw CborException.OutOfRange()
            -1L - n.toLong()
        }
        else -> throw CborException.WrongType()
    }

    fun tryULong(): ULong = when (case) {
        is CborCase.Unsigned -> case.value
        else -> throw CborException.WrongType()
    }

    fun tryUInt(): UInt {
        val v = tryULong()
        if (v > UInt.MAX_VALUE.toULong()) throw CborException.OutOfRange()
        return v.toUInt()
    }

    fun tryDouble(): Double = when (case) {
        is CborCase.Unsigned -> {
            val f = Exact.doubleFromULong(case.value)
            f ?: throw CborException.OutOfRange()
        }
        is CborCase.Negative -> {
            val f = Exact.doubleFromULong(case.value)
            if (f != null) -1.0 - f else throw CborException.OutOfRange()
        }
        is CborCase.CborSimple -> when (val simple = case.value) {
            is Simple.Float -> simple.value
            else -> throw CborException.WrongType()
        }
        else -> throw CborException.WrongType()
    }

    fun tryFloat(): Float = when (case) {
        is CborCase.Unsigned ->
            Exact.floatFromULong(case.value) ?: throw CborException.OutOfRange()
        is CborCase.Negative -> {
            val f = Exact.floatFromULong(case.value) ?: throw CborException.OutOfRange()
            -1.0f - f
        }
        is CborCase.CborSimple -> when (val simple = case.value) {
            is Simple.Float ->
                Exact.floatFromDouble(simple.value) ?: throw CborException.OutOfRange()
            else -> throw CborException.WrongType()
        }
        else -> throw CborException.WrongType()
    }

    // ---- Equality / Hash / Display ----

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Cbor) return false
        return case == other.case
    }

    override fun hashCode(): Int = case.hashCode()

    /** Diagnostic notation (Display format). */
    override fun toString(): String = diagnosticFlat

    val debugDescription: String get() = when (case) {
        is CborCase.Unsigned -> "unsigned(${case.value})"
        is CborCase.Negative -> "negative(${negativeValueToString(case.value)})"
        is CborCase.CborByteString -> "bytes(${case.value.toHexString()})"
        is CborCase.Text -> "text(\"${case.value}\")"
        is CborCase.Array -> "array(${case.value.map { it.debugDescription }})"
        is CborCase.CborMap -> debugDescriptionMap(case.value)
        is CborCase.Tagged -> "tagged(${case.tag}, ${case.item.debugDescription})"
        is CborCase.CborSimple -> "simple(${case.value.debugDescription})"
    }

    private fun debugDescriptionMap(map: CborMap): String {
        val pairs = map.toList().joinToString(", ") { (key, value) ->
            "0x${key.hex}: (${key.debugDescription}, ${value.debugDescription})"
        }
        return "map({$pairs})"
    }

    /** Display description — uses tag names when the tag carries one, else numeric. */
    val description: String get() = when (case) {
        is CborCase.Tagged -> "${case.tag}(${case.item.description})"
        is CborCase.Array -> "[${case.value.joinToString(", ") { it.description }}]"
        is CborCase.CborMap -> {
            val pairs = case.value.toList().joinToString(", ") { "${it.first.description}: ${it.second.description}" }
            "{$pairs}"
        }
        else -> diagnosticFlat
    }

    /** CBOR diagnostic notation per RFC 8949 (multiline for complex structures). */
    fun diagnostic(): String = diagnosticOpt(DiagFormatOpts())

    fun diagnosticAnnotated(): String = diagnosticOpt(DiagFormatOpts(annotate = true))

    fun summary(): String = diagnosticOpt(DiagFormatOpts(summarize = true, flat = true))

    fun hexAnnotated(): String = hexOpt(HexFormatOpts(annotate = true))

    val diagnosticFlat: String get() = when (case) {
        is CborCase.Unsigned -> "${case.value}"
        is CborCase.Negative -> negativeValueToString(case.value)
        is CborCase.CborByteString -> "h'${case.value.toHexString()}'"
        is CborCase.Text -> formatString(case.value)
        is CborCase.Array -> "[${case.value.joinToString(", ") { it.diagnosticFlat }}]"
        is CborCase.CborMap -> {
            val pairs = case.value.toList().joinToString(", ") { "${it.first.diagnosticFlat}: ${it.second.diagnosticFlat}" }
            "{$pairs}"
        }
        is CborCase.Tagged -> "${case.tag.value}(${case.item.diagnosticFlat})"
        is CborCase.CborSimple -> case.value.displayDescription
    }

    private fun formatString(s: String): String {
        val escaped = s.replace("\"", "\\\"")
        return "\"$escaped\""
    }
}

// Extension functions for common Kotlin types to CBOR
fun Int.toCbor(): Cbor = Cbor.fromInt(this)
fun Long.toCbor(): Cbor = Cbor.fromLong(this)
fun ULong.toCbor(): Cbor = Cbor.fromUnsigned(this)
fun UInt.toCbor(): Cbor = Cbor.fromUInt(this)
fun Double.toCbor(): Cbor = Cbor.fromDouble(this)
fun Float.toCbor(): Cbor = Cbor.fromFloat(this)
fun String.toCbor(): Cbor = Cbor.fromString(this)
fun Boolean.toCbor(): Cbor = Cbor.fromBoolean(this)
fun ByteArray.toCbor(): Cbor = Cbor.fromByteString(this)

fun List<Cbor>.toCbor(): Cbor = Cbor.fromArray(this)

internal fun negativeValueToString(value: ULong): String {
    if (value <= Long.MAX_VALUE.toULong()) {
        return (-1L - value.toLong()).toString()
    }
    return BigInteger(value.toString()).add(BigInteger.ONE).negate().toString()
}

/**
 * Returns a new list with elements sorted by their CBOR-encoded byte representation.
 */
fun List<Cbor>.sortByCborEncoding(): List<Cbor> = sortedWith(Cbor.cborComparator())
