from __future__ import annotations

from ._grid import Grid


class ChangeGrid:
    """Tracks which cells and their neighborhoods changed."""

    def __init__(self, width: int, height: int) -> None:
        self.grid: Grid[bool] = Grid(width, height, False)

    def set_changed(self, px: int, py: int) -> None:
        w = self.grid.width
        h = self.grid.height
        for oy in range(-1, 2):
            for ox in range(-1, 2):
                nx = (ox + px) % w
                ny = (oy + py) % h
                self.grid.set_value(True, nx, ny)
