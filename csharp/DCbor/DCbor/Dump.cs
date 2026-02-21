namespace BlockchainCommons.DCbor;

/// <summary>
/// Options controlling hex dump formatting.
/// </summary>
public class HexFormatOptions
{
    public bool Annotate { get; set; }
    public TagsStoreOption Tags { get; set; } = TagsStoreOption.DefaultGlobal;

    public HexFormatOptions WithAnnotate(bool v) { Annotate = v; return this; }
    public HexFormatOptions WithTags(TagsStoreOption v) { Tags = v; return this; }
}

/// <summary>
/// Hex-annotated dump rendering for CBOR values.
/// </summary>
public static class CborDump
{
    /// <summary>Returns the hex-encoded CBOR bytes.</summary>
    public static string HexDump(this Cbor cbor)
    {
        return cbor.Hex();
    }

    /// <summary>Returns annotated hex dump.</summary>
    public static string HexAnnotated(this Cbor cbor)
    {
        return cbor.HexOpt(new HexFormatOptions { Annotate = true });
    }

    /// <summary>Returns hex dump with custom options.</summary>
    public static string HexOpt(this Cbor cbor, HexFormatOptions opts)
    {
        if (!opts.Annotate)
            return cbor.Hex();

        var items = DumpItems(cbor, 0, opts);
        int noteColumn = 0;
        foreach (var item in items)
        {
            int len = item.FormatFirstColumn().Length;
            if (len > noteColumn) noteColumn = len;
        }
        // Round up to nearest multiple of 4
        noteColumn = ((noteColumn + 4) & ~3) - 1;

        var lines = items.Select(x => x.Format(noteColumn)).ToList();
        return string.Join("\n", lines);
    }

    private static List<DumpItem> DumpItems(Cbor cbor, int level, HexFormatOptions opts)
    {
        switch (cbor.Case)
        {
            case CborCase.UnsignedCase u:
                return new List<DumpItem>
                {
                    new(level, new List<byte[]> { cbor.ToCborData() },
                        $"unsigned({u.Value})")
                };

            case CborCase.NegativeCase n:
                return new List<DumpItem>
                {
                    new(level, new List<byte[]> { cbor.ToCborData() },
                        $"negative({-1 - (Int128)n.Value})")
                };

            case CborCase.ByteStringCase bs:
            {
                var header = Varint.EncodeVarInt((ulong)bs.Value.Length, MajorType.ByteString);
                var items = new List<DumpItem>
                {
                    new(level, new List<byte[]> { header },
                        $"bytes({bs.Value.Length})")
                };
                if (!bs.Value.IsEmpty)
                {
                    string? note = null;
                    try
                    {
                        var strictUtf8 = new System.Text.UTF8Encoding(false, true);
                        string str = strictUtf8.GetString(bs.Value.DataRef);
                        var sanitized = StringUtil.Sanitized(str);
                        if (sanitized != null)
                            note = StringUtil.Flanked(sanitized, "\"", "\"");
                    }
                    catch { /* not valid UTF-8 */ }
                    items.Add(new DumpItem(level + 1, new List<byte[]> { bs.Value.ToArray() }, note));
                }
                return items;
            }

            case CborCase.TextCase t:
            {
                var header = Varint.EncodeVarInt((ulong)System.Text.Encoding.UTF8.GetByteCount(t.Value), MajorType.Text);
                var utf8 = System.Text.Encoding.UTF8.GetBytes(t.Value);
                var headerData = new List<byte[]>();
                headerData.Add(new[] { header[0] });
                if (header.Length > 1)
                    headerData.Add(header[1..]);
                return new List<DumpItem>
                {
                    new(level, headerData, $"text({utf8.Length})"),
                    new(level + 1, new List<byte[]> { utf8 },
                        StringUtil.Flanked(t.Value, "\"", "\""))
                };
            }

            case CborCase.SimpleCase s:
            {
                var data = s.Value.CborData();
                return new List<DumpItem>
                {
                    new(level, new List<byte[]> { data }, s.Value.ToString())
                };
            }

            case CborCase.TaggedCase tg:
            {
                var header = Varint.EncodeVarInt(tg.Tag.Value, MajorType.Tagged);
                var headerData = new List<byte[]> { new[] { header[0] } };
                if (header.Length > 1)
                    headerData.Add(header[1..]);

                var noteComponents = new List<string> { $"tag({tg.Tag.Value})" };
                switch (opts.Tags)
                {
                    case TagsStoreOption.Global:
                    {
                        var name = GlobalTags.WithTags(store => store.AssignedNameForTag(tg.Tag));
                        if (name != null) noteComponents.Add(name);
                        break;
                    }
                    case TagsStoreOption.Custom custom:
                    {
                        var name = custom.Store.AssignedNameForTag(tg.Tag);
                        if (name != null) noteComponents.Add(name);
                        break;
                    }
                }

                var items = new List<DumpItem>
                {
                    new(level, headerData, string.Join(" ", noteComponents))
                };
                items.AddRange(DumpItems(tg.Item, level + 1, opts));
                return items;
            }

            case CborCase.ArrayCase a:
            {
                var header = Varint.EncodeVarInt((ulong)a.Value.Count, MajorType.Array);
                var headerData = new List<byte[]> { new[] { header[0] } };
                if (header.Length > 1)
                    headerData.Add(header[1..]);

                var items = new List<DumpItem>
                {
                    new(level, headerData, $"array({a.Value.Count})")
                };
                foreach (var item in a.Value)
                    items.AddRange(DumpItems(item, level + 1, opts));
                return items;
            }

            case CborCase.MapCase m:
            {
                var header = Varint.EncodeVarInt((ulong)m.Value.Count, MajorType.Map);
                var headerData = new List<byte[]> { new[] { header[0] } };
                if (header.Length > 1)
                    headerData.Add(header[1..]);

                var items = new List<DumpItem>
                {
                    new(level, headerData, $"map({m.Value.Count})")
                };
                foreach (var (key, value) in m.Value)
                {
                    items.AddRange(DumpItems(key, level + 1, opts));
                    items.AddRange(DumpItems(value, level + 1, opts));
                }
                return items;
            }

            default:
                throw new InvalidOperationException();
        }
    }

    private sealed class DumpItem
    {
        private readonly int _level;
        private readonly List<byte[]> _data;
        private readonly string? _note;

        internal DumpItem(int level, List<byte[]> data, string? note)
        {
            _level = level;
            _data = data;
            _note = note;
        }

        internal string Format(int noteColumn)
        {
            string col1 = FormatFirstColumn();
            if (_note == null)
                return col1;

            long paddingCount = Math.Max(1, Math.Min(39, noteColumn) - col1.Length + 1);
            string padding = new string(' ', (int)paddingCount);
            return $"{col1}{padding}# {_note}";
        }

        internal string FormatFirstColumn()
        {
            string indent = new string(' ', _level * 4);
            var hexParts = _data
                .Select(d => Convert.ToHexString(d).ToLowerInvariant())
                .Where(h => h.Length > 0)
                .ToList();
            return indent + string.Join(" ", hexParts);
        }
    }
}
