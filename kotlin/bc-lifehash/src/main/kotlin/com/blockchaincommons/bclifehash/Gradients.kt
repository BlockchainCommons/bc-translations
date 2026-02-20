package com.blockchaincommons.bclifehash

private fun grayscale(): ColorFunc = blend2(Color.BLACK, Color.WHITE)

private fun selectGrayscale(entropy: BitEnumerator): ColorFunc =
    if (entropy.next()) {
        grayscale()
    } else {
        reverse(grayscale())
    }

private fun makeHue(t: Double): Color = HSBColor.fromHue(t).color()

private fun spectrum(): ColorFunc =
    blend(
        listOf(
            Color.fromUInt8Values(0, 168, 222),
            Color.fromUInt8Values(51, 51, 145),
            Color.fromUInt8Values(233, 19, 136),
            Color.fromUInt8Values(235, 45, 46),
            Color.fromUInt8Values(253, 233, 43),
            Color.fromUInt8Values(0, 158, 84),
            Color.fromUInt8Values(0, 168, 222),
        ),
    )

private fun spectrumCmykSafe(): ColorFunc =
    blend(
        listOf(
            Color.fromUInt8Values(0, 168, 222),
            Color.fromUInt8Values(41, 60, 130),
            Color.fromUInt8Values(210, 59, 130),
            Color.fromUInt8Values(217, 63, 53),
            Color.fromUInt8Values(244, 228, 81),
            Color.fromUInt8Values(0, 158, 84),
            Color.fromUInt8Values(0, 168, 222),
        ),
    )

private fun adjustForLuminance(color: Color, contrastColor: Color): Color {
    val lum = color.luminance()
    val contrastLum = contrastColor.luminance()
    val threshold = 0.6
    val offset = kotlin.math.abs(lum - contrastLum)
    if (offset > threshold) {
        return color
    }

    val boost = 0.7
    val t = lerp(0.0, threshold, boost, 0.0, offset)
    return if (contrastLum > lum) {
        color.darken(t).burn(t * 0.6)
    } else {
        color.lighten(t).burn(t * 0.6)
    }
}

private fun monochromatic(entropy: BitEnumerator, hueGenerator: ColorFunc): ColorFunc {
    val hue = entropy.nextFrac()
    val isTint = entropy.next()
    val isReversed = entropy.next()
    val keyAdvance = entropy.nextFrac() * 0.3 + 0.05
    val neutralAdvance = entropy.nextFrac() * 0.3 + 0.05

    var keyColor = hueGenerator(hue)

    val contrastBrightness = if (isTint) {
        keyColor = keyColor.darken(0.5)
        1.0
    } else {
        0.0
    }

    val neutralColor = grayscale()(contrastBrightness)
    val keyColor2 = keyColor.lerpTo(neutralColor, keyAdvance)
    val neutralColor2 = neutralColor.lerpTo(keyColor, neutralAdvance)

    val gradient = blend2(keyColor2, neutralColor2)
    return if (isReversed) reverse(gradient) else gradient
}

private fun monochromaticFiducial(entropy: BitEnumerator): ColorFunc {
    val hue = entropy.nextFrac()
    val isReversed = entropy.next()
    val isTint = entropy.next()

    val contrastColor = if (isTint) Color.WHITE else Color.BLACK
    val keyColor = adjustForLuminance(spectrumCmykSafe()(hue), contrastColor)

    val gradient = blend(listOf(keyColor, contrastColor, keyColor))
    return if (isReversed) reverse(gradient) else gradient
}

private fun complementary(entropy: BitEnumerator, hueGenerator: ColorFunc): ColorFunc {
    val spectrum1 = entropy.nextFrac()
    val spectrum2 = modulo(spectrum1 + 0.5, 1.0)
    val lighterAdvance = entropy.nextFrac() * 0.3
    val darkerAdvance = entropy.nextFrac() * 0.3
    val isReversed = entropy.next()

    val color1 = hueGenerator(spectrum1)
    val color2 = hueGenerator(spectrum2)

    val luma1 = color1.luminance()
    val luma2 = color2.luminance()

    val (darkerColor, lighterColor) = if (luma1 > luma2) {
        color2 to color1
    } else {
        color1 to color2
    }

    val adjustedLighter = lighterColor.lighten(lighterAdvance)
    val adjustedDarker = darkerColor.darken(darkerAdvance)

    val gradient = blend2(adjustedDarker, adjustedLighter)
    return if (isReversed) reverse(gradient) else gradient
}

