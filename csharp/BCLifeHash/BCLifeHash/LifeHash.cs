using System.Security.Cryptography;
using System.Text;

namespace BlockchainCommons.BCLifeHash;

/// <summary>
/// LifeHash version controlling grid size, generation count, and color behavior.
/// </summary>
public enum LifeHashVersion
{
    Version1,
    Version2,
    Detailed,
    Fiducial,
    GrayscaleFiducial,
}

/// <summary>
/// An image produced by the LifeHash algorithm.
/// </summary>
public sealed class LifeHashImage
{
    /// <summary>
    /// Image width in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Image height in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Pixel components in row-major order (RGB or RGBA based on generation options).
    /// </summary>
    public byte[] Colors { get; }

    internal LifeHashImage(int width, int height, byte[] colors)
    {
        ArgumentNullException.ThrowIfNull(colors);
        Width = width;
        Height = height;
        Colors = colors;
    }
}

/// <summary>
/// LifeHash visual hashing algorithm.
/// </summary>
public static class LifeHash
{
    private static byte[] Sha256(byte[] data)
    {
        return SHA256.HashData(data);
    }

    private static LifeHashImage CreateImage(
        int width,
        int height,
        double[] floatColors,
        int moduleSize,
        bool hasAlpha)
    {
        if (moduleSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(moduleSize), "Module size must be positive");

        var scaledWidth = width * moduleSize;
        var scaledHeight = height * moduleSize;
        var resultComponents = hasAlpha ? 4 : 3;
        var scaledCapacity = scaledWidth * scaledHeight * resultComponents;

        var resultColors = new byte[scaledCapacity];

        // Match C++ loop order: outer loop uses scaled_width, inner uses scaled_height
        for (var targetY = 0; targetY < scaledWidth; targetY++)
        {
            for (var targetX = 0; targetX < scaledHeight; targetX++)
            {
                var sourceX = targetX / moduleSize;
                var sourceY = targetY / moduleSize;
                var sourceOffset = (sourceY * width + sourceX) * 3;

                var targetOffset = (targetY * scaledWidth + targetX) * resultComponents;

                resultColors[targetOffset] = (byte)(ColorMath.Clamped(floatColors[sourceOffset]) * 255.0);
                resultColors[targetOffset + 1] = (byte)(ColorMath.Clamped(floatColors[sourceOffset + 1]) * 255.0);
                resultColors[targetOffset + 2] = (byte)(ColorMath.Clamped(floatColors[sourceOffset + 2]) * 255.0);
                if (hasAlpha)
                    resultColors[targetOffset + 3] = 255;
            }
        }

        return new LifeHashImage(scaledWidth, scaledHeight, resultColors);
    }

    /// <summary>
    /// Creates a LifeHash image from UTF-8 text input.
    /// </summary>
    /// <param name="text">Text to hash.</param>
    /// <param name="version">LifeHash rendering version.</param>
    /// <param name="moduleSize">Scale factor for each logical cell.</param>
    /// <param name="hasAlpha">Whether to emit RGBA output instead of RGB.</param>
    public static LifeHashImage CreateFromUtf8(
        string text,
        LifeHashVersion version,
        int moduleSize,
        bool hasAlpha)
    {
        ArgumentNullException.ThrowIfNull(text);
        return CreateFromData(Encoding.UTF8.GetBytes(text), version, moduleSize, hasAlpha);
    }

    /// <summary>
    /// Creates a LifeHash image from arbitrary binary input by hashing it with SHA-256.
    /// </summary>
    /// <param name="data">Input bytes to hash.</param>
    /// <param name="version">LifeHash rendering version.</param>
    /// <param name="moduleSize">Scale factor for each logical cell.</param>
    /// <param name="hasAlpha">Whether to emit RGBA output instead of RGB.</param>
    public static LifeHashImage CreateFromData(
        byte[] data,
        LifeHashVersion version,
        int moduleSize,
        bool hasAlpha)
    {
        ArgumentNullException.ThrowIfNull(data);
        var digest = Sha256(data);
        return CreateFromDigest(digest, version, moduleSize, hasAlpha);
    }

