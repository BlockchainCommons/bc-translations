using System.Text.Json;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.BCTags;
using BlockchainCommons.BCUR;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.ProvenanceMark;

/// <summary>
/// Canonical provenance mark representation.
/// </summary>
public sealed class ProvenanceMark : IEquatable<ProvenanceMark>, ICborTagged, ICborTaggedEncodable, ICborTaggedDecodable, IUREncodable, IURDecodable
{
    private ProvenanceMark(
        ProvenanceMarkResolution resolution,
        byte[] key,
        byte[] hash,
        byte[] chainId,
        byte[] seqBytes,
        byte[] dateBytes,
        byte[] infoBytes,
        uint sequence,
        CborDate date)
    {
        _resolution = resolution;
        _key = (byte[])key.Clone();
        _hash = (byte[])hash.Clone();
        _chainId = (byte[])chainId.Clone();
        _seqBytes = (byte[])seqBytes.Clone();
        _dateBytes = (byte[])dateBytes.Clone();
        _infoBytes = (byte[])infoBytes.Clone();
        _sequence = sequence;
        _date = date;
    }

    private readonly ProvenanceMarkResolution _resolution;
    private readonly byte[] _key;
    private readonly byte[] _hash;
    private readonly byte[] _chainId;
    private readonly byte[] _seqBytes;
    private readonly byte[] _dateBytes;
    private readonly byte[] _infoBytes;
    private readonly uint _sequence;
    private readonly CborDate _date;

    public static IReadOnlyList<Tag> CborTags => GlobalTags.TagsForValues(BcTags.TagProvenanceMark);

    public ProvenanceMarkResolution Resolution => _resolution;

    public byte[] Key => (byte[])_key.Clone();

    public byte[] Hash => (byte[])_hash.Clone();

    public byte[] ChainId => (byte[])_chainId.Clone();

    public byte[] SeqBytes => (byte[])_seqBytes.Clone();

    public byte[] DateBytes => (byte[])_dateBytes.Clone();

    public uint Sequence => _sequence;

    public CborDate Date => _date;

    public byte[] Message()
    {
        var payload = Util.Combine(_chainId, _hash, _seqBytes, _dateBytes, _infoBytes);
        return Util.Combine(_key, CryptoUtils.Obfuscate(_key, payload));
    }

    public Cbor? Info()
    {
        if (_infoBytes.Length == 0)
        {
            return null;
        }

        return Cbor.TryFromData(_infoBytes);
    }

    public byte[] Id()
    {
        var identifier = new byte[32];
        Buffer.BlockCopy(_hash, 0, identifier, 0, _hash.Length);
        if (_hash.Length < 32)
        {
            var fingerprint = Fingerprint();
            Buffer.BlockCopy(fingerprint, 0, identifier, _hash.Length, 32 - _hash.Length);
        }
        return identifier;
    }

    public string IdHex() => Util.ToHex(Id());

    public string IdBytewords(int wordCount, bool prefix)
    {
        ValidateWordCount(wordCount);
        var words = Util.EncodeIdWords(Id().AsSpan(0, wordCount));
        return prefix ? $"🅟 {words}" : words;
    }

    public string IdBytemoji(int wordCount, bool prefix)
    {
        ValidateWordCount(wordCount);
        var emojis = Util.EncodeIdBytemojis(Id().AsSpan(0, wordCount));
        return prefix ? $"🅟 {emojis}" : emojis;
    }

    public string IdBytewordsMinimal(int wordCount, bool prefix)
    {
        ValidateWordCount(wordCount);
        var minimal = Util.EncodeIdMinimal(Id().AsSpan(0, wordCount));
        return prefix ? $"🅟 {minimal}" : minimal;
    }

