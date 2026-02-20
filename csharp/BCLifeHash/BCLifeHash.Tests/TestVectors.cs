using System.Text.Json;
using System.Text.Json.Serialization;
using BlockchainCommons.BCLifeHash;

namespace BlockchainCommons.BCLifeHash.Tests;

public class TestVectors
{
    private sealed class TestVector
    {
        [JsonPropertyName("input")]
        public string Input { get; set; } = "";

        [JsonPropertyName("input_type")]
        public string InputType { get; set; } = "";

        [JsonPropertyName("version")]
        public string Version { get; set; } = "";

        [JsonPropertyName("module_size")]
        public int ModuleSize { get; set; }

        [JsonPropertyName("has_alpha")]
        public bool HasAlpha { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("colors")]
        public int[] ColorsRaw { get; set; } = [];

        private byte[]? _colors;

        [JsonIgnore]
        public byte[] Colors => _colors ??= ColorsRaw.Select(c => (byte)c).ToArray();
    }

    private static LifeHashVersion ParseVersion(string s)
    {
        return s switch
        {
            "version1" => LifeHashVersion.Version1,
            "version2" => LifeHashVersion.Version2,
            "detailed" => LifeHashVersion.Detailed,
            "fiducial" => LifeHashVersion.Fiducial,
            "grayscale_fiducial" => LifeHashVersion.GrayscaleFiducial,
            _ => throw new ArgumentException($"Unknown version: {s}"),
        };
    }

    [Fact]
    public void TestAllVectors()
    {
        var jsonStr = File.ReadAllText("test-vectors.json");
        var vectors = JsonSerializer.Deserialize<TestVector[]>(jsonStr)!;

        Assert.Equal(35, vectors.Length);

        for (var i = 0; i < vectors.Length; i++)
        {
            var tv = vectors[i];
            var version = ParseVersion(tv.Version);

            LifeHashImage image;
            if (tv.InputType == "hex")
            {
                if (string.IsNullOrEmpty(tv.Input))
                {
                    image = LifeHash.CreateFromData([], version, tv.ModuleSize, tv.HasAlpha);
                }
                else
                {
                    var data = Convert.FromHexString(tv.Input);
                    image = LifeHash.CreateFromData(data, version, tv.ModuleSize, tv.HasAlpha);
                }
            }
            else
            {
                image = LifeHash.CreateFromUtf8(tv.Input, version, tv.ModuleSize, tv.HasAlpha);
            }

            Assert.Equal(tv.Width, image.Width);
            Assert.Equal(tv.Height, image.Height);
            Assert.Equal(tv.Colors.Length, image.Colors.Length);

            if (!image.Colors.AsSpan().SequenceEqual(tv.Colors))
            {
                var components = tv.HasAlpha ? 4 : 3;
                for (var j = 0; j < image.Colors.Length; j++)
                {
                    if (image.Colors[j] != tv.Colors[j])
                    {
                        var pixel = j / components;
                        var component = j % components;
                        var compName = new[] { "R", "G", "B", "A" }[component];
                        Assert.Fail(
                            $"Vector {i}: pixel data mismatch for input=\"{tv.Input}\" version={tv.Version}\n" +
                            $"First diff at byte {j} (pixel {pixel}, {compName}): got {image.Colors[j]}, expected {tv.Colors[j]}");
                    }
                }
            }
        }
    }
}
