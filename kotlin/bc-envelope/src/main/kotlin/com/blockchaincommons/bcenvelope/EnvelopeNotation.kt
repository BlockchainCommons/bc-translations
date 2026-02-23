package com.blockchaincommons.bcenvelope

import com.blockchaincommons.dcbor.*
import com.blockchaincommons.knownvalues.IS_A
import com.blockchaincommons.knownvalues.KnownValue

/**
 * Options for formatting envelopes in envelope notation.
 */
class EnvelopeFormatOpts(
    val flat: Boolean = false,
    val context: FormatContextOpt = FormatContextOpt.Global,
)

/**
 * The item types produced during envelope notation formatting.
 *
 * An [EnvelopeFormatItem] is a recursive tree that can be flattened
 * and rendered to text in either flat or hierarchical mode.
 */
sealed class EnvelopeFormatItem : Comparable<EnvelopeFormatItem> {
    data class Begin(val delimiter: String) : EnvelopeFormatItem()
    data class End(val delimiter: String) : EnvelopeFormatItem()
    data class Item(val text: String) : EnvelopeFormatItem()
    data object Separator : EnvelopeFormatItem()
    data class ListItems(val items: List<EnvelopeFormatItem>) : EnvelopeFormatItem()

    private fun index(): Int = when (this) {
        is Begin -> 1
        is End -> 2
        is Item -> 3
        is Separator -> 4
        is ListItems -> 5
    }

    fun flatten(): List<EnvelopeFormatItem> = when (this) {
        is ListItems -> items.flatMap { it.flatten() }
        else -> listOf(this)
    }

    fun format(opts: EnvelopeFormatOpts): String =
        if (opts.flat) formatFlat() else formatHierarchical()

    fun formatFlat(): String {
        val sb = StringBuilder()
        val items = flatten()
        for (item in items) {
            when (item) {
                is Begin -> {
                    if (!sb.endsWith(' ')) sb.append(' ')
                    sb.append(item.delimiter)
                    sb.append(' ')
                }
                is End -> {
                    if (!sb.endsWith(' ')) sb.append(' ')
                    sb.append(item.delimiter)
                    sb.append(' ')
                }
                is Item -> sb.append(item.text)
                is Separator -> {
                    val trimmed = sb.toString().trimEnd()
                    sb.clear()
                    sb.append(trimmed)
                    sb.append(", ")
                }
                is ListItems -> {
                    for (child in item.items) {
                        sb.append(child.formatFlat())
                    }
                }
            }
        }
        return sb.toString()
    }

    fun formatHierarchical(): String {
        val lines = mutableListOf<String>()
        var level = 0
        var currentLine = ""
        val items = nicen(flatten())
        for (item in items) {
            when (item) {
                is Begin -> {
                    if (item.delimiter.isNotEmpty()) {
                        val c = if (currentLine.isEmpty()) {
                            item.delimiter
                        } else {
                            addSpaceAtEndIfNeeded(currentLine) + item.delimiter
                        }
                        lines.add(indent(level) + c + "\n")
                    }
                    level++
                    currentLine = ""
                }
                is End -> {
                    if (currentLine.isNotEmpty()) {
                        lines.add(indent(level) + currentLine + "\n")
                        currentLine = ""
                    }
                    level--
                    lines.add(indent(level) + item.delimiter + "\n")
                }
                is Item -> {
                    currentLine += item.text
                }
                is Separator -> {
                    if (currentLine.isNotEmpty()) {
                        lines.add(indent(level) + currentLine + "\n")
                        currentLine = ""
                    }
                }
                is ListItems -> {
                    lines.add("<list>")
                }
            }
        }
        if (currentLine.isNotEmpty()) {
            lines.add(currentLine)
        }
        return lines.joinToString("")
    }

    override fun compareTo(other: EnvelopeFormatItem): Int {
        val indexCmp = index().compareTo(other.index())
        if (indexCmp != 0) return indexCmp
        return when {
            this is Begin && other is Begin -> delimiter.compareTo(other.delimiter)
            this is End && other is End -> delimiter.compareTo(other.delimiter)
            this is Item && other is Item -> text.compareTo(other.text)
            this is Separator && other is Separator -> 0
            this is ListItems && other is ListItems -> items.compareTo(other.items)
            else -> 0
        }
    }

