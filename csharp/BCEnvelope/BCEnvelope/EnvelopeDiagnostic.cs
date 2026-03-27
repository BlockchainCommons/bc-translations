using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// CBOR diagnostic and hex format methods (partial class on Envelope).
/// </summary>
public partial class Envelope
{
    /// <summary>
    /// Returns the CBOR diagnostic notation for this envelope, with annotations.
    /// </summary>
    public string DiagnosticAnnotated()
    {
        return GlobalFormatContext.WithFormatContext(context =>
            TaggedCbor().DiagnosticOpt(new DiagFormatOptions()
                .WithAnnotate(true)
                .WithTags(new TagsStoreOption.Custom(context.Tags))));
    }

    /// <summary>
    /// Returns the CBOR diagnostic notation for this envelope with custom options.
    /// </summary>
    public string DiagnosticOpt(DiagFormatOptions opts)
    {
        return TaggedCbor().DiagnosticOpt(opts);
    }

    /// <summary>
    /// Returns the CBOR diagnostic notation for this envelope.
    /// </summary>
    public string Diagnostic()
    {
        return TaggedCbor().Diagnostic();
    }

    /// <summary>
    /// Returns the CBOR hex dump of this envelope with the given options.
    /// </summary>
    public string HexOpt(HexFormatOptions opts)
    {
        var cbor = TaggedCbor();
        return cbor.HexOpt(opts);
    }

    /// <summary>
    /// Returns the annotated CBOR hex dump of this envelope.
    /// </summary>
    public string HexDump()
    {
        return HexOpt(new HexFormatOptions { Annotate = true });
    }

    /// <summary>
    /// Returns the raw CBOR hex string of this envelope.
    /// </summary>
    public string HexString()
    {
        return TaggedCbor().Hex();
    }
}
