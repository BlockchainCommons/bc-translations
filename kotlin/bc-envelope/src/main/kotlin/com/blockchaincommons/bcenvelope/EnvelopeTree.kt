package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.Digest
import com.blockchaincommons.bccomponents.DigestProvider
import com.blockchaincommons.bcur.UREncodable

/**
 * Format for displaying digests in the tree representation.
 */
enum class DigestDisplayFormat {
    /** Display a shortened version of the digest (first 8 characters). */
    Short,
    /** Display the full digest for each element in the tree. */
    Full,
    /** Display a ur:digest UR for each element in the tree. */
    UR,
}

/**
 * Options for tree-formatted output of an envelope.
 */
class TreeFormatOpts(
    val hideNodes: Boolean = false,
    val highlightingTarget: Set<Digest> = emptySet(),
    val context: FormatContextOpt = FormatContextOpt.Global,
    val digestDisplay: DigestDisplayFormat = DigestDisplayFormat.Short,
)

// -- Tree format extensions --

/** Returns a tree-formatted string representation of the envelope. */
fun Envelope.treeFormat(): String = treeFormatOpt(TreeFormatOpts())

/** Returns a tree-formatted string representation with options. */
fun Envelope.treeFormatOpt(opts: TreeFormatOpts): String {
    val elements = mutableListOf<TreeElement>()
    val visitor = { envelope: Envelope, level: Int, incomingEdge: EdgeType, _: Unit ->
        val elem = TreeElement(
            level = level,
            envelope = envelope,
            incomingEdge = incomingEdge,
            showId = !opts.hideNodes,
            isHighlighted = opts.highlightingTarget.contains(envelope.digest()),
        )
        elements.add(elem)
        Pair(Unit, false)
    }
    walk(opts.hideNodes, Unit, visitor)

    val formatElements = { elems: List<TreeElement>, ctx: FormatContext ->
        elems.joinToString("\n") { it.toFormattedString(ctx, opts.digestDisplay) }
    }

    return when (opts.context) {
        is FormatContextOpt.None -> {
            val defaultCtx = FormatContext()
            formatElements(elements, defaultCtx)
        }
        is FormatContextOpt.Global -> withFormatContext { ctx ->
            formatElements(elements, ctx)
        }
        is FormatContextOpt.Custom -> formatElements(elements, opts.context.context)
    }
}

/** Returns a text representation of the envelope's digest. */
fun Envelope.shortId(format: DigestDisplayFormat = DigestDisplayFormat.Short): String =
    when (format) {
        DigestDisplayFormat.Short -> digest().shortDescription()
        DigestDisplayFormat.Full -> digest().hex
        DigestDisplayFormat.UR -> (digest() as UREncodable).urString()
    }

/**
 * An element in the tree representation of an envelope.
 */
private class TreeElement(
    val level: Int,
    val envelope: Envelope,
    val incomingEdge: EdgeType,
    val showId: Boolean,
    val isHighlighted: Boolean,
) {
    fun toFormattedString(context: FormatContext, digestDisplay: DigestDisplayFormat): String {
        val parts = mutableListOf<String>()
        if (isHighlighted) parts.add("*")
        if (showId) parts.add(envelope.shortId(digestDisplay))
        incomingEdge.label()?.let { parts.add(it) }
        parts.add(envelope.summary(40, context))
        val line = parts.joinToString(" ")
        val indent = " ".repeat(level * 4)
        return "$indent$line"
    }
}
