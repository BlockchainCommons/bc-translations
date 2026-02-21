namespace BlockchainCommons.BCUR;

/// <summary>
/// A UR multipart encoder that wraps fountain encoding with UR string formatting.
/// </summary>
public sealed class MultipartEncoder
{
    private readonly FountainEncoder _encoder;
    private readonly string _urType;

    /// <summary>
    /// Creates a new MultipartEncoder for the given UR with a maximum fragment length.
    /// </summary>
    public MultipartEncoder(UR ur, int maxFragmentLength)
    {
        _urType = ur.UrTypeStr;
        var data = ur.Cbor.ToCborData();
        _encoder = new FountainEncoder(data, maxFragmentLength);
    }

    /// <summary>
    /// Returns the next UR part string.
    /// </summary>
    public string NextPart()
    {
        var part = _encoder.NextPart();
        var body = Bytewords.Encode(part.ToCbor(), BytewordsStyle.Minimal);
        return $"ur:{_urType}/{part.SequenceId}/{body}";
    }

    /// <summary>
    /// Returns the current count of already emitted parts.
    /// </summary>
    public int CurrentIndex => _encoder.CurrentSequence;

    /// <summary>
    /// Returns the number of segments the original message was split into.
    /// </summary>
    public int PartsCount => _encoder.FragmentCount;
}
