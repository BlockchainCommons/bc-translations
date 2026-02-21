namespace BlockchainCommons.DCbor;

/// <summary>
/// Global tag registry for the dcbor library.
/// Thread-safe access to a shared <see cref="TagsStore"/>.
/// </summary>
public static class GlobalTags
{
    private static readonly object _lock = new();
    private static TagsStore? _store;

    private static TagsStore GetStore()
    {
        if (_store != null) return _store;
        lock (_lock)
        {
            _store ??= new TagsStore();
            return _store;
        }
    }

    /// <summary>
    /// Executes an action with read access to the global tags store.
    /// </summary>
    public static T WithTags<T>(Func<TagsStore, T> action)
    {
        lock (_lock)
        {
            return action(GetStore());
        }
    }

    /// <summary>
    /// Executes an action with mutable access to the global tags store.
    /// </summary>
    public static void WithTagsMut(Action<TagsStore> action)
    {
        lock (_lock)
        {
            action(GetStore());
        }
    }

    /// <summary>
    /// Registers the built-in dcbor tags (date tag 1).
    /// </summary>
    public static void RegisterTags()
    {
        WithTagsMut(store =>
        {
            RegisterTagsIn(store);
        });
    }

    /// <summary>
    /// Registers the built-in dcbor tags in a specific store.
    /// </summary>
    public static void RegisterTagsIn(TagsStore store)
    {
        store.Insert(new Tag(CborTags.TagDate, CborTags.TagNameDate));
        store.SetSummarizer(CborTags.TagDate, (untaggedCbor, _) =>
        {
            var date = CborDate.FromUntaggedCbor(untaggedCbor);
            return date.ToString();
        });
    }

    /// <summary>
    /// Converts tag values to Tag objects using the global registry.
    /// </summary>
    public static List<Tag> TagsForValues(params ulong[] values)
    {
        return WithTags(store =>
        {
            return values.Select(v => store.TagForValue(v) ?? new Tag(v)).ToList();
        });
    }
}

/// <summary>
/// Well-known CBOR tag constants.
/// </summary>
public static class CborTags
{
    public const ulong TagDate = 1;
    public const string TagNameDate = "date";
}
