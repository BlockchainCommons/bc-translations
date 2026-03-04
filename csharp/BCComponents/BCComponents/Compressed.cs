using System.IO.Compression;
using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A compressed binary object with integrity verification.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Compressed"/> provides a way to efficiently store and transmit
/// binary data using the raw DEFLATE compression algorithm (RFC 1951). It
/// includes built-in integrity verification through a CRC32 checksum and
/// optional cryptographic digest.
/// </para>
/// <para>
/// If compression would increase the size of the data, the original
/// uncompressed data is stored instead.
/// </para>
/// </remarks>
public sealed class Compressed : IEquatable<Compressed>, ICborTaggedEncodable, IDigestProvider
{
    private readonly uint _checksum;
    private readonly int _decompressedSize;
    private readonly byte[] _compressedData;
    private readonly Digest? _digest;

    /// <summary>
    /// Creates a new <see cref="Compressed"/> object with the specified parameters.
    /// </summary>
    /// <remarks>
    /// This is a low-level constructor primarily intended for deserialization
    /// or when working with pre-compressed data.
    /// </remarks>
    /// <param name="checksum">CRC32 checksum of the decompressed data.</param>
    /// <param name="decompressedSize">Size of the original decompressed data in bytes.</param>
    /// <param name="compressedData">The compressed data bytes.</param>
    /// <param name="digest">Optional cryptographic digest of the content.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the compressed data is larger than the decompressed size.
    /// </exception>
    internal Compressed(uint checksum, int decompressedSize, byte[] compressedData, Digest? digest = null)
    {
        ArgumentNullException.ThrowIfNull(compressedData);
        if (compressedData.Length > decompressedSize)
        {
            throw BCComponentsException.Compression("compressed data is larger than decompressed size");
        }
        _checksum = checksum;
        _decompressedSize = decompressedSize;
        _compressedData = (byte[])compressedData.Clone();
        _digest = digest;
    }

    /// <summary>
    /// Creates a new <see cref="Compressed"/> object by compressing the provided data
    /// using the raw DEFLATE algorithm.
    /// </summary>
    /// <param name="data">The original data to compress.</param>
    /// <param name="digest">Optional cryptographic digest of the content.</param>
    /// <returns>A new <see cref="Compressed"/> object.</returns>
    public static Compressed FromDecompressedData(byte[] data, Digest? digest = null)
    {
        ArgumentNullException.ThrowIfNull(data);

        var checksum = Hash.Crc32(data);
        var decompressedSize = data.Length;

        byte[] compressedData = DeflateCompress(data);

        if (compressedData.Length != 0 && compressedData.Length < decompressedSize)
        {
            return new Compressed(checksum, decompressedSize, compressedData, digest);
        }
        else
        {
            return new Compressed(checksum, decompressedSize, (byte[])data.Clone(), digest);
        }
    }

    /// <summary>
    /// Decompresses and returns the original data.
    /// </summary>
    /// <returns>The decompressed data.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the compressed data is corrupt or the checksum does not match.
    /// </exception>
    public byte[] Decompress()
    {
        if (_compressedData.Length >= _decompressedSize)
        {
            return (byte[])_compressedData.Clone();
        }

        byte[] decompressedData;
        try
        {
            decompressedData = DeflateDecompress(_compressedData);
        }
        catch (Exception)
        {
            throw BCComponentsException.Compression("corrupt compressed data");
        }

        if (Hash.Crc32(decompressedData) != _checksum)
        {
            throw BCComponentsException.Compression("compressed data checksum mismatch");
        }

        return decompressedData;
    }

    /// <summary>Gets the size of the compressed data in bytes.</summary>
    public int CompressedSize => _compressedData.Length;

    /// <summary>Gets the size of the original decompressed data in bytes.</summary>
    public int DecompressedSize => _decompressedSize;

    /// <summary>Gets the CRC32 checksum of the decompressed data.</summary>
    public uint Checksum => _checksum;

    /// <summary>
    /// Gets the compression ratio (compressed size / decompressed size).
    /// </summary>
    /// <remarks>
    /// Values less than 1.0 indicate effective compression. A value of 1.0
    /// indicates no compression was applied. <see cref="double.NaN"/> if
    /// the decompressed size is zero.
    /// </remarks>
    public double CompressionRatio => (double)CompressedSize / _decompressedSize;

