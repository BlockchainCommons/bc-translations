package com.blockchaincommons.dcbor

/**
 * Options for CBOR diagnostic notation formatting.
 */
data class DiagFormatOpts(
    val annotate: Boolean = false,
    val summarize: Boolean = false,
    val flat: Boolean = false,
    val tags: TagsStoreOpt = TagsStoreOpt.Global
) {
    fun withAnnotate(value: Boolean) = copy(annotate = value)
    fun withSummarize(value: Boolean) = copy(summarize = value, flat = if (value) true else flat)
    fun withFlat(value: Boolean) = copy(flat = value)
    fun withTags(value: TagsStoreOpt) = copy(tags = value)
}

// Diagnostic formatting extensions for Cbor
fun Cbor.diagnosticOpt(opts: DiagFormatOpts): String {
    return diagItem(opts).format(opts)
}

private fun Cbor.diagItem(opts: DiagFormatOpts): DiagItem {
    return when (val case = cborCase) {
        is CborCase.Unsigned, is CborCase.Negative,
        is CborCase.CborByteString, is CborCase.Text,
        is CborCase.CborSimple -> DiagItem.Item(diagnosticFlat)

        is CborCase.Array -> {
            val items = case.value.map { it.diagItem(opts) }
            DiagItem.Group("[", "]", items, isPairs = false, comment = null)
        }

        is CborCase.CborMap -> {
            val items = case.value.toList().flatMap { (key, value) ->
                listOf(key.diagItem(opts), value.diagItem(opts))
            }
            DiagItem.Group("{", "}", items, isPairs = true, comment = null)
        }

        is CborCase.Tagged -> {
            if (opts.summarize) {
                val summarizer = when (opts.tags) {
                    is TagsStoreOpt.Custom -> opts.tags.store.summarizer(case.tag.value)
                    is TagsStoreOpt.Global -> GlobalTags.withTags { it.summarizer(case.tag.value) }
                    is TagsStoreOpt.None -> null
                }
                if (summarizer != null) {
                    try {
                        val summaryText = summarizer(case.item, opts.flat)
                        return DiagItem.Item(summaryText)
                    } catch (e: Exception) {
                        return DiagItem.Item("<error: ${e.message}>")
                    }
                }
            }

            val comment = if (opts.annotate) {
                when (opts.tags) {
                    is TagsStoreOpt.None -> null
                    is TagsStoreOpt.Custom -> opts.tags.store.assignedNameForTag(case.tag)
                    is TagsStoreOpt.Global -> GlobalTags.withTags { it.assignedNameForTag(case.tag) }
                }
            } else null

            val diagItem = case.item.diagItem(opts)
            DiagItem.Group("${case.tag.value}(", ")", listOf(diagItem), isPairs = false, comment = comment)
        }
    }
}

private sealed class DiagItem {
    data class Item(val string: String) : DiagItem()
    data class Group(
        val begin: String,
        val end: String,
        val items: List<DiagItem>,
        val isPairs: Boolean,
        val comment: String?
    ) : DiagItem()

    fun format(opts: DiagFormatOpts): String = formatOpt(0, "", opts)

    fun formatOpt(level: Int, separator: String, opts: DiagFormatOpts): String {
        return when (this) {
            is Item -> formatLine(level, opts, string, separator, null)
            is Group -> {
                if (!opts.flat && (containsGroup() || totalStringsLen() > 20 || greatestStringsLen() > 20)) {
                    multilineComposition(level, separator, opts)
                } else {
                    singleLineComposition(level, separator, opts)
                }
            }
        }
    }

    private fun formatLine(level: Int, opts: DiagFormatOpts, string: String, separator: String, comment: String?): String {
        val indent = if (opts.flat) "" else " ".repeat(level * 4)
        val result = "$indent$string$separator"
        return if (comment != null) "$result   / $comment /" else result
    }

    private fun singleLineComposition(level: Int, separator: String, opts: DiagFormatOpts): String {
        return when (this) {
            is Item -> formatLine(level, opts, string, separator, null)
            is Group -> {
                val components = items.map { item ->
                    when (item) {
                        is Item -> item.string
                        is Group -> item.singleLineComposition(level + 1, separator, opts)
                    }
                }
                val pairSeparator = if (isPairs) ": " else ", "
                val joined = joinElements(components, ", ", pairSeparator)
                val string = "$begin$joined$end"
                formatLine(level, opts, string, separator, comment)
            }
        }
    }

    private fun multilineComposition(level: Int, separator: String, opts: DiagFormatOpts): String {
        return when (this) {
            is Item -> string
            is Group -> {
                val lines = mutableListOf<String>()
                lines.add(formatLine(level, opts.copy(flat = false), begin, "", comment))
                for ((index, item) in items.withIndex()) {
                    val sep = when {
                        index == items.size - 1 -> ""
                        isPairs && index % 2 == 0 -> ":"
                        else -> ","
                    }
                    lines.add(item.formatOpt(level + 1, sep, opts))
                }
                lines.add(formatLine(level, opts, end, separator, null))
                lines.joinToString("\n")
            }
        }
    }

    fun totalStringsLen(): Int = when (this) {
        is Item -> string.length
        is Group -> items.fold(0) { acc, item -> acc + item.totalStringsLen() }
    }

    fun greatestStringsLen(): Int = when (this) {
        is Item -> string.length
        is Group -> items.fold(0) { acc, item -> maxOf(acc, item.totalStringsLen()) }
    }

    fun isGroup(): Boolean = this is Group

    fun containsGroup(): Boolean = when (this) {
        is Item -> false
        is Group -> items.any { it.isGroup() }
    }

    companion object {
        fun joinElements(elements: List<String>, itemSeparator: String, pairSeparator: String): String {
            val sb = StringBuilder()
            for ((index, item) in elements.withIndex()) {
                sb.append(item)
                if (index != elements.size - 1) {
                    if (index % 2 != 0) {
                        sb.append(itemSeparator)
                    } else {
                        sb.append(pairSeparator)
                    }
                }
            }
            return sb.toString()
        }
    }
}
