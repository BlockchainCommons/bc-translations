namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Discriminated union for selecting which format context to use.
/// </summary>
public abstract class FormatContextOpt
{
    private FormatContextOpt() { }

    /// <summary>No format context.</summary>
    public sealed class NoneOpt : FormatContextOpt
    {
        internal static readonly NoneOpt Instance = new();
    }

    /// <summary>Use the global format context.</summary>
    public sealed class GlobalOpt : FormatContextOpt
    {
        internal static readonly GlobalOpt Instance = new();
    }

    /// <summary>Use a custom format context.</summary>
    public sealed class CustomOpt : FormatContextOpt
    {
        public FormatContext Context { get; }
        public CustomOpt(FormatContext context) { Context = context; }
    }

    /// <summary>Returns the singleton None instance.</summary>
    public static FormatContextOpt None => NoneOpt.Instance;

    /// <summary>Returns the singleton Global instance (the default).</summary>
    public static FormatContextOpt Global => GlobalOpt.Instance;

    /// <summary>Creates a Custom instance wrapping the given context.</summary>
    public static FormatContextOpt Custom(FormatContext context) => new CustomOpt(context);
}
