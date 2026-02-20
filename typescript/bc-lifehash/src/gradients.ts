import { BitEnumerator } from './bit-enumerator.js';
import { Color, lerp, modulo } from './color.js';
import { type ColorFunc, blend, blend2, reverse } from './color-func.js';
import { HSBColor } from './hsb-color.js';
import { Version } from './version.js';

function grayscale(): ColorFunc {
    return blend2(Color.BLACK, Color.WHITE);
}

function selectGrayscale(entropy: BitEnumerator): ColorFunc {
    return entropy.next() ? grayscale() : reverse(grayscale());
}

function makeHue(t: number): Color {
    return HSBColor.fromHue(t).color();
}

function spectrum(): ColorFunc {
    return blend([
        Color.fromUint8Values(0, 168, 222),
        Color.fromUint8Values(51, 51, 145),
        Color.fromUint8Values(233, 19, 136),
        Color.fromUint8Values(235, 45, 46),
        Color.fromUint8Values(253, 233, 43),
        Color.fromUint8Values(0, 158, 84),
        Color.fromUint8Values(0, 168, 222),
    ]);
}

function spectrumCmykSafe(): ColorFunc {
    return blend([
        Color.fromUint8Values(0, 168, 222),
        Color.fromUint8Values(41, 60, 130),
        Color.fromUint8Values(210, 59, 130),
        Color.fromUint8Values(217, 63, 53),
        Color.fromUint8Values(244, 228, 81),
        Color.fromUint8Values(0, 158, 84),
        Color.fromUint8Values(0, 168, 222),
    ]);
}

function adjustForLuminance(color: Color, contrastColor: Color): Color {
    const lum = color.luminance();
    const contrastLum = contrastColor.luminance();
    const threshold = 0.6;
    const offset = Math.abs(lum - contrastLum);
    if (offset > threshold) {
        return color;
    }
    const boost = 0.7;
    const t = lerp(0.0, threshold, boost, 0.0, offset);
    if (contrastLum > lum) {
        return color.darken(t).burn(t * 0.6);
    }
    return color.lighten(t).burn(t * 0.6);
}

function monochromatic(
    entropy: BitEnumerator,
    hueGenerator: ColorFunc,
): ColorFunc {
    const hue = entropy.nextFrac();
    const isTint = entropy.next();
    const isReversed = entropy.next();
    const keyAdvance = entropy.nextFrac() * 0.3 + 0.05;
    const neutralAdvance = entropy.nextFrac() * 0.3 + 0.05;

    let keyColor = hueGenerator(hue);

    let contrastBrightness: number;
    if (isTint) {
        contrastBrightness = 1.0;
        keyColor = keyColor.darken(0.5);
    } else {
        contrastBrightness = 0.0;
    }
    const gs = grayscale();
    const neutralColor = gs(contrastBrightness);

    const keyColor2 = keyColor.lerpTo(neutralColor, keyAdvance);
    const neutralColor2 = neutralColor.lerpTo(keyColor, neutralAdvance);

    const gradient = blend2(keyColor2, neutralColor2);
    return isReversed ? reverse(gradient) : gradient;
}

function monochromaticFiducial(entropy: BitEnumerator): ColorFunc {
    const hue = entropy.nextFrac();
    const isReversed = entropy.next();
    const isTint = entropy.next();

    const contrastColor = isTint ? Color.WHITE : Color.BLACK;
    const spec = spectrumCmykSafe();
    const keyColor = adjustForLuminance(spec(hue), contrastColor);

    const gradient = blend([keyColor, contrastColor, keyColor]);
    return isReversed ? reverse(gradient) : gradient;
}

function complementary(
    entropy: BitEnumerator,
    hueGenerator: ColorFunc,
): ColorFunc {
    const spectrum1 = entropy.nextFrac();
    const spectrum2 = modulo(spectrum1 + 0.5, 1.0);
    const lighterAdvance = entropy.nextFrac() * 0.3;
    const darkerAdvance = entropy.nextFrac() * 0.3;
    const isReversed = entropy.next();

    const color1 = hueGenerator(spectrum1);
    const color2 = hueGenerator(spectrum2);

    const luma1 = color1.luminance();
    const luma2 = color2.luminance();

    const [darkerColor, lighterColor] =
        luma1 > luma2 ? [color2, color1] : [color1, color2];

    const adjustedLighter = lighterColor.lighten(lighterAdvance);
    const adjustedDarker = darkerColor.darken(darkerAdvance);

    const gradient = blend2(adjustedDarker, adjustedLighter);
    return isReversed ? reverse(gradient) : gradient;
}

