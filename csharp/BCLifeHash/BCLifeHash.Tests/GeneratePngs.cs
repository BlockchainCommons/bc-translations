using BlockchainCommons.BCLifeHash;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BlockchainCommons.BCLifeHash.Tests;

public class GeneratePngs
{
    [Fact]
    public void GenerateAllPngs()
    {
        var versions = new (string Name, LifeHashVersion Version)[]
        {
            ("version1", LifeHashVersion.Version1),
            ("version2", LifeHashVersion.Version2),
            ("detailed", LifeHashVersion.Detailed),
            ("fiducial", LifeHashVersion.Fiducial),
            ("grayscale_fiducial", LifeHashVersion.GrayscaleFiducial),
        };

        var outDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "out");

        foreach (var (name, version) in versions)
        {
            var dir = Path.Combine(outDir, name);
            Directory.CreateDirectory(dir);

            for (var i = 0; i < 100; i++)
            {
                var input = i.ToString();
                var image = LifeHash.CreateFromUtf8(input, version, 1, false);

                using var img = Image.LoadPixelData<Rgb24>(image.Colors, image.Width, image.Height);
                img.SaveAsPng(Path.Combine(dir, $"{i}.png"));
            }
        }
    }
}
