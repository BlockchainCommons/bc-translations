package bclifehash

import (
	"math"
	"slices"
)

func grayscale() colorFunc {
	return blend2(colorBlack, colorWhite)
}

func selectGrayscale(entropy *bitEnumerator) colorFunc {
	if entropy.next() {
		return grayscale()
	}
	return reverseColorFunc(grayscale())
}

func makeHue(t float64) color {
	return hsbFromHue(t).color()
}

func spectrum() colorFunc {
	return blend([]color{
		colorFromUint8Values(0, 168, 222),
		colorFromUint8Values(51, 51, 145),
		colorFromUint8Values(233, 19, 136),
		colorFromUint8Values(235, 45, 46),
		colorFromUint8Values(253, 233, 43),
		colorFromUint8Values(0, 158, 84),
		colorFromUint8Values(0, 168, 222),
	})
}

func spectrumCMYKSafe() colorFunc {
	return blend([]color{
		colorFromUint8Values(0, 168, 222),
		colorFromUint8Values(41, 60, 130),
		colorFromUint8Values(210, 59, 130),
		colorFromUint8Values(217, 63, 53),
		colorFromUint8Values(244, 228, 81),
		colorFromUint8Values(0, 158, 84),
		colorFromUint8Values(0, 168, 222),
	})
}

func adjustForLuminance(c, contrastColor color) color {
	lum := c.luminance()
	contrastLum := contrastColor.luminance()
	threshold := 0.6
	offset := math.Abs(lum - contrastLum)
	if offset > threshold {
		return c
	}
	boost := 0.7
	t := lerp(0.0, threshold, boost, 0.0, offset)
	if contrastLum > lum {
		return c.darken(t).burn(t * 0.6)
	}
	return c.lighten(t).burn(t * 0.6)
}

func monochromatic(entropy *bitEnumerator, hueGenerator colorFunc) colorFunc {
	hue := entropy.nextFrac()
	isTint := entropy.next()
	isReversed := entropy.next()
	keyAdvance := entropy.nextFrac()*0.3 + 0.05
	neutralAdvance := entropy.nextFrac()*0.3 + 0.05

	keyColor := hueGenerator(hue)

	var contrastBrightness float64
	if isTint {
		contrastBrightness = 1.0
		keyColor = keyColor.darken(0.5)
	} else {
		contrastBrightness = 0.0
	}
	gs := grayscale()
	neutralColor := gs(contrastBrightness)

	keyColor2 := keyColor.lerpTo(neutralColor, keyAdvance)
	neutralColor2 := neutralColor.lerpTo(keyColor, neutralAdvance)

	gradient := blend2(keyColor2, neutralColor2)
	if isReversed {
		return reverseColorFunc(gradient)
	}
	return gradient
}

func monochromaticFiducial(entropy *bitEnumerator) colorFunc {
	hue := entropy.nextFrac()
	isReversed := entropy.next()
	isTint := entropy.next()

	contrastColor := colorBlack
	if isTint {
		contrastColor = colorWhite
	}
	spec := spectrumCMYKSafe()
	specColor := spec(hue)
	keyColor := adjustForLuminance(specColor, contrastColor)

	gradient := blend([]color{keyColor, contrastColor, keyColor})
	if isReversed {
		return reverseColorFunc(gradient)
	}
	return gradient
}

func complementary(entropy *bitEnumerator, hueGenerator colorFunc) colorFunc {
	spectrum1 := entropy.nextFrac()
	spectrum2 := modulo(spectrum1+0.5, 1.0)
	lighterAdvance := entropy.nextFrac() * 0.3
	darkerAdvance := entropy.nextFrac() * 0.3
	isReversed := entropy.next()

	color1 := hueGenerator(spectrum1)
	color2 := hueGenerator(spectrum2)

	luma1 := color1.luminance()
	luma2 := color2.luminance()

	var darkerColor, lighterColor color
	if luma1 > luma2 {
		darkerColor = color2
		lighterColor = color1
	} else {
		darkerColor = color1
		lighterColor = color2
	}

	adjustedLighter := lighterColor.lighten(lighterAdvance)
	adjustedDarker := darkerColor.darken(darkerAdvance)

	gradient := blend2(adjustedDarker, adjustedLighter)
	if isReversed {
		return reverseColorFunc(gradient)
	}
	return gradient
}

func complementaryFiducial(entropy *bitEnumerator) colorFunc {
	spectrum1 := entropy.nextFrac()
	spectrum2 := modulo(spectrum1+0.5, 1.0)
	isTint := entropy.next()
	isReversed := entropy.next()
	neutralColorBias := entropy.next()

	neutralColor := colorBlack
	if isTint {
		neutralColor = colorWhite
	}
	spec := spectrumCMYKSafe()
	color1 := spec(spectrum1)
	color2 := spec(spectrum2)

	biasColor := color1
	if !neutralColorBias {
		biasColor = color2
	}
	biasedNeutralColor := neutralColor.lerpTo(biasColor, 0.2).burn(0.1)

	c1 := adjustForLuminance(color1, biasedNeutralColor)
	c2 := adjustForLuminance(color2, biasedNeutralColor)
	gradient := blend([]color{c1, biasedNeutralColor, c2})
	if isReversed {
		return reverseColorFunc(gradient)
	}
	return gradient
}

