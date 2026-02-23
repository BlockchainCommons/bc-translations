package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.ARID
import com.blockchaincommons.bctags.TAG_RESPONSE
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.tagsForValues
import com.blockchaincommons.knownvalues.ERROR
import com.blockchaincommons.knownvalues.KnownValue
import com.blockchaincommons.knownvalues.OK_VALUE
import com.blockchaincommons.knownvalues.RESULT
import com.blockchaincommons.knownvalues.UNKNOWN_VALUE

/**
 * A response to a [Request] containing either a successful result or an error.
 *
 * Responses are part of the expression system that enables distributed function
 * calls. Each response references the original request's [ARID] and contains
 * either a result or an error envelope.
 */
class Response private constructor(
    private val _success: Boolean,
    private val id: ARID?,
    private var resultOrError: Envelope,
) {
    /** Returns true if this is a successful response. */
    fun isSuccess(): Boolean = _success

    /** Returns true if this is a failure response. */
    fun isFailure(): Boolean = !_success

    /** Returns the request ID if known. */
    fun id(): ARID? = id

    /** Returns the request ID, throwing if not known. */
    fun expectId(): ARID = id ?: throw IllegalStateException("Expected an ID")

    /** Returns the result envelope if this is a success. */
    fun result(): Envelope {
        if (!_success) throw EnvelopeException.InvalidFormat()
        return resultOrError
    }

    /** Extracts a typed result value. */
    inline fun <reified T : Any> extractResult(): T = result().extractSubject()

    /** Returns the error envelope if this is a failure. */
    fun error(): Envelope {
        if (_success) throw EnvelopeException.InvalidFormat()
        return resultOrError
    }

    /** Extracts a typed error value. */
    inline fun <reified T : Any> extractError(): T = error().extractSubject()

    /** Sets the result value for a successful response. */
    fun withResult(result: Any): Response {
        require(_success) { "Cannot set result on a failed response" }
        resultOrError = result.asEnvelopeEncodable().toEnvelope()
        return this
    }

    /** Sets the result value if provided, or null otherwise. */
    fun withOptionalResult(result: Any?): Response {
        if (result != null) return withResult(result)
        return withResult(Envelope.null_())
    }

    /** Sets the error value for a failure response. */
    fun withError(error: Any): Response {
        require(!_success) { "Cannot set error on a successful response" }
        resultOrError = error.asEnvelopeEncodable().toEnvelope()
        return this
    }

    /** Sets the error value if provided, leaves default otherwise. */
    fun withOptionalError(error: Any?): Response {
        if (error != null) return withError(error)
        return this
    }

    /** Returns a human-readable summary. */
    fun summary(): String {
        return if (_success) {
            "id: ${id!!.shortDescription()}, result: ${resultOrError.formatFlat()}"
        } else {
            val idStr = id?.shortDescription() ?: "'Unknown'"
            "id: $idStr error: ${resultOrError.formatFlat()}"
        }
    }

    /** Converts this response to an envelope. */
    fun toEnvelope(): Envelope {
        val responseTag = tagsForValues(listOf(TAG_RESPONSE)).first()
        return if (_success) {
            Envelope.from(Cbor.tagged(responseTag, id!!.taggedCbor()))
                .addAssertion(RESULT, resultOrError)
        } else {
            val subject = if (id != null) {
                Envelope.from(Cbor.tagged(responseTag, id.taggedCbor()))
            } else {
                Envelope.from(Cbor.tagged(responseTag, UNKNOWN_VALUE.taggedCbor()))
            }
            subject.addAssertion(ERROR, resultOrError)
        }
    }

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Response) return false
        return _success == other._success && id == other.id &&
            resultOrError.isEquivalentTo(other.resultOrError)
    }

    override fun hashCode(): Int {
        var result = _success.hashCode()
        result = 31 * result + (id?.hashCode() ?: 0)
        result = 31 * result + resultOrError.hashCode()
        return result
    }

    override fun toString(): String = "Response(${summary()})"

    companion object {
        /** Creates a new successful response with the given request ID. */
        fun newSuccess(id: ARID): Response = Response(true, id, Envelope.ok())

        /** Creates a new failure response with the given request ID. */
        fun newFailure(id: ARID): Response = Response(false, id, Envelope.unknown())

        /** Creates a new early failure response without a request ID. */
        fun newEarlyFailure(): Response = Response(false, null, Envelope.unknown())

        /** Parses a response from an envelope. */
        fun fromEnvelope(envelope: Envelope): Response {
            val hasResult = try {
                envelope.assertionWithPredicate(RESULT)
                true
            } catch (_: Exception) { false }

            val hasError = try {
                envelope.assertionWithPredicate(ERROR)
                true
            } catch (_: Exception) { false }

            if (hasResult == hasError) throw EnvelopeException.InvalidFormat()

            val responseTag = tagsForValues(listOf(TAG_RESPONSE)).first()
            val (tag, idCbor) = envelope.subject().tryLeaf().tryTagged()
            if (tag != responseTag) throw EnvelopeException.InvalidFormat()

            if (hasResult) {
                val id = ARID.fromTaggedCbor(idCbor)
                val result = envelope.objectForPredicate(RESULT)
                return Response(true, id, result)
            }

            // Error case -- check if ID is Unknown KnownValue
            val id: ARID? = try {
                val kv = KnownValue.fromTaggedCbor(idCbor)
                if (kv == UNKNOWN_VALUE) null
                else throw EnvelopeException.InvalidFormat()
            } catch (_: EnvelopeException) {
                throw EnvelopeException.InvalidFormat()
            } catch (_: Exception) {
                ARID.fromTaggedCbor(idCbor)
            }
            val error = envelope.objectForPredicate(ERROR)
            return Response(false, id, error)
        }
    }
}
