namespace BlockchainCommons.BCLifeHash;

internal readonly struct HsbColor
{
    public readonly double Hue;
    public readonly double Saturation;
    public readonly double Brightness;

    public HsbColor(double hue, double saturation, double brightness)
    {
        Hue = hue;
        Saturation = saturation;
        Brightness = brightness;
    }

    public static HsbColor FromHue(double hue)
    {
        return new HsbColor(hue, 1.0, 1.0);
    }

    public Color ToColor()
    {
        var v = ColorMath.Clamped(Brightness);
        var s = ColorMath.Clamped(Saturation);

        if (s <= 0.0)
            return new Color(v, v, v);

        var h = ColorMath.Modulo(Hue, 1.0);
        if (h < 0.0)
            h += 1.0;
        h *= 6.0;
        // C++ uses floorf (f32 precision)
        var i = (int)MathF.Floor((float)h);
        var f = h - i;
        var p = v * (1.0 - s);
        var q = v * (1.0 - s * f);
        var t = v * (1.0 - s * (1.0 - f));

        return i switch
        {
            0 => new Color(v, t, p),
            1 => new Color(q, v, p),
            2 => new Color(p, v, t),
            3 => new Color(p, q, v),
            4 => new Color(t, p, v),
            5 => new Color(v, p, q),
            _ => throw new InvalidOperationException("Internal error in HSB conversion"),
        };
    }
}
