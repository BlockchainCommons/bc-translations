using System.Buffers.Binary;
using System.Numerics;

namespace BlockchainCommons.ProvenanceMark;

internal sealed class ChaCha20
{
    private readonly uint[] _state = new uint[16];
    private readonly byte[] _keystream = new byte[64];
    private int _position = 64;

    public ChaCha20(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
    {
        if (key.Length != 32)
        {
            throw new ArgumentException("ChaCha20 key must be 32 bytes", nameof(key));
        }

        if (nonce.Length != 12)
        {
            throw new ArgumentException("ChaCha20 nonce must be 12 bytes", nameof(nonce));
        }

        _state[0] = 0x61707865;
        _state[1] = 0x3320646e;
        _state[2] = 0x79622d32;
        _state[3] = 0x6b206574;

        for (var index = 0; index < 8; index++)
        {
            _state[4 + index] = BinaryPrimitives.ReadUInt32LittleEndian(key[(index * 4)..]);
        }

        _state[12] = 0;

        for (var index = 0; index < 3; index++)
        {
            _state[13 + index] = BinaryPrimitives.ReadUInt32LittleEndian(nonce[(index * 4)..]);
        }
    }

    public byte[] Process(ReadOnlySpan<byte> data)
    {
        var output = data.ToArray();
        ProcessInPlace(output);
        return output;
    }

    public void ProcessInPlace(Span<byte> data)
    {
        for (var index = 0; index < data.Length; index++)
        {
            if (_position >= 64)
            {
                GenerateBlock();
                _position = 0;
            }

            data[index] = (byte)(data[index] ^ _keystream[_position]);
            _position += 1;
        }
    }

    private void GenerateBlock()
    {
        var working = (uint[])_state.Clone();

        for (var round = 0; round < 10; round++)
        {
            QuarterRound(working, 0, 4, 8, 12);
            QuarterRound(working, 1, 5, 9, 13);
            QuarterRound(working, 2, 6, 10, 14);
            QuarterRound(working, 3, 7, 11, 15);

            QuarterRound(working, 0, 5, 10, 15);
            QuarterRound(working, 1, 6, 11, 12);
            QuarterRound(working, 2, 7, 8, 13);
            QuarterRound(working, 3, 4, 9, 14);
        }

        for (var index = 0; index < 16; index++)
        {
            var value = unchecked(working[index] + _state[index]);
            BinaryPrimitives.WriteUInt32LittleEndian(_keystream.AsSpan(index * 4, 4), value);
        }

        _state[12] = unchecked(_state[12] + 1);
    }

    private static void QuarterRound(uint[] state, int a, int b, int c, int d)
    {
        state[a] = unchecked(state[a] + state[b]);
        state[d] = BitOperations.RotateLeft(state[d] ^ state[a], 16);
        state[c] = unchecked(state[c] + state[d]);
        state[b] = BitOperations.RotateLeft(state[b] ^ state[c], 12);
        state[a] = unchecked(state[a] + state[b]);
        state[d] = BitOperations.RotateLeft(state[d] ^ state[a], 8);
        state[c] = unchecked(state[c] + state[d]);
        state[b] = BitOperations.RotateLeft(state[b] ^ state[c], 7);
    }
}