    public bool Precedes(ProvenanceMark next)
    {
        try
        {
            PrecedesOrThrow(next);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void PrecedesOrThrow(ProvenanceMark next)
    {
        if (next.Sequence == 0)
        {
            throw new ProvenanceMarkValidationException(new NonGenesisAtZeroIssue());
        }

        if (next._key.AsSpan().SequenceEqual(next._chainId))
        {
            throw new ProvenanceMarkValidationException(new InvalidGenesisKeyIssue());
        }

        if (_sequence != next.Sequence - 1)
        {
            throw new ProvenanceMarkValidationException(
                new SequenceGapIssue(_sequence + 1, next.Sequence));
        }

        if (_date.DateTimeValue > next._date.DateTimeValue)
        {
            throw new ProvenanceMarkValidationException(
                new DateOrderingIssue(_date, next._date));
        }

        var expectedHash = MakeHash(
            _resolution,
            _key,
            next._key,
            _chainId,
            _seqBytes,
            _dateBytes,
            _infoBytes);
        if (!_hash.AsSpan().SequenceEqual(expectedHash))
        {
            throw new ProvenanceMarkValidationException(
                new HashMismatchIssue(expectedHash, _hash));
        }
    }

    public bool IsGenesis() => _sequence == 0 && _key.AsSpan().SequenceEqual(_chainId);

    public string ToBytewords(BytewordsStyle style) => Bytewords.Encode(Message(), style);

    public string ToBytewords() => ToBytewords(BytewordsStyle.Standard);

    public string ToUrlEncoding() => Bytewords.Encode(TaggedCbor().ToCborData(), BytewordsStyle.Minimal);

    public Uri ToUrl(string baseUrl)
    {
        try
        {
            var builder = new UriBuilder(baseUrl);
            var existingQuery = builder.Query;
            var queryPrefix = string.IsNullOrEmpty(existingQuery)
                ? string.Empty
                : existingQuery.TrimStart('?') + "&";
            builder.Query = $"{queryPrefix}provenance={ToUrlEncoding()}";
            return builder.Uri;
        }
        catch (Exception ex)
        {
            throw ProvenanceMarkException.Url(ex.Message, ex);
        }
    }

    public string ToJson()
    {
        var fields = new Dictionary<string, object?>
        {
            ["seq"] = _sequence,
            ["date"] = Util.DateToIso8601(_date),
            ["res"] = _resolution.Code,
            ["chain_id"] = Util.ToBase64(_chainId),
            ["key"] = Util.ToBase64(_key),
            ["hash"] = Util.ToBase64(_hash)
        };
        if (_infoBytes.Length > 0)
        {
            fields["info_bytes"] = Util.ToBase64(_infoBytes);
        }
        return Util.SerializeJson(fields);
    }

    public string DebugString()
    {
        var components = new List<string>
        {
            $"key: {Util.ToHex(_key)}",
            $"hash: {Util.ToHex(_hash)}",
            $"chainID: {Util.ToHex(_chainId)}",
            $"seq: {_sequence}",
            $"date: {Util.DateToIso8601(_date)}"
        };

        var info = Info();
        if (info is not null)
        {
            components.Add($"info: {info.Diagnostic()}");
        }

        return $"ProvenanceMark({string.Join(", ", components)})";
    }

    public byte[] Fingerprint() => CryptoUtils.Sha256(TaggedCbor().ToCborData());

    public UR ToUr() => UR.Create(BcTags.TagNameProvenanceMark, UntaggedCbor());

    public string ToUrString() => ToUr().ToUrString();

    public Cbor ToCbor() => TaggedCbor();

    public Cbor UntaggedCbor()
    {
        return Cbor.FromList(
        [
            _resolution.ToCbor(),
            Cbor.ToByteString(Message())
        ]);
    }

    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    public Envelope ToEnvelope() => Envelope.Create(TaggedCbor());

    public override string ToString() => $"ProvenanceMark({IdHex()})";

    public bool Equals(ProvenanceMark? other)
    {
        return other is not null &&
            _resolution == other._resolution &&
            Message().AsSpan().SequenceEqual(other.Message());
    }

    public override bool Equals(object? obj) => Equals(obj as ProvenanceMark);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_resolution);
        foreach (var value in Message())
        {
            hash.Add(value);
        }
        return hash.ToHashCode();
    }

