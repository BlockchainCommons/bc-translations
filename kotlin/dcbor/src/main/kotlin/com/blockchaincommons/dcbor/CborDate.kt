package com.blockchaincommons.dcbor

import java.time.Instant
import java.time.LocalDate
import java.time.ZoneOffset
import java.time.ZonedDateTime
import java.time.format.DateTimeFormatter
import java.time.format.DateTimeParseException

/**
 * A CBOR-friendly representation of a date and time.
 *
 * Wraps [java.time.Instant] and supports encoding/decoding to CBOR with tag 1,
 * following RFC 8949 date/time standard.
 */
class CborDate private constructor(val instant: Instant) : CborTaggedEncodable, Comparable<CborDate> {

    /** The number of seconds since the Unix epoch. */
    val timestamp: Double get() =
        instant.epochSecond.toDouble() + instant.nano.toDouble() / 1_000_000_000.0

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_DATE))

    override fun untaggedCbor(): Cbor = Cbor.fromDouble(timestamp)

    override fun compareTo(other: CborDate): Int = instant.compareTo(other.instant)

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is CborDate) return false
        return instant == other.instant
    }

    override fun hashCode(): Int = instant.hashCode()

    override fun toString(): String {
        val dt = ZonedDateTime.ofInstant(instant, ZoneOffset.UTC)
        return if (dt.hour == 0 && dt.minute == 0 && dt.second == 0) {
            dt.toLocalDate().toString()
        } else {
            dt.format(DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss'Z'"))
        }
    }

    companion object {
        fun fromInstant(instant: Instant): CborDate = CborDate(instant)

        fun fromTimestamp(secondsSinceUnixEpoch: Double): CborDate {
            val wholeSeconds = secondsSinceUnixEpoch.toLong()
            val nanos = ((secondsSinceUnixEpoch - wholeSeconds) * 1_000_000_000.0).toLong()
            return CborDate(Instant.ofEpochSecond(wholeSeconds, nanos))
        }

        fun fromYmd(year: Int, month: Int, day: Int): CborDate {
            val dt = LocalDate.of(year, month, day).atStartOfDay(ZoneOffset.UTC)
            return CborDate(dt.toInstant())
        }

        fun fromYmdHms(year: Int, month: Int, day: Int, hour: Int, minute: Int, second: Int): CborDate {
            val dt = ZonedDateTime.of(year, month, day, hour, minute, second, 0, ZoneOffset.UTC)
            return CborDate(dt.toInstant())
        }

        fun fromString(value: String): CborDate {
            // Try RFC 3339 date-time
            try {
                val dt = ZonedDateTime.parse(value, DateTimeFormatter.ISO_DATE_TIME)
                return CborDate(dt.toInstant())
            } catch (_: DateTimeParseException) {}

            // Try date only
            try {
                val d = LocalDate.parse(value)
                return CborDate(d.atStartOfDay(ZoneOffset.UTC).toInstant())
            } catch (_: DateTimeParseException) {}

            throw CborException.InvalidDate(value)
        }

        fun now(): CborDate = CborDate(Instant.now())

        fun fromUntaggedCbor(cbor: Cbor): CborDate {
            val n: Double = cbor.tryDouble()
            return fromTimestamp(n)
        }

        fun fromTaggedCbor(cbor: Cbor): CborDate {
            val tags = tagsForValues(listOf(TAG_DATE))
            return CborTaggedUtils.fromTaggedCbor(cbor, tags) { fromUntaggedCbor(it) }
        }

        fun fromCbor(cbor: Cbor): CborDate = fromTaggedCbor(cbor)
    }
}
