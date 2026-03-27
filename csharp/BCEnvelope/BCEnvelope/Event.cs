using BlockchainCommons.BCComponents;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// An event notification that does not expect a response.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="Request"/> and <see cref="Response"/> which form a pair,
/// an Event is a standalone message for broadcasting information, logging, or
/// publishing notifications.
/// </para>
/// <para>
/// When serialized to an envelope, events are tagged with #6.40026 (TAG_EVENT).
/// </para>
/// </remarks>
/// <typeparam name="T">The type of content this event carries.</typeparam>
public sealed class Event<T>
{
    private readonly T _content;
    private readonly ARID _id;
    private readonly string _note;
    private readonly CborDate? _date;
    private readonly Func<T, Envelope> _contentEncoder;

    private Event(T content, ARID id, string note, CborDate? date, Func<T, Envelope> contentEncoder)
    {
        _content = content;
        _id = id;
        _note = note;
        _date = date;
        _contentEncoder = contentEncoder;
    }

    /// <summary>Returns the content of the event.</summary>
    public T Content => _content;

    /// <summary>Returns the unique identifier of the event.</summary>
    public ARID Id => _id;

    /// <summary>Returns the note attached to the event, or empty string.</summary>
    public string Note => _note;

    /// <summary>Returns the date attached to the event, if any.</summary>
    public CborDate? Date => _date;

    /// <summary>
    /// Returns a new event with the given note.
    /// </summary>
    /// <param name="note">The note to attach.</param>
    /// <returns>A new <see cref="Event{T}"/> with the note.</returns>
    public Event<T> WithNote(string note) =>
        new(_content, _id, note, _date, _contentEncoder);

    /// <summary>
    /// Returns a new event with the given date.
    /// </summary>
    /// <param name="date">The date to attach.</param>
    /// <returns>A new <see cref="Event{T}"/> with the date.</returns>
    public Event<T> WithDate(CborDate date) =>
        new(_content, _id, _note, date, _contentEncoder);

    /// <summary>
    /// Returns a human-readable summary.
    /// </summary>
    public string Summary() =>
        $"id: {_id.ShortDescription()}, content: {_contentEncoder(_content)}";

    /// <summary>
    /// Converts this event to an envelope.
    /// </summary>
    public Envelope ToEnvelope()
    {
        var eventTag = GlobalTags.TagsForValues(BcTags.TagEvent)[0];
        var envelope = Envelope.Create(Cbor.ToTaggedValue(eventTag, _id.TaggedCbor()))
            .AddAssertion(KnownValuesRegistry.Content, _contentEncoder(_content))
            .AddAssertionIf(!string.IsNullOrEmpty(_note), KnownValuesRegistry.Note, _note)
            .AddOptionalAssertion(KnownValuesRegistry.Date, _date);
        return envelope;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not Event<T> other) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(_id, other._id)
            && Equals(_content, other._content)
            && _note == other._note
            && Equals(_date, other._date);
    }

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(_content, _id, _note, _date);

    /// <inheritdoc/>
    public override string ToString() => $"Event({Summary()})";

    // --- Static factory methods ---

    /// <summary>
    /// Creates a new event with the given content, ID, and content encoder.
    /// </summary>
    /// <param name="content">The event payload.</param>
    /// <param name="id">Unique identifier for the event.</param>
    /// <param name="contentEncoder">A function that converts the content to an envelope.</param>
    /// <returns>A new <see cref="Event{T}"/>.</returns>
    public static Event<T> Create(T content, ARID id, Func<T, Envelope> contentEncoder) =>
        new(content, id, "", null, contentEncoder);

    /// <summary>
    /// Parses an event from an envelope.
    /// </summary>
    /// <param name="envelope">The envelope to parse.</param>
    /// <param name="contentDecoder">A function that converts an envelope to the content type.</param>
    /// <param name="contentEncoder">A function that converts the content to an envelope.</param>
    /// <returns>A new <see cref="Event{T}"/>.</returns>
    public static Event<T> FromEnvelope(
        Envelope envelope,
        Func<Envelope, T> contentDecoder,
        Func<T, Envelope> contentEncoder)
    {
        var eventTag = GlobalTags.TagsForValues(BcTags.TagEvent)[0];
        var idCbor = envelope.Subject.TryLeaf().TryIntoExpectedTaggedValue(eventTag);
        var id = ARID.FromTaggedCbor(idCbor);

        var contentEnvelope = envelope.ObjectForPredicate(KnownValuesRegistry.Content);
        var content = contentDecoder(contentEnvelope);

        var note = envelope.ExtractObjectForPredicateWithDefault<string>(
            KnownValuesRegistry.Note, "");
        var date = envelope.ExtractOptionalObjectForPredicate<CborDate>(
            KnownValuesRegistry.Date);

        return new Event<T>(content, id, note, date, contentEncoder);
    }
}

/// <summary>
/// Convenience factory methods for common event types.
/// </summary>
public static class Event
{
    /// <summary>
    /// Creates a new string event.
    /// </summary>
    /// <param name="content">The string content.</param>
    /// <param name="id">Unique identifier for the event.</param>
    /// <returns>A new string <see cref="Event{T}"/>.</returns>
    public static Event<string> OfString(string content, ARID id) =>
        Event<string>.Create(content, id, s => Envelope.Create(s));

    /// <summary>
    /// Parses a string event from an envelope.
    /// </summary>
    /// <param name="envelope">The envelope to parse.</param>
    /// <returns>A new string <see cref="Event{T}"/>.</returns>
    public static Event<string> StringFromEnvelope(Envelope envelope) =>
        Event<string>.FromEnvelope(
            envelope,
            e => e.ExtractSubject<string>(),
            s => Envelope.Create(s));
}