    /// <summary>
    /// Creates a LifeHash image from a precomputed SHA-256 digest.
    /// </summary>
    /// <param name="digest">A 32-byte SHA-256 digest.</param>
    /// <param name="version">LifeHash rendering version.</param>
    /// <param name="moduleSize">Scale factor for each logical cell.</param>
    /// <param name="hasAlpha">Whether to emit RGBA output instead of RGB.</param>
    public static LifeHashImage CreateFromDigest(
        byte[] digest,
        LifeHashVersion version,
        int moduleSize,
        bool hasAlpha)
    {
        ArgumentNullException.ThrowIfNull(digest);
        if (digest.Length != 32)
            throw new ArgumentException("Digest must be 32 bytes", nameof(digest));

        int length;
        int maxGenerations;
        switch (version)
        {
            case LifeHashVersion.Version1:
            case LifeHashVersion.Version2:
                length = 16;
                maxGenerations = 150;
                break;
            default:
                length = 32;
                maxGenerations = 300;
                break;
        }

        var currentCellGrid = new CellGrid(length, length);
        var nextCellGrid = new CellGrid(length, length);
        var currentChangeGrid = new ChangeGrid(length, length);
        var nextChangeGrid = new ChangeGrid(length, length);

        switch (version)
        {
            case LifeHashVersion.Version1:
                nextCellGrid.SetData(digest);
                break;
            case LifeHashVersion.Version2:
            {
                var hashed = Sha256(digest);
                nextCellGrid.SetData(hashed);
                break;
            }
            default:
            {
                byte[] digest1 = version == LifeHashVersion.GrayscaleFiducial
                    ? Sha256(digest)
                    : (byte[])digest.Clone();
                var digest2 = Sha256(digest1);
                var digest3 = Sha256(digest2);
                var digest4 = Sha256(digest3);
                var digestFinal = new byte[digest1.Length + digest2.Length + digest3.Length + digest4.Length];
                digest1.CopyTo(digestFinal, 0);
                digest2.CopyTo(digestFinal, digest1.Length);
                digest3.CopyTo(digestFinal, digest1.Length + digest2.Length);
                digest4.CopyTo(digestFinal, digest1.Length + digest2.Length + digest3.Length);
                nextCellGrid.SetData(digestFinal);
                break;
            }
        }

        nextChangeGrid.Grid.SetAll(true);

        var historySet = new HashSet<string>();
        var history = new List<byte[]>();

        while (history.Count < maxGenerations)
        {
            (currentCellGrid, nextCellGrid) = (nextCellGrid, currentCellGrid);
            (currentChangeGrid, nextChangeGrid) = (nextChangeGrid, currentChangeGrid);

            var data = currentCellGrid.GetData();
            var hash = Sha256(data);
            var hashKey = Convert.ToBase64String(hash);
            if (historySet.Contains(hashKey))
                break;
            historySet.Add(hashKey);
            history.Add(data);

            currentCellGrid.NextGeneration(currentChangeGrid, nextCellGrid, nextChangeGrid);
        }

        var fracGrid = new FracGrid(length, length);
        for (var i = 0; i < history.Count; i++)
        {
            currentCellGrid.SetData(history[i]);
            var frac = ColorMath.Clamped(ColorMath.LerpFrom(0.0, history.Count, i + 1));
            fracGrid.Overlay(currentCellGrid, frac);
        }

        // Normalize the frac_grid to [0, 1] (except version1)
        if (version != LifeHashVersion.Version1)
        {
            var minValue = double.PositiveInfinity;
            var maxValue = double.NegativeInfinity;
            fracGrid.Grid.ForAll((x, y) =>
            {
                var value = fracGrid.Grid.GetValue(x, y);
                if (value < minValue)
                    minValue = value;
                if (value > maxValue)
                    maxValue = value;
            });

            var fWidth = fracGrid.Grid.Width;
            var fHeight = fracGrid.Grid.Height;
            for (var y = 0; y < fHeight; y++)
            {
                for (var x = 0; x < fWidth; x++)
                {
                    var value = fracGrid.Grid.GetValue(x, y);
                    var normalized = ColorMath.LerpFrom(minValue, maxValue, value);
                    fracGrid.Grid.SetValue(normalized, x, y);
                }
            }
        }

        var entropy = new BitEnumerator((byte[])digest.Clone());

        switch (version)
        {
            case LifeHashVersion.Detailed:
                entropy.Next();
                break;
            case LifeHashVersion.Version2:
                entropy.NextUint2();
                break;
        }

        var gradient = Gradients.SelectGradient(entropy, version);
        var pattern = Patterns.SelectPattern(entropy, version);
        var colorGrid = new ColorGrid(fracGrid, gradient, pattern);

        return CreateImage(
            colorGrid.Grid.Width,
            colorGrid.Grid.Height,
            colorGrid.Colors(),
            moduleSize,
            hasAlpha
        );
    }
}
