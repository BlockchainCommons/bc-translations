"""Weighted random sampling using Vose's alias method."""

from __future__ import annotations

from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from ._xoshiro256 import Xoshiro256


class WeightedSampler:
    """Alias method (Vose's algorithm) for weighted random sampling."""

    __slots__ = ("_aliases", "_probs")

    def __init__(self, weights: list[float]) -> None:
        if any(w < 0.0 for w in weights):
            raise ValueError("negative weight encountered")
        summed = sum(weights)
        if summed <= 0.0:
            raise ValueError("weights must sum to a positive value")

        count = len(weights)
        weights = [w * count / summed for w in weights]

        small: list[int] = []
        large: list[int] = []
        for j in range(count - 1, -1, -1):
            if weights[j] < 1.0:
                small.append(j)
            else:
                large.append(j)

        probs = [0.0] * count
        aliases = [0] * count

        while small and large:
            a = small.pop()
            g = large.pop()
            probs[a] = weights[a]
            aliases[a] = g
            weights[g] += weights[a] - 1.0
            if weights[g] < 1.0:
                small.append(g)
            else:
                large.append(g)

        while large:
            g = large.pop()
            probs[g] = 1.0

        while small:
            a = small.pop()
            probs[a] = 1.0

        self._aliases = aliases
        self._probs = probs

    def next(self, xoshiro: Xoshiro256) -> int:
        """Sample the next value using the given RNG."""
        r1 = xoshiro.next_double()
        r2 = xoshiro.next_double()
        n = len(self._probs)
        i = int(n * r1)
        if r2 < self._probs[i]:
            return i
        return self._aliases[i]
