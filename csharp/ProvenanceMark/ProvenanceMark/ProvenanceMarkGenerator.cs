using System.Text.Json;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.BCRand;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.ProvenanceMark;

/// <summary>
/// Stateful provenance mark generator.
/// </summary>
public sealed class ProvenanceMarkGenerator : IEquatable<ProvenanceMarkGenerator>
{
    private ProvenanceMarkGenerator(
        ProvenanceMarkResolution resolution,
        ProvenanceSeed seed,
        byte[] chainId,
        uint nextSequence,
        RngState rngState)
    {
        _resolution = resolution;
        _seed = seed;
        _chainId = (byte[])chainId.Clone();
        _nextSequence = nextSequence;
        _rngState = rngState;
    }

    private readonly ProvenanceMarkResolution _resolution;
    private readonly ProvenanceSeed _seed;
    private readonly byte[] _chainId;
    private uint _nextSequence;
    private RngState _rngState;

    public ProvenanceMarkResolution Resolution => _resolution;

    public ProvenanceSeed Seed => _seed;

    public byte[] ChainId => (byte[])_chainId.Clone();

    public uint NextSequence => _nextSequence;

    public RngState RngState => _rngState;

    public ProvenanceMark Next(CborDate date, object? info = null)
    {
        var rng = Xoshiro256StarStar.FromData(_rngState.ToBytes());
        var sequence = _nextSequence;
        _nextSequence += 1;

        byte[] key;
        if (sequence == 0)
        {
            key = ChainId;
        }
        else
        {
            key = rng.NextBytes(_resolution.LinkLength());
            _rngState = RngState.FromBytes(rng.ToData());
        }

        var nextRng = rng.Copy();
        var nextKey = nextRng.NextBytes(_resolution.LinkLength());

        return ProvenanceMark.Create(
            _resolution,
            key,
            nextKey,
            _chainId,
            sequence,
            date,
            info);
    }

    public Envelope ToEnvelope()
    {
        return Envelope.Create(Cbor.ToByteString(ChainId))
            .AddType("provenance-generator")
            .AddAssertion("res", _resolution.ToCbor())
            .AddAssertion("seed", _seed.ToCbor())
            .AddAssertion("next-seq", _nextSequence)
            .AddAssertion("rng-state", _rngState.ToCbor());
    }

    public string ToJson()
    {
        return Util.SerializeJson(new Dictionary<string, object?>
        {
            ["res"] = _resolution.Code,
            ["seed"] = _seed.ToBase64(),
            ["chainID"] = Util.ToBase64(_chainId),
            ["nextSeq"] = _nextSequence,
            ["rngState"] = _rngState.ToBase64()
        });
    }

    public override string ToString() =>
        $"ProvenanceMarkGenerator(chainID: {Util.ToHex(_chainId)}, res: {_resolution}, seed: {_seed.Hex}, nextSeq: {_nextSequence}, rngState: {_rngState})";

    public bool Equals(ProvenanceMarkGenerator? other)
    {
        return other is not null &&
            _resolution == other._resolution &&
            _seed.Equals(other._seed) &&
            _chainId.AsSpan().SequenceEqual(other._chainId) &&
            _nextSequence == other._nextSequence &&
            _rngState.Equals(other._rngState);
    }

    public override bool Equals(object? obj) => Equals(obj as ProvenanceMarkGenerator);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_resolution);
        hash.Add(_seed);
        foreach (var value in _chainId)
        {
            hash.Add(value);
        }
        hash.Add(_nextSequence);
        hash.Add(_rngState);
        return hash.ToHashCode();
    }

    public static ProvenanceMarkGenerator CreateWithSeed(ProvenanceMarkResolution resolution, ProvenanceSeed seed)
    {
        var digest1 = CryptoUtils.Sha256(seed.ToBytes());
        var chainId = digest1[..resolution.LinkLength()];
        var digest2 = CryptoUtils.Sha256(digest1);
        return Create(resolution, seed, chainId, 0, RngState.FromBytes(digest2));
    }

    public static ProvenanceMarkGenerator CreateWithPassphrase(ProvenanceMarkResolution resolution, string passphrase)
    {
        return CreateWithSeed(resolution, ProvenanceSeed.CreateWithPassphrase(passphrase));
    }

    public static ProvenanceMarkGenerator CreateUsing(ProvenanceMarkResolution resolution, IRandomNumberGenerator rng)
    {
        return CreateWithSeed(resolution, ProvenanceSeed.CreateUsing(rng));
    }

    public static ProvenanceMarkGenerator CreateRandom(ProvenanceMarkResolution resolution)
    {
        return CreateWithSeed(resolution, ProvenanceSeed.Create());
    }

    public static ProvenanceMarkGenerator Create(
        ProvenanceMarkResolution resolution,
        ProvenanceSeed seed,
        byte[] chainId,
        uint nextSequence,
        RngState rngState)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(seed);
        ArgumentNullException.ThrowIfNull(chainId);
        ArgumentNullException.ThrowIfNull(rngState);

        if (chainId.Length != resolution.LinkLength())
        {
            throw ProvenanceMarkException.InvalidChainIdLength(resolution.LinkLength(), chainId.Length);
        }

        return new ProvenanceMarkGenerator(resolution, seed, chainId, nextSequence, rngState);
    }

    public static ProvenanceMarkGenerator FromEnvelope(Envelope envelope)
    {
        try
        {
            envelope.CheckType("provenance-generator");
            var chainId = envelope.Subject.TryByteString();
            const int expectedKeyCount = 5;
            if (envelope.Assertions.Count != expectedKeyCount)
            {
                throw ProvenanceMarkException.ExtraKeys(expectedKeyCount, envelope.Assertions.Count);
            }

            var resolution = ProvenanceMarkResolution.FromCbor(envelope.ObjectForPredicate("res").TryLeaf());
            var seed = ProvenanceSeed.FromCbor(envelope.ObjectForPredicate("seed").TryLeaf());
            var nextSequence = checked((uint)envelope.ObjectForPredicate("next-seq").TryLeaf().TryIntoUInt64());
            var rngState = RngState.FromCbor(envelope.ObjectForPredicate("rng-state").TryLeaf());
            return Create(resolution, seed, chainId, nextSequence, rngState);
        }
        catch (ProvenanceMarkException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw ProvenanceMarkException.Envelope(ex.Message, ex);
        }
    }

    public static ProvenanceMarkGenerator FromJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var resolution = ProvenanceMarkResolution.FromCode(root.GetProperty("res").GetInt32());
            var seed = ProvenanceSeed.FromBase64(root.GetProperty("seed").GetString()!);
            var chainId = Util.FromBase64(root.GetProperty("chainID").GetString()!);
            var nextSequence = root.GetProperty("nextSeq").GetUInt32();
            var rngState = RngState.FromBase64(root.GetProperty("rngState").GetString()!);
            return Create(resolution, seed, chainId, nextSequence, rngState);
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
}
