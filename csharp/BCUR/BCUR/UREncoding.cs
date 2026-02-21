namespace BlockchainCommons.BCUR;

/// <summary>
/// Whether a UR string represents a single-part or multi-part encoding.
/// </summary>
internal enum URKind
{
    SinglePart,
    MultiPart
}

/// <summary>
/// Low-level UR encoding and decoding functions (equivalent to the ur::ur module).
/// </summary>
internal static class UREncoding
{
    /// <summary>
    /// Encodes a data payload into a single-part UR string.
    /// </summary>
    internal static string Encode(byte[] data, string urType)
    {
        var body = Bytewords.Encode(data, BytewordsStyle.Minimal);
        return $"ur:{urType}/{body}";
    }

    /// <summary>
    /// Decodes a UR string into a (kind, data) tuple.
    /// </summary>
    internal static (URKind Kind, byte[] Data) Decode(string value)
    {
        if (!value.StartsWith("ur:", StringComparison.Ordinal))
            throw new URDecoderException("Invalid scheme");

        var withoutScheme = value[3..];
        var slashIndex = withoutScheme.IndexOf('/');
        if (slashIndex < 0)
            throw new URDecoderException("No type specified");

        var urType = withoutScheme[..slashIndex];

        // Validate type characters
        foreach (var c in urType)
        {
            if (c is not ((>= 'a' and <= 'z') or (>= '0' and <= '9') or '-'))
                throw new URDecoderException("Type contains invalid characters");
        }

        var afterType = withoutScheme[(slashIndex + 1)..];

        // Check for multi-part format: "seq-count/payload"
        var lastSlash = afterType.LastIndexOf('/');
        if (lastSlash < 0)
        {
            // Single-part
            var data = Bytewords.Decode(afterType, BytewordsStyle.Minimal);
            return (URKind.SinglePart, data);
        }
        else
        {
            // Multi-part: "idx-total/payload"
            var indices = afterType[..lastSlash];
            var payload = afterType[(lastSlash + 1)..];

            var dashIndex = indices.IndexOf('-');
            if (dashIndex < 0)
                throw new URDecoderException("Invalid indices");

            var idxStr = indices[..dashIndex];
            var totalStr = indices[(dashIndex + 1)..];

            // Check for additional slashes (too many components)
            if (totalStr.Contains('/'))
                throw new URDecoderException("Invalid indices");

            if (!ushort.TryParse(idxStr, out _) || !ushort.TryParse(totalStr, out _))
                throw new URDecoderException("Invalid indices");

            var data = Bytewords.Decode(payload, BytewordsStyle.Minimal);
            return (URKind.MultiPart, data);
        }
    }
}
