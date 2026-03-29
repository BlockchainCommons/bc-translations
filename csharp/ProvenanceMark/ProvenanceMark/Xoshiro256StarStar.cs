using System.Buffers.Binary;
using System.Numerics;

namespace BlockchainCommons.ProvenanceMark;

/// <summary>
/// Deterministic xoshiro256** PRNG used by provenance-mark.
/// </summary>
public sealed class Xoshiro256StarStar : IEquatable<Xoshiro256StarStar>
{
    private readonly ulong[] _state;

    private Xoshiro256StarStar(ulong[] state)
    {
        _state = state;
    }

    public ulong[] ToState() => (ulong[])_state.Clone();

    public byte[] ToData()
    {
        var data = new byte[32];
        for (var index = 0; index < 4; index++)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(index * 8, 8), _state[index]);
        }
        return data;
    }

    public Xoshiro256StarStar Copy() => FromState(_state);

    public ulong NextUInt64()
    {
        var result = BitOperations.RotateLeft(_state[1] * 5, 7) * 9;
        var t = _state[1] << 17;

        _state[2] ^= _state[0];
        _state[3] ^= _state[1];
        _state[1] ^= _state[2];
        _state[0] ^= _state[3];

        _state[2] ^= t;
        _state[3] = BitOperations.RotateLeft(_state[3], 45);

        return result;
    }

    public byte NextByte() => (byte)(NextUInt64() & 0xff);

    public byte[] NextBytes(int length)
    {
        var bytes = new byte[length];
        for (var index = 0; index < length; index++)
        {
            bytes[index] = NextByte();
        }
        return bytes;
    }

    public static Xoshiro256StarStar FromState(ReadOnlySpan<ulong> state)
    {
        if (state.Length != 4)
        {
            throw new ArgumentException("state must have 4 elements", nameof(state));
        }

        return new Xoshiro256StarStar(state.ToArray());
    }

    public static Xoshiro256StarStar FromData(ReadOnlySpan<byte> data)
    {
        if (data.Length != 32)
        {
            throw new ArgumentException("data must be 32 bytes", nameof(data));
        }

        var state = new ulong[4];
        for (var index = 0; index < 4; index++)
        {
            state[index] = BinaryPrimitives.ReadUInt64LittleEndian(data[(index * 8)..]);
        }

        return new Xoshiro256StarStar(state);
    }

    public bool Equals(Xoshiro256StarStar? other)
    {
        return other is not null && _state.AsSpan().SequenceEqual(other._state);
    }

    public override bool Equals(object? obj) => Equals(obj as Xoshiro256StarStar);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var value in _state)
        {
            hash.Add(value);
        }
        return hash.ToHashCode();
    }
}
