using System.Buffers.Binary;

namespace BlockchainCommons.BCUR;

/// <summary>
/// Encodes and decodes byte payloads according to the bytewords scheme.
/// </summary>
public static class Bytewords
{
    /// <summary>
    /// The 256 bytewords, indexed by byte value.
    /// </summary>
    private static readonly string[] WordValues =
    [
        "able", "acid", "also", "apex", "aqua", "arch", "atom", "aunt", "away",
        "axis", "back", "bald", "barn", "belt", "beta", "bias", "blue", "body",
        "brag", "brew", "bulb", "buzz", "calm", "cash", "cats", "chef", "city",
        "claw", "code", "cola", "cook", "cost", "crux", "curl", "cusp", "cyan",
        "dark", "data", "days", "deli", "dice", "diet", "door", "down", "draw",
        "drop", "drum", "dull", "duty", "each", "easy", "echo", "edge", "epic",
        "even", "exam", "exit", "eyes", "fact", "fair", "fern", "figs", "film",
        "fish", "fizz", "flap", "flew", "flux", "foxy", "free", "frog", "fuel",
        "fund", "gala", "game", "gear", "gems", "gift", "girl", "glow", "good",
        "gray", "grim", "guru", "gush", "gyro", "half", "hang", "hard", "hawk",
        "heat", "help", "high", "hill", "holy", "hope", "horn", "huts", "iced",
        "idea", "idle", "inch", "inky", "into", "iris", "iron", "item", "jade",
        "jazz", "join", "jolt", "jowl", "judo", "jugs", "jump", "junk", "jury",
        "keep", "keno", "kept", "keys", "kick", "kiln", "king", "kite", "kiwi",
        "knob", "lamb", "lava", "lazy", "leaf", "legs", "liar", "limp", "lion",
        "list", "logo", "loud", "love", "luau", "luck", "lung", "main", "many",
        "math", "maze", "memo", "menu", "meow", "mild", "mint", "miss", "monk",
        "nail", "navy", "need", "news", "next", "noon", "note", "numb", "obey",
        "oboe", "omit", "onyx", "open", "oval", "owls", "paid", "part", "peck",
        "play", "plus", "poem", "pool", "pose", "puff", "puma", "purr", "quad",
        "quiz", "race", "ramp", "real", "redo", "rich", "road", "rock", "roof",
        "ruby", "ruin", "runs", "rust", "safe", "saga", "scar", "sets", "silk",
        "skew", "slot", "soap", "solo", "song", "stub", "surf", "swan", "taco",
        "task", "taxi", "tent", "tied", "time", "tiny", "toil", "tomb", "toys",
        "trip", "tuna", "twin", "ugly", "undo", "unit", "urge", "user", "vast",
        "very", "veto", "vial", "vibe", "view", "visa", "void", "vows", "wall",
        "wand", "warm", "wasp", "wave", "waxy", "webs", "what", "when", "whiz",
        "wolf", "work", "yank", "yawn", "yell", "yoga", "yurt", "zaps", "zero",
        "zest", "zinc", "zone", "zoom"
    ];

