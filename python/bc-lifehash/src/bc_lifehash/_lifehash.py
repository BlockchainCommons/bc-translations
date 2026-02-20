from __future__ import annotations

import hashlib
from enum import Enum

from ._bit_enumerator import BitEnumerator
from ._cell_grid import CellGrid
from ._change_grid import ChangeGrid
from ._color import clamped, lerp_from
from ._color_grid import ColorGrid
from ._frac_grid import FracGrid
from ._gradients import select_gradient
from ._patterns import select_pattern


class Version(Enum):
    VERSION1 = "version1"
    VERSION2 = "version2"
    DETAILED = "detailed"
    FIDUCIAL = "fiducial"
    GRAYSCALE_FIDUCIAL = "grayscale_fiducial"


class Image:
    """LifeHash output image with RGB or RGBA pixel data."""

    __slots__ = ("width", "height", "colors")

    def __init__(self, width: int, height: int, colors: bytes) -> None:
        self.width = width
        self.height = height
        self.colors = colors


def _sha256(data: bytes) -> bytes:
    return hashlib.sha256(data).digest()


def _make_image(
    width: int,
    height: int,
    float_colors: list[float],
    module_size: int,
    has_alpha: bool,
) -> Image:
    if module_size <= 0:
        raise ValueError("Invalid module size")

    scaled_width = width * module_size
    scaled_height = height * module_size
    result_components = 4 if has_alpha else 3
    scaled_capacity = scaled_width * scaled_height * result_components

    result_colors = bytearray(scaled_capacity)

    # Match C++ loop order: outer loop uses scaled_width, inner uses scaled_height
    for target_y in range(scaled_width):
        for target_x in range(scaled_height):
            source_x = target_x // module_size
            source_y = target_y // module_size
            source_offset = (source_y * width + source_x) * 3

            target_offset = (target_y * scaled_width + target_x) * result_components

            result_colors[target_offset] = int(
                clamped(float_colors[source_offset]) * 255.0
            )
            result_colors[target_offset + 1] = int(
                clamped(float_colors[source_offset + 1]) * 255.0
            )
            result_colors[target_offset + 2] = int(
                clamped(float_colors[source_offset + 2]) * 255.0
            )
            if has_alpha:
                result_colors[target_offset + 3] = 255

    return Image(scaled_width, scaled_height, bytes(result_colors))


def make_from_utf8(
    s: str, version: Version, module_size: int, has_alpha: bool
) -> Image:
    return make_from_data(s.encode("utf-8"), version, module_size, has_alpha)


def make_from_data(
    data: bytes, version: Version, module_size: int, has_alpha: bool
) -> Image:
    digest = _sha256(data)
    return make_from_digest(digest, version, module_size, has_alpha)


def make_from_digest(
    digest: bytes, version: Version, module_size: int, has_alpha: bool
) -> Image:
    if len(digest) != 32:
        raise ValueError("Digest must be 32 bytes")

    if version in (Version.VERSION1, Version.VERSION2):
        length = 16
        max_generations = 150
    else:
        length = 32
        max_generations = 300

    current_cell_grid = CellGrid(length, length)
    next_cell_grid = CellGrid(length, length)
    current_change_grid = ChangeGrid(length, length)
    next_change_grid = ChangeGrid(length, length)

    if version == Version.VERSION1:
        next_cell_grid.set_data(digest)
    elif version == Version.VERSION2:
        hashed = _sha256(digest)
        next_cell_grid.set_data(hashed)
    else:
        digest1 = digest
        if version == Version.GRAYSCALE_FIDUCIAL:
            digest1 = _sha256(digest1)
        digest2 = _sha256(digest1)
        digest3 = _sha256(digest2)
        digest4 = _sha256(digest3)
        digest_final = digest1 + digest2 + digest3 + digest4
        next_cell_grid.set_data(digest_final)

    next_change_grid.grid.set_all(True)

    history_set: set[bytes] = set()
    history: list[bytes] = []

    while len(history) < max_generations:
        current_cell_grid, next_cell_grid = next_cell_grid, current_cell_grid
        current_change_grid, next_change_grid = next_change_grid, current_change_grid

        data_bytes = current_cell_grid.data()
        hash_val = _sha256(data_bytes)
        if hash_val in history_set:
            break
        history_set.add(hash_val)
        history.append(data_bytes)

        current_cell_grid.next_generation(
            current_change_grid, next_cell_grid, next_change_grid
        )

    frac_grid = FracGrid(length, length)
    for i, h in enumerate(history):
        current_cell_grid.set_data(h)
        frac = clamped(lerp_from(0.0, float(len(history)), float(i + 1)))
        frac_grid.overlay(current_cell_grid, frac)

    # Normalize the frac_grid to [0, 1] (except version1)
    if version != Version.VERSION1:
        min_value = float("inf")
        max_value = float("-inf")

        def find_minmax(x: int, y: int) -> None:
            nonlocal min_value, max_value
            value = frac_grid.grid.get_value(x, y)
            if value < min_value:
                min_value = value
            if value > max_value:
                max_value = value

        frac_grid.grid.for_all(find_minmax)

        w = frac_grid.grid.width
        h = frac_grid.grid.height
        for y in range(h):
            for x in range(w):
                value = frac_grid.grid.get_value(x, y)
                normalized = lerp_from(min_value, max_value, value)
                frac_grid.grid.set_value(normalized, x, y)

    entropy = BitEnumerator(digest)

    if version == Version.DETAILED:
        entropy.next()
    elif version == Version.VERSION2:
        entropy.next_uint2()

    gradient = select_gradient(entropy, version)
    pattern = select_pattern(entropy, version)
    color_grid = ColorGrid(frac_grid, gradient, pattern)

    return _make_image(
        color_grid.grid.width,
        color_grid.grid.height,
        color_grid.colors(),
        module_size,
        has_alpha,
    )