function complementaryFiducial(entropy: BitEnumerator): ColorFunc {
    const spectrum1 = entropy.nextFrac();
    const spectrum2 = modulo(spectrum1 + 0.5, 1.0);
    const isTint = entropy.next();
    const isReversed = entropy.next();
    const neutralColorBias = entropy.next();

    const neutralColor = isTint ? Color.WHITE : Color.BLACK;
    const spec = spectrumCmykSafe();
    const color1 = spec(spectrum1);
    const color2 = spec(spectrum2);

    const biasColor = neutralColorBias ? color1 : color2;
    const biasedNeutralColor = neutralColor.lerpTo(biasColor, 0.2).burn(0.1);

    const gradient = blend([
        adjustForLuminance(color1, biasedNeutralColor),
        biasedNeutralColor,
        adjustForLuminance(color2, biasedNeutralColor),
    ]);
    return isReversed ? reverse(gradient) : gradient;
}

function triadic(
    entropy: BitEnumerator,
    hueGenerator: ColorFunc,
): ColorFunc {
    const spectrum1 = entropy.nextFrac();
    const spectrum2 = modulo(spectrum1 + 1.0 / 3.0, 1.0);
    const spectrum3 = modulo(spectrum1 + 2.0 / 3.0, 1.0);
    const lighterAdvance = entropy.nextFrac() * 0.3;
    const darkerAdvance = entropy.nextFrac() * 0.3;
    const isReversed = entropy.next();

    const color1 = hueGenerator(spectrum1);
    const color2 = hueGenerator(spectrum2);
    const color3 = hueGenerator(spectrum3);

    const colors = [color1, color2, color3];
    colors.sort((a, b) => a.luminance() - b.luminance());

    const darkerColor = colors[0];
    const middleColor = colors[1];
    const lighterColor = colors[2];

    const adjustedLighter = lighterColor.lighten(lighterAdvance);
    const adjustedDarker = darkerColor.darken(darkerAdvance);

    const gradient = blend([adjustedLighter, middleColor, adjustedDarker]);
    return isReversed ? reverse(gradient) : gradient;
}

function triadicFiducial(entropy: BitEnumerator): ColorFunc {
    const spectrum1 = entropy.nextFrac();
    const spectrum2 = modulo(spectrum1 + 1.0 / 3.0, 1.0);
    const spectrum3 = modulo(spectrum1 + 2.0 / 3.0, 1.0);
    const isTint = entropy.next();
    const neutralInsertIndex = (entropy.nextUint8() % 2 + 1);
    const isReversed = entropy.next();

    const neutralColor = isTint ? Color.WHITE : Color.BLACK;

    const spec = spectrumCmykSafe();
    const colors = [spec(spectrum1), spec(spectrum2), spec(spectrum3)];

    if (neutralInsertIndex === 1) {
        colors[0] = adjustForLuminance(colors[0], neutralColor);
        colors[1] = adjustForLuminance(colors[1], neutralColor);
        colors[2] = adjustForLuminance(colors[2], colors[1]);
    } else {
        colors[1] = adjustForLuminance(colors[1], neutralColor);
        colors[2] = adjustForLuminance(colors[2], neutralColor);
        colors[0] = adjustForLuminance(colors[0], colors[1]);
    }

    colors.splice(neutralInsertIndex, 0, neutralColor);

    const gradient = blend(colors);
    return isReversed ? reverse(gradient) : gradient;
}

