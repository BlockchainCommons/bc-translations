from __future__ import annotations

import math
import struct


def _to_f32(x: float) -> float:
    """Cast to IEEE 754 single precision and back, matching C++ float."""
    return struct.unpack("f", struct.pack("f", x))[0]


def clamped(n: float) -> float:
    return max(0.0, min(1.0, n))


def modulo(dividend: float, divisor: float) -> float:
    """C++ fmodf-compatible modulo using f32 precision."""
    a = _to_f32(dividend)
    b = _to_f32(divisor)
    step1 = _to_f32(math.fmod(a, b))
    step2 = _to_f32(math.fmod(_to_f32(step1 + b), b))
    return float(step2)


def lerp_to(to_a: float, to_b: float, t: float) -> float:
    return t * (to_b - to_a) + to_a


def lerp_from(from_a: float, from_b: float, t: float) -> float:
    return (from_a - t) / (from_a - from_b)


def lerp(from_a: float, from_b: float, to_c: float, to_d: float, t: float) -> float:
    return lerp_to(to_c, to_d, lerp_from(from_a, from_b, t))


class Color:
    """RGB color with float components in [0, 1]."""

    __slots__ = ("r", "g", "b")

    def __init__(self, r: float, g: float, b: float) -> None:
        self.r = r
        self.g = g
        self.b = b

    @classmethod
    def from_uint8_values(cls, r: int, g: int, b: int) -> Color:
        return cls(r / 255.0, g / 255.0, b / 255.0)

    def lerp_to(self, other: Color, t: float) -> Color:
        f = clamped(t)
        red = clamped(self.r * (1.0 - f) + other.r * f)
        green = clamped(self.g * (1.0 - f) + other.g * f)
        blue = clamped(self.b * (1.0 - f) + other.b * f)
        return Color(red, green, blue)

    def lighten(self, t: float) -> Color:
        return self.lerp_to(WHITE, t)

    def darken(self, t: float) -> Color:
        return self.lerp_to(BLACK, t)

    def burn(self, t: float) -> Color:
        f = max(1.0 - t, 1.0e-7)
        return Color(
            min(1.0 - (1.0 - self.r) / f, 1.0),
            min(1.0 - (1.0 - self.g) / f, 1.0),
            min(1.0 - (1.0 - self.b) / f, 1.0),
        )

    def luminance(self) -> float:
        """Perceived luminance using single-precision intermediates."""
        rv = _to_f32(0.299 * self.r)
        gv = _to_f32(0.587 * self.g)
        bv = _to_f32(0.114 * self.b)
        val = _to_f32(_to_f32(rv * rv) + _to_f32(gv * gv) + _to_f32(bv * bv))
        return float(_to_f32(math.sqrt(float(val))))


WHITE = Color(1.0, 1.0, 1.0)
BLACK = Color(0.0, 0.0, 0.0)
