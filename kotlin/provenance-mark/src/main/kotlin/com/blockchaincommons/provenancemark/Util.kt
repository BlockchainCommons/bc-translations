package com.blockchaincommons.provenancemark

import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborDate
import com.blockchaincommons.dcbor.CborEncodable
import com.blockchaincommons.dcbor.toCbor
import com.fasterxml.jackson.databind.ObjectMapper
import com.fasterxml.jackson.module.kotlin.jacksonObjectMapper
import java.net.URL
import java.net.URLDecoder
import java.nio.charset.StandardCharsets
import java.util.Base64

internal object JsonSupport {
    val mapper: ObjectMapper = jacksonObjectMapper().findAndRegisterModules()
}

internal fun ByteArray.toHex(): String = joinToString("") { "%02x".format(it.toInt() and 0xFF) }

internal fun String.hexToByteArray(): ByteArray {
    require(length % 2 == 0) { "hex string length must be even" }
    return ByteArray(length / 2) { index ->
        val offset = index * 2
        substring(offset, offset + 2).toInt(16).toByte()
    }
}

internal fun ByteArray.toBase64(): String = Base64.getEncoder().encodeToString(this)

internal fun String.fromBase64(): ByteArray {
    return try {
        Base64.getDecoder().decode(this)
    } catch (e: IllegalArgumentException) {
        throw ProvenanceMarkException.Base64(e.message ?: "invalid base64 value")
    }
}

internal fun dateToIso8601(date: CborDate): String = date.toString()

internal fun dateFromIso8601(value: String): CborDate {
    return try {
        CborDate.fromString(value)
    } catch (e: Exception) {
        throw ProvenanceMarkException.InvalidDate(e.message ?: "cannot parse ISO 8601 date string")
    }
}

fun parseSeed(value: String): Result<ProvenanceSeed> = runCatching {
    ProvenanceSeed.fromBase64(value)
}

fun parseDate(value: String): Result<CborDate> = runCatching {
    dateFromIso8601(value)
}

internal fun anyToCbor(value: Any): Cbor = when (value) {
    is Cbor -> value
    is CborEncodable -> value.toCbor()
    is String -> value.toCbor()
    is Int -> value.toCbor()
    is Long -> value.toCbor()
    is UInt -> value.toCbor()
    is ULong -> value.toCbor()
    is Float -> value.toCbor()
    is Double -> value.toCbor()
    is Boolean -> value.toCbor()
    is ByteArray -> value.toCbor()
    else -> throw ProvenanceMarkException.Cbor("unsupported CBOR-encodable type: ${value::class.qualifiedName}")
}

internal fun anyToCborData(value: Any): ByteArray = anyToCbor(value).toCborData()

internal fun queryValue(url: URL, key: String): String? {
    val query = url.query ?: return null
    for (pair in query.split("&")) {
        if (pair.isEmpty()) continue
        val parts = pair.split("=", limit = 2)
        val k = URLDecoder.decode(parts[0], StandardCharsets.UTF_8)
        if (k == key) {
            return if (parts.size == 1) "" else URLDecoder.decode(parts[1], StandardCharsets.UTF_8)
        }
    }
    return null
}
