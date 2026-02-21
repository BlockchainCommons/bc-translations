namespace BlockchainCommons.DCbor;

/// <summary>
/// Provides sorting of collections by CBOR-encoded byte order.
/// </summary>
public static class CborSortable
{
    /// <summary>
    /// Sorts a list of CBOR values by their encoded byte representation
    /// (lexicographic order of the canonical CBOR encoding).
    /// </summary>
    public static List<Cbor> SortByCborEncoding(IEnumerable<Cbor> items)
    {
        var tagged = items.Select(item => (data: item.ToCborData(), item)).ToList();
        tagged.Sort((a, b) => a.data.AsSpan().SequenceCompareTo(b.data));
        return tagged.Select(t => t.item).ToList();
    }
}
