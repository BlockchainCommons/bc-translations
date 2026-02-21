package com.blockchaincommons.dcbor

/**
 * Represents an element during CBOR tree traversal.
 */
sealed class WalkElement {
    /** A single CBOR element. */
    data class Single(val cbor: Cbor) : WalkElement()
    /** A key-value pair from a map. */
    data class KeyValue(val key: Cbor, val value: Cbor) : WalkElement()

    fun asSingle(): Cbor? = (this as? Single)?.cbor
    fun asKeyValue(): Pair<Cbor, Cbor>? = (this as? KeyValue)?.let { it.key to it.value }

    val diagnosticFlat: String get() = when (this) {
        is Single -> cbor.diagnosticFlat
        is KeyValue -> "${key.diagnosticFlat}: ${value.diagnosticFlat}"
    }
}

/**
 * The type of incoming edge provided to the visitor during walk traversal.
 */
sealed class EdgeType {
    abstract val index: Int

    data object None : EdgeType() {
        override val index: Int get() = 0
    }
    data class ArrayElement(override val index: Int) : EdgeType()
    data object MapKeyValue : EdgeType() {
        override val index: Int get() = 0
    }
    data object MapKey : EdgeType() {
        override val index: Int get() = 0
    }
    data object MapValue : EdgeType() {
        override val index: Int get() = 0
    }
    data object TaggedContent : EdgeType() {
        override val index: Int get() = 0
    }

    val label: String? get() = when (this) {
        is None -> null
        is ArrayElement -> "arr[$index]"
        is MapKeyValue -> "kv"
        is MapKey -> "key"
        is MapValue -> "val"
        is TaggedContent -> "content"
    }
}

/**
 * Visitor function type for CBOR tree traversal.
 *
 * Parameters:
 * - element: The current element being visited
 * - level: The depth level (0 for root)
 * - edge: The relationship to the parent element
 * - state: Context passed from the parent's visitor call
 *
 * Returns: Pair of (state for children, stop flag).
 * If stop is true, children of this element will not be visited.
 */
typealias Visitor<State> = (element: WalkElement, level: Int, edge: EdgeType, state: State) -> Pair<State, Boolean>

/**
 * Walk the CBOR tree structure, calling the visitor for each element.
 */
fun <State> Cbor.walk(state: State, visitor: Visitor<State>) {
    walkInternal(0, EdgeType.None, state, visitor)
}

private fun <State> Cbor.walkInternal(
    level: Int,
    incomingEdge: EdgeType,
    state: State,
    visitor: Visitor<State>
) {
    val element = WalkElement.Single(this)
    val (newState, stop) = visitor(element, level, incomingEdge, state)
    if (stop) return

    val nextLevel = level + 1
    when (val case = cborCase) {
        is CborCase.Array -> {
            for ((index, item) in case.value.withIndex()) {
                item.walkInternal(nextLevel, EdgeType.ArrayElement(index), newState, visitor)
            }
        }
        is CborCase.CborMap -> {
            for ((key, value) in case.value.toList()) {
                val kvElement = WalkElement.KeyValue(key, value)
                val (kvState, kvStop) = visitor(kvElement, nextLevel, EdgeType.MapKeyValue, newState)
                if (kvStop) continue
                key.walkInternal(nextLevel, EdgeType.MapKey, kvState, visitor)
                value.walkInternal(nextLevel, EdgeType.MapValue, kvState, visitor)
            }
        }
        is CborCase.Tagged -> {
            case.item.walkInternal(nextLevel, EdgeType.TaggedContent, newState, visitor)
        }
        else -> { /* Primitive types have no children */ }
    }
}
