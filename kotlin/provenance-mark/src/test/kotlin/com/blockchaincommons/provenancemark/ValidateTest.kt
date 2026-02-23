package com.blockchaincommons.provenancemark

import com.blockchaincommons.dcbor.CborDate
import com.fasterxml.jackson.databind.JsonNode
import java.time.ZoneOffset
import java.time.ZonedDateTime
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFails
import kotlin.test.assertTrue

class ValidateTest {
    private val expected = loadJsonResource("/validate_expected.json")

    @Test
    fun testValidateEmpty() {
        val report = ProvenanceMark.validate(emptyList())
        assertExpectedFormats("test_validate_empty", report)
    }

    @Test
    fun testValidateSingleMark() {
        val marks = createTestMarks(1, ProvenanceMarkResolution.Low, "test")
        val report = ProvenanceMark.validate(marks)
        assertExpectedFormats("test_validate_single_mark", report)
    }

    @Test
    fun testValidateValidSequence() {
        val marks = createTestMarks(5, ProvenanceMarkResolution.Low, "test")
        val report = ProvenanceMark.validate(marks)
        assertExpectedFormats("test_validate_valid_sequence", report)
    }

    @Test
    fun testValidateDeduplication() {
        val marks = createTestMarks(3, ProvenanceMarkResolution.Low, "test")
        val marksWithDuplicates = listOf(
            marks[0],
            marks[1],
            marks[2],
            marks[0],
            marks[1],
            marks[0],
        )

        val report = ProvenanceMark.validate(marksWithDuplicates)
        assertExpectedFormats("test_validate_deduplication", report)
    }

    @Test
    fun testValidateMultipleChains() {
        val marks1 = createTestMarks(3, ProvenanceMarkResolution.Low, "alice")
        val marks2 = createTestMarks(3, ProvenanceMarkResolution.Low, "bob")
        val allMarks = marks1 + marks2

        val report = ProvenanceMark.validate(allMarks)
        assertExpectedFormats("test_validate_multiple_chains", report)
    }

    @Test
    fun testValidateMissingGenesis() {
        val marks = createTestMarks(5, ProvenanceMarkResolution.Low, "test")
        val marksWithoutGenesis = marks.drop(1)

        val report = ProvenanceMark.validate(marksWithoutGenesis)
        assertExpectedFormats("test_validate_missing_genesis", report)
    }

    @Test
    fun testValidateSequenceGap() {
        val marks = createTestMarks(5, ProvenanceMarkResolution.Low, "test")
        val marksWithGap = listOf(
            marks[0],
            marks[1],
            marks[3],
            marks[4],
        )

        val report = ProvenanceMark.validate(marksWithGap)
        assertExpectedFormats("test_validate_sequence_gap", report)
    }

    @Test
    fun testValidateOutOfOrder() {
        val marks = createTestMarks(5, ProvenanceMarkResolution.Low, "test")
        val marksOutOfOrder = listOf(
            marks[0],
            marks[1],
            marks[3],
            marks[2],
            marks[4],
        )

        val report = ProvenanceMark.validate(marksOutOfOrder)
        assertExpectedFormats("test_validate_out_of_order", report)
    }

    @Test
    fun testValidateHashMismatch() {
        registerTags()

        val marks = createTestMarks(3, ProvenanceMarkResolution.Low, "test")
        val mark0 = marks[0]
        val mark1 = marks[1]

        val date = CborDate.fromYmdHms(2023, 6, 22, 12, 0, 0)
        val badMark = ProvenanceMark.new(
            res = mark1.res(),
            key = mark1.key(),
            nextKey = mark0.hash(),
            chainId = mark1.chainId(),
            seq = 2u,
            date = date,
            info = null,
        )

        val report = ProvenanceMark.validate(listOf(mark0, mark1, badMark))
        assertExpectedFormats("test_validate_hash_mismatch", report)
    }

    @Test
    fun testValidateDateOrderingViolation() {
        val marks = createTestMarks(3, ProvenanceMarkResolution.Low, "test")
        val report = ProvenanceMark.validate(marks)
        assertExpectedFormats("test_validate_date_ordering_violation", report)
    }

    @Test
    fun testValidateMultipleSequencesInChain() {
        val marks = createTestMarks(7, ProvenanceMarkResolution.Low, "test")
        val marksWithGaps = listOf(
            marks[0],
            marks[1],
            marks[3],
            marks[4],
            marks[6],
        )

        val report = ProvenanceMark.validate(marksWithGaps)
        assertExpectedFormats("test_validate_multiple_sequences_in_chain", report)
    }

    @Test
    fun testValidatePrecedesOpt() {
        val marks = createTestMarks(3, ProvenanceMarkResolution.Low, "test")

        marks[0].precedesOpt(marks[1])
        marks[1].precedesOpt(marks[2])

        assertFails { marks[1].precedesOpt(marks[0]) }
        assertFails { marks[0].precedesOpt(marks[2]) }
    }

    @Test
    fun testValidateChainIdHex() {
        val marks = createTestMarks(2, ProvenanceMarkResolution.Low, "test")
        val report = ProvenanceMark.validate(marks)

        val chain = report.chains()[0]
        val chainIdHex = chain.chainIdHex()

        assertTrue(chainIdHex.all { it.isDigit() || it.lowercaseChar() in 'a'..'f' })
        assertEquals(marks[0].chainId().toHex(), chainIdHex)
    }

