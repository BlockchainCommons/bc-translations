package dcbor

import "math"

// Float16 is an IEEE 754 binary16 value.
type Float16 uint16

func Float16FromBits(bits uint16) Float16 {
	return Float16(bits)
}

func (h Float16) Bits() uint16 {
	return uint16(h)
}

func (h Float16) Float32() float32 {
	return halfBitsToFloat32(uint16(h))
}

func (h Float16) Float64() float64 {
	return float64(h.Float32())
}

func (h Float16) IsNaN() bool {
	return math.IsNaN(h.Float64())
}

func (h Float16) IsInf(sign int) bool {
	return math.IsInf(h.Float64(), sign)
}

func exactFloat16FromFloat64(value float64) (Float16, bool) {
	if math.IsNaN(value) {
		return Float16(0x7e00), true
	}
	if math.IsInf(value, 1) {
		return Float16(0x7c00), true
	}
	if math.IsInf(value, -1) {
		return Float16(0xfc00), true
	}
	if value > 65504.0 || value < -65504.0 {
		return 0, false
	}
	bits := float32ToHalfBits(float32(value))
	back := float64(halfBitsToFloat32(bits))
	if back == value {
		return Float16(bits), true
	}
	return 0, false
}

func halfBitsToFloat32(h uint16) float32 {
	sign := uint32(h>>15) & 0x1
	texp := uint32(h>>10) & 0x1f
	frac := uint32(h & 0x03ff)

	var bits uint32
	switch texp {
	case 0:
		if frac == 0 {
			bits = sign << 31
		} else {
			exp := int32(-14)
			for (frac & 0x0400) == 0 {
				frac <<= 1
				exp--
			}
			frac &= 0x03ff
			bits = (sign << 31) | (uint32(exp+127) << 23) | (frac << 13)
		}
	case 0x1f:
		bits = (sign << 31) | 0x7f800000 | (frac << 13)
	default:
		bits = (sign << 31) | ((texp + 112) << 23) | (frac << 13)
	}

	return math.Float32frombits(bits)
}

func float32ToHalfBits(f float32) uint16 {
	bits := math.Float32bits(f)
	sign := uint16((bits >> 16) & 0x8000)
	exp := int32((bits >> 23) & 0xff)
	frac := bits & 0x7fffff

	switch exp {
	case 0xff:
		if frac == 0 {
			return sign | 0x7c00
		}
		return sign | 0x7e00
	case 0:
		return sign
	}

	halfExp := exp - 127 + 15
	if halfExp >= 0x1f {
		return sign | 0x7c00
	}
	if halfExp <= 0 {
		if halfExp < -10 {
			return sign
		}
		frac |= 0x00800000
		shift := uint32(14 - halfExp)
		halfFrac := uint16(frac >> shift)
		if ((frac >> (shift - 1)) & 1) != 0 {
			halfFrac++
		}
		return sign | halfFrac
	}

	halfFrac := uint16(frac >> 13)
	if (frac & 0x1000) != 0 {
		halfFrac++
		if (halfFrac & 0x0400) != 0 {
			halfFrac = 0
			halfExp++
			if halfExp >= 0x1f {
				return sign | 0x7c00
			}
		}
	}

	return sign | uint16(halfExp<<10) | (halfFrac & 0x03ff)
}
