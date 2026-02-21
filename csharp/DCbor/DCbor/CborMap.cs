using System.Collections;

namespace BlockchainCommons.DCbor;

/// <summary>
/// A deterministic CBOR map whose keys are sorted by their encoded CBOR bytes.
/// Duplicate keys are rejected, and insertion order does not affect encoding.
/// </summary>
public sealed class CborMap : IEquatable<CborMap>, IEnumerable<(Cbor Key, Cbor Value)>
{
    private readonly SortedDictionary<MapKey, MapValue> _entries = new();

    public CborMap() { }

    public int Count => _entries.Count;
    public bool IsEmpty => _entries.Count == 0;

    /// <summary>
    /// Inserts a key-value pair. Overwrites if key already present.
    /// </summary>
    public void Insert(Cbor key, Cbor value)
    {
        var mk = new MapKey(key.ToCborData());
        _entries[mk] = new MapValue(key, value);
    }

    /// <summary>
    /// Inserts the next key during decoding. Verifies canonical ordering.
    /// </summary>
    internal void InsertNext(Cbor key, Cbor value)
    {
        var mk = new MapKey(key.ToCborData());
        if (_entries.Count == 0)
        {
            _entries[mk] = new MapValue(key, value);
            return;
        }
        if (_entries.ContainsKey(mk))
            throw new CborDuplicateMapKeyException();

        var lastKey = ((SortedDictionary<MapKey, MapValue>)_entries).Keys.Max()!;
        if (lastKey.CompareTo(mk) >= 0)
            throw new CborMisorderedMapKeyException();

        _entries[mk] = new MapValue(key, value);
    }

    /// <summary>
    /// Gets a value from the map given a key, converting both key and value types.
    /// Returns null/default if key not found or value conversion fails.
    /// </summary>
    public Cbor? GetValue(Cbor key)
    {
        var mk = new MapKey(key.ToCborData());
        return _entries.TryGetValue(mk, out var mv) ? mv.Value : null;
    }

    public bool ContainsKey(Cbor key)
    {
        var mk = new MapKey(key.ToCborData());
        return _entries.ContainsKey(mk);
    }

    /// <summary>
    /// Extracts a typed value from the map using a typed key.
    /// The key is converted to CBOR for lookup, and the value is converted
    /// from CBOR using the provided extraction function.
    /// Throws <see cref="CborWrongTypeException"/> if the key is not found.
    /// </summary>
    public Cbor Extract(Cbor key)
    {
        return GetValue(key) ?? throw new CborWrongTypeException();
    }

    /// <summary>
    /// Encodes this map to CBOR bytes.
    /// </summary>
    internal byte[] CborData()
    {
        var pairs = new List<(byte[] keyData, byte[] valueData)>();
        foreach (var kv in _entries)
        {
            pairs.Add((kv.Key.Data, kv.Value.Value.ToCborData()));
        }

        var buf = new List<byte>(Varint.EncodeVarInt((ulong)pairs.Count, MajorType.Map));
        foreach (var (keyData, valueData) in pairs)
        {
            buf.AddRange(keyData);
            buf.AddRange(valueData);
        }
        return buf.ToArray();
    }

    // --- Equality ---

    public bool Equals(CborMap? other)
    {
        if (other is null) return false;
        if (_entries.Count != other._entries.Count) return false;
        using var e1 = _entries.GetEnumerator();
        using var e2 = other._entries.GetEnumerator();
        while (e1.MoveNext() && e2.MoveNext())
        {
            if (!e1.Current.Key.Equals(e2.Current.Key)) return false;
            if (!e1.Current.Value.Equals(e2.Current.Value)) return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as CborMap);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var kv in _entries)
        {
            hash.Add(kv.Key);
            hash.Add(kv.Value);
        }
        return hash.ToHashCode();
    }

    // --- IEnumerable ---

    public IEnumerator<(Cbor Key, Cbor Value)> GetEnumerator()
    {
        foreach (var kv in _entries)
        {
            yield return (kv.Value.Key, kv.Value.Value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // --- Debug ---

    public override string ToString()
    {
        var items = new List<string>();
        foreach (var (key, value) in this)
        {
            items.Add($"{key}: {value}");
        }
        return "{" + string.Join(", ", items) + "}";
    }

    // --- Internal types ---

    private sealed class MapKey : IComparable<MapKey>, IEquatable<MapKey>
    {
        internal readonly byte[] Data;

        internal MapKey(byte[] data) { Data = data; }

        public int CompareTo(MapKey? other)
        {
            if (other is null) return 1;
            return Data.AsSpan().SequenceCompareTo(other.Data);
        }

        public bool Equals(MapKey? other)
        {
            if (other is null) return false;
            return Data.AsSpan().SequenceEqual(other.Data);
        }

        public override bool Equals(object? obj) => Equals(obj as MapKey);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var b in Data) hash.Add(b);
            return hash.ToHashCode();
        }
    }

    private sealed class MapValue : IEquatable<MapValue>
    {
        internal readonly Cbor Key;
        internal readonly Cbor Value;

        internal MapValue(Cbor key, Cbor value) { Key = key; Value = value; }

        public bool Equals(MapValue? other)
        {
            if (other is null) return false;
            return Key.Equals(other.Key) && Value.Equals(other.Value);
        }

        public override bool Equals(object? obj) => Equals(obj as MapValue);

        public override int GetHashCode() => HashCode.Combine(Key, Value);
    }
}
