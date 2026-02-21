using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCUR;

/// <summary>
/// A UR multipart decoder that receives UR part strings and reconstructs the original UR.
/// </summary>
public sealed class MultipartDecoder
{
    private URType? _urType;
    private readonly FountainDecoder _decoder = new();

    /// <summary>
    /// Receives a UR part string into the decoder.
    /// </summary>
    public void Receive(string value)
    {
        var decodedType = DecodeType(value);

        if (_urType is not null)
        {
            if (_urType != decodedType)
            {
                throw new UnexpectedTypeException(_urType.Value, decodedType.Value);
            }
        }
        else
        {
            _urType = decodedType;
        }

        // Decode the UR string to get the fountain part CBOR
        var (kind, data) = UREncoding.Decode(value);
        if (kind != URKind.MultiPart)
            throw new URDecoderException("Can't decode single-part UR as multi-part");

        var part = FountainPart.FromCbor(data);
        _decoder.Receive(part);
    }

    /// <summary>
    /// Returns whether the decoder is complete and the message is available.
    /// </summary>
    public bool IsComplete => _decoder.IsComplete;

    /// <summary>
    /// If complete, returns the reconstructed UR. Otherwise returns null.
    /// </summary>
    public UR? Message()
    {
        var messageData = _decoder.Message();
        if (messageData is null) return null;

        try
        {
            var cbor = Cbor.TryFromData(messageData);
            return UR.Create(_urType!, cbor);
        }
        catch (Exception ex) when (ex is not URException)
        {
            throw new URCborException(ex);
        }
    }

    private static URType DecodeType(string urString)
    {
        if (!urString.StartsWith("ur:", StringComparison.Ordinal))
            throw new InvalidSchemeException();

        var withoutScheme = urString[3..];
        var slashIndex = withoutScheme.IndexOf('/');
        if (slashIndex < 0)
            throw new InvalidTypeException();

        var typeStr = withoutScheme[..slashIndex];
        return new URType(typeStr);
    }
}
