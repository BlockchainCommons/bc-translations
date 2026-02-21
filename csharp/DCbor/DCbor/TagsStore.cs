namespace BlockchainCommons.DCbor;

/// <summary>
/// A delegate for summarizing tagged CBOR values as human-readable strings.
/// </summary>
/// <param name="untaggedCbor">The CBOR content without the tag.</param>
/// <param name="flat">Whether flat formatting is requested.</param>
/// <returns>A human-readable summary string.</returns>
public delegate string CborSummarizer(Cbor untaggedCbor, bool flat);

/// <summary>
/// Discriminated union for specifying which tag store to use for formatting.
/// </summary>
public abstract class TagsStoreOption
{
    private TagsStoreOption() { }

    public sealed class None : TagsStoreOption { internal static readonly None Instance = new(); }
    public sealed class Global : TagsStoreOption { internal static readonly Global Instance = new(); }
    public sealed class Custom : TagsStoreOption
    {
        public TagsStore Store { get; }
        public Custom(TagsStore store) { Store = store; }
    }

    public static TagsStoreOption NoTags => None.Instance;
    public static implicit operator TagsStoreOption(TagsStore store) => new Custom(store);

    // Default is Global
    internal static TagsStoreOption DefaultGlobal => Global.Instance;
}

/// <summary>
/// Registry that maps between CBOR tag values and human-readable names,
/// with optional summarizer delegates.
/// </summary>
public class TagsStore
{
    private readonly Dictionary<ulong, Tag> _tagsByValue = new();
    private readonly Dictionary<string, Tag> _tagsByName = new();
    private readonly Dictionary<ulong, CborSummarizer> _summarizers = new();

    public TagsStore() { }

    public TagsStore(IEnumerable<Tag> tags)
    {
        foreach (var tag in tags)
            Insert(tag);
    }

    public void Insert(Tag tag)
    {
        string name = tag.Name ?? throw new ArgumentException("Tag must have a name for registration");
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Tag name must not be empty");

        if (_tagsByValue.TryGetValue(tag.Value, out var existing))
        {
            if (existing.Name != name)
                throw new InvalidOperationException(
                    $"Attempt to register tag: {tag.Value} '{existing.Name}' with different name: '{name}'");
        }

        _tagsByValue[tag.Value] = tag;
        _tagsByName[name] = tag;
    }

    public void InsertAll(IEnumerable<Tag> tags)
    {
        foreach (var tag in tags)
            Insert(tag);
    }

    public void SetSummarizer(ulong tagValue, CborSummarizer summarizer)
    {
        _summarizers[tagValue] = summarizer;
    }

    // --- Lookup ---

    public string? AssignedNameForTag(Tag tag)
    {
        return _tagsByValue.TryGetValue(tag.Value, out var t) ? t.Name : null;
    }

    public string NameForTag(Tag tag)
    {
        return AssignedNameForTag(tag) ?? tag.Value.ToString();
    }

    public Tag? TagForValue(ulong value)
    {
        return _tagsByValue.TryGetValue(value, out var tag) ? tag : null;
    }

    public Tag? TagForName(string name)
    {
        return _tagsByName.TryGetValue(name, out var tag) ? tag : null;
    }

    public string NameForValue(ulong value)
    {
        var tag = TagForValue(value);
        return tag?.Name ?? value.ToString();
    }

    public CborSummarizer? GetSummarizer(ulong tagValue)
    {
        return _summarizers.TryGetValue(tagValue, out var s) ? s : null;
    }

}
