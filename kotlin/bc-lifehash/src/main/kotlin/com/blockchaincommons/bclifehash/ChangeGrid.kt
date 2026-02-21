package com.blockchaincommons.bclifehash

/**
 * Tracks which cells (and their neighbors) changed between generations.
 */
internal class ChangeGrid(width: Int, height: Int) {
    val grid = Grid(width, height, false)

    fun setChanged(px: Int, py: Int) {
        val width = grid.width
        val height = grid.height
        for (oy in -1..1) {
            for (ox in -1..1) {
                val nx = ((ox + px) % width + width) % width
                val ny = ((oy + py) % height + height) % height
                grid.setValue(true, nx, ny)
            }
        }
    }
}