func triadic(entropy *bitEnumerator, hueGenerator colorFunc) colorFunc {
	spectrum1 := entropy.nextFrac()
	spectrum2 := modulo(spectrum1+1.0/3.0, 1.0)
	spectrum3 := modulo(spectrum1+2.0/3.0, 1.0)
	lighterAdvance := entropy.nextFrac() * 0.3
	darkerAdvance := entropy.nextFrac() * 0.3
	isReversed := entropy.next()

	color1 := hueGenerator(spectrum1)
	color2 := hueGenerator(spectrum2)
	color3 := hueGenerator(spectrum3)

	// Sort by luminance (ascending)
	colors := []color{color1, color2, color3}
	slices.SortFunc(colors, func(a, b color) int {
		la, lb := a.luminance(), b.luminance()
		if la < lb {
			return -1
		}
		if la > lb {
			return 1
		}
		return 0
	})

	darkerColor := colors[0]
	middleColor := colors[1]
	lighterColor := colors[2]

	adjustedLighter := lighterColor.lighten(lighterAdvance)
	adjustedDarker := darkerColor.darken(darkerAdvance)

	gradient := blend([]color{adjustedLighter, middleColor, adjustedDarker})
	if isReversed {
		return reverseColorFunc(gradient)
	}
	return gradient
}

func triadicFiducial(entropy *bitEnumerator) colorFunc {
	spectrum1 := entropy.nextFrac()
	spectrum2 := modulo(spectrum1+1.0/3.0, 1.0)
	spectrum3 := modulo(spectrum1+2.0/3.0, 1.0)
	isTint := entropy.next()
	neutralInsertIndex := int(entropy.nextUint8()%2 + 1)
	isReversed := entropy.next()

	neutralColor := colorBlack
	if isTint {
		neutralColor = colorWhite
	}

	spec := spectrumCMYKSafe()
	colors := []color{spec(spectrum1), spec(spectrum2), spec(spectrum3)}

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
		panic("internal error")
	}

	colors = slices.Insert(colors, neutralInsertIndex, neutralColor)

	gradient := blend(colors)
	if isReversed {
		return reverseColorFunc(gradient)
	}
	return gradient
}

func analogous(entropy *bitEnumerator, hueGenerator colorFunc) colorFunc {
	spectrum1 := entropy.nextFrac()
	spectrum2 := modulo(spectrum1+1.0/12.0, 1.0)
	spectrum3 := modulo(spectrum1+2.0/12.0, 1.0)
	spectrum4 := modulo(spectrum1+3.0/12.0, 1.0)
	advance := entropy.nextFrac()*0.5 + 0.2
	isReversed := entropy.next()

	color1 := hueGenerator(spectrum1)
	color2 := hueGenerator(spectrum2)
	color3 := hueGenerator(spectrum3)
	color4 := hueGenerator(spectrum4)

	var darkestColor, darkColor, lightColor, lightestColor color
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

	adjustedDarkest := darkestColor.darken(advance)
	adjustedDark := darkColor.darken(advance / 2.0)
	adjustedLight := lightColor.lighten(advance / 2.0)
	adjustedLightest := lightestColor.lighten(advance)

	gradient := blend([]color{adjustedDarkest, adjustedDark, adjustedLight, adjustedLightest})
	if isReversed {
		return reverseColorFunc(gradient)
	}
	return gradient
}

func analogousFiducial(entropy *bitEnumerator) colorFunc {
	spectrum1 := entropy.nextFrac()
	spectrum2 := modulo(spectrum1+1.0/10.0, 1.0)
	spectrum3 := modulo(spectrum1+2.0/10.0, 1.0)
	isTint := entropy.next()
	neutralInsertIndex := int(entropy.nextUint8()%2 + 1)
	isReversed := entropy.next()

	neutralColor := colorBlack
	if isTint {
		neutralColor = colorWhite
	}

	spec := spectrumCMYKSafe()
	colors := []color{spec(spectrum1), spec(spectrum2), spec(spectrum3)}

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
		panic("internal error")
	}

	colors = slices.Insert(colors, neutralInsertIndex, neutralColor)

	gradient := blend(colors)
	if isReversed {
		return reverseColorFunc(gradient)
	}
	return gradient
}

func selectGradient(entropy *bitEnumerator, version Version) colorFunc {
	if version == GrayscaleFiducial {
		return selectGrayscale(entropy)
	}

	value := entropy.nextUint2()

	switch value {
	case 0:
		switch version {
		case Version1:
			return monochromatic(entropy, makeHue)
		case Version2, Detailed:
			return monochromatic(entropy, spectrumCMYKSafe())
		case Fiducial:
			return monochromaticFiducial(entropy)
		default:
			return grayscale()
		}
	case 1:
		switch version {
		case Version1:
			return complementary(entropy, spectrum())
		case Version2, Detailed:
			return complementary(entropy, spectrumCMYKSafe())
		case Fiducial:
			return complementaryFiducial(entropy)
		default:
			return grayscale()
		}
	case 2:
		switch version {
		case Version1:
			return triadic(entropy, spectrum())
		case Version2, Detailed:
			return triadic(entropy, spectrumCMYKSafe())
		case Fiducial:
			return triadicFiducial(entropy)
		default:
			return grayscale()
		}
	case 3:
		switch version {
		case Version1:
			return analogous(entropy, spectrum())
		case Version2, Detailed:
			return analogous(entropy, spectrumCMYKSafe())
		case Fiducial:
			return analogousFiducial(entropy)
		default:
			return grayscale()
		}
	default:
		return grayscale()
	}
}
