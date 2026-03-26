namespace BlockchainCommons.KnownValues;

/// <summary>
/// A store that maps between known values and their assigned names.
/// </summary>
public sealed class KnownValuesStore
{
    private readonly Dictionary<ulong, KnownValue> _knownValuesByRawValue;
    private readonly Dictionary<string, KnownValue> _knownValuesByAssignedName;

    /// <summary>
    /// Creates an empty <see cref="KnownValuesStore"/>.
    /// </summary>
    public KnownValuesStore()
        : this(Array.Empty<KnownValue>())
    {
    }

    /// <summary>
    /// Creates a new <see cref="KnownValuesStore"/> populated with the given
    /// known values.
    /// </summary>
    public KnownValuesStore(IEnumerable<KnownValue> knownValues)
    {
        ArgumentNullException.ThrowIfNull(knownValues);

        _knownValuesByRawValue = new Dictionary<ulong, KnownValue>();
        _knownValuesByAssignedName = new Dictionary<string, KnownValue>(StringComparer.Ordinal);

        foreach (var knownValue in knownValues)
        {
            InsertInternal(knownValue, _knownValuesByRawValue, _knownValuesByAssignedName);
        }
    }

    /// <summary>
    /// Creates a copy of this <see cref="KnownValuesStore"/>.
    /// </summary>
    public KnownValuesStore Clone() => new(_knownValuesByRawValue.Values);

    /// <summary>
    /// Inserts a known value into the store.
    /// </summary>
    public void Insert(KnownValue knownValue)
    {
        ArgumentNullException.ThrowIfNull(knownValue);

        InsertInternal(knownValue, _knownValuesByRawValue, _knownValuesByAssignedName);
    }

    /// <summary>
    /// Returns the assigned name for a known value if it exists in the store.
    /// </summary>
    public string? AssignedName(KnownValue knownValue)
    {
        ArgumentNullException.ThrowIfNull(knownValue);

        return _knownValuesByRawValue.TryGetValue(knownValue.Value, out var stored)
            ? stored.AssignedName
            : null;
    }

    /// <summary>
    /// Returns a display name for the given known value.
    /// </summary>
    public string Name(KnownValue knownValue)
    {
        ArgumentNullException.ThrowIfNull(knownValue);

        return AssignedName(knownValue) ?? knownValue.Name;
    }

    /// <summary>
    /// Looks up a known value by its assigned name.
    /// </summary>
    public KnownValue? KnownValueNamed(string assignedName)
    {
        ArgumentNullException.ThrowIfNull(assignedName);

        return _knownValuesByAssignedName.GetValueOrDefault(assignedName);
    }

    /// <summary>
    /// Retrieves a known value for the given raw value using an optional store.
    /// </summary>
    public static KnownValue KnownValueForRawValue(
        ulong rawValue,
        KnownValuesStore? knownValues)
    {
        return knownValues is not null
            && knownValues._knownValuesByRawValue.TryGetValue(rawValue, out var stored)
            ? stored
            : new KnownValue(rawValue);
    }

    /// <summary>
    /// Retrieves a known value for the given assigned name using an optional
    /// store.
    /// </summary>
    public static KnownValue? KnownValueForName(
        string name,
        KnownValuesStore? knownValues)
    {
        ArgumentNullException.ThrowIfNull(name);

        return knownValues?.KnownValueNamed(name);
    }

    /// <summary>
    /// Returns a display name for the given known value using an optional
    /// store.
    /// </summary>
    public static string NameForKnownValue(
        KnownValue knownValue,
        KnownValuesStore? knownValues)
    {
        ArgumentNullException.ThrowIfNull(knownValue);

        return knownValues?.AssignedName(knownValue) ?? knownValue.Name;
    }

    /// <summary>
    /// Loads and inserts known values from a directory containing JSON registry
    /// files.
    /// </summary>
    public int LoadFromDirectory(string path)
    {
        var values = DirectoryLoader.LoadFromDirectory(path);
        var count = values.Count;

        foreach (var value in values)
        {
            Insert(value);
        }

        return count;
    }

    /// <summary>
    /// Loads known values from directories specified by the given
    /// configuration.
    /// </summary>
    public LoadResult LoadFromConfig(DirectoryConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var result = DirectoryLoader.LoadFromConfig(config);

        foreach (var value in result.GetValues())
        {
            Insert(value);
        }

        return result;
    }

    private static void InsertInternal(
        KnownValue knownValue,
        Dictionary<ulong, KnownValue> knownValuesByRawValue,
        Dictionary<string, KnownValue> knownValuesByAssignedName)
    {
        if (knownValuesByRawValue.TryGetValue(knownValue.Value, out var oldValue)
            && oldValue.AssignedName is { } oldName)
        {
            knownValuesByAssignedName.Remove(oldName);
        }

        knownValuesByRawValue[knownValue.Value] = knownValue;

        if (knownValue.AssignedName is { } assignedName)
        {
            knownValuesByAssignedName[assignedName] = knownValue;
        }
    }
}
