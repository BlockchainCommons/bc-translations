using System.Text;

namespace BlockchainCommons.DCbor;

/// <summary>
/// Decodes binary CBOR data into <see cref="Cbor"/> values, enforcing dCBOR rules.
/// </summary>
internal static class CborDecoder
{
    internal static Cbor DecodeCbor(ReadOnlySpan<byte> data)
    {
        var (cbor, len) = DecodeInternal(data);
        int remaining = data.Length - len;
        if (remaining > 0)
            throw new CborUnusedDataException(remaining);
        return cbor;
    }

    private static (MajorType majorType, byte headerValue) ParseHeader(byte header)
    {
        var majorType = (MajorType)(header >> 5);
        byte headerValue = (byte)(header & 31);
        return (majorType, headerValue);
    }

    private static (MajorType majorType, ulong value, int varintLen) ParseHeaderVarint(
        ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            throw new CborUnderrunException();

        byte header = data[0];
        var (majorType, headerValue) = ParseHeader(header);
        int dataRemaining = data.Length - 1;

        ulong value;
        int varintLen;

        switch (headerValue)
        {
            case <= 23:
                value = headerValue;
                varintLen = 1;
                break;

            case 24:
                if (dataRemaining < 1) throw new CborUnderrunException();
                value = data[1];
                if (value < 24) throw new CborNonCanonicalNumericException();
                varintLen = 2;
                break;

            case 25:
                if (dataRemaining < 2) throw new CborUnderrunException();
                value = ((ulong)data[1] << 8) | data[2];
                if (value <= byte.MaxValue && header != 0xf9)
                    throw new CborNonCanonicalNumericException();
                varintLen = 3;
                break;

            case 26:
                if (dataRemaining < 4) throw new CborUnderrunException();
                value = ((ulong)data[1] << 24) | ((ulong)data[2] << 16)
                      | ((ulong)data[3] << 8) | data[4];
                if (value <= ushort.MaxValue && header != 0xfa)
                    throw new CborNonCanonicalNumericException();
                varintLen = 5;
                break;

            case 27:
                if (dataRemaining < 8) throw new CborUnderrunException();
                value = ((ulong)data[1] << 56) | ((ulong)data[2] << 48)
                      | ((ulong)data[3] << 40) | ((ulong)data[4] << 32)
                      | ((ulong)data[5] << 24) | ((ulong)data[6] << 16)
                      | ((ulong)data[7] << 8) | data[8];
                if (value <= uint.MaxValue && header != 0xfb)
                    throw new CborNonCanonicalNumericException();
                varintLen = 9;
                break;

            default:
                throw new CborUnsupportedHeaderValueException(headerValue);
        }

        return (majorType, value, varintLen);
    }

    private static ReadOnlySpan<byte> ParseBytes(ReadOnlySpan<byte> data, int len)
    {
        if (data.Length < len)
            throw new CborUnderrunException();
        return data[..len];
    }

    private static (Cbor cbor, int consumed) DecodeInternal(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            throw new CborUnderrunException();

        var (majorType, value, headerLen) = ParseHeaderVarint(data);

        switch (majorType)
        {
            case MajorType.Unsigned:
                return (new Cbor(CborCase.Unsigned(value)), headerLen);

            case MajorType.Negative:
                return (new Cbor(CborCase.Negative(value)), headerLen);

            case MajorType.ByteString:
            {
                int dataLen = (int)value;
                var bytes = ParseBytes(data[headerLen..], dataLen);
                return (Cbor.FromByteString(bytes.ToArray()), headerLen + dataLen);
            }

            case MajorType.Text:
            {
                int dataLen = (int)value;
                var buf = ParseBytes(data[headerLen..], dataLen);
                string str;
                try
                {
                    str = Encoding.UTF8.GetString(buf);
                }
                catch (Exception ex)
                {
                    throw new CborInvalidStringException(ex.Message);
                }
                if (!str.IsNormalized(System.Text.NormalizationForm.FormC))
                    throw new CborNonCanonicalStringException();
                return (Cbor.FromString(str), headerLen + dataLen);
            }

            case MajorType.Array:
            {
                int pos = headerLen;
                var items = new List<Cbor>();
                for (ulong i = 0; i < value; i++)
                {
                    var (item, itemLen) = DecodeInternal(data[pos..]);
                    items.Add(item);
                    pos += itemLen;
                }
                return (new Cbor(CborCase.Array(items)), pos);
            }

            case MajorType.Map:
            {
                int pos = headerLen;
                var map = new CborMap();
                for (ulong i = 0; i < value; i++)
                {
                    var (key, keyLen) = DecodeInternal(data[pos..]);
                    pos += keyLen;
                    var (val, valLen) = DecodeInternal(data[pos..]);
                    pos += valLen;
                    map.InsertNext(key, val);
                }
                return (new Cbor(CborCase.Map(map)), pos);
            }

            case MajorType.Tagged:
            {
                var (item, itemLen) = DecodeInternal(data[headerLen..]);
                var tagged = Cbor.ToTaggedValue(value, item);
                return (tagged, headerLen + itemLen);
            }

            case MajorType.Simple:
            {
                switch (headerLen)
                {
                    case 3:
                    {
                        // f16
                        ushort bits = (ushort)value;
                        FloatEncoding.ValidateCanonicalF16(bits);
                        double f = FloatEncoding.HalfToDouble(bits);
                        return (Cbor.FromDouble(f), headerLen);
                    }
                    case 5:
                    {
                        // f32
                        float f = BitConverter.UInt32BitsToSingle((uint)value);
                        FloatEncoding.ValidateCanonicalF32(f);
                        return (Cbor.FromFloat(f), headerLen);
                    }
                    case 9:
                    {
                        // f64
                        double f = BitConverter.UInt64BitsToDouble(value);
                        FloatEncoding.ValidateCanonicalF64(f);
                        return (Cbor.FromDouble(f), headerLen);
                    }
                    default:
                    {
                        return value switch
                        {
                            20 => (Cbor.False(), headerLen),
                            21 => (Cbor.True(), headerLen),
                            22 => (Cbor.Null(), headerLen),
                            _ => throw new CborInvalidSimpleValueException(),
                        };
                    }
                }
            }

            default:
                throw new InvalidOperationException($"Unknown major type: {majorType}");
        }
    }
}
