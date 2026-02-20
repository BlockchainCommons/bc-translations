from __future__ import annotations

from ._cell_grid import CellGrid
from ._grid import Grid


class FracGrid:
    """Overlays cell grid states as fractional values."""

    def __init__(self, width: int, height: int) -> None:
        self.grid: Grid[float] = Grid(width, height, 0.0)

    def overlay(self, cell_grid: CellGrid, frac: float) -> None:
        w = self.grid.width
        h = self.grid.height
        for y in range(h):
            for x in range(w):
                if cell_grid.grid.get_value(x, y):
                    self.grid.set_value(frac, x, y)
