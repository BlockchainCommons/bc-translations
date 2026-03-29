using System.Text;
using System.Text.Json;
using BlockchainCommons.BCUR;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.ProvenanceMark;

/// <summary>
/// Public parsing helpers mirroring the Rust utility module.
/// </summary>
public static class Util
{
    /// <summary>
    /// Parses a base64-encoded provenance seed.
    /// </summary>
    public static ProvenanceSeed ParseSeed(string value) => ProvenanceSeed.FromBase64(value);

    /// <summary>
    /// Parses an ISO-8601 date string into a CBOR date.
    /// </summary>
    public static CborDate ParseDate(string value) => DateFromIso8601(value);

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    internal static readonly JsonSerializerOptions JsonIndentedOptions = new()
    {
        WriteIndented = true
    };

    internal static string ToHex(ReadOnlySpan<byte> data)
    {
        return Convert.ToHexString(data).ToLowerInvariant();
    }

    internal static string ToBase64(ReadOnlySpan<byte> data)
    {
        return Convert.ToBase64String(data);
    }

    internal static byte[] FromBase64(string value)
    {
        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException ex)
        {
            throw ProvenanceMarkException.Base64(ex.Message, ex);
        }
    }

    internal static string DateToIso8601(CborDate date) => date.ToString();

    internal static CborDate DateFromIso8601(string value)
    {
        try
        {
            return CborDate.FromString(value);
        }
        catch (Exception ex)
        {
            throw ProvenanceMarkException.InvalidDate(ex.Message);
        }
    }

    internal static Cbor AnyToCbor(object value)
    {
        return value switch
        {
            Cbor cbor => cbor,
            ICborEncodable encodable => encodable.ToCbor(),
            string text => Cbor.FromString(text),
            bool boolean => Cbor.FromBool(boolean),
            byte number => Cbor.FromUInt(number),
            ushort number => Cbor.FromUInt(number),
            uint number => Cbor.FromUInt(number),
            ulong number => Cbor.FromUInt(number),
            sbyte number => Cbor.FromInt(number),
            short number => Cbor.FromInt(number),
            int number => Cbor.FromInt(number),
            long number => Cbor.FromInt(number),
            float number => Cbor.FromFloat(number),
            double number => Cbor.FromDouble(number),
            byte[] bytes => Cbor.ToByteString(bytes),
            ByteString byteString => Cbor.FromByteString(byteString),
            _ => throw ProvenanceMarkException.Cbor($"unsupported CBOR-encodable type: {value.GetType().FullName}")
        };
    }

    internal static byte[] AnyToCborData(object value) => AnyToCbor(value).ToCborData();

    internal static string? QueryValue(Uri url, string key)
    {
        var query = url.Query;
        if (string.IsNullOrEmpty(query))
        {
            return null;
        }

        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            var currentKey = Uri.UnescapeDataString(parts[0]);
            if (!string.Equals(currentKey, key, StringComparison.Ordinal))
            {
                continue;
            }

            return parts.Length == 1 ? string.Empty : Uri.UnescapeDataString(parts[1]);
        }

        return null;
    }

    internal static string SerializeJson<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    internal static string SerializeJsonIndented<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonIndentedOptions);
    }

    internal static byte[] Combine(params byte[][] parts)
    {
        var totalLength = 0;
        foreach (var part in parts)
        {
            totalLength += part.Length;
        }

        var combined = new byte[totalLength];
        var offset = 0;
        foreach (var part in parts)
        {
            Buffer.BlockCopy(part, 0, combined, offset, part.Length);
            offset += part.Length;
        }

        return combined;
    }

    internal static byte[] Slice(byte[] data, Range range)
    {
        var (offset, length) = range.GetOffsetAndLength(data.Length);
        var slice = new byte[length];
        Buffer.BlockCopy(data, offset, slice, 0, length);
        return slice;
    }

    internal static string EncodeIdWords(ReadOnlySpan<byte> data)
    {
        var words = new string[data.Length];
        for (var index = 0; index < data.Length; index++)
        {
            words[index] = Bytewords.Words[data[index]];
        }
        return string.Join(" ", words).ToUpperInvariant();
    }

    internal static string EncodeIdBytemojis(ReadOnlySpan<byte> data)
    {
        var emojis = new string[data.Length];
        for (var index = 0; index < data.Length; index++)
        {
            emojis[index] = Bytewords.Bytemojis[data[index]];
        }
        return string.Join(" ", emojis).ToUpperInvariant();
    }

    internal static string EncodeIdMinimal(ReadOnlySpan<byte> data)
    {
        var builder = new StringBuilder(data.Length * 2);
        for (var index = 0; index < data.Length; index++)
        {
            var word = Bytewords.Words[data[index]];
            builder.Append(char.ToUpperInvariant(word[0]));
            builder.Append(char.ToUpperInvariant(word[^1]));
        }
        return builder.ToString();
    }
}
