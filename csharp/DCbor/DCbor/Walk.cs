namespace BlockchainCommons.DCbor;

/// <summary>
/// An element or element pair visited during CBOR tree traversal.
/// </summary>
public abstract class WalkElement
{
    private WalkElement() { }

    /// <summary>A single CBOR element.</summary>
    public sealed class SingleElement : WalkElement
    {
        public Cbor Value { get; }
        public SingleElement(Cbor value) { Value = value; }
    }

    /// <summary>A key-value pair from a map.</summary>
    public sealed class KeyValueElement : WalkElement
    {
        public Cbor Key { get; }
        public Cbor Value { get; }
        public KeyValueElement(Cbor key, Cbor value) { Key = key; Value = value; }
    }

    public Cbor? AsSingle() => this is SingleElement s ? s.Value : null;

    public (Cbor Key, Cbor Value)? AsKeyValue() =>
        this is KeyValueElement kv ? (kv.Key, kv.Value) : null;

    public string DiagnosticFlat()
    {
        return this switch
        {
            SingleElement s => s.Value.DiagnosticFlat(),
            KeyValueElement kv =>
                $"{kv.Key.DiagnosticFlat()}: {kv.Value.DiagnosticFlat()}",
            _ => throw new InvalidOperationException(),
        };
    }
}

/// <summary>
/// The type of incoming edge during traversal.
/// </summary>
public abstract class EdgeType : IEquatable<EdgeType>
{
    private EdgeType() { }

    public sealed class NoneEdge : EdgeType
    {
        internal static readonly NoneEdge Instance = new();
    }

    public sealed class ArrayElementEdge : EdgeType
    {
        public int Index { get; }
        public ArrayElementEdge(int index) { Index = index; }
    }

    public sealed class MapKeyValueEdge : EdgeType
    {
        internal static readonly MapKeyValueEdge Instance = new();
    }

    public sealed class MapKeyEdge : EdgeType
    {
        internal static readonly MapKeyEdge Instance = new();
    }

    public sealed class MapValueEdge : EdgeType
    {
        internal static readonly MapValueEdge Instance = new();
    }

    public sealed class TaggedContentEdge : EdgeType
    {
        internal static readonly TaggedContentEdge Instance = new();
    }

    // --- Factory ---

    public static EdgeType None => NoneEdge.Instance;
    public static EdgeType ArrayElement(int index) => new ArrayElementEdge(index);
    public static EdgeType MapKeyValue => MapKeyValueEdge.Instance;
    public static EdgeType MapKey => MapKeyEdge.Instance;
    public static EdgeType MapValue => MapValueEdge.Instance;
    public static EdgeType TaggedContent => TaggedContentEdge.Instance;

    // --- Label ---

    public string? Label()
    {
        return this switch
        {
            ArrayElementEdge a => $"arr[{a.Index}]",
            MapKeyValueEdge => "kv",
            MapKeyEdge => "key",
            MapValueEdge => "val",
            TaggedContentEdge => "content",
            NoneEdge => null,
            _ => null,
        };
    }

    // --- Equality ---

    public bool Equals(EdgeType? other)
    {
        if (other is null) return false;
        return (this, other) switch
        {
            (NoneEdge, NoneEdge) => true,
            (ArrayElementEdge a, ArrayElementEdge b) => a.Index == b.Index,
            (MapKeyValueEdge, MapKeyValueEdge) => true,
            (MapKeyEdge, MapKeyEdge) => true,
            (MapValueEdge, MapValueEdge) => true,
            (TaggedContentEdge, TaggedContentEdge) => true,
            _ => false,
        };
    }

    public override bool Equals(object? obj) => Equals(obj as EdgeType);

    public override int GetHashCode()
    {
        return this switch
        {
            NoneEdge => 0,
            ArrayElementEdge a => HashCode.Combine(1, a.Index),
            MapKeyValueEdge => 2,
            MapKeyEdge => 3,
            MapValueEdge => 4,
            TaggedContentEdge => 5,
            _ => -1,
        };
    }
}

/// <summary>
/// A visitor function called for each element during CBOR tree traversal.
/// Returns (newState, stop) where stop=true prevents descent into children.
/// </summary>
public delegate (TState State, bool Stop) CborVisitor<TState>(
    WalkElement element, int level, EdgeType edge, TState state);

/// <summary>
/// Walk extension methods for CBOR traversal.
/// </summary>
public static class CborWalk
{
    /// <summary>
    /// Walks the CBOR structure, calling the visitor for each element.
    /// </summary>
    public static void Walk<TState>(this Cbor cbor, TState initialState, CborVisitor<TState> visitor)
    {
        WalkInternal(cbor, 0, EdgeType.None, initialState, visitor);
    }

    private static void WalkInternal<TState>(
        Cbor cbor, int level, EdgeType incomingEdge, TState state, CborVisitor<TState> visitor)
    {
        // Visit this element as a single element
        var element = new WalkElement.SingleElement(cbor);
        var (newState, stop) = visitor(element, level, incomingEdge, state);
        if (stop) return;

        int nextLevel = level + 1;
        switch (cbor.Case)
        {
            case CborCase.ArrayCase a:
                for (int i = 0; i < a.Value.Count; i++)
                {
                    WalkInternal(a.Value[i], nextLevel, EdgeType.ArrayElement(i), newState, visitor);
                }
                break;

            case CborCase.MapCase m:
                foreach (var (key, value) in m.Value)
                {
                    // First visit the key-value pair as a semantic unit
                    var kvElement = new WalkElement.KeyValueElement(key, value);
                    var (kvState, kvStop) = visitor(kvElement, nextLevel, EdgeType.MapKeyValue, newState);
                    if (kvStop) continue;

                    // Then visit key and value individually
                    WalkInternal(key, nextLevel, EdgeType.MapKey, kvState, visitor);
                    WalkInternal(value, nextLevel, EdgeType.MapValue, kvState, visitor);
                }
                break;

            case CborCase.TaggedCase tg:
                WalkInternal(tg.Item, nextLevel, EdgeType.TaggedContent, newState, visitor);
                break;

            // Primitives have no children
            default:
                break;
        }
    }
}