function analogous(
    entropy: BitEnumerator,
    hueGenerator: ColorFunc,
): ColorFunc {
    const spectrum1 = entropy.nextFrac();
    const spectrum2 = modulo(spectrum1 + 1.0 / 12.0, 1.0);
    const spectrum3 = modulo(spectrum1 + 2.0 / 12.0, 1.0);
    const spectrum4 = modulo(spectrum1 + 3.0 / 12.0, 1.0);
    const advance = entropy.nextFrac() * 0.5 + 0.2;
    const isReversed = entropy.next();

    const color1 = hueGenerator(spectrum1);
    const color2 = hueGenerator(spectrum2);
    const color3 = hueGenerator(spectrum3);
    const color4 = hueGenerator(spectrum4);

    let darkestColor: Color, darkColor: Color, lightColor: Color, lightestColor: Color;
    if (color1.luminance() < color4.luminance()) {
        darkestColor = color1;
        darkColor = color2;
        lightColor = color3;
        lightestColor = color4;
    } else {
        darkestColor = color4;
        darkColor = color3;
        lightColor = color2;
        lightestColor = color1;
    }

    const adjustedDarkest = darkestColor.darken(advance);
    const adjustedDark = darkColor.darken(advance / 2.0);
    const adjustedLight = lightColor.lighten(advance / 2.0);
    const adjustedLightest = lightestColor.lighten(advance);

    const gradient = blend([adjustedDarkest, adjustedDark, adjustedLight, adjustedLightest]);
    return isReversed ? reverse(gradient) : gradient;
}

function analogousFiducial(entropy: BitEnumerator): ColorFunc {
    const spectrum1 = entropy.nextFrac();
    const spectrum2 = modulo(spectrum1 + 1.0 / 10.0, 1.0);
    const spectrum3 = modulo(spectrum1 + 2.0 / 10.0, 1.0);
    const isTint = entropy.next();
    const neutralInsertIndex = (entropy.nextUint8() % 2 + 1);
    const isReversed = entropy.next();

    const neutralColor = isTint ? Color.WHITE : Color.BLACK;

    const spec = spectrumCmykSafe();
    const colors = [spec(spectrum1), spec(spectrum2), spec(spectrum3)];

    if (neutralInsertIndex === 1) {
        colors[0] = adjustForLuminance(colors[0], neutralColor);
        colors[1] = adjustForLuminance(colors[1], neutralColor);
        colors[2] = adjustForLuminance(colors[2], colors[1]);
    } else {
        colors[1] = adjustForLuminance(colors[1], neutralColor);
        colors[2] = adjustForLuminance(colors[2], neutralColor);
        colors[0] = adjustForLuminance(colors[0], colors[1]);
    }

    colors.splice(neutralInsertIndex, 0, neutralColor);

    const gradient = blend(colors);
    return isReversed ? reverse(gradient) : gradient;
}

export function selectGradient(
    entropy: BitEnumerator,
    version: Version,
): ColorFunc {
    if (version === Version.GrayscaleFiducial) {
        return selectGrayscale(entropy);
    }

    const value = entropy.nextUint2();

    switch (value) {
        case 0:
            switch (version) {
                case Version.Version1:
                    return monochromatic(entropy, makeHue);
                case Version.Version2:
                case Version.Detailed:
                    return monochromatic(entropy, spectrumCmykSafe());
                case Version.Fiducial:
                    return monochromaticFiducial(entropy);
                default:
                    return grayscale();
            }
        case 1:
            switch (version) {
                case Version.Version1:
                    return complementary(entropy, spectrum());
                case Version.Version2:
                case Version.Detailed:
                    return complementary(entropy, spectrumCmykSafe());
                case Version.Fiducial:
                    return complementaryFiducial(entropy);
                default:
                    return grayscale();
            }
        case 2:
            switch (version) {
                case Version.Version1:
                    return triadic(entropy, spectrum());
                case Version.Version2:
                case Version.Detailed:
                    return triadic(entropy, spectrumCmykSafe());
                case Version.Fiducial:
                    return triadicFiducial(entropy);
                default:
                    return grayscale();
            }
        case 3:
            switch (version) {
                case Version.Version1:
                    return analogous(entropy, spectrum());
                case Version.Version2:
                case Version.Detailed:
                    return analogous(entropy, spectrumCmykSafe());
                case Version.Fiducial:
                    return analogousFiducial(entropy);
                default:
                    return grayscale();
            }
        default:
            return grayscale();
    }
}
