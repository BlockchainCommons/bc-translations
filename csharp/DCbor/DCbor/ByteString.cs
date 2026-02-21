using System.Collections;

namespace BlockchainCommons.DCbor;

/// <summary>
/// Immutable wrapper around a byte array for CBOR byte strings (major type 2).
/// </summary>
public sealed class ByteString : IEquatable<ByteString>, IComparable<ByteString>, IEnumerable<byte>
{
    private readonly byte[] _data;

    public ByteString(byte[] data)
    {
        _data = (byte[])data.Clone();
    }

    public ByteString(ReadOnlySpan<byte> data)
    {
        _data = data.ToArray();
    }

    public static ByteString Empty { get; } = new ByteString(Array.Empty<byte>());

    /// <summary>
    /// Returns a reference to the underlying byte data.
    /// </summary>
    public ReadOnlySpan<byte> Data => _data;

    /// <summary>
    /// Returns the byte data as an array (copy).
    /// </summary>
    public byte[] ToArray() => (byte[])_data.Clone();

    public int Length => _data.Length;
    public bool IsEmpty => _data.Length == 0;

    public byte this[int index] => _data[index];

    public ByteString Extend(byte[] other)
    {
        var result = new byte[_data.Length + other.Length];
        _data.CopyTo(result, 0);
        other.CopyTo(result, _data.Length);
        return new ByteString(result);
    }

    // --- Equality ---

    public bool Equals(ByteString? other)
    {
        if (other is null) return false;
        return _data.AsSpan().SequenceEqual(other._data);
    }

    public override bool Equals(object? obj) => Equals(obj as ByteString);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in _data) hash.Add(b);
        return hash.ToHashCode();
    }

    public static bool operator ==(ByteString? a, ByteString? b)
    {
        if (a is null) return b is null;
        return a.Equals(b);
    }

    public static bool operator !=(ByteString? a, ByteString? b) => !(a == b);

    // --- Comparable ---

    public int CompareTo(ByteString? other)
    {
        if (other is null) return 1;
        return _data.AsSpan().SequenceCompareTo(other._data);
    }

    // --- Conversions ---

    public static implicit operator ByteString(byte[] data) => new(data);

    public string ToHex() => Convert.ToHexString(_data).ToLowerInvariant();

    public override string ToString() => $"ByteString({ToHex()})";

    // --- IEnumerable ---

    public IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)_data).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

    // --- Internal access for encoding ---

    internal byte[] DataRef => _data;
}