private fun complementaryFiducial(entropy: BitEnumerator): ColorFunc {
    val spectrum1 = entropy.nextFrac()
    val spectrum2 = modulo(spectrum1 + 0.5, 1.0)
    val isTint = entropy.next()
    val isReversed = entropy.next()
    val neutralColorBias = entropy.next()

    val neutralColor = if (isTint) Color.WHITE else Color.BLACK
    val color1 = spectrumCmykSafe()(spectrum1)
    val color2 = spectrumCmykSafe()(spectrum2)

    val biasColor = if (neutralColorBias) color1 else color2
    val biasedNeutralColor = neutralColor.lerpTo(biasColor, 0.2).burn(0.1)

    val gradient = blend(
        listOf(
            adjustForLuminance(color1, biasedNeutralColor),
            biasedNeutralColor,
            adjustForLuminance(color2, biasedNeutralColor),
        ),
    )
    return if (isReversed) reverse(gradient) else gradient
}

private fun triadic(entropy: BitEnumerator, hueGenerator: ColorFunc): ColorFunc {
    val spectrum1 = entropy.nextFrac()
    val spectrum2 = modulo(spectrum1 + 1.0 / 3.0, 1.0)
    val spectrum3 = modulo(spectrum1 + 2.0 / 3.0, 1.0)
    val lighterAdvance = entropy.nextFrac() * 0.3
    val darkerAdvance = entropy.nextFrac() * 0.3
    val isReversed = entropy.next()

    val color1 = hueGenerator(spectrum1)
    val color2 = hueGenerator(spectrum2)
    val color3 = hueGenerator(spectrum3)

    val colorsByLum = arrayOf(color1, color2, color3).sortedBy { it.luminance() }

    val darkerColor = colorsByLum[0]
    val middleColor = colorsByLum[1]
    val lighterColor = colorsByLum[2]

    val adjustedLighter = lighterColor.lighten(lighterAdvance)
    val adjustedDarker = darkerColor.darken(darkerAdvance)

    val gradient = blend(listOf(adjustedLighter, middleColor, adjustedDarker))
    return if (isReversed) reverse(gradient) else gradient
}

private fun triadicFiducial(entropy: BitEnumerator): ColorFunc {
    val spectrum1 = entropy.nextFrac()
    val spectrum2 = modulo(spectrum1 + 1.0 / 3.0, 1.0)
    val spectrum3 = modulo(spectrum1 + 2.0 / 3.0, 1.0)
    val isTint = entropy.next()
    val neutralInsertIndex = (entropy.nextUInt8() % 2) + 1
    val isReversed = entropy.next()

    val neutralColor = if (isTint) Color.WHITE else Color.BLACK
    val colors = mutableListOf(
        spectrumCmykSafe()(spectrum1),
        spectrumCmykSafe()(spectrum2),
        spectrumCmykSafe()(spectrum3),
    )

    when (neutralInsertIndex) {
        1 -> {
            colors[0] = adjustForLuminance(colors[0], neutralColor)
            colors[1] = adjustForLuminance(colors[1], neutralColor)
            colors[2] = adjustForLuminance(colors[2], colors[1])
        }

        2 -> {
            colors[1] = adjustForLuminance(colors[1], neutralColor)
            colors[2] = adjustForLuminance(colors[2], neutralColor)
            colors[0] = adjustForLuminance(colors[0], colors[1])
        }

        else -> error("Internal error")
    }

    colors.add(neutralInsertIndex, neutralColor)
    val gradient = blend(colors)
    return if (isReversed) reverse(gradient) else gradient
}

