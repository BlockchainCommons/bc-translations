namespace BlockchainCommons.BCUR;

/// <summary>
/// A validated UR type string. UR types may only contain lowercase letters, digits, and hyphens.
/// </summary>
public sealed class URType : IEquatable<URType>
{
    private readonly string _value;

    /// <summary>
    /// Creates a new URType from the provided type string.
    /// </summary>
    /// <exception cref="InvalidTypeException">If the type string contains invalid characters.</exception>
    public URType(string urType)
    {
        if (string.IsNullOrEmpty(urType) || !IsValidUrType(urType))
        {
            throw new InvalidTypeException();
        }
        _value = urType;
    }

    /// <summary>
    /// Returns the string representation of the URType.
    /// </summary>
    public string Value => _value;

    private static bool IsValidUrType(string s)
    {
        foreach (var c in s)
        {
            if (c is not ((>= 'a' and <= 'z') or (>= '0' and <= '9') or '-'))
            {
                return false;
            }
        }
        return true;
    }

    public bool Equals(URType? other) => other is not null && _value == other._value;
    public override bool Equals(object? obj) => obj is URType other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    public override string ToString() => _value;

    public static bool operator ==(URType? left, URType? right) =>
        left is null ? right is null : left.Equals(right);
    public static bool operator !=(URType? left, URType? right) => !(left == right);
}