    override fun toString(): String = when (this) {
        is Begin -> ".begin($delimiter)"
        is End -> ".end($delimiter)"
        is Item -> ".item($text)"
        is Separator -> ".separator"
        is ListItems -> ".list($items)"
    }

    companion object {
        fun indent(level: Int): String = " ".repeat(level * 4)

        fun addSpaceAtEndIfNeeded(s: String): String = when {
            s.isEmpty() -> " "
            s.endsWith(' ') -> s
            else -> "$s "
        }

        fun nicen(items: List<EnvelopeFormatItem>): List<EnvelopeFormatItem> {
            val input = items.toMutableList()
            val result = mutableListOf<EnvelopeFormatItem>()
            while (input.isNotEmpty()) {
                val current = input.removeAt(0)
                if (input.isEmpty()) {
                    result.add(current)
                    break
                }
                if (current is End && input[0] is Begin) {
                    val beginStr = (input[0] as Begin).delimiter
                    result.add(End("${current.delimiter} $beginStr"))
                    result.add(Begin(""))
                    input.removeAt(0)
                } else {
                    result.add(current)
                }
            }
            return result
        }
    }
}

private fun List<EnvelopeFormatItem>.compareTo(other: List<EnvelopeFormatItem>): Int {
    val minSize = minOf(size, other.size)
    for (i in 0 until minSize) {
        val cmp = this[i].compareTo(other[i])
        if (cmp != 0) return cmp
    }
    return size.compareTo(other.size)
}

// -- EnvelopeFormat interface --

/** Implementers define how to be formatted in envelope notation. */
interface EnvelopeFormat {
    fun formatItem(opts: EnvelopeFormatOpts): EnvelopeFormatItem
}

// -- Cbor format item --

/** Formats a CBOR value in envelope notation. */
fun Cbor.envelopeFormatItem(opts: EnvelopeFormatOpts): EnvelopeFormatItem {
    val case = this.cborCase
    val envelopeTags = tagsForValues(listOf(com.blockchaincommons.bctags.TAG_ENVELOPE))
    if (case is CborCase.Tagged && case.tag in envelopeTags) {
        return try {
            val envelope = Envelope.fromUntaggedCbor(case.item)
            envelope.envelopeFormatItem(opts)
        } catch (_: Exception) {
            EnvelopeFormatItem.Item("<error>")
        }
    }
    val summary = try {
        this.envelopeSummary(Int.MAX_VALUE, opts.context)
    } catch (_: Exception) {
        "<error>"
    }
    return EnvelopeFormatItem.Item(summary)
}

// -- Envelope format item --

