namespace BlockchainCommons.BCLifeHash;

internal static class Gradients
{
    private static Func<double, Color> Grayscale()
    {
        return ColorFunc.Blend2(Color.Black, Color.White);
    }

    private static Func<double, Color> SelectGrayscale(BitEnumerator entropy)
    {
        return entropy.Next() ? Grayscale() : ColorFunc.Reverse(Grayscale());
    }

    private static Color MakeHue(double t)
    {
        return HsbColor.FromHue(t).ToColor();
    }

    private static Func<double, Color> Spectrum()
    {
        return ColorFunc.Blend([
            Color.FromRgb(0, 168, 222),
            Color.FromRgb(51, 51, 145),
            Color.FromRgb(233, 19, 136),
            Color.FromRgb(235, 45, 46),
            Color.FromRgb(253, 233, 43),
            Color.FromRgb(0, 158, 84),
            Color.FromRgb(0, 168, 222),
        ]);
    }

    private static Func<double, Color> SpectrumCmykSafe()
    {
        return ColorFunc.Blend([
            Color.FromRgb(0, 168, 222),
            Color.FromRgb(41, 60, 130),
            Color.FromRgb(210, 59, 130),
            Color.FromRgb(217, 63, 53),
            Color.FromRgb(244, 228, 81),
            Color.FromRgb(0, 158, 84),
            Color.FromRgb(0, 168, 222),
        ]);
    }

    private static Color AdjustForLuminance(Color color, Color contrastColor)
    {
        var lum = color.Luminance();
        var contrastLum = contrastColor.Luminance();
        var threshold = 0.6;
        var offset = Math.Abs(lum - contrastLum);
        if (offset > threshold)
            return color;
        var boost = 0.7;
        var t = ColorMath.Lerp(0.0, threshold, boost, 0.0, offset);
        if (contrastLum > lum)
            return color.Darken(t).Burn(t * 0.6);
        return color.Lighten(t).Burn(t * 0.6);
    }

    private static Func<double, Color> Monochromatic(BitEnumerator entropy, Func<double, Color> hueGenerator)
    {
        var hue = entropy.NextFrac();
        var isTint = entropy.Next();
        var isReversed = entropy.Next();
        var keyAdvance = entropy.NextFrac() * 0.3 + 0.05;
        var neutralAdvance = entropy.NextFrac() * 0.3 + 0.05;

        var keyColor = hueGenerator(hue);

        double contrastBrightness;
        if (isTint)
        {
            contrastBrightness = 1.0;
            keyColor = keyColor.Darken(0.5);
        }
        else
        {
            contrastBrightness = 0.0;
        }
        var gs = Grayscale();
        var neutralColor = gs(contrastBrightness);

        var keyColor2 = keyColor.LerpTo(neutralColor, keyAdvance);
        var neutralColor2 = neutralColor.LerpTo(keyColor, neutralAdvance);

        var gradient = ColorFunc.Blend2(keyColor2, neutralColor2);
        return isReversed ? ColorFunc.Reverse(gradient) : gradient;
    }

    private static Func<double, Color> MonochromaticFiducial(BitEnumerator entropy)
    {
        var hue = entropy.NextFrac();
        var isReversed = entropy.Next();
        var isTint = entropy.Next();

        var contrastColor = isTint ? Color.White : Color.Black;
        var spec = SpectrumCmykSafe();
        var keyColor = AdjustForLuminance(spec(hue), contrastColor);

        var gradient = ColorFunc.Blend([keyColor, contrastColor, keyColor]);
        return isReversed ? ColorFunc.Reverse(gradient) : gradient;
    }

    private static Func<double, Color> Complementary(BitEnumerator entropy, Func<double, Color> hueGenerator)
    {
        var spectrum1 = entropy.NextFrac();
        var spectrum2 = ColorMath.Modulo(spectrum1 + 0.5, 1.0);
        var lighterAdvance = entropy.NextFrac() * 0.3;
        var darkerAdvance = entropy.NextFrac() * 0.3;
        var isReversed = entropy.Next();

        var color1 = hueGenerator(spectrum1);
        var color2 = hueGenerator(spectrum2);

        var luma1 = color1.Luminance();
        var luma2 = color2.Luminance();

        Color darkerColor, lighterColor;
        if (luma1 > luma2)
        {
            darkerColor = color2;
            lighterColor = color1;
        }
        else
        {
            darkerColor = color1;
            lighterColor = color2;
        }

        var adjustedLighter = lighterColor.Lighten(lighterAdvance);
        var adjustedDarker = darkerColor.Darken(darkerAdvance);

        var gradient = ColorFunc.Blend2(adjustedDarker, adjustedLighter);
        return isReversed ? ColorFunc.Reverse(gradient) : gradient;
    }

