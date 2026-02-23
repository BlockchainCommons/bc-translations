package com.blockchaincommons.provenancemark

import com.fasterxml.jackson.databind.JsonNode
import com.fasterxml.jackson.module.kotlin.jacksonObjectMapper
import kotlin.test.fail

private val testMapper = jacksonObjectMapper().findAndRegisterModules()

fun loadJsonResource(path: String): JsonNode {
    val stream = requireNotNull(TestSupport::class.java.getResourceAsStream(path)) {
        "Missing test resource: $path"
    }
    stream.use {
        return testMapper.readTree(it)
    }
}

fun normalizeExpectedPrettyJson(expected: String): String {
    val normalized = normalizeBlock(expected)
    return testMapper.writerWithDefaultPrettyPrinter().writeValueAsString(testMapper.readTree(normalized))
}

fun normalizeBlock(value: String): String {
    val normalized = value.replace("\r\n", "\n")
    val lines = normalized.split("\n")
    val nonEmptyLines = lines.filter { it.isNotBlank() }
    if (nonEmptyLines.isEmpty()) {
        return ""
    }

    val firstNonEmptyLine = nonEmptyLines.first()
    val firstIndent = firstNonEmptyLine.takeWhile { it == ' ' }.length

    val baselineIndent = if (firstIndent == 0 && nonEmptyLines.size > 1) {
        val remaining = nonEmptyLines.drop(1).map { it.takeWhile { ch -> ch == ' ' }.length }
        remaining.minOrNull() ?: 0
    } else {
        nonEmptyLines.map { it.takeWhile { ch -> ch == ' ' }.length }.minOrNull() ?: 0
    }

    var firstNonEmptySeen = false
    return lines.joinToString("\n") { line ->
        if (line.isBlank()) {
            line
        } else if (!firstNonEmptySeen) {
            firstNonEmptySeen = true
            line
        } else {
            if (line.length >= baselineIndent) line.drop(baselineIndent) else line
        }
    }.trim()
}

fun assertActualExpected(actual: String, expected: String) {
    if (actual == expected) {
        return
    }

    fail(
        buildString {
            appendLine("actual and expected differ")
            appendLine("--- actual ---")
            appendLine(actual)
            appendLine("--- expected ---")
            appendLine(expected)
        }
    )
}

fun hex(value: String): ByteArray {
    require(value.length % 2 == 0) { "hex string length must be even" }
    return ByteArray(value.length / 2) { index ->
        val offset = index * 2
        value.substring(offset, offset + 2).toInt(16).toByte()
    }
}

private object TestSupport
