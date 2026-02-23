package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.Digest

/**
 * Orientation for Mermaid diagram layout.
 */
enum class MermaidOrientation {
    LeftToRight,
    TopToBottom,
    RightToLeft,
    BottomToTop;

    override fun toString(): String = when (this) {
        LeftToRight -> "LR"
        TopToBottom -> "TB"
        RightToLeft -> "RL"
        BottomToTop -> "BT"
    }
}

/**
 * Theme for Mermaid diagrams.
 */
enum class MermaidTheme {
    Default,
    Neutral,
    Dark,
    Forest,
    Base;

    override fun toString(): String = when (this) {
        Default -> "default"
        Neutral -> "neutral"
        Dark -> "dark"
        Forest -> "forest"
        Base -> "base"
    }
}

/**
 * Options for Mermaid diagram output.
 */
class MermaidFormatOpts(
    val hideNodes: Boolean = false,
    val monochrome: Boolean = false,
    val theme: MermaidTheme = MermaidTheme.Default,
    val orientation: MermaidOrientation = MermaidOrientation.LeftToRight,
    val highlightingTarget: Set<Digest> = emptySet(),
    val context: FormatContextOpt = FormatContextOpt.Global,
)

// -- Mermaid format extensions --

/** Returns a Mermaid flowchart for this envelope. */
fun Envelope.mermaidFormat(): String = mermaidFormatOpt(MermaidFormatOpts())

/** Returns a Mermaid flowchart for this envelope with options. */
fun Envelope.mermaidFormatOpt(opts: MermaidFormatOpts): String {
    val elements = mutableListOf<MermaidElement>()
    var nextId = 0
    val visitor = { envelope: Envelope, level: Int, incomingEdge: EdgeType, parent: MermaidElement? ->
        val id = nextId++
        val elem = MermaidElement(
            id = id,
            level = level,
            envelope = envelope,
            incomingEdge = incomingEdge,
            showId = !opts.hideNodes,
            isHighlighted = opts.highlightingTarget.contains(envelope.digest()),
            parent = parent,
        )
        elements.add(elem)
        Pair(elem, false)
    }
    walk(opts.hideNodes, null, visitor)

    val elementIds = elements.map { it.id }.toMutableSet()
    val lines = mutableListOf<String>()
    lines.add("%%{ init: { 'theme': '${opts.theme}', 'flowchart': { 'curve': 'basis' } } }%%")
    lines.add("graph ${opts.orientation}")

    val nodeStyles = mutableListOf<String>()
    val linkStyles = mutableListOf<String>()
    var linkIndex = 0

    for (element in elements) {
        val indent = "    ".repeat(element.level)
        val content = if (element.parent != null) {
            val thisLinkStyles = mutableListOf<String>()
            if (!opts.monochrome) {
                element.incomingEdge.linkStrokeColor()?.let { color ->
                    thisLinkStyles.add("stroke:$color")
                }
            }
            if (element.isHighlighted && element.parent.isHighlighted) {
                thisLinkStyles.add("stroke-width:4px")
            } else {
                thisLinkStyles.add("stroke-width:2px")
            }
            if (thisLinkStyles.isNotEmpty()) {
                linkStyles.add("linkStyle $linkIndex ${thisLinkStyles.joinToString(",")}")
            }
            linkIndex++
            element.formatEdge(elementIds)
        } else {
            element.formatNode(elementIds)
        }

        val thisNodeStyles = mutableListOf<String>()
        if (!opts.monochrome) {
            thisNodeStyles.add("stroke:${element.envelope.nodeColor()}")
        }
        if (element.isHighlighted) {
            thisNodeStyles.add("stroke-width:6px")
        } else {
            thisNodeStyles.add("stroke-width:4px")
        }
        if (thisNodeStyles.isNotEmpty()) {
            nodeStyles.add("style ${element.id} ${thisNodeStyles.joinToString(",")}")
        }
        lines.add("$indent$content")
    }

    lines.addAll(nodeStyles)
    lines.addAll(linkStyles)

    return lines.joinToString("\n")
}

// -- Envelope helpers for mermaid --

internal fun Envelope.mermaidFrame(): Pair<String, String> = when (case()) {
    is EnvelopeCase.Node -> "((" to "))"
    is EnvelopeCase.Leaf -> "[" to "]"
    is EnvelopeCase.Wrapped -> "[/" to "\\]"
    is EnvelopeCase.AssertionCase -> "([" to "])"
    is EnvelopeCase.Elided -> "{{" to "}}"
    is EnvelopeCase.KnownValueCase -> "[/" to "/]"
    is EnvelopeCase.Encrypted -> ">" to "]"
    is EnvelopeCase.CompressedCase -> "[[" to "]]"
}

internal fun Envelope.nodeColor(): String = when (case()) {
    is EnvelopeCase.Node -> "red"
    is EnvelopeCase.Leaf -> "teal"
    is EnvelopeCase.Wrapped -> "blue"
    is EnvelopeCase.AssertionCase -> "green"
    is EnvelopeCase.Elided -> "gray"
    is EnvelopeCase.KnownValueCase -> "goldenrod"
    is EnvelopeCase.Encrypted -> "coral"
    is EnvelopeCase.CompressedCase -> "purple"
}

internal fun EdgeType.linkStrokeColor(): String? = when (this) {
    EdgeType.Subject -> "red"
    EdgeType.Content -> "blue"
    EdgeType.Predicate -> "cyan"
    EdgeType.Object -> "magenta"
    else -> null
}

/**
 * An element in the Mermaid flowchart representation.
 */
private class MermaidElement(
    val id: Int,
    val level: Int,
    val envelope: Envelope,
    val incomingEdge: EdgeType,
    val showId: Boolean,
    val isHighlighted: Boolean,
    val parent: MermaidElement?,
) {
    fun formatNode(elementIds: MutableSet<Int>): String {
        if (elementIds.contains(id)) {
            elementIds.remove(id)
            val lines = mutableListOf<String>()
            val summary = withFormatContext { ctx ->
                envelope.summary(20, ctx).replace("\"", "&quot;")
            }
            lines.add(summary)
            if (showId) {
                lines.add(envelope.digest().shortDescription())
            }
            val joinedLines = lines.joinToString("<br>")
            val (frameL, frameR) = envelope.mermaidFrame()
            return "$id${frameL}\"${joinedLines}\"${frameR}"
        }
        return "$id"
    }

    fun formatEdge(elementIds: MutableSet<Int>): String {
        val parentElement = parent!!
        val arrow = incomingEdge.label()?.let { "-- $it -->" } ?: "-->"
        return "${parentElement.formatNode(elementIds)} $arrow ${formatNode(elementIds)}"
    }
}
