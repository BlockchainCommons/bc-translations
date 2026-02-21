namespace BlockchainCommons.DCbor;

/// <summary>
/// Represents a CBOR tag (major type 6) with optional associated name.
/// Tags are equal if their numeric values are equal, regardless of name.
/// </summary>
public sealed class Tag : IEquatable<Tag>
{
    public ulong Value { get; }
    private readonly string? _name;

    public Tag(ulong value, string name)
    {
        Value = value;
        _name = name;
    }

    public Tag(ulong value)
    {
        Value = value;
        _name = null;
    }

    public string? Name => _name;

    // --- Equality (based on value only) ---

    public bool Equals(Tag? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as Tag);
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Tag? a, Tag? b)
    {
        if (a is null) return b is null;
        return a.Equals(b);
    }

    public static bool operator !=(Tag? a, Tag? b) => !(a == b);

    // --- Display ---

    public override string ToString() => _name ?? Value.ToString();
}
