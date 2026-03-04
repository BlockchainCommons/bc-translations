using System.Buffers.Binary;
using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCComponents;

/// <summary>
/// A deterministic random number generator based on HKDF-HMAC-SHA256.
/// </summary>
/// <remarks>
/// Generates deterministic pseudorandom output from key material and a salt
/// using HKDF key derivation. Manages an internal buffer (page) and refills
/// it as needed.
/// </remarks>
public sealed class HKDFRng : IRandomNumberGenerator, IDisposable
{
    /// <summary>The default page length in bytes for each HKDF call.</summary>
    public const int DefaultPageLength = 32;

    private readonly byte[] _keyMaterial;
    private readonly string _salt;
    private readonly int _pageLength;
    private int _pageIndex;
    private byte[] _buffer;
    private int _offset;

    /// <summary>
    /// Creates a new HKDF-based deterministic RNG.
    /// </summary>
    /// <param name="keyMaterial">The seed material to derive random numbers from.</param>
    /// <param name="salt">A salt value to mix with the key material.</param>
    /// <param name="pageLength">The number of bytes to generate in each HKDF call.</param>
    public HKDFRng(byte[] keyMaterial, string salt, int pageLength = DefaultPageLength)
    {
        _keyMaterial = (byte[])keyMaterial.Clone();
        _salt = salt;
        _pageLength = pageLength;
        _pageIndex = 0;
        _buffer = Array.Empty<byte>();
        _offset = 0;
    }

    /// <inheritdoc/>
    public uint NextUInt32()
    {
        var bytes = RandomData(4);
        return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
    }

    /// <inheritdoc/>
    public ulong NextUInt64()
    {
        var bytes = RandomData(8);
        return BinaryPrimitives.ReadUInt64LittleEndian(bytes);
    }

    /// <inheritdoc/>
    public byte[] RandomData(int size)
    {
        var result = new byte[size];
        int remaining = size;
        int pos = 0;
        while (remaining > 0)
        {
            RefillIfNeeded();
            int available = _buffer.Length - _offset;
            int toCopy = Math.Min(remaining, available);
            Array.Copy(_buffer, _offset, result, pos, toCopy);
            _offset += toCopy;
            pos += toCopy;
            remaining -= toCopy;
        }
        return result;
    }

    /// <inheritdoc/>
    public void FillRandomData(Span<byte> data)
    {
        var bytes = RandomData(data.Length);
        bytes.CopyTo(data);
    }

    private void RefillIfNeeded()
    {
        if (_offset >= _buffer.Length)
        {
            var saltBytes = System.Text.Encoding.UTF8.GetBytes($"{_salt}-{_pageIndex}");
            _buffer = Hash.HkdfHmacSha256(_keyMaterial, saltBytes, _pageLength);
            _pageIndex++;
            _offset = 0;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Array.Clear(_keyMaterial);
        Array.Clear(_buffer);
    }
}
