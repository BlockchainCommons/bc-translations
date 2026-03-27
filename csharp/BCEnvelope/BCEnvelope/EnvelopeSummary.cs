using System.Text;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Summary rendering for envelope content.
/// </summary>
public partial class Envelope
{
    /// <summary>
    /// Returns a short summary of the envelope's content with a maximum length.
    /// </summary>
    /// <param name="maxLength">The maximum length for text content.</param>
    /// <param name="context">The formatting context.</param>
    /// <returns>A short summary string.</returns>
    public string Summary(int maxLength, FormatContext context)
    {
        return Case switch
        {
            EnvelopeCase.NodeCase => "NODE",
            EnvelopeCase.LeafCase l => l.Cbor.EnvelopeSummary(maxLength, FormatContextOpt.Custom(context)),
            EnvelopeCase.WrappedCase => "WRAPPED",
            EnvelopeCase.AssertionCase => "ASSERTION",
            EnvelopeCase.ElidedCase => "ELIDED",
            EnvelopeCase.KnownValueCase kv =>
                KnownValuesStore.KnownValueForRawValue(kv.Value.Value, context.KnownValues)
                    .ToString()
                    .FlankedBy("'", "'"),
            EnvelopeCase.EncryptedCase => "ENCRYPTED",
            EnvelopeCase.CompressedCase => "COMPRESSED",
            _ => throw new InvalidOperationException(),
        };
    }
}

/// <summary>
/// Extension methods for CBOR envelope summary rendering.
/// </summary>
public static class CborEnvelopeSummary
{
    /// <summary>
    /// Returns a summary of a CBOR value for use in envelope notation.
    /// </summary>
    /// <param name="cbor">The CBOR value to summarize.</param>
    /// <param name="maxLength">Maximum length for text values.</param>
    /// <param name="context">The format context option.</param>
    /// <returns>A summary string.</returns>
    public static string EnvelopeSummary(this Cbor cbor, int maxLength, FormatContextOpt context)
    {
        switch (cbor.Case)
        {
            case CborCase.UnsignedCase u:
                return u.Value.ToString();

            case CborCase.NegativeCase n:
                return (-1 - (Int128)n.Value).ToString();

            case CborCase.ByteStringCase bs:
                return $"Bytes({bs.Value.Length})";

            case CborCase.TextCase t:
            {
                var s = t.Value;
                if (s.Length > maxLength)
                {
                    s = new string(s.Take(maxLength).ToArray()) + "\u2026";
                }
                return s.Replace("\n", "\\n").FlankedBy("\"", "\"");
            }

            case CborCase.SimpleCase sv:
                return sv.Value.ToString() ?? "";

            case CborCase.ArrayCase:
            case CborCase.MapCase:
            case CborCase.TaggedCase:
                return DiagnosticWithContext(cbor, context);

            default:
                throw new InvalidOperationException();
        }
    }

    private static string DiagnosticWithContext(Cbor cbor, FormatContextOpt context)
    {
        return context switch
        {
            FormatContextOpt.NoneOpt =>
                cbor.DiagnosticOpt(new DiagFormatOptions()
                    .WithSummarize(true)
                    .WithTags(TagsStoreOption.NoTags)),

            FormatContextOpt.GlobalOpt =>
                GlobalFormatContext.WithFormatContext(ctx =>
                    cbor.DiagnosticOpt(new DiagFormatOptions()
                        .WithSummarize(true)
                        .WithTags(new TagsStoreOption.Custom(ctx.Tags)))),

            FormatContextOpt.CustomOpt custom =>
                cbor.DiagnosticOpt(new DiagFormatOptions()
                    .WithSummarize(true)
                    .WithTags(new TagsStoreOption.Custom(custom.Context.Tags))),

            _ => throw new InvalidOperationException(),
        };
    }
}
