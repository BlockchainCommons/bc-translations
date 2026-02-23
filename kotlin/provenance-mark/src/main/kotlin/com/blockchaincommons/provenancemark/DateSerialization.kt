package com.blockchaincommons.provenancemark

import com.blockchaincommons.dcbor.CborDate
import java.time.Instant
import java.time.ZoneOffset
import java.time.ZonedDateTime

private val referenceDate: Instant = ZonedDateTime
    .of(2001, 1, 1, 0, 0, 0, 0, ZoneOffset.UTC)
    .toInstant()

object DateSerialization {
    fun serialize2Bytes(date: CborDate): ByteArray {
        val components = ZonedDateTime.ofInstant(date.instant, ZoneOffset.UTC)
        val year = components.year
        val month = components.monthValue
        val day = components.dayOfMonth

        val yy = year - 2023
        if (yy !in 0 until 128) {
            throw ProvenanceMarkException.YearOutOfRange(year)
        }
        if (month !in 1..12 || day !in 1..31) {
            throw ProvenanceMarkException.InvalidMonthOrDay(year, month, day)
        }

        val value = ((yy shl 9) or (month shl 5) or day) and 0xFFFF
        return byteArrayOf(((value shr 8) and 0xFF).toByte(), (value and 0xFF).toByte())
    }

    fun deserialize2Bytes(bytes: ByteArray): CborDate {
        require(bytes.size == 2) { "2-byte date requires exactly 2 bytes" }
        val value = ((bytes[0].toInt() and 0xFF) shl 8) or (bytes[1].toInt() and 0xFF)
        val day = value and 0b11111
        val month = (value shr 5) and 0b1111
        val yy = (value shr 9) and 0b1111111
        val year = yy + 2023

        if (month !in 1..12 || day !in rangeOfDaysInMonth(year, month)) {
            throw ProvenanceMarkException.InvalidMonthOrDay(year, month, day)
        }

        return try {
            CborDate.fromYmdHms(year, month, day, 0, 0, 0)
        } catch (_: Exception) {
            throw ProvenanceMarkException.InvalidDate(
                "Cannot construct date $year-${"%02d".format(month)}-${"%02d".format(day)}"
            )
        }
    }

    fun serialize4Bytes(date: CborDate): ByteArray {
        val seconds = date.instant.epochSecond - referenceDate.epochSecond
        if (seconds < 0 || seconds > UInt.MAX_VALUE.toLong()) {
            throw ProvenanceMarkException.DateOutOfRange("seconds value too large for u32")
        }
        val n = seconds.toUInt()
        return byteArrayOf(
            (n shr 24).toByte(),
            (n shr 16).toByte(),
            (n shr 8).toByte(),
            n.toByte(),
        )
    }

    fun deserialize4Bytes(bytes: ByteArray): CborDate {
        require(bytes.size == 4) { "4-byte date requires exactly 4 bytes" }
        val n =
            ((bytes[0].toUInt() and 0xFFu) shl 24) or
                ((bytes[1].toUInt() and 0xFFu) shl 16) or
                ((bytes[2].toUInt() and 0xFFu) shl 8) or
                (bytes[3].toUInt() and 0xFFu)
        val instant = referenceDate.plusSeconds(n.toLong())
        return CborDate.fromInstant(instant)
    }

    fun serialize6Bytes(date: CborDate): ByteArray {
        val millis = date.instant.toEpochMilli() - referenceDate.toEpochMilli()
        if (millis < 0) {
            throw ProvenanceMarkException.DateOutOfRange("milliseconds value too large for u64")
        }
        val n = millis.toULong()
        if (n > 0xe5940a78a7ffuL) {
            throw ProvenanceMarkException.DateOutOfRange("date exceeds maximum representable value")
        }
        val full = ByteArray(8)
        var value = n
        for (i in 7 downTo 0) {
            full[i] = (value and 0xFFu).toByte()
            value = value shr 8
        }
        return full.copyOfRange(2, 8)
    }

    fun deserialize6Bytes(bytes: ByteArray): CborDate {
        require(bytes.size == 6) { "6-byte date requires exactly 6 bytes" }
        val full = ByteArray(8)
        System.arraycopy(bytes, 0, full, 2, 6)
        var n = 0uL
        for (b in full) {
            n = (n shl 8) or (b.toUByte().toULong())
        }
        if (n > 0xe5940a78a7ffuL) {
            throw ProvenanceMarkException.DateOutOfRange("date exceeds maximum representable value")
        }
        val instant = referenceDate.plusMillis(n.toLong())
        return CborDate.fromInstant(instant)
    }
}

fun CborDate.serialize2Bytes(): ByteArray = DateSerialization.serialize2Bytes(this)
fun CborDate.serialize4Bytes(): ByteArray = DateSerialization.serialize4Bytes(this)
fun CborDate.serialize6Bytes(): ByteArray = DateSerialization.serialize6Bytes(this)

fun deserialize2Bytes(bytes: ByteArray): CborDate = DateSerialization.deserialize2Bytes(bytes)
fun deserialize4Bytes(bytes: ByteArray): CborDate = DateSerialization.deserialize4Bytes(bytes)
fun deserialize6Bytes(bytes: ByteArray): CborDate = DateSerialization.deserialize6Bytes(bytes)

fun rangeOfDaysInMonth(year: Int, month: Int): IntRange {
    val nextMonth = if (month == 12) {
        ZonedDateTime.of(year + 1, 1, 1, 0, 0, 0, 0, ZoneOffset.UTC)
    } else {
        ZonedDateTime.of(year, month + 1, 1, 0, 0, 0, 0, ZoneOffset.UTC)
    }
    val lastDay = nextMonth.minusDays(1).dayOfMonth
    return 1..lastDay
}
