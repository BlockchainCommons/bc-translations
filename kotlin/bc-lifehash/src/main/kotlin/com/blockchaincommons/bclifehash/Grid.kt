package com.blockchaincommons.bclifehash

/**
 * A two-dimensional grid backed by a flat list.
 *
 * Coordinates wrap toroidally: out-of-range indices wrap around using
 * modular arithmetic, making the grid behave as a torus for neighbor
 * lookups.
 */
internal class Grid<T>(
    val width: Int,
    val height: Int,
    defaultValue: T,
) {
    internal val storage: MutableList<T> = MutableList(width * height) { defaultValue }

    private fun offset(x: Int, y: Int): Int = y * width + x

    private fun circularIndex(index: Int, modulus: Int): Int = ((index % modulus) + modulus) % modulus

    fun fill(value: T) {
        storage.fill(value)
    }

    fun setValue(value: T, x: Int, y: Int) {
        storage[offset(x, y)] = value
    }

    fun getValue(x: Int, y: Int): T = storage[offset(x, y)]

    fun forEach(block: (x: Int, y: Int) -> Unit) {
        for (y in 0 until height) {
            for (x in 0 until width) {
                block(x, y)
            }
        }
    }

    fun forNeighborhood(px: Int, py: Int, block: (offsetX: Int, offsetY: Int, neighborX: Int, neighborY: Int) -> Unit) {
        for (oy in -1..1) {
            for (ox in -1..1) {
                val nx = circularIndex(ox + px, width)
                val ny = circularIndex(oy + py, height)
                block(ox, oy, nx, ny)
            }
        }
    }
}
