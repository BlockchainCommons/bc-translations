package com.blockchaincommons.bcenvelope

/**
 * The type of incoming edge provided to the visitor during envelope traversal.
 *
 * Each edge type represents a specific relationship within the envelope
 * structure during a walk operation.
 */
enum class EdgeType {
    /** No incoming edge (root). */
    None,

    /** Element is the subject of a node. */
    Subject,

    /** Element is an assertion on a node. */
    Assertion,

    /** Element is the predicate of an assertion. */
    Predicate,

    /** Element is the object of an assertion. */
    Object,

    /** Element is the content wrapped by another envelope. */
    Content;

    /** Returns a short text label for tree formatting, or null if none. */
    fun label(): String? = when (this) {
        Subject -> "subj"
        Content -> "cont"
        Predicate -> "pred"
        Object -> "obj"
        else -> null
    }
}
