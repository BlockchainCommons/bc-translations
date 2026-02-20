namespace BlockchainCommons.BCLifeHash;

internal static class ColorFunc
{
    public static Func<double, Color> Reverse(Func<double, Color> c)
    {
        return t => c(1.0 - t);
    }

    public static Func<double, Color> Blend2(Color color1, Color color2)
    {
        return t => color1.LerpTo(color2, t);
    }

    public static Func<double, Color> Blend(Color[] colors)
    {
        var count = colors.Length;
        return count switch
        {
            0 => Blend2(Color.Black, Color.Black),
            1 => Blend2(colors[0], colors[0]),
            2 => Blend2(colors[0], colors[1]),
            _ => t =>
            {
                if (t >= 1.0)
                    return colors[count - 1];
                if (t <= 0.0)
                    return colors[0];
                var segments = count - 1;
                var s = t * segments;
                var segment = (int)s;
                var segmentFrac = ColorMath.Modulo(s, 1.0);
                var c1 = colors[segment];
                var c2 = colors[segment + 1];
                return c1.LerpTo(c2, segmentFrac);
            },
        };
    }
}
