func grayscale() -> ColorFunc {
    blend2(.black, .white)
}

func selectGrayscale(_ entropy: BitEnumerator) -> ColorFunc {
    entropy.next() ? grayscale() : reverse(grayscale())
}

func makeHue(_ t: Double) -> Color {
    HSBColor.fromHue(t).color()
}

func spectrum() -> ColorFunc {
    blend([
        Color.fromUInt8Values(0, 168, 222),
        Color.fromUInt8Values(51, 51, 145),
        Color.fromUInt8Values(233, 19, 136),
        Color.fromUInt8Values(235, 45, 46),
        Color.fromUInt8Values(253, 233, 43),
        Color.fromUInt8Values(0, 158, 84),
        Color.fromUInt8Values(0, 168, 222),
    ])
}

func spectrumCMYKSafe() -> ColorFunc {
    blend([
        Color.fromUInt8Values(0, 168, 222),
        Color.fromUInt8Values(41, 60, 130),
        Color.fromUInt8Values(210, 59, 130),
        Color.fromUInt8Values(217, 63, 53),
        Color.fromUInt8Values(244, 228, 81),
        Color.fromUInt8Values(0, 158, 84),
        Color.fromUInt8Values(0, 168, 222),
    ])
}

func adjustForLuminance(_ color: Color, _ contrastColor: Color) -> Color {
    let lum = color.luminance()
    let contrastLum = contrastColor.luminance()
    let threshold = 0.6
    let offset = abs(lum - contrastLum)
    if offset > threshold {
        return color
    }
    let boost = 0.7
    let t = lerp(0.0, threshold, boost, 0.0, offset)
    if contrastLum > lum {
        return color.darken(t).burn(t * 0.6)
    }
    return color.lighten(t).burn(t * 0.6)
}

func monochromatic(_ entropy: BitEnumerator, _ hueGenerator: @escaping ColorFunc) -> ColorFunc {
    let hue = entropy.nextFrac()
    let isTint = entropy.next()
    let isReversed = entropy.next()
    let keyAdvance = entropy.nextFrac() * 0.3 + 0.05
    let neutralAdvance = entropy.nextFrac() * 0.3 + 0.05

    var keyColor = hueGenerator(hue)

    let contrastBrightness: Double
    if isTint {
        contrastBrightness = 1.0
        keyColor = keyColor.darken(0.5)
    } else {
        contrastBrightness = 0.0
    }

    let gs = grayscale()
    let neutralColor = gs(contrastBrightness)

    let keyColor2 = keyColor.lerpTo(neutralColor, keyAdvance)
    let neutralColor2 = neutralColor.lerpTo(keyColor, neutralAdvance)

    let gradient = blend2(keyColor2, neutralColor2)
    return isReversed ? reverse(gradient) : gradient
}

func monochromaticFiducial(_ entropy: BitEnumerator) -> ColorFunc {
    let hue = entropy.nextFrac()
    let isReversed = entropy.next()
    let isTint = entropy.next()

    let contrastColor = isTint ? Color.white : Color.black
    let spec = spectrumCMYKSafe()
    let keyColor = adjustForLuminance(spec(hue), contrastColor)

    let gradient = blend([keyColor, contrastColor, keyColor])
    return isReversed ? reverse(gradient) : gradient
}

func complementary(_ entropy: BitEnumerator, _ hueGenerator: @escaping ColorFunc) -> ColorFunc {
    let spectrum1 = entropy.nextFrac()
    let spectrum2 = modulo(spectrum1 + 0.5, 1.0)
    let lighterAdvance = entropy.nextFrac() * 0.3
    let darkerAdvance = entropy.nextFrac() * 0.3
    let isReversed = entropy.next()

    let color1 = hueGenerator(spectrum1)
    let color2 = hueGenerator(spectrum2)

    let luma1 = color1.luminance()
    let luma2 = color2.luminance()

    let darkerColor: Color
    let lighterColor: Color
    if luma1 > luma2 {
        darkerColor = color2
        lighterColor = color1
    } else {
        darkerColor = color1
        lighterColor = color2
    }

    let adjustedLighter = lighterColor.lighten(lighterAdvance)
    let adjustedDarker = darkerColor.darken(darkerAdvance)

    let gradient = blend2(adjustedDarker, adjustedLighter)
    return isReversed ? reverse(gradient) : gradient
}

