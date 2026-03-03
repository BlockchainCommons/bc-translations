using System.Collections;

namespace BlockchainCommons.SSKR;

/// <summary>
/// A secret to be split into shares.
/// </summary>
public sealed class Secret : IEquatable<Secret>, IEnumerable<byte>
{
    private readonly byte[] _data;

    private Secret(ReadOnlySpan<byte> data)
    {
        _data = data.ToArray();
    }

    /// <summary>
    /// Creates a new <see cref="Secret"/> with the given data.
    /// </summary>
    /// <param name="data">The secret data to be split into shares.</param>
    /// <exception cref="SSKRException">
    /// Thrown if the secret is shorter than <see cref="Sskr.MinSecretLen"/>,
    /// longer than <see cref="Sskr.MaxSecretLen"/>, or has odd length.
    /// </exception>
    public static Secret Create(ReadOnlySpan<byte> data)
    {
        var len = data.Length;
        if (len < Sskr.MinSecretLen)
            throw new SSKRException(SskrError.SecretTooShort);
        if (len > Sskr.MaxSecretLen)
            throw new SSKRException(SskrError.SecretTooLong);
        if ((len & 1) != 0)
            throw new SSKRException(SskrError.SecretLengthNotEven);

        return new Secret(data);
    }

    /// <summary>Returns the length of the secret.</summary>
    public int Length => _data.Length;

    /// <summary>Returns <c>true</c> if the secret is empty.</summary>
    public bool IsEmpty => _data.Length == 0;

    /// <summary>Returns a read-only view of the secret data.</summary>
    public ReadOnlySpan<byte> Data => _data;

    /// <summary>Returns a cloned byte array of the secret data.</summary>
    public byte[] ToArray() => (byte[])_data.Clone();

    /// <summary>Returns a cloned copy of this <see cref="Secret"/>.</summary>
    public Secret Clone() => new(_data);

    public bool Equals(Secret? other)
    {
        if (other is null)
            return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    public override bool Equals(object? obj) => Equals(obj as Secret);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data)
            hash.Add(b);
        return hash.ToHashCode();
    }

    public static bool operator ==(Secret? left, Secret? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Secret? left, Secret? right) => !(left == right);

    public override string ToString()
    {
        return $"Secret({Convert.ToHexString(_data).ToLowerInvariant()})";
    }

    public IEnumerator<byte> GetEnumerator()
    {
        return ((IEnumerable<byte>)_data).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _data.GetEnumerator();
    }

    internal byte[] DataRef => _data;
}