    /// <summary>
    /// The 256 bytemojis, indexed by byte value.
    /// </summary>
    private static readonly string[] BytemojiValues =
    [
        "😀", "😂", "😆", "😉", "🙄", "😋", "😎", "😍", "😘", "😭", "🫠", "🥱",
        "🤩", "😶", "🤨", "🫥", "🥵", "🥶", "😳", "🤪", "😵", "😡", "🤢", "😇",
        "🤠", "🤡", "🥳", "🥺", "😬", "🤑", "🙃", "🤯", "😈", "👹", "👺", "💀",
        "👻", "👽", "😺", "😹", "😻", "😽", "🙀", "😿", "🫶", "🤲", "🙌", "🤝",
        "👍", "👎", "👈", "👆", "💪", "👄", "🦷", "👂", "👃", "🧠", "👀", "🤚",
        "🦶", "🍎", "🍊", "🍋", "🍌", "🍉", "🍇", "🍓", "🫐", "🍒", "🍑", "🍍",
        "🥝", "🍆", "🥑", "🥦", "🍅", "🌽", "🥕", "🫒", "🧄", "🥐", "🥯", "🍞",
        "🧀", "🥚", "🍗", "🌭", "🍔", "🍟", "🍕", "🌮", "🥙", "🍱", "🍜", "🍤",
        "🍚", "🥠", "🍨", "🍦", "🎂", "🪴", "🌵", "🌱", "💐", "🍁", "🍄", "🌹",
        "🌺", "🌼", "🌻", "🌸", "💨", "🌊", "💧", "💦", "🌀", "🌈", "🌞", "🌝",
        "🌛", "🌜", "🌙", "🌎", "💫", "⭐", "🪐", "🌐", "💛", "💔", "💘", "💖",
        "💕", "🏁", "🚩", "💬", "💯", "🚫", "🔴", "🔷", "🟩", "🛑", "🔺", "🚗",
        "🚑", "🚒", "🚜", "🛵", "🚨", "🚀", "🚁", "🛟", "🚦", "🏰", "🎡", "🎢",
        "🎠", "🏠", "🔔", "🔑", "🚪", "🪑", "🎈", "💌", "📦", "📫", "📖", "📚",
        "📌", "🧮", "🔒", "💎", "📷", "⏰", "⏳", "📡", "💡", "💰", "🧲", "🧸",
        "🎁", "🎀", "🎉", "🪭", "👑", "🫖", "🔭", "🛁", "🏆", "🥁", "🎷", "🎺",
        "🏀", "🏈", "🎾", "🏓", "✨", "🔥", "💥", "👕", "👚", "👖", "🩳", "👗",
        "👔", "🧢", "👓", "🧶", "🧵", "💍", "👠", "👟", "🧦", "🧤", "👒", "👜",
        "🐱", "🐶", "🐭", "🐹", "🐰", "🦊", "🐻", "🐼", "🐨", "🐯", "🦁", "🐮",
        "🐷", "🐸", "🐵", "🐔", "🐥", "🦆", "🦉", "🐴", "🦄", "🐝", "🐛", "🦋",
        "🐌", "🐞", "🐢", "🐺", "🐍", "🪽", "🐙", "🦑", "🪼", "🦞", "🦀", "🐚",
        "🦭", "🐟", "🐬", "🐳"
    ];

    /// <summary>
    /// The 256 bytewords, indexed by byte value.
    /// </summary>
    public static IReadOnlyList<string> Words { get; } = Array.AsReadOnly(WordValues);

    /// <summary>
    /// The 256 bytemojis, indexed by byte value.
    /// </summary>
    public static IReadOnlyList<string> Bytemojis { get; } = Array.AsReadOnly(BytemojiValues);

    // Minimal encoding: first and last letter of each word
    private static readonly string[] Minimals = BuildMinimals();

    // Lookup: full word -> byte value
    private static readonly Dictionary<string, byte> WordIndex = BuildWordIndex();

    // Lookup: minimal (2-char) -> byte value
    private static readonly Dictionary<string, byte> MinimalIndex = BuildMinimalIndex();

    private static string[] BuildMinimals()
    {
        var result = new string[256];
        for (int i = 0; i < 256; i++)
        {
            var w = WordValues[i];
            result[i] = $"{w[0]}{w[^1]}";
        }
        return result;
    }

    private static Dictionary<string, byte> BuildWordIndex()
    {
        var dict = new Dictionary<string, byte>(256);
        for (int i = 0; i < 256; i++)
        {
            dict[WordValues[i]] = (byte)i;
        }
        return dict;
    }

    private static Dictionary<string, byte> BuildMinimalIndex()
    {
        var dict = new Dictionary<string, byte>(256);
        for (int i = 0; i < 256; i++)
        {
            dict[Minimals[i]] = (byte)i;
        }
        return dict;
    }

