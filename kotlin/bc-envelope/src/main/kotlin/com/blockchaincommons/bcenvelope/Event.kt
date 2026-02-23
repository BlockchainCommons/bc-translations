package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.ARID
import com.blockchaincommons.bctags.TAG_EVENT
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborDate
import com.blockchaincommons.dcbor.tagsForValues
import com.blockchaincommons.knownvalues.CONTENT
import com.blockchaincommons.knownvalues.DATE
import com.blockchaincommons.knownvalues.NOTE

/**
 * An event notification that does not expect a response.
 *
 * Unlike [Request] and [Response] which form a pair, an Event is a standalone
 * message for broadcasting information, logging, or publishing notifications.
 *
 * @param T The type of content this event carries.
 */
class Event<T> private constructor(
    private val content: T,
    private val id: ARID,
    private val note: String = "",
    private val date: CborDate? = null,
    private val contentEncoder: (T) -> Envelope,
) {
    /** Returns the content of the event. */
    fun content(): T = content

    /** Returns the unique identifier of the event. */
    fun id(): ARID = id

    /** Returns the note attached to the event, or empty string. */
    fun note(): String = note

    /** Returns the date attached to the event, if any. */
    fun date(): CborDate? = date

    /** Returns a new event with the given note. */
    fun withNote(newNote: String): Event<T> =
        Event(content, id, newNote, date, contentEncoder)

    /** Returns a new event with the given date. */
    fun withDate(newDate: CborDate): Event<T> =
        Event(content, id, note, newDate, contentEncoder)

    /** Returns a human-readable summary. */
    fun summary(): String =
        "id: ${id.shortDescription()}, content: ${contentEncoder(content).formatFlat()}"

    /** Converts this event to an envelope. */
    fun toEnvelope(): Envelope {
        val eventTag = tagsForValues(listOf(TAG_EVENT)).first()
        var envelope = Envelope.from(Cbor.tagged(eventTag, id.taggedCbor()))
            .addAssertion(CONTENT, contentEncoder(content))
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
        if (other !is Event<*>) return false
        return id == other.id && content == other.content && note == other.note && date == other.date
    }

    override fun hashCode(): Int {
        var result = content.hashCode()
        result = 31 * result + id.hashCode()
        result = 31 * result + note.hashCode()
        result = 31 * result + (date?.hashCode() ?: 0)
        return result
    }

    override fun toString(): String = "Event(${summary()})"

    companion object {
        /**
         * Creates a new event with the given content and ID.
         *
         * @param content The event payload.
         * @param id Unique identifier for the event.
         * @param contentEncoder A function that converts the content to an envelope.
         */
        fun <T> create(
            content: T,
            id: ARID,
            contentEncoder: (T) -> Envelope,
        ): Event<T> = Event(content, id, contentEncoder = contentEncoder)

        /**
         * Creates a new string event.
         */
        fun ofString(content: String, id: ARID): Event<String> =
            create(content, id) { Envelope.from(it) }

        /**
         * Parses an event from an envelope.
         *
         * @param envelope The envelope to parse.
         * @param contentDecoder A function that converts an envelope to the content type.
         */
        fun <T> fromEnvelope(
            envelope: Envelope,
            contentDecoder: (Envelope) -> T,
            contentEncoder: (T) -> Envelope,
        ): Event<T> {
            val eventTag = tagsForValues(listOf(TAG_EVENT)).first()
            val (tag, idCbor) = envelope.subject().tryLeaf().tryTagged()
            if (tag != eventTag) throw EnvelopeException.InvalidFormat()
            val id = ARID.fromTaggedCbor(idCbor)

            val contentEnvelope = envelope.objectForPredicate(CONTENT)
            val content = contentDecoder(contentEnvelope)

            val note: String = envelope.extractOptionalObjectForPredicate(NOTE) ?: ""
            val date: CborDate? = envelope.extractOptionalObjectForPredicate<CborDate>(DATE)

            return Event(content, id, note, date, contentEncoder)
        }

        /** Parses a String event from an envelope. */
        fun stringFromEnvelope(envelope: Envelope): Event<String> =
            fromEnvelope(
                envelope,
                contentDecoder = { it.extractSubject<String>() },
                contentEncoder = { Envelope.from(it) },
            )
    }
}
