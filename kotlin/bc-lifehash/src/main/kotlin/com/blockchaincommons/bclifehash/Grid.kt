package com.blockchaincommons.bclifehash

class Grid<T>(
    val width: Int,
    val height: Int,
    defaultValue: T,
) {
    val storage: MutableList<T> = MutableList(width * height) { defaultValue }

    private fun offset(x: Int, y: Int): Int = y * width + x

    private fun circularIndex(index: Int, modulus: Int): Int = ((index % modulus) + modulus) % modulus

    fun setAll(value: T) {
        storage.fill(value)
    }

    fun setValue(value: T, x: Int, y: Int) {
        storage[offset(x, y)] = value
    }

    fun getValue(x: Int, y: Int): T = storage[offset(x, y)]

    fun forAll(block: (Int, Int) -> Unit) {
        for (y in 0 until height) {
            for (x in 0 until width) {
                block(x, y)
            }
        }
    }

    fun forNeighborhood(px: Int, py: Int, block: (Int, Int, Int, Int) -> Unit) {
        for (oy in -1..1) {
            for (ox in -1..1) {
                val nx = circularIndex(ox + px, width)
                val ny = circularIndex(oy + py, height)
                block(ox, oy, nx, ny)
            }
        }
    }
}
