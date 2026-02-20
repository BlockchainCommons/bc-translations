package bclifehash

// colorFunc is a function that maps a parameter in [0, 1] to a color.
type colorFunc func(float64) color

func reverseColorFunc(c colorFunc) colorFunc {
	return func(t float64) color {
		return c(1.0 - t)
	}
}

func blend2(color1, color2 color) colorFunc {
	return func(t float64) color {
		return color1.lerpTo(color2, t)
	}
}

func blend(colors []color) colorFunc {
	count := len(colors)
	switch count {
	case 0:
		return blend2(colorBlack, colorBlack)
	case 1:
		return blend2(colors[0], colors[0])
	case 2:
		return blend2(colors[0], colors[1])
	default:
		// Capture a copy of the slice to avoid aliasing.
		captured := make([]color, count)
		copy(captured, colors)
		return func(t float64) color {
			if t >= 1.0 {
				return captured[count-1]
			}
			if t <= 0.0 {
				return captured[0]
			}
			segments := count - 1
			s := t * float64(segments)
			segment := int(s)
			segmentFrac := modulo(s, 1.0)
			c1 := captured[segment]
			c2 := captured[segment+1]
			return c1.lerpTo(c2, segmentFrac)
		}
	}
}
