namespace BlockchainCommons.BCLifeHash;

internal sealed class ColorGrid
{
    public Grid<Color> Grid { get; }

    private readonly record struct Transform(bool Transpose, bool ReflectX, bool ReflectY);

    public ColorGrid(FracGrid fracGrid, Func<double, Color> gradient, Pattern pattern)
    {
        var multiplier = pattern == Pattern.Fiducial ? 1 : 2;
        var targetWidth = fracGrid.Grid.Width * multiplier;
        var targetHeight = fracGrid.Grid.Height * multiplier;

        Grid = new Grid<Color>(targetWidth, targetHeight);
        var maxX = targetWidth - 1;
        var maxY = targetHeight - 1;

        Transform[] transforms = pattern switch
        {
            Pattern.Snowflake =>
            [
                new(false, false, false),
                new(false, true, false),
                new(false, false, true),
                new(false, true, true),
            ],
            Pattern.Pinwheel =>
            [
                new(false, false, false),
                new(true, true, false),
                new(true, false, true),
                new(false, true, true),
            ],
            Pattern.Fiducial =>
            [
                new(false, false, false),
            ],
            _ => throw new InvalidOperationException("Unknown pattern"),
        };

        var fracWidth = fracGrid.Grid.Width;
        var fracHeight = fracGrid.Grid.Height;
        for (var y = 0; y < fracHeight; y++)
        {
            for (var x = 0; x < fracWidth; x++)
            {
                var value = fracGrid.Grid.GetValue(x, y);
                var color = gradient(value);
                foreach (var t in transforms)
                {
                    var px = x;
                    var py = y;
                    if (t.Transpose)
                        (px, py) = (py, px);
                    if (t.ReflectX)
                        px = maxX - px;
                    if (t.ReflectY)
                        py = maxY - py;
                    Grid.SetValue(color, px, py);
                }
            }
        }
    }

    public double[] Colors()
    {
        var result = new double[Grid.Storage.Length * 3];
        for (var i = 0; i < Grid.Storage.Length; i++)
        {
            var c = Grid.Storage[i];
            result[i * 3] = c.R;
            result[i * 3 + 1] = c.G;
            result[i * 3 + 2] = c.B;
        }
        return result;
    }
}
