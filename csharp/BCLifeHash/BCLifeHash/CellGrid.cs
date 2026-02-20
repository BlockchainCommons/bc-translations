using System.Diagnostics;

namespace BlockchainCommons.BCLifeHash;

internal sealed class CellGrid
{
    public Grid<bool> Grid { get; }

    public CellGrid(int width, int height)
    {
        Grid = new Grid<bool>(width, height);
    }

    private static bool IsAliveInNextGeneration(bool currentAlive, int neighborsCount)
    {
        if (currentAlive)
            return neighborsCount == 2 || neighborsCount == 3;
        return neighborsCount == 3;
    }

    private int CountNeighbors(int px, int py)
    {
        var total = 0;
        Grid.ForNeighborhood(px, py, (ox, oy, nx, ny) =>
        {
            if (ox == 0 && oy == 0)
                return;
            if (Grid.GetValue(nx, ny))
                total++;
        });
        return total;
    }

    public byte[] GetData()
    {
        var a = new BitAggregator();
        Grid.ForAll((x, y) =>
        {
            a.Append(Grid.GetValue(x, y));
        });
        return a.GetData();
    }

    public void SetData(byte[] data)
    {
        Debug.Assert(Grid.Width * Grid.Height == data.Length * 8);
        var e = new BitEnumerator(data);
        var i = 0;
        e.ForAll(b =>
        {
            Grid.Storage[i] = b;
            i++;
        });
        Debug.Assert(i == Grid.Storage.Length);
    }

    public void NextGeneration(
        ChangeGrid currentChangeGrid,
        CellGrid nextCellGrid,
        ChangeGrid nextChangeGrid)
    {
        nextCellGrid.Grid.SetAll(false);
        nextChangeGrid.Grid.SetAll(false);
        var width = Grid.Width;
        var height = Grid.Height;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var currentAlive = Grid.GetValue(x, y);
                if (currentChangeGrid.Grid.GetValue(x, y))
                {
                    var neighborsCount = CountNeighbors(x, y);
                    var nextAlive = IsAliveInNextGeneration(currentAlive, neighborsCount);
                    if (nextAlive)
                        nextCellGrid.Grid.SetValue(true, x, y);
                    if (currentAlive != nextAlive)
                        nextChangeGrid.SetChanged(x, y);
                }
                else
                {
                    nextCellGrid.Grid.SetValue(currentAlive, x, y);
                }
            }
        }
    }
}
