package bclifehash

import "math"

// hsbColor represents a color in the Hue-Saturation-Brightness color space.
type hsbColor struct {
	hue        float64
	saturation float64
	brightness float64
}

func hsbFromHue(hue float64) hsbColor {
	return hsbColor{hue: hue, saturation: 1.0, brightness: 1.0}
}

func (hsb hsbColor) color() color {
	v := clamped(hsb.brightness)
	s := clamped(hsb.saturation)

	if s <= 0.0 {
		return newColor(v, v, v)
	}

	h := modulo(hsb.hue, 1.0)
	if h < 0.0 {
		h += 1.0
	}
	h *= 6.0
	// C++ uses floorf (f32 precision)
	i := int(math.Floor(float64(float32(h))))
	f := h - float64(i)
	p := v * (1.0 - s)
	q := v * (1.0 - s*f)
	t := v * (1.0 - s*(1.0-f))

	switch i {
	case 0:
		return newColor(v, t, p)
	case 1:
		return newColor(q, v, p)
	case 2:
		return newColor(p, v, t)
	case 3:
		return newColor(p, q, v)
	case 4:
		return newColor(t, p, v)
	case 5:
		return newColor(v, p, q)
	default:
		panic("internal error in HSB conversion")
	}
}
