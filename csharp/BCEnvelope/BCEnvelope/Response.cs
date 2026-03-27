using BlockchainCommons.BCComponents;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// A response to a <see cref="Request"/> containing either a successful result
/// or an error.
/// </summary>
/// <remarks>
/// <para>
/// Responses are part of the expression system that enables distributed function
/// calls. Each response references the original request's <see cref="ARID"/>
/// and contains either a result or an error envelope.
/// </para>
/// <para>
/// When serialized to an envelope, responses are tagged with #6.40005 (TAG_RESPONSE).
/// </para>
/// </remarks>
public sealed class Response
{
    private readonly bool _isSuccess;
    private readonly ARID? _id;
    private Envelope _resultOrError;

    private Response(bool isSuccess, ARID? id, Envelope resultOrError)
    {
        _isSuccess = isSuccess;
        _id = id;
        _resultOrError = resultOrError;
    }

    // --- Factory Methods ---

    /// <summary>
    /// Creates a new successful response with the specified request ID.
    /// </summary>
    /// <remarks>
    /// By default, the result will be the 'OK' known value. Use <see cref="WithResult"/>
    /// to set a specific result value.
    /// </remarks>
    /// <param name="id">The ID of the request this response corresponds to.</param>
    /// <returns>A new successful <see cref="Response"/>.</returns>
    public static Response NewSuccess(ARID id) =>
        new(true, id, Envelope.Ok());

    /// <summary>
    /// Creates a new failure response with the specified request ID.
    /// </summary>
    /// <remarks>
    /// By default, the error will be the 'Unknown' known value. Use <see cref="WithError"/>
    /// to set a specific error message.
    /// </remarks>
    /// <param name="id">The ID of the request this response corresponds to.</param>
    /// <returns>A new failure <see cref="Response"/>.</returns>
    public static Response NewFailure(ARID id) =>
        new(false, id, Envelope.Unknown());

    /// <summary>
    /// Creates a new early failure response without a request ID.
    /// </summary>
    /// <remarks>
    /// An early failure occurs when the error happens before the request
    /// has been fully processed, so the request ID is not known.
    /// </remarks>
    /// <returns>A new early failure <see cref="Response"/>.</returns>
    public static Response NewEarlyFailure() =>
        new(false, null, Envelope.Unknown());

    // --- Properties ---

    /// <summary>Returns true if this is a successful response.</summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>Returns true if this is a failure response.</summary>
    public bool IsFailure => !_isSuccess;

    /// <summary>Returns the request ID if known, or null.</summary>
    public ARID? Id => _id;

    /// <summary>
    /// Returns the request ID, throwing if not known.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the ID is not known.</exception>
    public ARID ExpectId() => _id ?? throw new InvalidOperationException("Expected an ID");

    // --- Success Composition ---

    /// <summary>
    /// Sets the result value for a successful response.
    /// </summary>
    /// <param name="result">The result value.</param>
    /// <returns>This response with the result set.</returns>
    /// <exception cref="InvalidOperationException">Thrown if called on a failure response.</exception>
    public Response WithResult(object result)
    {
        if (!_isSuccess)
            throw new InvalidOperationException("Cannot set result on a failed response");
        _resultOrError = EnvelopeExtensions.ToEnvelope(result);
        return this;
    }

    /// <summary>
    /// Sets the result value if provided, or null otherwise.
    /// </summary>
    /// <param name="result">The optional result value.</param>
    /// <returns>This response with the result set.</returns>
    /// <exception cref="InvalidOperationException">Thrown if called on a failure response.</exception>
    public Response WithOptionalResult(object? result)
    {
        if (result is not null) return WithResult(result);
        return WithResult(Envelope.Null());
    }

    // --- Failure Composition ---

    /// <summary>
    /// Sets the error value for a failure response.
    /// </summary>
    /// <param name="error">The error value.</param>
    /// <returns>This response with the error set.</returns>
    /// <exception cref="InvalidOperationException">Thrown if called on a successful response.</exception>
    public Response WithError(object error)
    {
        if (_isSuccess)
            throw new InvalidOperationException("Cannot set error on a successful response");
        _resultOrError = EnvelopeExtensions.ToEnvelope(error);
        return this;
    }

    /// <summary>
    /// Sets the error value if provided, leaves default otherwise.
    /// </summary>
    /// <param name="error">The optional error value.</param>
    /// <returns>This response with the error set.</returns>
    /// <exception cref="InvalidOperationException">Thrown if called on a successful response.</exception>
    public Response WithOptionalError(object? error)
    {
        if (error is not null) return WithError(error);
        return this;
    }

    // --- Result/Error Access ---

