namespace BlockchainCommons.BCUR;

/// <summary>
/// A fountain decoder that receives parts and reconstructs the original message
/// using XOR-based fountain decoding.
/// </summary>
internal sealed class FountainDecoder
{
    private readonly SortedDictionary<int, FountainPart> _decoded = new();
    private readonly HashSet<string> _received = new();
    private readonly Dictionary<string, FountainPart> _buffer = new();
    private readonly Stack<(int Index, FountainPart Part)> _queue = new();
    private int _sequenceCount;
    private int _messageLength;
    private uint _checksum;
    private int _fragmentLength;

    /// <summary>
    /// Returns whether the decoder has received enough parts to reconstruct the message.
    /// </summary>
    internal bool IsComplete => _messageLength != 0 && _decoded.Count == _sequenceCount;

    /// <summary>
    /// Receives a fountain-encoded part into the decoder.
    /// Returns true if the part was new and useful, false if already received or decoder complete.
    /// </summary>
    internal bool Receive(FountainPart part)
    {
        if (IsComplete) return false;

        if (part.SequenceCount == 0 || part.Data.Length == 0 || part.MessageLength == 0)
            throw new FountainException("expected non-empty part");

        if (_received.Count == 0)
        {
            _sequenceCount = part.SequenceCount;
            _messageLength = part.MessageLength;
            _checksum = part.Checksum;
            _fragmentLength = part.Data.Length;
        }
        else if (!Validate(part))
        {
            throw new FountainException("part is inconsistent with previous ones");
        }

        var indexes = part.Indexes();
        var key = IndexKey(indexes);
        if (_received.Contains(key))
            return false;

        _received.Add(key);

        if (part.IsSimple)
        {
            ProcessSimple(part);
        }
        else
        {
            ProcessComplex(part);
        }
        return true;
    }

    /// <summary>
    /// Checks whether a part is consistent with previously received parts.
    /// Returns false for a fresh decoder with no received parts.
    /// </summary>
    internal bool Validate(FountainPart part)
    {
        if (_received.Count == 0) return false;

        return part.SequenceCount == _sequenceCount
            && part.MessageLength == _messageLength
            && part.Checksum == _checksum
            && part.Data.Length == _fragmentLength;
    }

    /// <summary>
    /// If complete, returns the decoded message. Otherwise returns null.
    /// </summary>
    internal byte[]? Message()
    {
        if (!IsComplete) return null;

        var combined = new byte[_sequenceCount * _fragmentLength];
        for (int i = 0; i < _sequenceCount; i++)
        {
            if (!_decoded.TryGetValue(i, out var part))
                throw new FountainException("expected item");
            Array.Copy(part.Data, 0, combined, i * _fragmentLength, _fragmentLength);
        }

        // Verify padding
        for (int i = _messageLength; i < combined.Length; i++)
        {
            if (combined[i] != 0)
                throw new FountainException("invalid padding");
        }

        var result = new byte[_messageLength];
        Array.Copy(combined, result, _messageLength);
        return result;
    }

    private void ProcessSimple(FountainPart part)
    {
        var indexes = part.Indexes();
        var index = indexes[0];
        _decoded[index] = part.Clone();
        _queue.Push((index, part));
        ProcessQueue();
    }

    private void ProcessQueue()
    {
        while (_queue.Count > 0)
        {
            var (index, simple) = _queue.Pop();

            var toProcess = _buffer.Keys
                .Where(k => ParseIndexKey(k).Contains(index))
                .ToList();

            foreach (var key in toProcess)
            {
                var part = _buffer[key];
                _buffer.Remove(key);

                var keyIndexes = ParseIndexKey(key);
                keyIndexes.Remove(index);
                FountainUtils.Xor(part.Data, simple.Data);

                if (keyIndexes.Count == 1)
                {
                    var newIdx = keyIndexes[0];
                    _decoded[newIdx] = part.Clone();
                    _queue.Push((newIdx, part));
                }
                else
                {
                    _buffer[IndexKey(keyIndexes)] = part;
                }
            }
        }
    }

    private void ProcessComplex(FountainPart part)
    {
        var indexes = part.Indexes();

        var toRemove = indexes.Where(idx => _decoded.ContainsKey(idx)).ToList();
        if (indexes.Count == toRemove.Count)
            return;

        var workPart = part.Clone();
        var remainingIndexes = new List<int>(indexes);

        foreach (var remove in toRemove)
        {
            remainingIndexes.Remove(remove);
            FountainUtils.Xor(workPart.Data, _decoded[remove].Data);
        }

        if (remainingIndexes.Count == 1)
        {
            var idx = remainingIndexes[0];
            _decoded[idx] = workPart.Clone();
            _queue.Push((idx, workPart));
        }
        else
        {
            _buffer[IndexKey(remainingIndexes)] = workPart;
        }
    }

    private static string IndexKey(List<int> indexes)
    {
        return string.Join(",", indexes);
    }

    private static List<int> ParseIndexKey(string key)
    {
        return key.Split(',').Select(int.Parse).ToList();
    }
}