private fun analogous(entropy: BitEnumerator, hueGenerator: ColorFunc): ColorFunc {
    val spectrum1 = entropy.nextFrac()
    val spectrum2 = modulo(spectrum1 + 1.0 / 12.0, 1.0)
    val spectrum3 = modulo(spectrum1 + 2.0 / 12.0, 1.0)
    val spectrum4 = modulo(spectrum1 + 3.0 / 12.0, 1.0)
    val advance = entropy.nextFrac() * 0.5 + 0.2
    val isReversed = entropy.next()

    val color1 = hueGenerator(spectrum1)
    val color2 = hueGenerator(spectrum2)
    val color3 = hueGenerator(spectrum3)
    val color4 = hueGenerator(spectrum4)

    val (darkestColor, darkColor, lightColor, lightestColor) =
        if (color1.luminance() < color4.luminance()) {
            listOf(color1, color2, color3, color4)
        } else {
            listOf(color4, color3, color2, color1)
        }

    val adjustedDarkest = darkestColor.darken(advance)
    val adjustedDark = darkColor.darken(advance / 2.0)
    val adjustedLight = lightColor.lighten(advance / 2.0)
    val adjustedLightest = lightestColor.lighten(advance)

    val gradient = blend(listOf(adjustedDarkest, adjustedDark, adjustedLight, adjustedLightest))
    return if (isReversed) reverse(gradient) else gradient
}

private fun analogousFiducial(entropy: BitEnumerator): ColorFunc {
    val spectrum1 = entropy.nextFrac()
    val spectrum2 = modulo(spectrum1 + 1.0 / 10.0, 1.0)
    val spectrum3 = modulo(spectrum1 + 2.0 / 10.0, 1.0)
    val isTint = entropy.next()
    val neutralInsertIndex = (entropy.nextUInt8() % 2) + 1
    val isReversed = entropy.next()

    val neutralColor = if (isTint) Color.WHITE else Color.BLACK
    val colors = mutableListOf(
        spectrumCmykSafe()(spectrum1),
        spectrumCmykSafe()(spectrum2),
        spectrumCmykSafe()(spectrum3),
    )

    when (neutralInsertIndex) {
        1 -> {
            colors[0] = adjustForLuminance(colors[0], neutralColor)
            colors[1] = adjustForLuminance(colors[1], neutralColor)
            colors[2] = adjustForLuminance(colors[2], colors[1])
        }

        2 -> {
            colors[1] = adjustForLuminance(colors[1], neutralColor)
            colors[2] = adjustForLuminance(colors[2], neutralColor)
            colors[0] = adjustForLuminance(colors[0], colors[1])
        }

        else -> error("Internal error")
    }

    colors.add(neutralInsertIndex, neutralColor)
    val gradient = blend(colors)
    return if (isReversed) reverse(gradient) else gradient
}

fun selectGradient(entropy: BitEnumerator, version: Version): ColorFunc {
    if (version == Version.GrayscaleFiducial) {
        return selectGrayscale(entropy)
    }

    return when (entropy.nextUInt2()) {
        0 -> when (version) {
            Version.Version1 -> monochromatic(entropy, ::makeHue)
            Version.Version2, Version.Detailed -> monochromatic(entropy, spectrumCmykSafe())
            Version.Fiducial -> monochromaticFiducial(entropy)
            Version.GrayscaleFiducial -> grayscale()
        }

        1 -> when (version) {
            Version.Version1 -> complementary(entropy, spectrum())
            Version.Version2, Version.Detailed -> complementary(entropy, spectrumCmykSafe())
            Version.Fiducial -> complementaryFiducial(entropy)
            Version.GrayscaleFiducial -> grayscale()
        }

        2 -> when (version) {
            Version.Version1 -> triadic(entropy, spectrum())
            Version.Version2, Version.Detailed -> triadic(entropy, spectrumCmykSafe())
            Version.Fiducial -> triadicFiducial(entropy)
            Version.GrayscaleFiducial -> grayscale()
        }

        3 -> when (version) {
            Version.Version1 -> analogous(entropy, spectrum())
            Version.Version2, Version.Detailed -> analogous(entropy, spectrumCmykSafe())
            Version.Fiducial -> analogousFiducial(entropy)
            Version.GrayscaleFiducial -> grayscale()
        }

        else -> grayscale()
    }
}
