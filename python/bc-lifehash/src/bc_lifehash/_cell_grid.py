from __future__ import annotations

from ._bit_enumerator import BitAggregator, BitEnumerator
from ._change_grid import ChangeGrid
from ._grid import Grid


class CellGrid:
    """Conway's Game of Life grid."""

    def __init__(self, width: int, height: int) -> None:
        self.grid: Grid[bool] = Grid(width, height, False)

    @staticmethod
    def _is_alive_in_next_generation(
        current_alive: bool, neighbors_count: int
    ) -> bool:
        if current_alive:
            return neighbors_count == 2 or neighbors_count == 3
        return neighbors_count == 3

    def _count_neighbors(self, px: int, py: int) -> int:
        total = 0

        def count(ox: int, oy: int, nx: int, ny: int) -> None:
            nonlocal total
            if ox == 0 and oy == 0:
                return
            if self.grid.get_value(nx, ny):
                total += 1

        self.grid.for_neighborhood(px, py, count)
        return total

    def data(self) -> bytes:
        a = BitAggregator()

        def append_bit(x: int, y: int) -> None:
            a.append(self.grid.get_value(x, y))

        self.grid.for_all(append_bit)
        return a.data()

    def set_data(self, data: bytes | bytearray) -> None:
        e = BitEnumerator(data)
        i = 0

        def set_bit(b: bool) -> None:
            nonlocal i
            self.grid[i] = b
            i += 1

        e.for_all(set_bit)

    def next_generation(
        self,
        current_change_grid: ChangeGrid,
        next_cell_grid: CellGrid,
        next_change_grid: ChangeGrid,
    ) -> None:
        next_cell_grid.grid.set_all(False)
        next_change_grid.grid.set_all(False)
        w = self.grid.width
        h = self.grid.height
        for y in range(h):
            for x in range(w):
                current_alive = self.grid.get_value(x, y)
                if current_change_grid.grid.get_value(x, y):
                    neighbors_count = self._count_neighbors(x, y)
                    next_alive = self._is_alive_in_next_generation(
                        current_alive, neighbors_count
                    )
                    if next_alive:
                        next_cell_grid.grid.set_value(True, x, y)
                    if current_alive != next_alive:
                        next_change_grid.set_changed(x, y)
                else:
                    next_cell_grid.grid.set_value(current_alive, x, y)
