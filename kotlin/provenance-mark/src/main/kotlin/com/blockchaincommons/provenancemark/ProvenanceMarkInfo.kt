package com.blockchaincommons.provenancemark

import com.blockchaincommons.bcur.UR

class ProvenanceMarkInfo private constructor(
    private val ur: UR,
    private val bytewords: String,
    private val bytemoji: String,
    private val comment: String,
    private val mark: ProvenanceMark,
) {
    fun mark(): ProvenanceMark = mark

    fun ur(): UR = ur

    fun bytewords(): String = bytewords

    fun bytemoji(): String = bytemoji

    fun comment(): String = comment

    fun markdownSummary(): String {
        val lines = mutableListOf<String>()
        lines.add("---")

        lines.add("")
        lines.add(mark.date().toString())

        lines.add("")
        lines.add("#### $ur")

        lines.add("")
        lines.add("#### `$bytewords`")

        lines.add("")
        lines.add(bytemoji)

        lines.add("")
        if (comment.isNotEmpty()) {
            lines.add(comment)
            lines.add("")
        }

        return lines.joinToString("\n")
    }

    fun toJson(): String {
        val fields = linkedMapOf<String, Any>(
            "ur" to ur.toString(),
            "bytewords" to bytewords,
            "bytemoji" to bytemoji,
            "mark" to JsonSupport.mapper.readTree(mark.toJson()),
        )
        if (comment.isNotEmpty()) {
            fields["comment"] = comment
        }
        return JsonSupport.mapper.writeValueAsString(fields)
    }

    companion object {
        fun new(mark: ProvenanceMark, comment: String): ProvenanceMarkInfo {
            val ur = mark.ur()
            val bytewords = mark.bytewordsIdentifier(prefix = true)
            val bytemoji = mark.bytemojiIdentifier(prefix = true)
            return ProvenanceMarkInfo(ur, bytewords, bytemoji, comment, mark)
        }

        fun fromJson(json: String): ProvenanceMarkInfo {
            val node = JsonSupport.mapper.readTree(json)
            val ur = UR.fromUrString(node.path("ur").asText())
            val bytewords = node.path("bytewords").asText()
            val bytemoji = node.path("bytemoji").asText()
            val comment = if (node.has("comment")) node.path("comment").asText() else ""

            val mark = ProvenanceMark.fromUr(ur)
            return ProvenanceMarkInfo(
                ur = ur,
                bytewords = bytewords,
                bytemoji = bytemoji,
                comment = comment,
                mark = mark,
            )
        }
    }
}
