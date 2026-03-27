namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// An expression in a Gordian Envelope.
/// </summary>
/// <remarks>
/// <para>
/// An expression consists of a function (the subject of the envelope) and zero
/// or more parameters (as assertions on the envelope). It represents a
/// computation or function call that can be evaluated.
/// </para>
/// <para>
/// Expressions form the foundation for Gordian Envelope's ability to represent
/// computations, queries, and function calls within the envelope structure.
/// </para>
/// </remarks>
public sealed class Expression
{
    private Function _function;
    private Envelope _envelope;

    private Expression(Function function, Envelope envelope)
    {
        _function = function;
        _envelope = envelope;
    }

    /// <summary>
    /// Creates a new expression with the given function.
    /// </summary>
    /// <param name="function">The function identifier for this expression.</param>
    /// <returns>A new <see cref="Expression"/>.</returns>
    public Expression(Function function)
        : this(function, Envelope.Create(function.TaggedCbor()))
    {
    }

    /// <summary>
    /// Creates a new expression with a named function.
    /// </summary>
    /// <param name="name">The function name string.</param>
    /// <returns>A new <see cref="Expression"/>.</returns>
    public Expression(string name)
        : this(Function.NewNamed(name))
    {
    }

    /// <summary>Returns the function of this expression.</summary>
    public Function Function => _function;

    /// <summary>Returns the envelope representing this expression.</summary>
    public Envelope ExpressionEnvelope => _envelope;

    /// <summary>
    /// Adds a parameter with a value to the expression.
    /// </summary>
    /// <param name="parameter">The parameter identifier.</param>
    /// <param name="value">The value for the parameter.</param>
    /// <returns>This expression with the parameter added.</returns>
    public Expression WithParameter(Parameter parameter, object value)
    {
        var assertion = Envelope.CreateAssertion(
            parameter.TaggedCbor(),
            EnvelopeExtensions.ToEnvelope(value));
        _envelope = _envelope.AddAssertionEnvelope(assertion);
        return this;
    }

    /// <summary>
    /// Adds a parameter with an optional value to the expression.
    /// </summary>
    /// <param name="parameter">The parameter identifier.</param>
    /// <param name="value">The optional value for the parameter.</param>
    /// <returns>This expression, with the parameter added if the value is not null.</returns>
    public Expression WithOptionalParameter(Parameter parameter, object? value)
    {
        if (value is not null)
            return WithParameter(parameter, value);
        return this;
    }

    /// <summary>
    /// Returns the argument (object) for the given parameter.
    /// </summary>
    /// <param name="param">The parameter to look up.</param>
    /// <returns>The argument envelope for the parameter.</returns>
    public Envelope ObjectForParameter(Parameter param) =>
        _envelope.ObjectForPredicate(param.TaggedCbor());

    /// <summary>
    /// Returns all arguments (objects) for the given parameter.
    /// </summary>
    /// <param name="param">The parameter to look up.</param>
    /// <returns>A list of all matching argument envelopes.</returns>
    public List<Envelope> ObjectsForParameter(Parameter param) =>
        _envelope.ObjectsForPredicate(param.TaggedCbor());

    /// <summary>
    /// Returns the argument for the given parameter, decoded as the specified type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="param">The parameter to look up.</param>
    /// <returns>The argument decoded as type T.</returns>
    public T ExtractObjectForParameter<T>(Parameter param) =>
        _envelope.ExtractObjectForPredicate<T>(param.TaggedCbor());

    /// <summary>
    /// Returns the argument for the given parameter, decoded as the specified type,
    /// or default if the parameter is not found.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="param">The parameter to look up.</param>
    /// <returns>The argument decoded as type T, or default if not found.</returns>
    public T? ExtractOptionalObjectForParameter<T>(Parameter param) =>
        _envelope.ExtractOptionalObjectForPredicate<T>(param.TaggedCbor());

    /// <summary>
    /// Returns all arguments for the given parameter, decoded as the specified type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="param">The parameter to look up.</param>
    /// <returns>A list of all matching arguments decoded as type T.</returns>
    public List<T> ExtractObjectsForParameter<T>(Parameter param) =>
        _envelope.ExtractObjectsForPredicate<T>(param.TaggedCbor());

    /// <summary>
    /// Converts this expression to an envelope.
    /// </summary>
    public Envelope ToEnvelope() => _envelope;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not Expression other) return false;
        if (ReferenceEquals(this, other)) return true;
        return _envelope.IsEquivalentTo(other._envelope);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => _envelope.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => _envelope.ToString() ?? "";

    /// <summary>
    /// Creates an expression from an envelope.
    /// </summary>
    /// <param name="envelope">The envelope to parse.</param>
    /// <param name="expectedFunction">An optional expected function for validation.</param>
    /// <returns>A new <see cref="Expression"/>.</returns>
    /// <exception cref="EnvelopeException">Thrown if the envelope cannot be parsed as an expression,
    /// or if the expected function does not match.</exception>
    public static Expression FromEnvelope(Envelope envelope, Function? expectedFunction = null)
    {
        var function = Function.FromTaggedCbor(envelope.Subject.TryLeaf());
        if (expectedFunction is not null && function != expectedFunction)
            throw EnvelopeException.InvalidFormat();
        return new Expression(function, envelope);
    }
}
