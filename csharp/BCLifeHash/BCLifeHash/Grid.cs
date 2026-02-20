using System.Runtime.CompilerServices;

namespace BlockchainCommons.BCLifeHash;

internal sealed class Grid<T> where T : struct
{
    public int Width { get; }
    public int Height { get; }
    public T[] Storage { get; }

    public Grid(int width, int height)
    {
        Width = width;
        Height = height;
        Storage = new T[width * height];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Offset(int x, int y)
    {
        return y * Width + x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CircularIndex(int index, int modulus)
    {
        return ((index % modulus) + modulus) % modulus;
    }

    public void SetAll(T value)
    {
        Storage.AsSpan().Fill(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetValue(T value, int x, int y)
    {
        Storage[Offset(x, y)] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValue(int x, int y)
    {
        return Storage[Offset(x, y)];
    }

    public void ForAll(Action<int, int> f)
    {
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                f(x, y);
    }

    public void ForNeighborhood(int px, int py, Action<int, int, int, int> f)
    {
        for (var oy = -1; oy <= 1; oy++)
            for (var ox = -1; ox <= 1; ox++)
            {
                var nx = CircularIndex(ox + px, Width);
                var ny = CircularIndex(oy + py, Height);
                f(ox, oy, nx, ny);
            }
    }
}
