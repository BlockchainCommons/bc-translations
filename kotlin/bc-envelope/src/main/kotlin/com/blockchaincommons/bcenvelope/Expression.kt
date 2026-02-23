package com.blockchaincommons.bcenvelope

import com.blockchaincommons.dcbor.Cbor

/**
 * An expression in a Gordian Envelope.
 *
 * An expression consists of a function (the subject of the envelope) and zero
 * or more parameters (as assertions on the envelope). It represents a
 * computation or function call that can be evaluated.
 */
class Expression private constructor(
    private val function: Function,
    @PublishedApi internal var envelope: Envelope,
) {
    /** Creates a new expression with the given function. */
    constructor(function: Function) : this(
        function = function,
        envelope = Envelope.from(function.taggedCbor()),
    )

    /** Creates a new expression from a function name string. */
    constructor(name: String) : this(Function.Named(name))

    /** Returns the function of this expression. */
    fun function(): Function = function

    /** Returns the envelope representing this expression. */
    fun expressionEnvelope(): Envelope = envelope

    /** Adds a parameter with a value to the expression. */
    fun withParameter(parameter: Parameter, value: Any): Expression {
        val assertion = Envelope.newAssertion(
            parameter.taggedCbor(),
            value.asEnvelopeEncodable().toEnvelope(),
        )
        envelope = envelope.addAssertionEnvelope(assertion)
        return this
    }

    /** Adds a parameter with an optional value. */
    fun withOptionalParameter(parameter: Parameter, value: Any?): Expression {
        if (value != null) return withParameter(parameter, value)
        return this
    }

    /** Returns the argument for the given parameter. */
    fun objectForParameter(param: Parameter): Envelope =
        envelope.objectForPredicate(param.taggedCbor())

    /** Returns all arguments for the given parameter. */
    fun objectsForParameter(param: Parameter): List<Envelope> =
        envelope.objectsForPredicate(param.taggedCbor())

    /** Returns the argument for the given parameter, decoded as a specific type. */
    inline fun <reified T : Any> extractObjectForParameter(param: Parameter): T =
        envelope.extractObjectForPredicate(param.taggedCbor())

    /** Returns the argument for the given parameter as the given type, or null. */
    inline fun <reified T : Any> extractOptionalObjectForParameter(param: Parameter): T? =
        envelope.extractOptionalObjectForPredicate(param.taggedCbor())

    /** Returns an array of arguments for the given parameter, decoded as the given type. */
    inline fun <reified T : Any> extractObjectsForParameter(param: Parameter): List<T> =
        envelope.extractObjectsForPredicate(param.taggedCbor())

    /** Converts this expression to an envelope. */
    fun toEnvelope(): Envelope = envelope

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Expression) return false
        return envelope.isEquivalentTo(other.envelope)
    }

    override fun hashCode(): Int = envelope.hashCode()

    override fun toString(): String = envelope.formatFlat()

    companion object {
        /** Creates an expression from an envelope. */
        fun fromEnvelope(envelope: Envelope, expectedFunction: Function? = null): Expression {
            val function = Function.fromTaggedCbor(envelope.subject().tryLeaf())
            if (expectedFunction != null && function != expectedFunction) {
                throw EnvelopeException.InvalidFormat()
            }
            return Expression(function, envelope)
        }
    }
}
