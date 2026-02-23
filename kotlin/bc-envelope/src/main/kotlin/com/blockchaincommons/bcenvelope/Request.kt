package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.ARID
import com.blockchaincommons.bctags.TAG_REQUEST
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborDate
import com.blockchaincommons.dcbor.tagsForValues
import com.blockchaincommons.knownvalues.BODY
import com.blockchaincommons.knownvalues.DATE
import com.blockchaincommons.knownvalues.NOTE

/**
 * A request message for executing a function with parameters.
 *
 * Requests are part of the expression system that enables distributed function
 * calls. Each request contains a body (an [Expression]) and a unique [ARID]
 * identifier for tracking and correlation.
 */
class Request private constructor(
    @PublishedApi internal var body: Expression,
    val id: ARID,
    private val note: String = "",
    private val date: CborDate? = null,
) {
    /** Creates a new request from an expression body and ID. */
    constructor(body: Expression, id: ARID) : this(
        body = body,
        id = id,
        note = "",
        date = null,
    )

    /** Creates a new request with a function and ID. */
    constructor(function: Function, id: ARID) : this(Expression(function), id)

    /** Creates a new request with a named function and ID. */
    constructor(name: String, id: ARID) : this(Expression(name), id)

    /** Returns the body expression of the request. */
    fun body(): Expression = body

    /** Returns the unique identifier of the request. */
    fun id(): ARID = id

    /** Returns the note attached to the request, or empty string. */
    fun note(): String = note

    /** Returns the date attached to the request, if any. */
    fun date(): CborDate? = date

    /** Returns a new request with the given note. */
    fun withNote(newNote: String): Request =
        Request(body, id, newNote, date)

    /** Returns a new request with the given date. */
    fun withDate(newDate: CborDate): Request =
        Request(body, id, note, newDate)

    /** Adds a parameter to the request. */
    fun withParameter(parameter: Parameter, value: Any): Request {
        body = body.withParameter(parameter, value)
        return this
    }

    /** Adds an optional parameter to the request. */
    fun withOptionalParameter(parameter: Parameter, value: Any?): Request {
        body = body.withOptionalParameter(parameter, value)
        return this
    }

    /** Returns the argument for the given parameter. */
    fun objectForParameter(param: Parameter): Envelope =
        body.objectForParameter(param)

    /** Returns all arguments for the given parameter. */
    fun objectsForParameter(param: Parameter): List<Envelope> =
        body.objectsForParameter(param)

    /** Returns the argument for the given parameter, decoded as a specific type. */
    inline fun <reified T : Any> extractObjectForParameter(param: Parameter): T =
        body.extractObjectForParameter(param)

    /** Returns the argument for the given parameter as the given type, or null. */
    inline fun <reified T : Any> extractOptionalObjectForParameter(param: Parameter): T? =
        body.extractOptionalObjectForParameter(param)

    /** Returns the function of the request. */
    fun function(): Function = body.function()

    /** Returns the expression envelope of the request. */
    fun expressionEnvelope(): Envelope = body.expressionEnvelope()

    /** Returns a human-readable summary. */
    fun summary(): String =
        "id: ${id.shortDescription()}, body: ${body.expressionEnvelope().formatFlat()}"

    /** Converts this request to an envelope. */
    fun toEnvelope(): Envelope {
        val requestTag = tagsForValues(listOf(TAG_REQUEST)).first()
        var envelope = Envelope.from(Cbor.tagged(requestTag, id.taggedCbor()))
            .addAssertion(BODY, body.toEnvelope())
        if (note.isNotEmpty()) {
            envelope = envelope.addAssertion(NOTE, note)
        }
        val d = date
        if (d != null) {
            envelope = envelope.addAssertion(DATE, d)
        }
        return envelope
    }

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Request) return false
        return id == other.id && body == other.body && note == other.note && date == other.date
    }

    override fun hashCode(): Int {
        var result = body.hashCode()
        result = 31 * result + id.hashCode()
        result = 31 * result + note.hashCode()
        result = 31 * result + (date?.hashCode() ?: 0)
        return result
    }

    override fun toString(): String = "Request(${summary()})"

    companion object {
        /** Parses a request from an envelope. */
        fun fromEnvelope(
            envelope: Envelope,
            expectedFunction: Function? = null,
        ): Request {
            val bodyEnvelope = envelope.objectForPredicate(BODY)
            val expression = Expression.fromEnvelope(bodyEnvelope, expectedFunction)

            val requestTag = tagsForValues(listOf(TAG_REQUEST)).first()
            val (tag, idCbor) = envelope.subject().tryLeaf().tryTagged()
            if (tag != requestTag) {
                throw EnvelopeException.InvalidFormat()
            }
            val id = ARID.fromTaggedCbor(idCbor)

            val note: String = envelope.extractOptionalObjectForPredicate(NOTE) ?: ""
            val date: CborDate? = envelope.extractOptionalObjectForPredicate<CborDate>(DATE)

            return Request(expression, id, note, date)
        }
    }
}
