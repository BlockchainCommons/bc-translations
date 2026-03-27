namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// A store that maps parameters to their assigned names.
/// </summary>
/// <remarks>
/// <see cref="ParametersStore"/> maintains a registry of parameters and their
/// human-readable names, which is useful for displaying and debugging
/// expression parameters.
/// </remarks>
public sealed class ParametersStore
{
    private readonly Dictionary<Parameter, string> _dict = new();

    /// <summary>
    /// Creates a new <see cref="ParametersStore"/> with the given parameters.
    /// </summary>
    /// <param name="parameters">The parameters to register.</param>
    public ParametersStore(IEnumerable<Parameter> parameters)
    {
        foreach (var parameter in parameters)
            Insert(parameter);
    }

    /// <summary>
    /// Creates a new empty <see cref="ParametersStore"/>.
    /// </summary>
    public ParametersStore() { }

    /// <summary>
    /// Inserts a parameter into the store.
    /// </summary>
    /// <param name="parameter">The parameter to insert. Must be a known (numeric) parameter.</param>
    /// <exception cref="ArgumentException">If the parameter is not a known parameter.</exception>
    public void Insert(Parameter parameter)
    {
        if (!parameter.IsKnown)
            throw new ArgumentException("Only known parameters can be inserted into ParametersStore.");
        _dict[parameter] = parameter.Name;
    }

    /// <summary>
    /// Returns the assigned name for a parameter, if it exists in the store.
    /// </summary>
    /// <param name="parameter">The parameter to look up.</param>
    /// <returns>The assigned name, or <c>null</c> if not found.</returns>
    public string? AssignedName(Parameter parameter) =>
        _dict.TryGetValue(parameter, out var name) ? name : null;

    /// <summary>
    /// Returns the name for a parameter from this store or from the parameter itself.
    /// </summary>
    /// <param name="parameter">The parameter to look up.</param>
    /// <returns>The name of the parameter.</returns>
    public string NameOf(Parameter parameter) =>
        AssignedName(parameter) ?? parameter.Name;

    /// <summary>
    /// Returns the name of a parameter, using an optional store.
    /// </summary>
    /// <param name="parameter">The parameter to look up.</param>
    /// <param name="store">An optional <see cref="ParametersStore"/>.</param>
    /// <returns>The name of the parameter.</returns>
    public static string NameForParameter(Parameter parameter, ParametersStore? store) =>
        store?.AssignedName(parameter) ?? parameter.Name;
}

/// <summary>
/// Well-known parameter constants and the global parameters store.
/// </summary>
public static class Parameters
{
    /// <summary>The blank parameter, used for single-parameter functions (ID 1, "_").</summary>
    public static readonly Parameter Blank = Parameter.NewKnown(1, "_");
    /// <summary>The left-hand side parameter for binary operations (ID 2, "lhs").</summary>
    public static readonly Parameter Lhs = Parameter.NewKnown(2, "lhs");
    /// <summary>The right-hand side parameter for binary operations (ID 3, "rhs").</summary>
    public static readonly Parameter Rhs = Parameter.NewKnown(3, "rhs");

    private static readonly Lazy<ParametersStore> _globalStore =
        new(() => new ParametersStore(new[] { Blank, Lhs, Rhs }),
            LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Gets the global shared store of well-known parameters.
    /// </summary>
    public static ParametersStore GlobalStore => _globalStore.Value;
}
