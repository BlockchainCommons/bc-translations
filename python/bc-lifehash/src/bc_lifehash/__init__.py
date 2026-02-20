"""LifeHash visual hashing algorithm.

``bc_lifehash`` is a method of hash visualization based on Conway's Game of
Life that creates beautiful icons that are deterministic, yet distinct and
unique given the input data.

The basic concept is to take a SHA-256 hash of the input data and then use the
256-bit digest as a 16x16 pixel "seed" for running the cellular automata known
as Conway's Game of Life.  After the pattern becomes stable (or begins
repeating) the resulting history is used to compile a grayscale image of all
the states from the first to last generation.  Some bits of the initial hash
are then used to deterministically apply symmetry and color.

Five LifeHash versions are supported via the ``Version`` enum:

- **VERSION1** / **VERSION2** -- 16x16 grid, up to 150 generations.
- **DETAILED** -- 32x32 grid, up to 300 generations, richer color gradients.
- **FIDUCIAL** -- 32x32, designed for use as fiducial markers.
- **GRAYSCALE_FIDUCIAL** -- Same as Fiducial but rendered in grayscale.
"""

from ._lifehash import Image, Version, make_from_data, make_from_digest, make_from_utf8

__all__ = [
    "Image",
    "Version",
    "make_from_data",
    "make_from_digest",
    "make_from_utf8",
]
