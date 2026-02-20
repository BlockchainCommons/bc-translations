package bclifehash

import "math"

// color represents an RGB color with components in [0, 1].
type color struct {
	r, g, b float64
}

var (
	colorWhite = color{1, 1, 1}
	colorBlack = color{0, 0, 0}
)

func newColor(r, g, b float64) color {
	return color{r, g, b}
}

func colorFromUint8Values(r, g, b uint8) color {
	return color{
		r: float64(r) / 255.0,
		g: float64(g) / 255.0,
		b: float64(b) / 255.0,
	}
}

// clamped constrains n to [0, 1].
func clamped(n float64) float64 {
	return math.Min(math.Max(n, 0), 1)
}

// modulo uses float32 precision to match the C++ reference (fmodf).
func modulo(dividend, divisor float64) float64 {
	a := float32(dividend)
	b := float32(divisor)
	step1 := float32(math.Mod(float64(a), float64(b)))
	step2 := float32(math.Mod(float64(step1+b), float64(b)))
	return float64(step2)
}

// lerpTo interpolates from toA to toB at parameter t.
func lerpTo(toA, toB, t float64) float64 {
	return t*(toB-toA) + toA
}

// lerpFrom computes the inverse interpolation: where t falls in [fromA, fromB].
func lerpFrom(fromA, fromB, t float64) float64 {
	return (fromA - t) / (fromA - fromB)
}

// lerp maps t from [fromA, fromB] to [toC, toD].
func lerp(fromA, fromB, toC, toD, t float64) float64 {
	return lerpTo(toC, toD, lerpFrom(fromA, fromB, t))
}

func (c color) lerpTo(other color, t float64) color {
	f := clamped(t)
	return newColor(
		clamped(c.r*(1.0-f)+other.r*f),
		clamped(c.g*(1.0-f)+other.g*f),
		clamped(c.b*(1.0-f)+other.b*f),
	)
}

func (c color) lighten(t float64) color {
	return c.lerpTo(colorWhite, t)
}

func (c color) darken(t float64) color {
	return c.lerpTo(colorBlack, t)
}

func (c color) burn(t float64) color {
	f := math.Max(1.0-t, 1.0e-7)
	return newColor(
		math.Min(1.0-(1.0-c.r)/f, 1.0),
		math.Min(1.0-(1.0-c.g)/f, 1.0),
		math.Min(1.0-(1.0-c.b)/f, 1.0),
	)
}

// luminance computes perceived luminance using float32 precision
// to match the C++ reference (sqrtf/powf).
func (c color) luminance() float64 {
	rv := float32(0.299 * c.r)
	gv := float32(0.587 * c.g)
	bv := float32(0.114 * c.b)
	val := rv*rv + gv*gv + bv*bv
	// sqrtf precision: float32(sqrt(float64(val))) == sqrtf(val) per IEEE 754
	return float64(float32(math.Sqrt(float64(val))))
}
