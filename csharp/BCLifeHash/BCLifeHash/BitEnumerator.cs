namespace BlockchainCommons.BCLifeHash;

internal sealed class BitEnumerator
{
    private readonly byte[] _data;
    private int _index;
    private byte _mask;

    public BitEnumerator(byte[] data)
    {
        _data = data;
        _index = 0;
        _mask = 0x80;
    }

    public bool HasNext => _mask != 0 || _index != _data.Length - 1;

    public bool Next()
    {
        if (!HasNext)
            throw new InvalidOperationException("BitEnumerator underflow");

        if (_mask == 0)
        {
            _mask = 0x80;
            _index++;
        }

        var b = (_data[_index] & _mask) != 0;
        _mask >>= 1;
        return b;
    }

    public uint NextUint2()
    {
        uint bitMask = 0x02;
        uint value = 0;
        for (var i = 0; i < 2; i++)
        {
            if (Next())
                value |= bitMask;
            bitMask >>= 1;
        }
        return value;
    }

    public uint NextUint8()
    {
        uint bitMask = 0x80;
        uint value = 0;
        for (var i = 0; i < 8; i++)
        {
            if (Next())
                value |= bitMask;
            bitMask >>= 1;
        }
        return value;
    }

    public uint NextUint16()
    {
        uint bitMask = 0x8000;
        uint value = 0;
        for (var i = 0; i < 16; i++)
        {
            if (Next())
                value |= bitMask;
            bitMask >>= 1;
        }
        return value;
    }

    public double NextFrac()
    {
        return NextUint16() / 65535.0;
    }

    public void ForAll(Action<bool> f)
    {
        while (HasNext)
            f(Next());
    }
}

internal sealed class BitAggregator
{
    private readonly List<byte> _data = new();
    private byte _bitMask;

    public void Append(bool bit)
    {
        if (_bitMask == 0)
        {
            _bitMask = 0x80;
            _data.Add(0);
        }

        if (bit)
            _data[^1] |= _bitMask;

        _bitMask >>= 1;
    }

    public byte[] GetData()
    {
        return _data.ToArray();
    }
}
