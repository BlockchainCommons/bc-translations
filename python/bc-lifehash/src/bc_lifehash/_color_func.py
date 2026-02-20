from __future__ import annotations

from typing import Callable

from ._color import Color, modulo

ColorFunc = Callable[[float], Color]


def reverse(c: ColorFunc) -> ColorFunc:
    return lambda t: c(1.0 - t)


def blend2(color1: Color, color2: Color) -> ColorFunc:
    return lambda t: color1.lerp_to(color2, t)


def blend(colors: list[Color]) -> ColorFunc:
    count = len(colors)
    if count == 0:
        return blend2(Color(0.0, 0.0, 0.0), Color(0.0, 0.0, 0.0))
    if count == 1:
        return blend2(colors[0], colors[0])
    if count == 2:
        return blend2(colors[0], colors[1])

    def _blend(t: float) -> Color:
        if t >= 1.0:
            return colors[count - 1]
        if t <= 0.0:
            return colors[0]
        segments = count - 1
        s = t * segments
        segment = int(s)
        segment_frac = modulo(s, 1.0)
        c1 = colors[segment]
        c2 = colors[segment + 1]
        return c1.lerp_to(c2, segment_frac)

    return _blend
