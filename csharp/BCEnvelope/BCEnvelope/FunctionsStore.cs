namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// A store that maps functions to their assigned names.
/// </summary>
/// <remarks>
/// <see cref="FunctionsStore"/> maintains a registry of functions and their
/// human-readable names, which is useful for displaying and debugging
/// expression functions.
/// </remarks>
public sealed class FunctionsStore
{
    private readonly Dictionary<Function, string> _dict = new();

    /// <summary>
    /// Creates a new <see cref="FunctionsStore"/> with the given functions.
    /// </summary>
    /// <param name="functions">The functions to register.</param>
    public FunctionsStore(IEnumerable<Function> functions)
    {
        foreach (var function in functions)
            Insert(function);
    }

    /// <summary>
    /// Creates a new empty <see cref="FunctionsStore"/>.
    /// </summary>
    public FunctionsStore() { }

    /// <summary>
    /// Inserts a function into the store.
    /// </summary>
    /// <param name="function">The function to insert. Must be a known (numeric) function.</param>
    /// <exception cref="ArgumentException">If the function is not a known function.</exception>
    public void Insert(Function function)
    {
        if (!function.IsKnown)
            throw new ArgumentException("Only known functions can be inserted into FunctionsStore.");
        _dict[function] = function.Name;
    }

    /// <summary>
    /// Returns the assigned name for a function, if it exists in the store.
    /// </summary>
    /// <param name="function">The function to look up.</param>
    /// <returns>The assigned name, or <c>null</c> if not found.</returns>
    public string? AssignedName(Function function) =>
        _dict.TryGetValue(function, out var name) ? name : null;

    /// <summary>
    /// Returns the name for a function from this store or from the function itself.
    /// </summary>
    /// <param name="function">The function to look up.</param>
    /// <returns>The name of the function.</returns>
    public string NameOf(Function function) =>
        AssignedName(function) ?? function.Name;

    /// <summary>
    /// Returns the name of a function, using an optional store.
    /// </summary>
    /// <param name="function">The function to look up.</param>
    /// <param name="store">An optional <see cref="FunctionsStore"/>.</param>
    /// <returns>The name of the function.</returns>
    public static string NameForFunction(Function function, FunctionsStore? store) =>
        store?.AssignedName(function) ?? function.Name;
}

/// <summary>
/// Well-known function constants and the global functions store.
/// </summary>
public static class Functions
{
    /// <summary>Addition function (ID 1).</summary>
    public static readonly Function Add = Function.NewKnown(1, "add");
    /// <summary>Subtraction function (ID 2).</summary>
    public static readonly Function Sub = Function.NewKnown(2, "sub");
    /// <summary>Multiplication function (ID 3).</summary>
    public static readonly Function Mul = Function.NewKnown(3, "mul");
    /// <summary>Division function (ID 4).</summary>
    public static readonly Function Div = Function.NewKnown(4, "div");
    /// <summary>Unary negation function (ID 5).</summary>
    public static readonly Function Neg = Function.NewKnown(5, "neg");
    /// <summary>Less than function (ID 6).</summary>
    public static readonly Function Lt = Function.NewKnown(6, "lt");
    /// <summary>Less than or equal function (ID 7).</summary>
    public static readonly Function Le = Function.NewKnown(7, "le");
    /// <summary>Greater than function (ID 8).</summary>
    public static readonly Function Gt = Function.NewKnown(8, "gt");
    /// <summary>Greater than or equal function (ID 9).</summary>
    public static readonly Function Ge = Function.NewKnown(9, "ge");
    /// <summary>Equal function (ID 10).</summary>
    public static readonly Function Eq = Function.NewKnown(10, "eq");
    /// <summary>Not equal function (ID 11).</summary>
    public static readonly Function Ne = Function.NewKnown(11, "ne");
    /// <summary>Logical AND function (ID 12).</summary>
    public static readonly Function And = Function.NewKnown(12, "and");
    /// <summary>Logical OR function (ID 13).</summary>
    public static readonly Function Or = Function.NewKnown(13, "or");
    /// <summary>Logical XOR function (ID 14).</summary>
    public static readonly Function Xor = Function.NewKnown(14, "xor");
    /// <summary>Logical NOT function (ID 15).</summary>
    public static readonly Function Not = Function.NewKnown(15, "not");

    private static readonly Lazy<FunctionsStore> _globalStore =
        new(() => new FunctionsStore(new[] { Add, Sub, Mul, Div }),
            LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Gets the global shared store of well-known functions.
    /// </summary>
    public static FunctionsStore GlobalStore => _globalStore.Value;
}
