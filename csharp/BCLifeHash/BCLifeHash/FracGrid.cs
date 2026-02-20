namespace BlockchainCommons.BCLifeHash;

internal sealed class FracGrid
{
    public Grid<double> Grid { get; }

    public FracGrid(int width, int height)
    {
        Grid = new Grid<double>(width, height);
    }

    public void Overlay(CellGrid cellGrid, double frac)
    {
        var width = Grid.Width;
        var height = Grid.Height;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (cellGrid.Grid.GetValue(x, y))
                    Grid.SetValue(frac, x, y);
            }
        }
    }
}
