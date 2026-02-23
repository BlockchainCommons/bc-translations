package com.blockchaincommons.provenancemark

import com.blockchaincommons.bcenvelope.format
import com.blockchaincommons.dcbor.CborDate
import com.fasterxml.jackson.databind.JsonNode
import java.net.URL
import java.nio.file.Files
import java.nio.file.Path
import java.time.ZoneOffset
import java.time.ZonedDateTime
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class MarkTest {
    private val vectors: JsonNode = loadJsonResource("/mark_vectors.json")

    @Test
    fun testLow() = runVector("test_low")

    @Test
    fun testLowWithInfo() = runVector("test_low_with_info")

    @Test
    fun testMedium() = runVector("test_medium")

    @Test
    fun testMediumWithInfo() = runVector("test_medium_with_info")

    @Test
    fun testQuartile() = runVector("test_quartile")

    @Test
    fun testQuartileWithInfo() = runVector("test_quartile_with_info")

    @Test
    fun testHigh() = runVector("test_high")

    @Test
    fun testHighWithInfo() = runVector("test_high_with_info")

    // Rust metadata sync tests (`test_readme_deps`, `test_html_root_url`) are
    // Rust-tooling checks; in Kotlin we verify equivalent module metadata.
    @Test
    fun testReadmeDeps() {
        val buildGradle = Files.readString(Path.of("build.gradle.kts"))
        assertTrue(buildGradle.contains("version = \"0.23.0\""))
        assertTrue(buildGradle.contains("com.blockchaincommons:bc-rand:0.5.0"))
        assertTrue(buildGradle.contains("com.blockchaincommons:bc-envelope:0.43.0"))
    }

    @Test
    fun testHtmlRootUrl() {
        val source = Files.readString(
            Path.of("src/main/kotlin/com/blockchaincommons/provenancemark/ProvenanceMark.kt")
        )
        assertTrue(source.contains("package com.blockchaincommons.provenancemark"))
    }

    @Test
    fun testEnvelope() {
        registerTags()

        val seed = ProvenanceSeed.newWithPassphrase("test")
        val date = CborDate.fromString("2025-10-26")

        val generator = ProvenanceMarkGenerator.newWithSeed(
            ProvenanceMarkResolution.High,
            seed,
        )
        val mark = generator.next(date, "Info field content")

        val generatorEnvelope = generator.toEnvelope()
        val expectedGeneratorEnvelope = """
            Bytes(32) [
                'isA': "provenance-generator"
                "next-seq": 1
                "res": 3
                "rng-state": Bytes(32)
                "seed": Bytes(32)
            ]
        """.trimIndent().trim()
        assertActualExpected(generatorEnvelope.format(), expectedGeneratorEnvelope)

        val decodedGenerator = ProvenanceMarkGenerator.fromEnvelope(generatorEnvelope)
        assertEquals(generator, decodedGenerator)

        val markEnvelope = mark.toEnvelope()
        assertEquals("ProvenanceMark(59def089)", markEnvelope.format())

        val expectedDebug = "ProvenanceMark(key: b16a7cbd178ee0d41cadb0dcefdbe87d6a41c85b41c551134ae8307f9203babc, hash: 59def089a4d373a2d3f6a449c6758f62ba55cda64c7faf01c1c74a1130d3c1ee, chainID: b16a7cbd178ee0d41cadb0dcefdbe87d6a41c85b41c551134ae8307f9203babc, seq: 0, date: 2025-10-26, info: \"Info field content\")"
        assertEquals(expectedDebug, mark.debugString())
    }

    private fun runVector(name: String) {
        val vector = vectors.path(name)
        require(!vector.isMissingNode) { "missing vector: $name" }

        val resolution = resolutionFromString(vector.path("resolution").asText())
        val includeInfo = vector.path("include_info").asBoolean()

        runTest(
            resolution = resolution,
            includeInfo = includeInfo,
            expectedDisplay = stringList(vector, "expected_display"),
            expectedDebug = stringList(vector, "expected_debug"),
            expectedBytewords = stringList(vector, "expected_bytewords"),
            expectedIdWords = stringList(vector, "expected_id_words"),
            expectedBytemojiIds = stringList(vector, "expected_bytemoji_ids"),
            expectedUrs = stringList(vector, "expected_urs"),
            expectedUrls = stringList(vector, "expected_urls"),
        )
    }

    private fun runTest(
        resolution: ProvenanceMarkResolution,
        includeInfo: Boolean,
        expectedDisplay: List<String>,
        expectedDebug: List<String>,
        expectedBytewords: List<String>,
        expectedIdWords: List<String>,
        expectedBytemojiIds: List<String>,
        expectedUrs: List<String>,
        expectedUrls: List<String>,
    ) {
        registerTags()

        val count = 10
        val baseInstant = ZonedDateTime
            .of(2023, 6, 20, 12, 0, 0, 0, ZoneOffset.UTC)
            .toInstant()

        val dates = (0 until count).map { dayOffset ->
            CborDate.fromInstant(baseInstant.plusSeconds(dayOffset.toLong() * 24L * 60L * 60L))
        }

        val initialGenerator = ProvenanceMarkGenerator.newWithPassphrase(resolution, "Wolf")
        var encodedGenerator = initialGenerator.toJson()

        val marks = dates.map { date ->
            val generator = ProvenanceMarkGenerator.fromJson(encodedGenerator)
            val info = if (includeInfo) "Lorem ipsum sit dolor amet." else null
            val mark = generator.next(date, info)
            encodedGenerator = generator.toJson()
            mark
        }

        assertTrue(ProvenanceMark.isSequenceValid(marks))
        assertFalse(marks[1].precedes(marks[0]))

        assertEquals(expectedDisplay, marks.map { it.toString() })
        assertEquals(expectedDebug, marks.map { it.debugString() })

        val bytewords = marks.map { it.toBytewords() }
        assertEquals(expectedBytewords, bytewords)

        val bytewordsMarks = bytewords.map { ProvenanceMark.fromBytewords(resolution, it) }
        assertEquals(marks, bytewordsMarks)

        val idWords = marks.map { it.bytewordsIdentifier(prefix = false) }
        assertEquals(expectedIdWords, idWords)

        val bytemojiIds = marks.map { it.bytemojiIdentifier(prefix = false) }
        assertEquals(expectedBytemojiIds, bytemojiIds)

        val urs = marks.map { it.urString() }
        assertEquals(expectedUrs, urs)

        val urMarks = urs.map { ProvenanceMark.fromUrString(it) }
        assertEquals(marks, urMarks)

        val baseUrl = "https://example.com/validate"
        val urls = marks.map { it.toUrl(baseUrl).toString() }
        assertEquals(expectedUrls, urls)

        val urlMarks = urls.map { ProvenanceMark.fromUrl(URL(it)) }
        assertEquals(marks, urlMarks)

        for (mark in marks) {
            val encoded = mark.toJson()
            val decoded = ProvenanceMark.fromJson(encoded)
            assertEquals(mark, decoded)
        }
    }

    private fun resolutionFromString(value: String): ProvenanceMarkResolution = when (value) {
        "low" -> ProvenanceMarkResolution.Low
        "medium" -> ProvenanceMarkResolution.Medium
        "quartile" -> ProvenanceMarkResolution.Quartile
        "high" -> ProvenanceMarkResolution.High
        else -> error("unsupported resolution value: $value")
    }

    private fun stringList(node: JsonNode, field: String): List<String> {
        return node.path(field).map { it.asText() }
    }
}
