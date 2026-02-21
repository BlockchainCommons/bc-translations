using System.Collections;

namespace BlockchainCommons.DCbor;

/// <summary>
/// A deterministic CBOR set. Internally implemented as a <see cref="CborMap"/>
/// where each item is stored as both key and value. Items are ordered by
/// their encoded CBOR byte representation.
/// </summary>
public sealed class CborSet : IEquatable<CborSet>, IEnumerable<Cbor>
{
    private readonly CborMap _inner = new();

    public CborSet() { }

    public int Count => _inner.Count;
    public bool IsEmpty => _inner.IsEmpty;

    /// <summary>Inserts an item into the set.</summary>
    public void Insert(Cbor value)
    {
        _inner.Insert(value, value);
    }

    /// <summary>
    /// Inserts the next item during decoding. Validates canonical ordering.
    /// </summary>
    internal void InsertNext(Cbor value)
    {
        _inner.InsertNext(value, value);
    }

    /// <summary>Returns true if the set contains the given value.</summary>
    public bool Contains(Cbor value)
    {
        return _inner.ContainsKey(value);
    }

    /// <summary>Returns the set contents as a list.</summary>
    public List<Cbor> ToList()
    {
        var result = new List<Cbor>();
        foreach (var (_, value) in _inner)
            result.Add(value);
        return result;
    }

    /// <summary>Creates a set from a list of CBOR values.</summary>
    public static CborSet FromList(IEnumerable<Cbor> items)
    {
        var set = new CborSet();
        foreach (var item in items)
            set.Insert(item);
        return set;
    }

    /// <summary>
    /// Creates a set from a list of CBOR values, validating canonical order.
    /// </summary>
    public static CborSet TryFromList(IEnumerable<Cbor> items)
    {
        var set = new CborSet();
        foreach (var item in items)
            set.InsertNext(item);
        return set;
    }

    /// <summary>Encodes this set to CBOR bytes (as an array).</summary>
    public byte[] CborData()
    {
        var items = ToList();
        var cbor = new Cbor(CborCase.Array(items));
        return cbor.ToCborData();
    }

    /// <summary>Converts this set to a CBOR array value.</summary>
    public Cbor ToCbor()
    {
        return new Cbor(CborCase.Array(ToList()));
    }

    public static implicit operator Cbor(CborSet set) => set.ToCbor();

    // --- IEnumerable ---

    public IEnumerator<Cbor> GetEnumerator()
    {
        foreach (var (_, value) in _inner)
            yield return value;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // --- Equality ---

    public bool Equals(CborSet? other)
    {
        if (other is null) return false;
        return _inner.Equals(other._inner);
    }

    public override bool Equals(object? obj) => Equals(obj as CborSet);
    public override int GetHashCode() => _inner.GetHashCode();

    public static bool operator ==(CborSet? a, CborSet? b)
    {
        if (a is null) return b is null;
        return a.Equals(b);
    }

    public static bool operator !=(CborSet? a, CborSet? b) => !(a == b);

    public override string ToString()
    {
        return "[" + string.Join(", ", this.Select(x => x.ToString())) + "]";
    }
}