    /// <summary>
    /// Encodes a byte payload into a bytewords string with CRC32 checksum appended.
    /// </summary>
    public static string Encode(ReadOnlySpan<byte> data, BytewordsStyle style)
    {
        var checksum = Crc32.Checksum(data);
        var checksumBytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(checksumBytes, checksum);

        var (lookup, separator) = style switch
        {
            BytewordsStyle.Standard => (Words, " "),
            BytewordsStyle.Uri => (Words, "-"),
            BytewordsStyle.Minimal => (Minimals, ""),
            _ => throw new ArgumentOutOfRangeException(nameof(style))
        };

        var parts = new string[data.Length + 4];
        for (int i = 0; i < data.Length; i++)
        {
            parts[i] = lookup[data[i]];
        }
        for (int i = 0; i < 4; i++)
        {
            parts[data.Length + i] = lookup[checksumBytes[i]];
        }

        return string.Join(separator, parts);
    }

    /// <summary>
    /// Decodes a bytewords string back into a byte payload, verifying the CRC32 checksum.
    /// </summary>
    public static byte[] Decode(string encoded, BytewordsStyle style)
    {
        foreach (var c in encoded)
        {
            if (c > 127)
            {
                throw new BytewordsException("bytewords string contains non-ASCII characters");
            }
        }

        var allBytes = style switch
        {
            BytewordsStyle.Standard => DecodeFromWords(encoded.Split(' '), WordIndex),
            BytewordsStyle.Uri => DecodeFromWords(encoded.Split('-'), WordIndex),
            BytewordsStyle.Minimal => DecodeMinimal(encoded),
            _ => throw new ArgumentOutOfRangeException(nameof(style))
        };

        return StripChecksum(allBytes);
    }

    private static byte[] DecodeFromWords(string[] words, Dictionary<string, byte> index)
    {
        var result = new byte[words.Length];
        for (int i = 0; i < words.Length; i++)
        {
            if (!index.TryGetValue(words[i], out var b))
            {
                throw new BytewordsException("invalid word");
            }
            result[i] = b;
        }
        return result;
    }

    private static byte[] DecodeMinimal(string encoded)
    {
        if (encoded.Length % 2 != 0)
        {
            throw new BytewordsException("invalid length");
        }

        var result = new byte[encoded.Length / 2];
        for (int i = 0; i < result.Length; i++)
        {
            var key = encoded.Substring(i * 2, 2);
            if (!MinimalIndex.TryGetValue(key, out var b))
            {
                throw new BytewordsException("invalid word");
            }
            result[i] = b;
        }
        return result;
    }

    private static byte[] StripChecksum(byte[] data)
    {
        if (data.Length < 4)
        {
            throw new BytewordsException("invalid checksum");
        }

        var payloadLen = data.Length - 4;
        var payload = data.AsSpan(0, payloadLen);
        var checksum = data.AsSpan(payloadLen, 4);

        var expected = Crc32.Checksum(payload);
        var expectedBytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(expectedBytes, expected);

        if (!checksum.SequenceEqual(expectedBytes))
        {
            throw new BytewordsException("invalid checksum");
        }

        return payload.ToArray();
    }

    /// <summary>
    /// Encodes a 4-byte slice as a string of bytewords for identification purposes.
    /// </summary>
    public static string Identifier(byte[] data)
    {
        if (data.Length != 4) throw new ArgumentException("data must be exactly 4 bytes", nameof(data));
        return string.Join(" ", data.Select(b => WordValues[b]));
    }

    /// <summary>
    /// Encodes a 4-byte slice as a string of bytemojis for identification purposes.
    /// </summary>
    public static string BytemojiIdentifier(byte[] data)
    {
        if (data.Length != 4) throw new ArgumentException("data must be exactly 4 bytes", nameof(data));
        return string.Join(" ", data.Select(b => BytemojiValues[b]));
    }
}