    /// <summary>Gets the optional digest associated with this compressed data.</summary>
    public Digest? Digest => _digest;

    /// <summary>Gets whether this compressed data has an associated digest.</summary>
    public bool HasDigest => _digest is not null;

    // --- IDigestProvider ---

    /// <inheritdoc/>
    /// <exception cref="BCComponentsException">
    /// Thrown if there is no digest associated with this compressed data.
    /// </exception>
    Digest IDigestProvider.GetDigest() =>
        _digest ?? throw BCComponentsException.InvalidData("Compressed", "has no digest");

    // --- CBOR ---

    /// <summary>Gets the CBOR tags for this type.</summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagCompressed);

    /// <inheritdoc/>
    public Cbor UntaggedCbor()
    {
        var elements = new List<Cbor>
        {
            Cbor.FromUInt(_checksum),
            Cbor.FromUInt((ulong)_decompressedSize),
            Cbor.ToByteString(_compressedData),
        };
        if (_digest is not null)
        {
            elements.Add(_digest.TaggedCbor());
        }
        return Cbor.FromList(elements);
    }

    /// <inheritdoc/>
    public Cbor ToCbor() => TaggedCbor();

    /// <inheritdoc/>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <inheritdoc/>
    public byte[] TaggedCborData() => TaggedCbor().ToCborData();

    /// <summary>Decodes a <see cref="Compressed"/> from untagged CBOR.</summary>
    /// <param name="cbor">The CBOR value to decode.</param>
    /// <returns>The decoded <see cref="Compressed"/> instance.</returns>
    public static Compressed FromUntaggedCbor(Cbor cbor)
    {
        var elements = cbor.TryIntoArray();
        if (elements.Count < 3 || elements.Count > 4)
        {
            throw BCComponentsException.InvalidData("Compressed", "invalid number of elements");
        }

        var checksum = (uint)elements[0].TryIntoUInt64();
        var decompressedSize = (int)elements[1].TryIntoUInt64();
        var compressedData = elements[2].TryIntoByteString();
        Digest? digest = null;
        if (elements.Count == 4)
        {
            digest = Digest.FromTaggedCbor(elements[3]);
        }

        return new Compressed(checksum, decompressedSize, compressedData, digest);
    }

    /// <summary>Decodes a <see cref="Compressed"/> from tagged CBOR.</summary>
    /// <param name="cbor">The tagged CBOR value to decode.</param>
    /// <returns>The decoded <see cref="Compressed"/> instance.</returns>
    public static Compressed FromTaggedCbor(Cbor cbor)
    {
        var tags = CborTags;
        foreach (var tag in tags)
        {
            try
            {
                var item = cbor.TryIntoExpectedTaggedValue(tag);
                return FromUntaggedCbor(item);
            }
            catch (CborWrongTagException) { }
            catch (CborWrongTypeException) { }
        }
        throw new CborWrongTypeException();
    }

    // --- Equality ---

    /// <inheritdoc/>
    public bool Equals(Compressed? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _checksum == other._checksum
            && _decompressedSize == other._decompressedSize
            && _compressedData.AsSpan().SequenceEqual(other._compressedData)
            && Equals(_digest, other._digest);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Compressed);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_checksum);
        hash.Add(_decompressedSize);
        foreach (var b in _compressedData) hash.Add(b);
        hash.Add(_digest);
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var checksumHex = _checksum.ToString("x8");
        var digestStr = _digest is not null ? _digest.ShortDescription() : "None";
        return $"Compressed(checksum: {checksumHex}, size: {CompressedSize}/{_decompressedSize}, ratio: {CompressionRatio:F2}, digest: {digestStr})";
    }

    // --- Private helpers ---

    private static byte[] DeflateCompress(byte[] data)
    {
        using var ms = new MemoryStream();
        using (var deflate = new DeflateStream(ms, CompressionLevel.Optimal, leaveOpen: true))
        {
            deflate.Write(data, 0, data.Length);
        }
        return ms.ToArray();
    }

    private static byte[] DeflateDecompress(byte[] compressedData)
    {
        using var input = new MemoryStream(compressedData);
        using var deflate = new DeflateStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        deflate.CopyTo(output);
        return output.ToArray();
    }
}