    private static Func<double, Color> ComplementaryFiducial(BitEnumerator entropy)
    {
        var spectrum1 = entropy.NextFrac();
        var spectrum2 = ColorMath.Modulo(spectrum1 + 0.5, 1.0);
        var isTint = entropy.Next();
        var isReversed = entropy.Next();
        var neutralColorBias = entropy.Next();

        var neutralColor = isTint ? Color.White : Color.Black;
        var spec = SpectrumCmykSafe();
        var color1 = spec(spectrum1);
        var color2 = spec(spectrum2);

        var biasColor = neutralColorBias ? color1 : color2;
        var biasedNeutralColor = neutralColor.LerpTo(biasColor, 0.2).Burn(0.1);

        var gradient = ColorFunc.Blend([
            AdjustForLuminance(color1, biasedNeutralColor),
            biasedNeutralColor,
            AdjustForLuminance(color2, biasedNeutralColor),
        ]);
        return isReversed ? ColorFunc.Reverse(gradient) : gradient;
    }

    private static Func<double, Color> Triadic(BitEnumerator entropy, Func<double, Color> hueGenerator)
    {
        var spectrum1 = entropy.NextFrac();
        var spectrum2 = ColorMath.Modulo(spectrum1 + 1.0 / 3.0, 1.0);
        var spectrum3 = ColorMath.Modulo(spectrum1 + 2.0 / 3.0, 1.0);
        var lighterAdvance = entropy.NextFrac() * 0.3;
        var darkerAdvance = entropy.NextFrac() * 0.3;
        var isReversed = entropy.Next();

        var color1 = hueGenerator(spectrum1);
        var color2 = hueGenerator(spectrum2);
        var color3 = hueGenerator(spectrum3);

        var colors = new[] { color1, color2, color3 };
        Array.Sort(colors, (a, b) => a.Luminance().CompareTo(b.Luminance()));

        var darkerColor = colors[0];
        var middleColor = colors[1];
        var lighterColor = colors[2];

        var adjustedLighter = lighterColor.Lighten(lighterAdvance);
        var adjustedDarker = darkerColor.Darken(darkerAdvance);

        var gradient = ColorFunc.Blend([adjustedLighter, middleColor, adjustedDarker]);
        return isReversed ? ColorFunc.Reverse(gradient) : gradient;
    }

    private static Func<double, Color> TriadicFiducial(BitEnumerator entropy)
    {
        var spectrum1 = entropy.NextFrac();
        var spectrum2 = ColorMath.Modulo(spectrum1 + 1.0 / 3.0, 1.0);
        var spectrum3 = ColorMath.Modulo(spectrum1 + 2.0 / 3.0, 1.0);
        var isTint = entropy.Next();
        var neutralInsertIndex = (int)(entropy.NextUint8() % 2 + 1);
        var isReversed = entropy.Next();

        var neutralColor = isTint ? Color.White : Color.Black;

        var spec = SpectrumCmykSafe();
        var colors = new List<Color> { spec(spectrum1), spec(spectrum2), spec(spectrum3) };

        switch (neutralInsertIndex)
        {
            case 1:
                colors[0] = AdjustForLuminance(colors[0], neutralColor);
                colors[1] = AdjustForLuminance(colors[1], neutralColor);
                colors[2] = AdjustForLuminance(colors[2], colors[1]);
                break;
            case 2:
                colors[1] = AdjustForLuminance(colors[1], neutralColor);
                colors[2] = AdjustForLuminance(colors[2], neutralColor);
                colors[0] = AdjustForLuminance(colors[0], colors[1]);
                break;
            default:
                throw new InvalidOperationException("Internal error");
        }

        colors.Insert(neutralInsertIndex, neutralColor);

        var gradient = ColorFunc.Blend(colors.ToArray());
        return isReversed ? ColorFunc.Reverse(gradient) : gradient;
    }