/** Formats an Envelope in envelope notation. */
fun Envelope.envelopeFormatItem(opts: EnvelopeFormatOpts): EnvelopeFormatItem {
    return when (val c = case()) {
        is EnvelopeCase.Leaf -> c.cbor.envelopeFormatItem(opts)
        is EnvelopeCase.Wrapped -> EnvelopeFormatItem.ListItems(listOf(
            EnvelopeFormatItem.Begin("{"),
            c.envelope.envelopeFormatItem(opts),
            EnvelopeFormatItem.End("}"),
        ))
        is EnvelopeCase.AssertionCase -> c.assertion.envelopeFormatItem(opts)
        is EnvelopeCase.KnownValueCase -> c.value.envelopeFormatItem(opts)
        is EnvelopeCase.Encrypted -> EnvelopeFormatItem.Item("ENCRYPTED")
        is EnvelopeCase.CompressedCase -> EnvelopeFormatItem.Item("COMPRESSED")
        is EnvelopeCase.Node -> {
            val items = mutableListOf<EnvelopeFormatItem>()

            val subjectItem = c.subject.envelopeFormatItem(opts)
            var elidedCount = 0
            var encryptedCount = 0
            var compressedCount = 0
            val typeAssertionItems = mutableListOf<List<EnvelopeFormatItem>>()
            val assertionItems = mutableListOf<List<EnvelopeFormatItem>>()

            for (assertion in c.assertions) {
                when (assertion.case()) {
                    is EnvelopeCase.Elided -> elidedCount++
                    is EnvelopeCase.Encrypted -> encryptedCount++
                    is EnvelopeCase.CompressedCase -> compressedCount++
                    else -> {
                        val item = listOf(assertion.envelopeFormatItem(opts))
                        var isTypeAssertion = false
                        val predicate = assertion.asPredicate()
                        if (predicate != null) {
                            val knownValue = predicate.subject().asKnownValue()
                            if (knownValue != null && knownValue == IS_A) {
                                isTypeAssertion = true
                            }
                        }
                        if (isTypeAssertion) {
                            typeAssertionItems.add(item)
                        } else {
                            assertionItems.add(item)
                        }
                    }
                }
            }
            typeAssertionItems.sortWith(compareBy { it.first() })
            assertionItems.sortWith(compareBy { it.first() })
            assertionItems.addAll(0, typeAssertionItems)

            if (compressedCount > 1) {
                assertionItems.add(listOf(EnvelopeFormatItem.Item("COMPRESSED ($compressedCount)")))
            } else if (compressedCount > 0) {
                assertionItems.add(listOf(EnvelopeFormatItem.Item("COMPRESSED")))
            }
            if (elidedCount > 1) {
                assertionItems.add(listOf(EnvelopeFormatItem.Item("ELIDED ($elidedCount)")))
            } else if (elidedCount > 0) {
                assertionItems.add(listOf(EnvelopeFormatItem.Item("ELIDED")))
            }
            if (encryptedCount > 1) {
                assertionItems.add(listOf(EnvelopeFormatItem.Item("ENCRYPTED ($encryptedCount)")))
            } else if (encryptedCount > 0) {
                assertionItems.add(listOf(EnvelopeFormatItem.Item("ENCRYPTED")))
            }

            // Intersperse with separators
            val joinedItems = mutableListOf<List<EnvelopeFormatItem>>()
            for ((i, assertionItem) in assertionItems.withIndex()) {
                joinedItems.add(assertionItem)
                if (i < assertionItems.size - 1) {
                    joinedItems.add(listOf(EnvelopeFormatItem.Separator))
                }
            }

            val needsBraces = c.subject.isSubjectAssertion()

            if (needsBraces) items.add(EnvelopeFormatItem.Begin("{"))
            items.add(subjectItem)
            if (needsBraces) items.add(EnvelopeFormatItem.End("}"))
            items.add(EnvelopeFormatItem.Begin("["))
            items.addAll(joinedItems.flatten())
            items.add(EnvelopeFormatItem.End("]"))
            EnvelopeFormatItem.ListItems(items)
        }
        is EnvelopeCase.Elided -> EnvelopeFormatItem.Item("ELIDED")
    }
}

/** Formats an Assertion in envelope notation. */
fun Assertion.envelopeFormatItem(opts: EnvelopeFormatOpts): EnvelopeFormatItem {
    return EnvelopeFormatItem.ListItems(listOf(
        predicate().envelopeFormatItem(opts),
        EnvelopeFormatItem.Item(": "),
        objectEnvelope().envelopeFormatItem(opts),
    ))
}

/** Formats a KnownValue in envelope notation. */
fun KnownValue.envelopeFormatItem(opts: EnvelopeFormatOpts): EnvelopeFormatItem {
    val name = when (opts.context) {
        is FormatContextOpt.None -> name.flankedBy("'", "'")
        is FormatContextOpt.Global -> {
            withFormatContext { context ->
                context.knownValues().assignedName(this)?.flankedBy("'", "'")
                    ?: name.flankedBy("'", "'")
            }
        }
        is FormatContextOpt.Custom -> {
            opts.context.context.knownValues().assignedName(this)?.flankedBy("'", "'")
                ?: name.flankedBy("'", "'")
        }
    }
    return EnvelopeFormatItem.Item(name)
}

// -- Envelope formatting extensions --

/** Returns the envelope notation for this envelope with options. */
fun Envelope.formatOpt(
    flat: Boolean = false,
    context: FormatContextOpt = FormatContextOpt.Global,
): String {
    val opts = EnvelopeFormatOpts(flat, context)
    return envelopeFormatItem(opts).format(opts).trim()
}

/** Returns the envelope notation for this envelope. */
fun Envelope.format(): String = formatOpt()

/** Returns the envelope notation for this envelope in flat format. */
fun Envelope.formatFlat(): String = formatOpt(flat = true)