func complementaryFiducial(_ entropy: BitEnumerator) -> ColorFunc {
    let spectrum1 = entropy.nextFrac()
    let spectrum2 = modulo(spectrum1 + 0.5, 1.0)
    let isTint = entropy.next()
    let isReversed = entropy.next()
    let neutralColorBias = entropy.next()

    let neutralColor = isTint ? Color.white : Color.black
    let spec = spectrumCMYKSafe()
    let color1 = spec(spectrum1)
    let color2 = spec(spectrum2)

    let biasColor = neutralColorBias ? color1 : color2
    let biasedNeutralColor = neutralColor.lerpTo(biasColor, 0.2).burn(0.1)

    let gradient = blend([
        adjustForLuminance(color1, biasedNeutralColor),
        biasedNeutralColor,
        adjustForLuminance(color2, biasedNeutralColor),
    ])
    return isReversed ? reverse(gradient) : gradient
}

func triadic(_ entropy: BitEnumerator, _ hueGenerator: @escaping ColorFunc) -> ColorFunc {
    let spectrum1 = entropy.nextFrac()
    let spectrum2 = modulo(spectrum1 + 1.0 / 3.0, 1.0)
    let spectrum3 = modulo(spectrum1 + 2.0 / 3.0, 1.0)
    let lighterAdvance = entropy.nextFrac() * 0.3
    let darkerAdvance = entropy.nextFrac() * 0.3
    let isReversed = entropy.next()

    let color1 = hueGenerator(spectrum1)
    let color2 = hueGenerator(spectrum2)
    let color3 = hueGenerator(spectrum3)

    var colors = [color1, color2, color3]
    colors.sort { $0.luminance() < $1.luminance() }

    let darkerColor = colors[0]
    let middleColor = colors[1]
    let lighterColor = colors[2]

    let adjustedLighter = lighterColor.lighten(lighterAdvance)
    let adjustedDarker = darkerColor.darken(darkerAdvance)

    let gradient = blend([adjustedLighter, middleColor, adjustedDarker])
    return isReversed ? reverse(gradient) : gradient
}

func triadicFiducial(_ entropy: BitEnumerator) -> ColorFunc {
    let spectrum1 = entropy.nextFrac()
    let spectrum2 = modulo(spectrum1 + 1.0 / 3.0, 1.0)
    let spectrum3 = modulo(spectrum1 + 2.0 / 3.0, 1.0)
    let isTint = entropy.next()
    let neutralInsertIndex = Int(entropy.nextUInt8() % 2 + 1)
    let isReversed = entropy.next()

    let neutralColor = isTint ? Color.white : Color.black

    let spec = spectrumCMYKSafe()
    var colors = [spec(spectrum1), spec(spectrum2), spec(spectrum3)]

    switch neutralInsertIndex {
    case 1:
        colors[0] = adjustForLuminance(colors[0], neutralColor)
        colors[1] = adjustForLuminance(colors[1], neutralColor)
        colors[2] = adjustForLuminance(colors[2], colors[1])
    case 2:
        colors[1] = adjustForLuminance(colors[1], neutralColor)
        colors[2] = adjustForLuminance(colors[2], neutralColor)
        colors[0] = adjustForLuminance(colors[0], colors[1])
    default:
        preconditionFailure("Internal error")
    }

    colors.insert(neutralColor, at: neutralInsertIndex)

    let gradient = blend(colors)
    return isReversed ? reverse(gradient) : gradient
}

