using BlockchainCommons.BCComponents;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// A request message for executing a function with parameters.
/// </summary>
/// <remarks>
/// <para>
/// Requests are part of the expression system that enables distributed function
/// calls and communication between systems. Each request contains a body (an
/// <see cref="Expression"/>) that represents the function to be executed, and
/// a unique identifier (<see cref="ARID"/>) for tracking and correlation.
/// </para>
/// <para>
/// When serialized to an envelope, requests are tagged with #6.40004 (TAG_REQUEST).
/// </para>
/// </remarks>
public sealed class Request
{
    private Expression _body;
    private readonly ARID _id;
    private readonly string _note;
    private readonly CborDate? _date;

    private Request(Expression body, ARID id, string note, CborDate? date)
    {
        _body = body;
        _id = id;
        _note = note;
        _date = date;
    }

    /// <summary>
    /// Creates a new request from an expression body and ID.
    /// </summary>
    /// <param name="body">The expression to be executed.</param>
    /// <param name="id">Unique identifier for the request.</param>
    public Request(Expression body, ARID id)
        : this(body, id, "", null)
    {
    }

    /// <summary>
    /// Creates a new request with a function and ID.
    /// </summary>
    /// <param name="function">The function to be executed.</param>
    /// <param name="id">Unique identifier for the request.</param>
    public Request(Function function, ARID id)
        : this(new Expression(function), id)
    {
    }

    /// <summary>
    /// Creates a new request with a named function and ID.
    /// </summary>
    /// <param name="name">The function name string.</param>
    /// <param name="id">Unique identifier for the request.</param>
    public Request(string name, ARID id)
        : this(new Expression(name), id)
    {
    }

    /// <summary>Returns the body expression of the request.</summary>
    public Expression Body => _body;

    /// <summary>Returns the unique identifier of the request.</summary>
    public ARID Id => _id;

    /// <summary>Returns the note attached to the request, or empty string.</summary>
    public string Note => _note;

    /// <summary>Returns the date attached to the request, if any.</summary>
    public CborDate? Date => _date;

    /// <summary>Returns the function of the request.</summary>
    public Function Function => _body.Function;

    /// <summary>Returns the expression envelope of the request.</summary>
    public Envelope ExpressionEnvelope => _body.ExpressionEnvelope;

    /// <summary>
    /// Returns a new request with the given note.
    /// </summary>
    /// <param name="note">The note to attach.</param>
    /// <returns>A new <see cref="Request"/> with the note.</returns>
    public Request WithNote(string note) =>
        new(_body, _id, note, _date);

    /// <summary>
    /// Returns a new request with the given date.
    /// </summary>
    /// <param name="date">The date to attach.</param>
    /// <returns>A new <see cref="Request"/> with the date.</returns>
    public Request WithDate(CborDate date) =>
        new(_body, _id, _note, date);

    /// <summary>
    /// Adds a parameter to the request.
    /// </summary>
    /// <param name="parameter">The parameter identifier.</param>
    /// <param name="value">The value for the parameter.</param>
    /// <returns>This request with the parameter added.</returns>
    public Request WithParameter(Parameter parameter, object value)
    {
        _body = _body.WithParameter(parameter, value);
        return this;
    }

    /// <summary>
    /// Adds an optional parameter to the request.
    /// </summary>
    /// <param name="parameter">The parameter identifier.</param>
    /// <param name="value">The optional value for the parameter.</param>
    /// <returns>This request, with the parameter added if the value is not null.</returns>
    public Request WithOptionalParameter(Parameter parameter, object? value)
    {
        _body = _body.WithOptionalParameter(parameter, value);
        return this;
    }

    /// <summary>
    /// Returns the argument for the given parameter.
    /// </summary>
    public Envelope ObjectForParameter(Parameter param) =>
        _body.ObjectForParameter(param);

    /// <summary>
    /// Returns all arguments for the given parameter.
    /// </summary>
    public List<Envelope> ObjectsForParameter(Parameter param) =>
        _body.ObjectsForParameter(param);

    /// <summary>
    /// Returns the argument for the given parameter, decoded as the specified type.
    /// </summary>
    public T ExtractObjectForParameter<T>(Parameter param) =>
        _body.ExtractObjectForParameter<T>(param);

    /// <summary>
    /// Returns the argument for the given parameter as the given type, or default.
    /// </summary>
    public T? ExtractOptionalObjectForParameter<T>(Parameter param) =>
        _body.ExtractOptionalObjectForParameter<T>(param);

    /// <summary>
    /// Returns a human-readable summary.
    /// </summary>
    public string Summary() =>
        $"id: {_id.ShortDescription()}, body: {_body.ExpressionEnvelope}";

    /// <summary>
    /// Converts this request to an envelope.
    /// </summary>
    public Envelope ToEnvelope()
    {
        var requestTag = GlobalTags.TagsForValues(BcTags.TagRequest)[0];
        var envelope = Envelope.Create(Cbor.ToTaggedValue(requestTag, _id.TaggedCbor()))
            .AddAssertion(KnownValuesRegistry.Body, _body.ToEnvelope())
            .AddAssertionIf(!string.IsNullOrEmpty(_note), KnownValuesRegistry.Note, _note)
            .AddOptionalAssertion(KnownValuesRegistry.Date, _date);
        return envelope;
    }

    /// <summary>
    /// Parses a request from an envelope.
    /// </summary>
    /// <param name="envelope">The envelope to parse.</param>
    /// <param name="expectedFunction">An optional expected function for validation.</param>
    /// <returns>A new <see cref="Request"/>.</returns>
    public static Request FromEnvelope(Envelope envelope, Function? expectedFunction = null)
    {
        var bodyEnvelope = envelope.ObjectForPredicate(KnownValuesRegistry.Body);
        var expression = Expression.FromEnvelope(bodyEnvelope, expectedFunction);

        var requestTag = GlobalTags.TagsForValues(BcTags.TagRequest)[0];
        var idCbor = envelope.Subject.TryLeaf().TryIntoExpectedTaggedValue(requestTag);
        var id = ARID.FromTaggedCbor(idCbor);

        var note = envelope.ExtractObjectForPredicateWithDefault<string>(
            KnownValuesRegistry.Note, "");
        var date = envelope.ExtractOptionalObjectForPredicate<CborDate>(
            KnownValuesRegistry.Date);

        return new Request(expression, id, note, date);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not Request other) return false;
        if (ReferenceEquals(this, other)) return true;
        return _id == other._id
            && _body.Equals(other._body)
            && _note == other._note
            && Equals(_date, other._date);
    }

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(_body, _id, _note, _date);

    /// <inheritdoc/>
    public override string ToString() => $"Request({Summary()})";
}
