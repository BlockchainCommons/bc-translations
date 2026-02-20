package bclifehash

type pattern int

const (
	patternSnowflake pattern = iota
	patternPinwheel
	patternFiducial
)

func selectPattern(entropy *bitEnumerator, version Version) pattern {
	switch version {
	case Fiducial, GrayscaleFiducial:
		return patternFiducial
	default:
		if entropy.next() {
			return patternSnowflake
		}
		return patternPinwheel
	}
}
