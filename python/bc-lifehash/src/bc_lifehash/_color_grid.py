from __future__ import annotations

from typing import NamedTuple

from ._color import Color
from ._color_func import ColorFunc
from ._frac_grid import FracGrid
from ._grid import Grid
from ._patterns import Pattern


class _Transform(NamedTuple):
    transpose: bool
    reflect_x: bool
    reflect_y: bool


class ColorGrid:
    """Applies gradient and symmetry transforms to a FracGrid."""

    def __init__(
        self, frac_grid: FracGrid, gradient: ColorFunc, pattern: Pattern
    ) -> None:
        multiplier = 1 if pattern == Pattern.FIDUCIAL else 2
        target_width = frac_grid.grid.width * multiplier
        target_height = frac_grid.grid.height * multiplier

        self.grid: Grid[Color] = Grid(
            target_width, target_height, Color(0.0, 0.0, 0.0)
        )
        max_x = target_width - 1
        max_y = target_height - 1

        if pattern == Pattern.SNOWFLAKE:
            transforms = [
                _Transform(False, False, False),
                _Transform(False, True, False),
                _Transform(False, False, True),
                _Transform(False, True, True),
            ]
        elif pattern == Pattern.PINWHEEL:
            transforms = [
                _Transform(False, False, False),
                _Transform(True, True, False),
                _Transform(True, False, True),
                _Transform(False, True, True),
            ]
        else:  # FIDUCIAL
            transforms = [
                _Transform(False, False, False),
            ]

        frac_width = frac_grid.grid.width
        frac_height = frac_grid.grid.height
        for y in range(frac_height):
            for x in range(frac_width):
                value = frac_grid.grid.get_value(x, y)
                color = gradient(value)
                for t in transforms:
                    px = x
                    py = y
                    if t.transpose:
                        px, py = py, px
                    if t.reflect_x:
                        px = max_x - px
                    if t.reflect_y:
                        py = max_y - py
                    self.grid.set_value(color, px, py)

    def colors(self) -> list[float]:
        result: list[float] = []
        for c in self.grid:
            result.append(c.r)
            result.append(c.g)
            result.append(c.b)
        return result
