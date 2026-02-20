from __future__ import annotations

from typing import Callable, TypeVar

T = TypeVar("T")


class Grid(list[T]):
    """Generic toroidal grid with neighborhood traversal.

    Stores values in a flat list with width*height elements.
    Wraps around edges for neighborhood queries.
    """

    width: int
    height: int

    def __init__(self, width: int, height: int, default: T) -> None:
        super().__init__([default] * (width * height))
        self.width = width
        self.height = height

    def _offset(self, x: int, y: int) -> int:
        return y * self.width + x

    @staticmethod
    def _circular_index(index: int, modulus: int) -> int:
        return (index % modulus + modulus) % modulus

    def set_all(self, value: T) -> None:
        for i in range(len(self)):
            self[i] = value

    def set_value(self, value: T, x: int, y: int) -> None:
        self[self._offset(x, y)] = value

    def get_value(self, x: int, y: int) -> T:
        return self[self._offset(x, y)]

    def for_all(self, f: Callable[[int, int], None]) -> None:
        for y in range(self.height):
            for x in range(self.width):
                f(x, y)

    def for_neighborhood(
        self, px: int, py: int, f: Callable[[int, int, int, int], None]
    ) -> None:
        for oy in range(-1, 2):
            for ox in range(-1, 2):
                nx = self._circular_index(ox + px, self.width)
                ny = self._circular_index(oy + py, self.height)
                f(ox, oy, nx, ny)