    public static ProvenanceMark Create(
        ProvenanceMarkResolution resolution,
        byte[] key,
        byte[] nextKey,
        byte[] chainId,
        uint sequence,
        CborDate date,
        object? info = null)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(nextKey);
        ArgumentNullException.ThrowIfNull(chainId);

        if (key.Length != resolution.LinkLength())
        {
            throw ProvenanceMarkException.InvalidKeyLength(resolution.LinkLength(), key.Length);
        }

        if (nextKey.Length != resolution.LinkLength())
        {
            throw ProvenanceMarkException.InvalidNextKeyLength(resolution.LinkLength(), nextKey.Length);
        }

        if (chainId.Length != resolution.LinkLength())
        {
            throw ProvenanceMarkException.InvalidChainIdLength(resolution.LinkLength(), chainId.Length);
        }

        var dateBytes = resolution.SerializeDate(date);
        var seqBytes = resolution.SerializeSeq(sequence);
        var normalizedDate = resolution.DeserializeDate(dateBytes);
        var infoBytes = info is null ? Array.Empty<byte>() : Util.AnyToCborData(info);

        var hash = MakeHash(resolution, key, nextKey, chainId, seqBytes, dateBytes, infoBytes);
        return new ProvenanceMark(
            resolution,
            key,
            hash,
            chainId,
            seqBytes,
            dateBytes,
            infoBytes,
            sequence,
            normalizedDate);
    }

    public static ProvenanceMark FromMessage(ProvenanceMarkResolution resolution, byte[] message)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(message);

        if (message.Length < resolution.FixedLength())
        {
            throw ProvenanceMarkException.InvalidMessageLength(resolution.FixedLength(), message.Length);
        }

        var key = Util.Slice(message, resolution.KeyRange());
        var payload = CryptoUtils.Obfuscate(key, message[resolution.LinkLength()..]);
        var hash = Util.Slice(payload, resolution.HashRange());
        var chainId = Util.Slice(payload, resolution.ChainIdRange());
        var seqBytes = Util.Slice(payload, resolution.SeqBytesRange());
        var sequence = resolution.DeserializeSeq(seqBytes);
        var dateBytes = Util.Slice(payload, resolution.DateBytesRange());
        var date = resolution.DeserializeDate(dateBytes);
        var infoStart = resolution.InfoRangeStart();
        var infoBytes = infoStart < payload.Length ? payload[infoStart..] : Array.Empty<byte>();

        if (infoBytes.Length > 0)
        {
            try
            {
                _ = Cbor.TryFromData(infoBytes);
            }
            catch (Exception)
            {
                throw ProvenanceMarkException.InvalidInfoCbor();
            }
        }

        return new ProvenanceMark(
            resolution,
            key,
            hash,
            chainId,
            seqBytes,
            dateBytes,
            infoBytes,
            sequence,
            date);
    }

    public static ProvenanceMark FromBytewords(ProvenanceMarkResolution resolution, string value)
    {
        try
        {
            var message = Bytewords.Decode(value, BytewordsStyle.Standard);
            return FromMessage(resolution, message);
        }
        catch (Exception ex)
        {
            throw ProvenanceMarkException.Bytewords(ex.Message, ex);
        }
    }

    public static ProvenanceMark FromUrlEncoding(string value)
    {
        try
        {
            var cborData = Bytewords.Decode(value, BytewordsStyle.Minimal);
            var cbor = Cbor.TryFromData(cborData);
            return FromTaggedCbor(cbor);
        }
        catch (ProvenanceMarkException)
        {
            throw;
        }
        catch (Exception ex) when (ex is URException or CborException)
        {
            throw ex is URException
                ? ProvenanceMarkException.Bytewords(ex.Message, ex)
                : ProvenanceMarkException.Cbor(ex.Message, ex);
        }
    }

    public static ProvenanceMark FromUrl(string url)
    {
        try
        {
            return FromUrl(new Uri(url));
        }
        catch (UriFormatException ex)
        {
            throw ProvenanceMarkException.Url(ex.Message, ex);
        }
    }

    public static ProvenanceMark FromUrl(Uri url)
    {
        var value = Util.QueryValue(url, "provenance");
        if (value is null)
        {
            throw ProvenanceMarkException.MissingUrlParameter("provenance");
        }

        return FromUrlEncoding(value);
    }

    public static ProvenanceMark FromJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var resolution = ProvenanceMarkResolution.FromCode(root.GetProperty("res").GetInt32());
            var key = Util.FromBase64(root.GetProperty("key").GetString()!);
            var hash = Util.FromBase64(root.GetProperty("hash").GetString()!);
            var chainId = Util.FromBase64(root.GetProperty("chain_id").GetString()!);
            var sequence = root.GetProperty("seq").GetUInt32();
            var date = Util.DateFromIso8601(root.GetProperty("date").GetString()!);
            var infoBytes = root.TryGetProperty("info_bytes", out var infoProperty)
                ? Util.FromBase64(infoProperty.GetString()!)
                : Array.Empty<byte>();

            var seqBytes = resolution.SerializeSeq(sequence);
            var dateBytes = resolution.SerializeDate(date);

            return new ProvenanceMark(
                resolution,
                key,
                hash,
                chainId,
                seqBytes,
                dateBytes,
                infoBytes,
                sequence,
                date);
        }
        catch (ProvenanceMarkException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw ProvenanceMarkException.Json(ex.Message, ex);
        }
    }

    public static ProvenanceMark FromUntaggedCbor(Cbor cbor)
    {
        try
        {
            var values = cbor.TryIntoArray();
            if (values.Count != 2)
            {
                throw ProvenanceMarkException.Cbor("Invalid provenance mark length");
            }

            var resolution = ProvenanceMarkResolution.FromCbor(values[0]);
            var message = values[1].TryIntoByteString();
            return FromMessage(resolution, message);
        }
        catch (ProvenanceMarkException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw ProvenanceMarkException.Cbor(ex.Message, ex);
        }
    }

    public static ProvenanceMark FromTaggedCbor(Cbor cbor)
    {
        foreach (var tag in CborTags)
        {
            try
            {
                var item = cbor.TryIntoExpectedTaggedValue(tag);
                return FromUntaggedCbor(item);
            }
            catch (CborWrongTagException)
            {
            }
            catch (CborWrongTypeException)
            {
            }
        }

        throw ProvenanceMarkException.Cbor("wrong tag for provenance mark");
    }

    public static ProvenanceMark FromTaggedCborData(byte[] data)
    {
        return FromTaggedCbor(Cbor.TryFromData(data));
    }

    public static ProvenanceMark FromUr(UR ur)
    {
        try
        {
            ur.CheckType(BcTags.TagNameProvenanceMark);
            return FromUntaggedCbor(ur.Cbor);
        }
        catch (UnexpectedTypeException ex)
        {
            throw ProvenanceMarkException.Cbor(ex.Message, ex);
        }
    }

    public static ProvenanceMark FromUrString(string urString) => FromUr(UR.FromUrString(urString));

    public static ProvenanceMark FromEnvelope(Envelope envelope)
    {
        try
        {
            var leaf = envelope.Subject.TryLeaf();
            return FromTaggedCbor(leaf);
        }
        catch (Exception ex)
        {
            throw ProvenanceMarkException.Cbor($"envelope error: {ex.Message}", ex);
        }
    }

    public static bool IsSequenceValid(IReadOnlyList<ProvenanceMark> marks)
    {
        if (marks.Count < 2)
        {
            return false;
        }

        if (marks[0].Sequence == 0 && !marks[0].IsGenesis())
        {
            return false;
        }

        for (var index = 0; index < marks.Count - 1; index++)
        {
            if (!marks[index].Precedes(marks[index + 1]))
            {
                return false;
            }
        }

        return true;
    }

    public static ValidationReport Validate(IEnumerable<ProvenanceMark> marks) => ValidationReport.Validate(marks);

    public static IReadOnlyList<string> DisambiguatedIdBytewords(IReadOnlyList<ProvenanceMark> marks, bool prefix)
    {
        var ids = marks.Select(mark => mark.Id()).ToList();
        var lengths = MinimalNoncollidingPrefixLengths(ids);
        var results = new List<string>(ids.Count);
        for (var index = 0; index < ids.Count; index++)
        {
            var value = Util.EncodeIdWords(ids[index].AsSpan(0, lengths[index]));
            results.Add(prefix ? $"🅟 {value}" : value);
        }
        return results;
    }

    public static IReadOnlyList<string> DisambiguatedIdBytemoji(IReadOnlyList<ProvenanceMark> marks, bool prefix)
    {
        var ids = marks.Select(mark => mark.Id()).ToList();
        var lengths = MinimalNoncollidingPrefixLengths(ids);
        var results = new List<string>(ids.Count);
        for (var index = 0; index < ids.Count; index++)
        {
            var value = Util.EncodeIdBytemojis(ids[index].AsSpan(0, lengths[index]));
            results.Add(prefix ? $"🅟 {value}" : value);
        }
        return results;
    }

    public static void RegisterTagsIn(FormatContext context)
    {
        GlobalFormatContext.RegisterTagsIn(context);
        context.Tags.SetSummarizer(BcTags.TagProvenanceMark, (untaggedCbor, _) =>
        {
            return FromUntaggedCbor(untaggedCbor).ToString();
        });
    }

    public static void RegisterTags()
    {
        BcTags.RegisterTags();
        GlobalFormatContext.WithFormatContext(RegisterTagsIn);
    }

    private static byte[] MakeHash(
        ProvenanceMarkResolution resolution,
        byte[] key,
        byte[] nextKey,
        byte[] chainId,
        byte[] seqBytes,
        byte[] dateBytes,
        byte[] infoBytes)
    {
        var data = Util.Combine(key, nextKey, chainId, seqBytes, dateBytes, infoBytes);
        return CryptoUtils.Sha256Prefix(data, resolution.LinkLength());
    }

    private static void ValidateWordCount(int wordCount)
    {
        if (wordCount is < 4 or > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(wordCount), $"word_count must be 4..=32, got {wordCount}");
        }
    }

    private static int[] MinimalNoncollidingPrefixLengths(IReadOnlyList<byte[]> ids)
    {
        var lengths = Enumerable.Repeat(4, ids.Count).ToArray();
        var groups = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        for (var index = 0; index < ids.Count; index++)
        {
            var key = Util.ToHex(ids[index].AsSpan(0, 4));
            if (!groups.TryGetValue(key, out var list))
            {
                list = [];
                groups[key] = list;
            }
            list.Add(index);
        }

        foreach (var group in groups.Values)
        {
            if (group.Count > 1)
            {
                ResolveCollisionGroup(ids, group, lengths);
            }
        }

        return lengths;
    }

    private static void ResolveCollisionGroup(IReadOnlyList<byte[]> ids, IReadOnlyList<int> initialIndices, int[] lengths)
    {
        var unresolved = initialIndices.ToList();
        for (var prefixLength = 5; prefixLength <= 32; prefixLength++)
        {
            var subGroups = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            foreach (var index in unresolved)
            {
                var key = Util.ToHex(ids[index].AsSpan(0, prefixLength));
                if (!subGroups.TryGetValue(key, out var list))
                {
                    list = [];
                    subGroups[key] = list;
                }
                list.Add(index);
            }

            var nextUnresolved = new List<int>();
            foreach (var subGroup in subGroups.Values)
            {
                if (subGroup.Count == 1)
                {
                    lengths[subGroup[0]] = prefixLength;
                }
                else
                {
                    nextUnresolved.AddRange(subGroup);
                }
            }

            if (nextUnresolved.Count == 0)
            {
                return;
            }

            unresolved = nextUnresolved;
        }

        foreach (var index in unresolved)
        {
            lengths[index] = 32;
        }
    }
}
