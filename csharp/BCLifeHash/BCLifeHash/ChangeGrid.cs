namespace BlockchainCommons.BCLifeHash;

internal sealed class ChangeGrid
{
    public Grid<bool> Grid { get; }

    public ChangeGrid(int width, int height)
    {
        Grid = new Grid<bool>(width, height);
    }

    public void SetChanged(int px, int py)
    {
        var width = Grid.Width;
        var height = Grid.Height;
        for (var oy = -1; oy <= 1; oy++)
        {
            for (var ox = -1; ox <= 1; ox++)
            {
                var nx = (((ox + px) % width) + width) % width;
                var ny = (((oy + py) % height) + height) % height;
                Grid.SetValue(true, nx, ny);
            }
        }
    }
}
