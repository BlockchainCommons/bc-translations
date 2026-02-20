from __future__ import annotations

import math

from ._color import Color, _to_f32, clamped, modulo


class HSBColor:
    """HSB/HSV color with conversion to RGB."""

    __slots__ = ("hue", "saturation", "brightness")

    def __init__(self, hue: float, saturation: float, brightness: float) -> None:
        self.hue = hue
        self.saturation = saturation
        self.brightness = brightness

    @classmethod
    def from_hue(cls, hue: float) -> HSBColor:
        return cls(hue, 1.0, 1.0)

    def color(self) -> Color:
        v = clamped(self.brightness)
        s = clamped(self.saturation)

        if s <= 0.0:
            return Color(v, v, v)

        h = modulo(self.hue, 1.0)
        if h < 0.0:
            h += 1.0
        h *= 6.0
        # C++ uses floorf (f32 precision)
        i = int(math.floor(_to_f32(h)))
        f = h - float(i)
        p = v * (1.0 - s)
        q = v * (1.0 - s * f)
        t = v * (1.0 - s * (1.0 - f))

        if i == 0:
            return Color(v, t, p)
        elif i == 1:
            return Color(q, v, p)
        elif i == 2:
            return Color(p, v, t)
        elif i == 3:
            return Color(p, q, v)
        elif i == 4:
            return Color(t, p, v)
        elif i == 5:
            return Color(v, p, q)
        else:
            raise RuntimeError("Internal error in HSB conversion")
