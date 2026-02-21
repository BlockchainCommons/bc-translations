namespace BlockchainCommons.BCUR;

/// <summary>
/// A fountain encoder that splits a message into fragments and emits an
/// unbounded stream of parts, including original and XOR-combined fragments.
/// </summary>
internal sealed class FountainEncoder
{
    private readonly List<byte[]> _parts;
    private readonly int _messageLength;
    private readonly uint _checksum;
    private int _currentSequence;

    internal FountainEncoder(byte[] message, int maxFragmentLength)
    {
        if (message.Length == 0)
            throw new FountainException("expected non-empty message");
        if (maxFragmentLength <= 0)
            throw new FountainException("expected positive maximum fragment length");

        _messageLength = message.Length;
        _checksum = Crc32.Checksum(message);

        var fragmentLength = FountainUtils.FragmentLength(message.Length, maxFragmentLength);
        _parts = FountainUtils.Partition(message, fragmentLength);
        _currentSequence = 0;
    }

    /// <summary>
    /// Returns the current count of how many parts have been emitted.
    /// </summary>
    internal int CurrentSequence => _currentSequence;

    /// <summary>
    /// Returns the number of fragments the message was split into.
    /// </summary>
    internal int FragmentCount => _parts.Count;

    /// <summary>
    /// Whether all original fragments have been emitted at least once.
    /// </summary>
    internal bool IsComplete => _currentSequence >= _parts.Count;

    /// <summary>
    /// Returns the next part to be emitted.
    /// </summary>
    internal FountainPart NextPart()
    {
        _currentSequence++;
        var indexes = FountainUtils.ChooseFragments(_currentSequence, _parts.Count, _checksum);

        var mixed = new byte[_parts[0].Length];
        foreach (var idx in indexes)
        {
            FountainUtils.Xor(mixed, _parts[idx]);
        }

        return new FountainPart(_currentSequence, _parts.Count, _messageLength, _checksum, mixed);
    }
}
