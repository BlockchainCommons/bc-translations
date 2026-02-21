package com.blockchaincommons.bclifehash

import com.fasterxml.jackson.databind.PropertyNamingStrategies
import com.fasterxml.jackson.module.kotlin.jacksonObjectMapper
import com.fasterxml.jackson.module.kotlin.readValue
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.fail

private data class TestVector(
    val input: String,
    val inputType: String,
    val version: String,
    val moduleSize: Int,
    val hasAlpha: Boolean,
    val width: Int,
    val height: Int,
    val colors: List<Int>,
)

private fun parseVersion(value: String): Version =
    when (value) {
        "version1" -> Version.Version1
        "version2" -> Version.Version2
        "detailed" -> Version.Detailed
        "fiducial" -> Version.Fiducial
        "grayscale_fiducial" -> Version.GrayscaleFiducial
        else -> error("Unknown version: $value")
    }

private fun decodeHex(hex: String): ByteArray {
    require(hex.length % 2 == 0) { "Hex string must have even length" }
    val bytes = ByteArray(hex.length / 2)
    for (i in bytes.indices) {
        val start = i * 2
        val byteValue = hex.substring(start, start + 2).toInt(16)
        bytes[i] = byteValue.toByte()
    }
    return bytes
}

private val mapper = jacksonObjectMapper().apply {
    propertyNamingStrategy = PropertyNamingStrategies.SNAKE_CASE
}

class TestVectorsTest {
    @Test
    fun testAllVectors() {
        val json =
            checkNotNull(this::class.java.getResourceAsStream("/test-vectors.json")) {
                "Missing test-vectors.json in test resources"
            }.bufferedReader().use { it.readText() }

        val vectors: List<TestVector> = mapper.readValue(json)
        assertEquals(35, vectors.size, "Expected 35 test vectors")

        for ((index, vector) in vectors.withIndex()) {
            val version = parseVersion(vector.version)
            val image =
                if (vector.inputType == "hex") {
                    if (vector.input.isEmpty()) {
                        LifeHash.fromData(byteArrayOf(), version, vector.moduleSize, vector.hasAlpha)
                    } else {
                        LifeHash.fromData(decodeHex(vector.input), version, vector.moduleSize, vector.hasAlpha)
                    }
                } else {
                    LifeHash.fromUtf8(vector.input, version, vector.moduleSize, vector.hasAlpha)
                }

            assertEquals(
                vector.width,
                image.width,
                "Vector $index: width mismatch for input=${vector.input} version=${vector.version}",
            )
            assertEquals(
                vector.height,
                image.height,
                "Vector $index: height mismatch for input=${vector.input} version=${vector.version}",
            )
            assertEquals(
                vector.colors.size,
                image.colors.size,
                "Vector $index: colors length mismatch for input=${vector.input} version=${vector.version}",
            )

            if (vector.colors.size != image.colors.size) {
                continue
            }

            val components = if (vector.hasAlpha) 4 else 3
            for (byteIndex in image.colors.indices) {
                val got = image.colors[byteIndex].toUByte().toInt()
                val expected = vector.colors[byteIndex]
                if (got != expected) {
                    val pixel = byteIndex / components
                    val component = byteIndex % components
                    val componentName = arrayOf("R", "G", "B", "A")[component]
                    fail(
                        "Vector $index: pixel data mismatch for input=${vector.input} version=${vector.version}\n" +
                            "First diff at byte $byteIndex (pixel $pixel, $componentName): got $got, expected $expected",
                    )
                }
            }
        }
    }
}