func analogous(_ entropy: BitEnumerator, _ hueGenerator: @escaping ColorFunc) -> ColorFunc {
    let spectrum1 = entropy.nextFrac()
    let spectrum2 = modulo(spectrum1 + 1.0 / 12.0, 1.0)
    let spectrum3 = modulo(spectrum1 + 2.0 / 12.0, 1.0)
    let spectrum4 = modulo(spectrum1 + 3.0 / 12.0, 1.0)
    let advance = entropy.nextFrac() * 0.5 + 0.2
    let isReversed = entropy.next()

    let color1 = hueGenerator(spectrum1)
    let color2 = hueGenerator(spectrum2)
    let color3 = hueGenerator(spectrum3)
    let color4 = hueGenerator(spectrum4)

    let darkestColor: Color
    let darkColor: Color
    let lightColor: Color
    let lightestColor: Color

    if color1.luminance() < color4.luminance() {
        darkestColor = color1
        darkColor = color2
        lightColor = color3
        lightestColor = color4
    } else {
        darkestColor = color4
        darkColor = color3
        lightColor = color2
        lightestColor = color1
    }

    let adjustedDarkest = darkestColor.darken(advance)
    let adjustedDark = darkColor.darken(advance / 2.0)
    let adjustedLight = lightColor.lighten(advance / 2.0)
    let adjustedLightest = lightestColor.lighten(advance)

    let gradient = blend([adjustedDarkest, adjustedDark, adjustedLight, adjustedLightest])
    return isReversed ? reverse(gradient) : gradient
}

func analogousFiducial(_ entropy: BitEnumerator) -> ColorFunc {
    let spectrum1 = entropy.nextFrac()
    let spectrum2 = modulo(spectrum1 + 1.0 / 10.0, 1.0)
    let spectrum3 = modulo(spectrum1 + 2.0 / 10.0, 1.0)
    let isTint = entropy.next()
    let neutralInsertIndex = Int(entropy.nextUInt8() % 2 + 1)
    let isReversed = entropy.next()

    let neutralColor = isTint ? Color.white : Color.black

    let spec = spectrumCMYKSafe()
    var colors = [spec(spectrum1), spec(spectrum2), spec(spectrum3)]

    switch neutralInsertIndex {
    case 1:
        colors[0] = adjustForLuminance(colors[0], neutralColor)
        colors[1] = adjustForLuminance(colors[1], neutralColor)
        colors[2] = adjustForLuminance(colors[2], colors[1])
    case 2:
        colors[1] = adjustForLuminance(colors[1], neutralColor)
        colors[2] = adjustForLuminance(colors[2], neutralColor)
        colors[0] = adjustForLuminance(colors[0], colors[1])
    default:
        preconditionFailure("Internal error")
    }

    colors.insert(neutralColor, at: neutralInsertIndex)

    let gradient = blend(colors)
    return isReversed ? reverse(gradient) : gradient
}

func selectGradient(_ entropy: BitEnumerator, _ version: Version) -> ColorFunc {
    if version == .grayscaleFiducial {
        return selectGrayscale(entropy)
    }

    let value = entropy.nextUInt2()

    switch value {
    case 0:
        switch version {
        case .version1:
            return monochromatic(entropy, makeHue)
        case .version2, .detailed:
            let spec = spectrumCMYKSafe()
            return monochromatic(entropy, spec)
        case .fiducial:
            return monochromaticFiducial(entropy)
        case .grayscaleFiducial:
            return grayscale()
        }
    case 1:
        switch version {
        case .version1:
            let spec = spectrum()
            return complementary(entropy, spec)
        case .version2, .detailed:
            let spec = spectrumCMYKSafe()
            return complementary(entropy, spec)
        case .fiducial:
            return complementaryFiducial(entropy)
        case .grayscaleFiducial:
            return grayscale()
        }
    case 2:
        switch version {
        case .version1:
            let spec = spectrum()
            return triadic(entropy, spec)
        case .version2, .detailed:
            let spec = spectrumCMYKSafe()
            return triadic(entropy, spec)
        case .fiducial:
            return triadicFiducial(entropy)
        case .grayscaleFiducial:
            return grayscale()
        }
    case 3:
        switch version {
        case .version1:
            let spec = spectrum()
            return analogous(entropy, spec)
        case .version2, .detailed:
            let spec = spectrumCMYKSafe()
            return analogous(entropy, spec)
        case .fiducial:
            return analogousFiducial(entropy)
        case .grayscaleFiducial:
            return grayscale()
        }
    default:
        return grayscale()
    }
}
