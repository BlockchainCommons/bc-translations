package com.blockchaincommons.provenancemark

import com.blockchaincommons.dcbor.CborDate

enum class ValidationReportFormat {
    Text,
    JsonCompact,
    JsonPretty,
}

sealed class ValidationIssue {
    class HashMismatch(expected: ByteArray, actual: ByteArray) : ValidationIssue() {
        val expected: ByteArray = expected.copyOf()
        val actual: ByteArray = actual.copyOf()

        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is HashMismatch) return false
            return expected.contentEquals(other.expected) && actual.contentEquals(other.actual)
        }

        override fun hashCode(): Int {
            var result = expected.contentHashCode()
            result = 31 * result + actual.contentHashCode()
            return result
        }

        override fun toString(): String {
            return "hash mismatch: expected ${expected.toHex()}, got ${actual.toHex()}"
        }
    }

    data object KeyMismatch : ValidationIssue() {
        override fun toString(): String =
            "key mismatch: current hash was not generated from next key"
    }

    data class SequenceGap(val expected: UInt, val actual: UInt) : ValidationIssue() {
        override fun toString(): String =
            "sequence number gap: expected $expected, got $actual"
    }

    data class DateOrdering(val previous: CborDate, val next: CborDate) : ValidationIssue() {
        override fun toString(): String =
            "date must be equal or later: previous is $previous, next is $next"
    }

    data object NonGenesisAtZero : ValidationIssue() {
        override fun toString(): String = "non-genesis mark at sequence 0"
    }

    data object InvalidGenesisKey : ValidationIssue() {
        override fun toString(): String = "genesis mark must have key equal to chain_id"
    }
}

class FlaggedMark private constructor(
    private val mark: ProvenanceMark,
    private val issues: List<ValidationIssue>,
) {
    fun mark(): ProvenanceMark = mark

    fun issues(): List<ValidationIssue> = issues.toList()

    companion object {
        fun new(mark: ProvenanceMark): FlaggedMark = FlaggedMark(mark, emptyList())

        fun withIssue(mark: ProvenanceMark, issue: ValidationIssue): FlaggedMark =
            FlaggedMark(mark, listOf(issue))
    }
}

class SequenceReport(
    private val startSeq: UInt,
    private val endSeq: UInt,
    private val marks: List<FlaggedMark>,
) {
    fun startSeq(): UInt = startSeq

    fun endSeq(): UInt = endSeq

    fun marks(): List<FlaggedMark> = marks.toList()
}

class ChainReport(
    private val chainId: ByteArray,
    private val hasGenesis: Boolean,
    private val marks: List<ProvenanceMark>,
    private val sequences: List<SequenceReport>,
) {
    fun chainId(): ByteArray = chainId.copyOf()

    fun hasGenesis(): Boolean = hasGenesis

    fun marks(): List<ProvenanceMark> = marks.toList()

    fun sequences(): List<SequenceReport> = sequences.toList()

    fun chainIdHex(): String = chainId.toHex()
}