    @Test
    fun testValidateWithInfo() {
        registerTags()

        val generator = ProvenanceMarkGenerator.newWithPassphrase(ProvenanceMarkResolution.Low, "test")
        val baseInstant = ZonedDateTime.of(2023, 6, 20, 12, 0, 0, 0, ZoneOffset.UTC).toInstant()

        val marks = (0 until 3).map { dayOffset ->
            val date = CborDate.fromInstant(baseInstant.plusSeconds(dayOffset.toLong() * 24L * 60L * 60L))
            generator.next(date, "Test info")
        }

        val report = ProvenanceMark.validate(marks)
        assertExpectedFormats("test_validate_with_info", report)
    }

    @Test
    fun testValidateSortedChains() {
        val marks1 = createTestMarks(2, ProvenanceMarkResolution.Low, "zebra")
        val marks2 = createTestMarks(2, ProvenanceMarkResolution.Low, "apple")
        val marks3 = createTestMarks(2, ProvenanceMarkResolution.Low, "middle")

        val allMarks = marks1 + marks2 + marks3
        val report = ProvenanceMark.validate(allMarks)
        assertExpectedFormats("test_validate_sorted_chains", report)
    }

    @Test
    fun testValidateGenesisCheck() {
        val marks = createTestMarks(3, ProvenanceMarkResolution.Low, "test")

        val reportWithGenesis = ProvenanceMark.validate(marks)
        assertExpectedFormats("test_validate_date_ordering_violation", reportWithGenesis)

        val marksWithoutGenesis = marks.drop(1)
        val reportWithoutGenesis = ProvenanceMark.validate(marksWithoutGenesis)
        assertExpectedFormats("test_validate_genesis_check", reportWithoutGenesis)
    }

    @Test
    fun testValidateDateOrderingViolationConstructed() {
        registerTags()

        val marks = createTestMarks(2, ProvenanceMarkResolution.Low, "test")
        val mark0 = marks[0]

        val generator = ProvenanceMarkGenerator.newWithPassphrase(ProvenanceMarkResolution.Low, "test")
        generator.next(mark0.date(), null)

        val earlierDate = CborDate.fromYmdHms(2023, 6, 19, 12, 0, 0)
        val badMark = generator.next(earlierDate, null)

        val report = ProvenanceMark.validate(listOf(mark0, badMark))
        assertExpectedFormats("test_validate_date_ordering_violation_constructed", report)
    }

    @Test
    fun testValidateNonGenesisAtSeqZero() {
        registerTags()

        val marks = createTestMarks(2, ProvenanceMarkResolution.Low, "test")
        val mark0 = marks[0]
        val mark1 = marks[1]

        val date = CborDate.fromYmdHms(2023, 6, 21, 12, 0, 0)
        val badMark = ProvenanceMark.new(
            res = mark1.res(),
            key = mark1.key(),
            nextKey = mark1.hash(),
            chainId = mark1.chainId(),
            seq = 0u,
            date = date,
            info = null,
        )

        val report = ProvenanceMark.validate(listOf(mark0, badMark))
        assertExpectedFormats("test_validate_non_genesis_at_seq_zero", report)
    }

    @Test
    fun testValidateInvalidGenesisKeyConstructed() {
        registerTags()

        val marks = createTestMarks(2, ProvenanceMarkResolution.Low, "test")
        val mark0 = marks[0]
        val mark1 = marks[1]

        val date = CborDate.fromYmdHms(2023, 6, 21, 12, 0, 0)
        val badMark = ProvenanceMark.new(
            res = mark1.res(),
            key = mark1.chainId(),
            nextKey = mark1.hash(),
            chainId = mark1.chainId(),
            seq = 1u,
            date = date,
            info = null,
        )

        val report = ProvenanceMark.validate(listOf(mark0, badMark))
        assertExpectedFormats("test_validate_invalid_genesis_key_constructed", report)
    }

    private fun createTestMarks(
        count: Int,
        resolution: ProvenanceMarkResolution,
        passphrase: String,
    ): List<ProvenanceMark> {
        registerTags()

        val generator = ProvenanceMarkGenerator.newWithPassphrase(resolution, passphrase)
        val baseInstant = ZonedDateTime.of(2023, 6, 20, 12, 0, 0, 0, ZoneOffset.UTC).toInstant()

        return (0 until count).map { dayOffset ->
            val date = CborDate.fromInstant(baseInstant.plusSeconds(dayOffset.toLong() * 24L * 60L * 60L))
            generator.next(date, null)
        }
    }

    private fun assertExpectedFormats(name: String, report: ValidationReport) {
        val case = expected.path(name)
        require(!case.isMissingNode) { "missing expected case: $name" }

        if (case.has("json_pretty")) {
            val expectedPretty = normalizeExpectedPrettyJson(case.path("json_pretty").asText())
            val actualPretty = report.format(ValidationReportFormat.JsonPretty)
            assertActualExpected(actualPretty, expectedPretty)
        }

        if (case.has("json_compact")) {
            val expectedCompact = normalizeBlock(case.path("json_compact").asText())
            val actualCompact = normalizeBlock(report.format(ValidationReportFormat.JsonCompact))
            assertActualExpected(actualCompact, expectedCompact)
        }

        if (case.has("text")) {
            val expectedText = normalizeText(case.path("text").asText())
            val actualText = normalizeText(report.format(ValidationReportFormat.Text))
            assertActualExpected(actualText, expectedText)
        }
    }

    private fun normalizeText(value: String): String {
        return normalizeBlock(value)
            .lines()
            .map { it.trimEnd() }
            .joinToString("\n")
            .trim()
    }
}