    private static Func<double, Color> Analogous(BitEnumerator entropy, Func<double, Color> hueGenerator)
    {
        var spectrum1 = entropy.NextFrac();
        var spectrum2 = ColorMath.Modulo(spectrum1 + 1.0 / 12.0, 1.0);
        var spectrum3 = ColorMath.Modulo(spectrum1 + 2.0 / 12.0, 1.0);
        var spectrum4 = ColorMath.Modulo(spectrum1 + 3.0 / 12.0, 1.0);
        var advance = entropy.NextFrac() * 0.5 + 0.2;
        var isReversed = entropy.Next();

        var color1 = hueGenerator(spectrum1);
        var color2 = hueGenerator(spectrum2);
        var color3 = hueGenerator(spectrum3);
        var color4 = hueGenerator(spectrum4);

        Color darkestColor, darkColor, lightColor, lightestColor;
        if (color1.Luminance() < color4.Luminance())
        {
            darkestColor = color1;
            darkColor = color2;
            lightColor = color3;
            lightestColor = color4;
        }
        else
        {
            darkestColor = color4;
            darkColor = color3;
            lightColor = color2;
            lightestColor = color1;
        }

        var adjustedDarkest = darkestColor.Darken(advance);
        var adjustedDark = darkColor.Darken(advance / 2.0);
        var adjustedLight = lightColor.Lighten(advance / 2.0);
        var adjustedLightest = lightestColor.Lighten(advance);

        var gradient = ColorFunc.Blend([adjustedDarkest, adjustedDark, adjustedLight, adjustedLightest]);
        return isReversed ? ColorFunc.Reverse(gradient) : gradient;
    }

    private static Func<double, Color> AnalogousFiducial(BitEnumerator entropy)
    {
        var spectrum1 = entropy.NextFrac();
        var spectrum2 = ColorMath.Modulo(spectrum1 + 1.0 / 10.0, 1.0);
        var spectrum3 = ColorMath.Modulo(spectrum1 + 2.0 / 10.0, 1.0);
        var isTint = entropy.Next();
        var neutralInsertIndex = (int)(entropy.NextUint8() % 2 + 1);
        var isReversed = entropy.Next();

        var neutralColor = isTint ? Color.White : Color.Black;

        var spec = SpectrumCmykSafe();
        var colors = new List<Color> { spec(spectrum1), spec(spectrum2), spec(spectrum3) };

        switch (neutralInsertIndex)
        {
            case 1:
                colors[0] = AdjustForLuminance(colors[0], neutralColor);
                colors[1] = AdjustForLuminance(colors[1], neutralColor);
                colors[2] = AdjustForLuminance(colors[2], colors[1]);
                break;
            case 2:
                colors[1] = AdjustForLuminance(colors[1], neutralColor);
                colors[2] = AdjustForLuminance(colors[2], neutralColor);
                colors[0] = AdjustForLuminance(colors[0], colors[1]);
                break;
            default:
                throw new InvalidOperationException("Internal error");
        }

        colors.Insert(neutralInsertIndex, neutralColor);

        var gradient = ColorFunc.Blend(colors.ToArray());
        return isReversed ? ColorFunc.Reverse(gradient) : gradient;
    }

    public static Func<double, Color> SelectGradient(BitEnumerator entropy, LifeHashVersion version)
    {
        if (version == LifeHashVersion.GrayscaleFiducial)
            return SelectGrayscale(entropy);

        var value = entropy.NextUint2();

        return value switch
        {
            0 => version switch
            {
                LifeHashVersion.Version1 => Monochromatic(entropy, MakeHue),
                LifeHashVersion.Version2 or LifeHashVersion.Detailed => Monochromatic(entropy, SpectrumCmykSafe()),
                LifeHashVersion.Fiducial => MonochromaticFiducial(entropy),
                _ => Grayscale(),
            },
            1 => version switch
            {
                LifeHashVersion.Version1 => Complementary(entropy, Spectrum()),
                LifeHashVersion.Version2 or LifeHashVersion.Detailed => Complementary(entropy, SpectrumCmykSafe()),
                LifeHashVersion.Fiducial => ComplementaryFiducial(entropy),
                _ => Grayscale(),
            },
            2 => version switch
            {
                LifeHashVersion.Version1 => Triadic(entropy, Spectrum()),
                LifeHashVersion.Version2 or LifeHashVersion.Detailed => Triadic(entropy, SpectrumCmykSafe()),
                LifeHashVersion.Fiducial => TriadicFiducial(entropy),
                _ => Grayscale(),
            },
            3 => version switch
            {
                LifeHashVersion.Version1 => Analogous(entropy, Spectrum()),
                LifeHashVersion.Version2 or LifeHashVersion.Detailed => Analogous(entropy, SpectrumCmykSafe()),
                LifeHashVersion.Fiducial => AnalogousFiducial(entropy),
                _ => Grayscale(),
            },
            _ => Grayscale(),
        };
    }
}