class ValidationReport private constructor(
    private val marks: List<ProvenanceMark>,
    private val chains: List<ChainReport>,
) {
    fun marks(): List<ProvenanceMark> = marks.toList()

    fun chains(): List<ChainReport> = chains.toList()

    fun format(format: ValidationReportFormat): String {
        return when (format) {
            ValidationReportFormat.Text -> formatText()
            ValidationReportFormat.JsonCompact -> {
                try {
                    JsonSupport.mapper.writeValueAsString(toJsonMap())
                } catch (_: Exception) {
                    ""
                }
            }
            ValidationReportFormat.JsonPretty -> {
                try {
                    JsonSupport.mapper.writerWithDefaultPrettyPrinter().writeValueAsString(toJsonMap())
                } catch (_: Exception) {
                    ""
                }
            }
        }
    }

    fun hasIssues(): Boolean {
        for (chain in chains) {
            if (!chain.hasGenesis()) return true
        }

        for (chain in chains) {
            for (sequence in chain.sequences()) {
                for (mark in sequence.marks()) {
                    if (mark.issues().isNotEmpty()) return true
                }
            }
        }

        if (chains.size > 1) return true
        if (chains.size == 1 && chains[0].sequences().size > 1) return true

        return false
    }

    private fun formatText(): String {
        if (!isInteresting()) {
            return ""
        }

        val lines = mutableListOf<String>()
        lines.add("Total marks: ${marks.size}")
        lines.add("Chains: ${chains.size}")
        lines.add("")

        for ((index, chain) in chains.withIndex()) {
            val chainIdHex = chain.chainIdHex()
            val shortChainId = if (chainIdHex.length > 8) chainIdHex.substring(0, 8) else chainIdHex

            lines.add("Chain ${index + 1}: $shortChainId")

            if (!chain.hasGenesis()) {
                lines.add("  Warning: No genesis mark found")
            }

            for (sequence in chain.sequences()) {
                for (flaggedMark in sequence.marks()) {
                    val mark = flaggedMark.mark()
                    val shortId = mark.identifier()
                    val seqNum = mark.seq()

                    val annotations = mutableListOf<String>()
                    if (mark.isGenesis()) {
                        annotations.add("genesis mark")
                    }

                    for (issue in flaggedMark.issues()) {
                        val issueString = when (issue) {
                            is ValidationIssue.SequenceGap -> "gap: ${issue.expected} missing"
                            is ValidationIssue.DateOrdering -> "date ${issue.previous} < ${issue.next}"
                            is ValidationIssue.HashMismatch -> "hash mismatch"
                            ValidationIssue.KeyMismatch -> "key mismatch"
                            ValidationIssue.NonGenesisAtZero -> "non-genesis at seq 0"
                            ValidationIssue.InvalidGenesisKey -> "invalid genesis key"
                        }
                        annotations.add(issueString)
                    }

                    if (annotations.isEmpty()) {
                        lines.add("  $seqNum: $shortId")
                    } else {
                        lines.add("  $seqNum: $shortId (${annotations.joinToString(", ")})")
                    }
                }
            }

            lines.add("")
        }

        return lines.joinToString("\n").trimEnd()
    }

    private fun isInteresting(): Boolean {
        if (chains.isEmpty()) return false

        for (chain in chains) {
            if (!chain.hasGenesis()) return true
        }

        if (chains.size == 1) {
            val chain = chains[0]
            if (chain.sequences().size == 1) {
                val sequence = chain.sequences()[0]
                if (sequence.marks().all { it.issues().isEmpty() }) {
                    return false
                }
            }
        }

        return true
    }

    private fun toJsonMap(): Map<String, Any> {
        val root = linkedMapOf<String, Any>()
        root["marks"] = marks.map { it.urString() }
        root["chains"] = chains.map { chain ->
            val chainMap = linkedMapOf<String, Any>()
            chainMap["chain_id"] = chain.chainIdHex()
            chainMap["has_genesis"] = chain.hasGenesis()
            chainMap["marks"] = chain.marks().map { it.urString() }
            chainMap["sequences"] = chain.sequences().map { sequence ->
                val sequenceMap = linkedMapOf<String, Any>()
                sequenceMap["start_seq"] = sequence.startSeq().toLong()
                sequenceMap["end_seq"] = sequence.endSeq().toLong()
                sequenceMap["marks"] = sequence.marks().map { flagged ->
                    val flaggedMap = linkedMapOf<String, Any>()
                    flaggedMap["mark"] = flagged.mark().urString()
                    flaggedMap["issues"] = flagged.issues().map { issue -> issueToJsonMap(issue) }
                    flaggedMap
                }
                sequenceMap
            }
            chainMap
        }
        return root
    }

    private fun issueToJsonMap(issue: ValidationIssue): Map<String, Any> {
        return when (issue) {
            is ValidationIssue.HashMismatch -> {
                linkedMapOf(
                    "type" to "HashMismatch",
                    "data" to linkedMapOf(
                        "expected" to issue.expected.toHex(),
                        "actual" to issue.actual.toHex(),
                    ),
                )
            }
            ValidationIssue.KeyMismatch -> linkedMapOf("type" to "KeyMismatch")
            is ValidationIssue.SequenceGap -> {
                linkedMapOf(
                    "type" to "SequenceGap",
                    "data" to linkedMapOf(
                        "expected" to issue.expected.toLong(),
                        "actual" to issue.actual.toLong(),
                    ),
                )
            }
            is ValidationIssue.DateOrdering -> {
                linkedMapOf(
                    "type" to "DateOrdering",
                    "data" to linkedMapOf(
                        "previous" to dateToIso8601(issue.previous),
                        "next" to dateToIso8601(issue.next),
                    ),
                )
            }
            ValidationIssue.NonGenesisAtZero -> linkedMapOf("type" to "NonGenesisAtZero")
            ValidationIssue.InvalidGenesisKey -> linkedMapOf("type" to "InvalidGenesisKey")
        }
    }

    private class ChainBin(
        val chainId: ByteArray,
        val marks: MutableList<ProvenanceMark>,
    )

    companion object {
        fun validate(marks: List<ProvenanceMark>): ValidationReport {
            val seen = HashSet<ProvenanceMark>()
            val deduplicated = mutableListOf<ProvenanceMark>()
            for (mark in marks) {
                if (seen.add(mark)) {
                    deduplicated.add(mark)
                }
            }

            val chainBins = linkedMapOf<String, ChainBin>()
            for (mark in deduplicated) {
                val key = mark.chainId().toHex()
                val bin = chainBins.getOrPut(key) {
                    ChainBin(mark.chainId(), mutableListOf())
                }
                bin.marks.add(mark)
            }

            val chains = mutableListOf<ChainReport>()
            for ((_, bin) in chainBins) {
                val chainMarks = bin.marks.sortedBy { it.seq() }
                val hasGenesis = chainMarks.firstOrNull()?.let { it.seq() == 0u && it.isGenesis() } ?: false
                val sequences = buildSequenceBins(chainMarks)
                chains.add(
                    ChainReport(
                        chainId = bin.chainId,
                        hasGenesis = hasGenesis,
                        marks = chainMarks,
                        sequences = sequences,
                    )
                )
            }

            chains.sortBy { it.chainIdHex() }

            return ValidationReport(
                marks = deduplicated,
                chains = chains,
            )
        }

        private fun buildSequenceBins(marks: List<ProvenanceMark>): List<SequenceReport> {
            val sequences = mutableListOf<SequenceReport>()
            var currentSequence = mutableListOf<FlaggedMark>()

            for (index in marks.indices) {
                val mark = marks[index]
                if (index == 0) {
                    currentSequence.add(FlaggedMark.new(mark))
                } else {
                    val prev = marks[index - 1]
                    try {
                        prev.precedesOpt(mark)
                        currentSequence.add(FlaggedMark.new(mark))
                    } catch (e: Exception) {
                        if (currentSequence.isNotEmpty()) {
                            sequences.add(createSequenceReport(currentSequence))
                        }

                        val issue = if (e is ProvenanceMarkException.Validation) {
                            e.issue
                        } else {
                            ValidationIssue.KeyMismatch
                        }
                        currentSequence = mutableListOf(FlaggedMark.withIssue(mark, issue))
                    }
                }
            }

            if (currentSequence.isNotEmpty()) {
                sequences.add(createSequenceReport(currentSequence))
            }

            return sequences
        }

        private fun createSequenceReport(marks: List<FlaggedMark>): SequenceReport {
            val startSeq = marks.firstOrNull()?.mark()?.seq() ?: 0u
            val endSeq = marks.lastOrNull()?.mark()?.seq() ?: 0u
            return SequenceReport(
                startSeq = startSeq,
                endSeq = endSeq,
                marks = marks.toList(),
            )
        }
    }
}
