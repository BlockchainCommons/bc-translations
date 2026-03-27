using BlockchainCommons.BCComponents;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Context object for formatting Gordian Envelopes with annotations.
/// </summary>
/// <remarks>
/// Provides information about CBOR tags, known values, functions, and parameters
/// used to annotate the output of envelope formatting functions.
/// </remarks>
public sealed class FormatContext
{
    private readonly TagsStore _tags;
    private readonly KnownValuesStore _knownValues;
    private readonly FunctionsStore _functions;
    private readonly ParametersStore _parameters;

    /// <summary>
    /// Creates a new format context with the specified components.
    /// </summary>
    public FormatContext(
        TagsStore? tags = null,
        KnownValuesStore? knownValues = null,
        FunctionsStore? functions = null,
        ParametersStore? parameters = null)
    {
        _tags = tags ?? new TagsStore();
        _knownValues = knownValues ?? new KnownValuesStore();
        _functions = functions ?? new FunctionsStore();
        _parameters = parameters ?? new ParametersStore();
    }

    /// <summary>Returns the CBOR tags registry.</summary>
    public TagsStore Tags => _tags;

    /// <summary>Returns the known values registry.</summary>
    public KnownValuesStore KnownValues => _knownValues;

    /// <summary>Returns the functions registry.</summary>
    public FunctionsStore Functions => _functions;

    /// <summary>Returns the parameters registry.</summary>
    public ParametersStore Parameters => _parameters;

    // --- TagsStore delegation ---

    /// <summary>Returns the assigned name for a tag if one exists.</summary>
    public string? AssignedNameForTag(Tag tag) => _tags.AssignedNameForTag(tag);

    /// <summary>Returns a name for a tag, either the assigned name or a generic representation.</summary>
    public string NameForTag(Tag tag) => _tags.NameForTag(tag);

    /// <summary>Looks up a tag by its name.</summary>
    public Tag? TagForName(string name) => _tags.TagForName(name);

    /// <summary>Looks up a tag by its numeric value.</summary>
    public Tag? TagForValue(ulong value) => _tags.TagForValue(value);

    /// <summary>Returns a CBOR summarizer for a tag value if one exists.</summary>
    public CborSummarizer? GetSummarizer(ulong tagValue) => _tags.GetSummarizer(tagValue);

    /// <summary>Returns a name for a tag value.</summary>
    public string NameForValue(ulong value) => _tags.NameForValue(value);
}

/// <summary>
/// Thread-safe global format context singleton.
/// </summary>
public static class GlobalFormatContext
{
    private static readonly object _lock = new();
    private static FormatContext? _context;

    /// <summary>
    /// Gets the global format context, initializing it if necessary.
    /// </summary>
    public static FormatContext Get()
    {
        if (_context is not null)
            return _context;

        lock (_lock)
        {
            if (_context is not null)
                return _context;

            // Ensure component tags are registered in the global dcbor tag store
            TagsRegistry.RegisterTags();

            var ctx = new FormatContext(
                tags: new TagsStore(),
                knownValues: KnownValuesRegistry.KnownValues.Get(),
                functions: Functions.GlobalStore,
                parameters: Parameters.GlobalStore);

            RegisterTagsIn(ctx);
            _context = ctx;
            return _context;
        }
    }

    /// <summary>
    /// Executes an action with read access to the global format context.
    /// </summary>
    public static T WithFormatContext<T>(Func<FormatContext, T> action)
    {
        return action(Get());
    }

    /// <summary>
    /// Executes an action with the global format context (void version).
    /// </summary>
    public static void WithFormatContext(Action<FormatContext> action)
    {
        action(Get());
    }

    /// <summary>
    /// Registers standard tags and summarizers in a format context.
    /// </summary>
    public static void RegisterTagsIn(FormatContext context)
    {
        // Register standard component tags
        TagsRegistry.RegisterTagsIn(context.Tags);

        // Known value summarizer
        var knownValues = context.KnownValues;
        context.Tags.SetSummarizer(BcTags.TagKnownValue, (untaggedCbor, _) =>
        {
            var kv = KnownValue.FromUntaggedCbor(untaggedCbor);
            return knownValues.Name(kv).FlankedBy("'", "'");
        });

        // Function summarizer
        var functions = context.Functions;
        context.Tags.SetSummarizer(BcTags.TagFunction, (untaggedCbor, _) =>
        {
            var f = Function.FromUntaggedCbor(untaggedCbor);
            return FunctionsStore.NameForFunction(f, functions).FlankedBy("\u00AB", "\u00BB");
        });

        // Parameter summarizer
        var parameters = context.Parameters;
        context.Tags.SetSummarizer(BcTags.TagParameter, (untaggedCbor, _) =>
        {
            var p = Parameter.FromUntaggedCbor(untaggedCbor);
            return ParametersStore.NameForParameter(p, parameters).FlankedBy("\u2770", "\u2771");
        });

        // Request summarizer
        var clonedContext1 = context;
        context.Tags.SetSummarizer(BcTags.TagRequest, (untaggedCbor, flat) =>
        {
            var e = Envelope.CreateLeaf(untaggedCbor);
            var formatted = e.FormatOpt(new EnvelopeFormatOpts(flat, FormatContextOpt.Custom(clonedContext1)));
            return formatted.FlankedBy("request(", ")");
        });

        // Response summarizer
        var clonedContext2 = context;
        context.Tags.SetSummarizer(BcTags.TagResponse, (untaggedCbor, flat) =>
        {
            var e = Envelope.CreateLeaf(untaggedCbor);
            var formatted = e.FormatOpt(new EnvelopeFormatOpts(flat, FormatContextOpt.Custom(clonedContext2)));
            return formatted.FlankedBy("response(", ")");
        });

        // Event summarizer
        var clonedContext3 = context;
        context.Tags.SetSummarizer(BcTags.TagEvent, (untaggedCbor, flat) =>
        {
            var e = Envelope.CreateLeaf(untaggedCbor);
            var formatted = e.FormatOpt(new EnvelopeFormatOpts(flat, FormatContextOpt.Custom(clonedContext3)));
            return formatted.FlankedBy("event(", ")");
        });
    }

    /// <summary>
    /// Registers standard tags in the global format context.
    /// </summary>
    public static void RegisterTags()
    {
        // Accessing the global format context triggers initialization
        Get();
    }
}