    /// <summary>
    /// Returns the result envelope if this is a successful response.
    /// </summary>
    /// <exception cref="EnvelopeException">Thrown if this is a failure response.</exception>
    public Envelope Result()
    {
        if (!_isSuccess)
            throw EnvelopeException.InvalidResponse();
        return _resultOrError;
    }

    /// <summary>
    /// Extracts a typed result value from a successful response.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <returns>The result decoded as type T.</returns>
    /// <exception cref="EnvelopeException">Thrown if this is a failure response.</exception>
    public T ExtractResult<T>() => Result().ExtractSubject<T>();

    /// <summary>
    /// Returns the error envelope if this is a failure response.
    /// </summary>
    /// <exception cref="EnvelopeException">Thrown if this is a successful response.</exception>
    public Envelope Error()
    {
        if (_isSuccess)
            throw EnvelopeException.InvalidResponse();
        return _resultOrError;
    }

    /// <summary>
    /// Extracts a typed error value from a failure response.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <returns>The error decoded as type T.</returns>
    /// <exception cref="EnvelopeException">Thrown if this is a successful response.</exception>
    public T ExtractError<T>() => Error().ExtractSubject<T>();

    // --- Summary ---

    /// <summary>
    /// Returns a human-readable summary.
    /// </summary>
    public string Summary()
    {
        if (_isSuccess)
            return $"id: {_id!.ShortDescription()}, result: {_resultOrError}";
        var idStr = _id is not null ? _id.ShortDescription() : "'Unknown'";
        return $"id: {idStr} error: {_resultOrError}";
    }

    // --- Envelope Conversion ---

    /// <summary>
    /// Converts this response to an envelope.
    /// </summary>
    public Envelope ToEnvelope()
    {
        var responseTag = GlobalTags.TagsForValues(BcTags.TagResponse)[0];

        if (_isSuccess)
        {
            return Envelope.Create(Cbor.ToTaggedValue(responseTag, _id!.TaggedCbor()))
                .AddAssertion(KnownValuesRegistry.Result, _resultOrError);
        }

        Envelope subject;
        if (_id is not null)
        {
            subject = Envelope.Create(Cbor.ToTaggedValue(responseTag, _id.TaggedCbor()));
        }
        else
        {
            subject = Envelope.Create(Cbor.ToTaggedValue(responseTag, KnownValuesRegistry.UnknownValue.TaggedCbor()));
        }
        return subject.AddAssertion(KnownValuesRegistry.Error, _resultOrError);
    }

    /// <summary>
    /// Parses a response from an envelope.
    /// </summary>
    /// <param name="envelope">The envelope to parse.</param>
    /// <returns>A new <see cref="Response"/>.</returns>
    /// <exception cref="EnvelopeException">Thrown if the envelope is not a valid response.</exception>
    public static Response FromEnvelope(Envelope envelope)
    {
        bool hasResult;
        try
        {
            envelope.AssertionWithPredicate(KnownValuesRegistry.Result);
            hasResult = true;
        }
        catch
        {
            hasResult = false;
        }

        bool hasError;
        try
        {
            envelope.AssertionWithPredicate(KnownValuesRegistry.Error);
            hasError = true;
        }
        catch
        {
            hasError = false;
        }

        if (hasResult == hasError)
            throw EnvelopeException.InvalidResponse();

        var responseTag = GlobalTags.TagsForValues(BcTags.TagResponse)[0];
        var idCbor = envelope.Subject.TryLeaf().TryIntoExpectedTaggedValue(responseTag);

        if (hasResult)
        {
            var id = ARID.FromTaggedCbor(idCbor);
            var result = envelope.ObjectForPredicate(KnownValuesRegistry.Result);
            return new Response(true, id, result);
        }

        // Error case -- check if ID is Unknown KnownValue
        ARID? errorId;
        try
        {
            var kv = KnownValue.FromTaggedCbor(idCbor);
            if (kv == KnownValuesRegistry.UnknownValue)
                errorId = null;
            else
                throw EnvelopeException.InvalidResponse();
        }
        catch (EnvelopeException)
        {
            throw;
        }
        catch
        {
            errorId = ARID.FromTaggedCbor(idCbor);
        }

        var error = envelope.ObjectForPredicate(KnownValuesRegistry.Error);
        return new Response(false, errorId, error);
    }

    // --- Equality ---

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not Response other) return false;
        if (ReferenceEquals(this, other)) return true;
        return _isSuccess == other._isSuccess
            && Equals(_id, other._id)
            && _resultOrError.IsEquivalentTo(other._resultOrError);
    }

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(_isSuccess, _id, _resultOrError);

    /// <inheritdoc/>
    public override string ToString() => $"Response({Summary()})";
}
