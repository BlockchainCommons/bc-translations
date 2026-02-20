using System.Runtime.CompilerServices;

namespace BlockchainCommons.BCLifeHash;

internal readonly struct Color
{
    public double R { get; }
    public double G { get; }
    public double B { get; }

    public static readonly Color White = new(1.0, 1.0, 1.0);
    public static readonly Color Black = new(0.0, 0.0, 0.0);
    public static readonly Color Red = new(1.0, 0.0, 0.0);
    public static readonly Color Green = new(0.0, 1.0, 0.0);
    public static readonly Color Blue = new(0.0, 0.0, 1.0);
    public static readonly Color Cyan = new(0.0, 1.0, 1.0);
    public static readonly Color Magenta = new(1.0, 0.0, 1.0);
    public static readonly Color Yellow = new(1.0, 1.0, 0.0);

    public Color(double r, double g, double b)
    {
        R = r;
        G = g;
        B = b;
    }

    public static Color FromRgb(byte r, byte g, byte b)
    {
        return new Color(r / 255.0, g / 255.0, b / 255.0);
    }

    public Color LerpTo(Color other, double t)
    {
        var f = ColorMath.Clamped(t);
        var red = ColorMath.Clamped(R * (1.0 - f) + other.R * f);
        var green = ColorMath.Clamped(G * (1.0 - f) + other.G * f);
        var blue = ColorMath.Clamped(B * (1.0 - f) + other.B * f);
        return new Color(red, green, blue);
    }

    public Color Lighten(double t)
    {
        return LerpTo(White, t);
    }

    public Color Darken(double t)
    {
        return LerpTo(Black, t);
    }

    public Color Burn(double t)
    {
        var f = Math.Max(1.0 - t, 1.0e-7);
        return new Color(
            Math.Min(1.0 - (1.0 - R) / f, 1.0),
            Math.Min(1.0 - (1.0 - G) / f, 1.0),
            Math.Min(1.0 - (1.0 - B) / f, 1.0)
        );
    }

    /// <summary>Luminance using f32 precision (sqrtf/powf) for C++ compatibility.</summary>
    public double Luminance()
    {
        var r = (float)(0.299 * R);
        var g = (float)(0.587 * G);
        var b = (float)(0.114 * B);
        var val = MathF.Pow(r, 2) + MathF.Pow(g, 2) + MathF.Pow(b, 2);
        return MathF.Sqrt(val);
    }
}

internal static class ColorMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Clamped(double n)
    {
        return Math.Clamp(n, 0.0, 1.0);
    }

    /// <summary>
    /// Uses f32 intermediate precision (fmodf emulation) for C++ compatibility.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Modulo(double dividend, double divisor)
    {
        var a = (float)dividend % (float)divisor;
        var b = (a + (float)divisor) % (float)divisor;
        return b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double LerpTo(double toA, double toB, double t)
    {
        return t * (toB - toA) + toA;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double LerpFrom(double fromA, double fromB, double t)
    {
        return (fromA - t) / (fromA - fromB);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Lerp(double fromA, double fromB, double toC, double toD, double t)
    {
        return LerpTo(toC, toD, LerpFrom(fromA, fromB, t));
    }
}
